using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IFixedUpdate, IUpdate
{
    public GameObject Object => gameObject;             // From interface
    public float MaxHealth { get; private set; }        // Max health

    // References
    [SerializeField] private Rigidbody2D _rb;           // Reference to rigidbody component

    // Stats
    [SerializeField] private float health = 100f;       // Current health
    [SerializeField] private float damage;              // Damage to player when reaches end
    [SerializeField] private float speed;               // How fast this enemy is

    // Pathfinding
    [SerializeField] private float distanceToStop;      // Distance to when enemy has reached it's target
    [SerializeField] private float turnSpeed;           // How fast enemy can alter direction of movement
    private PathPoint currentTarget;                    // Current pathpoint target
    private Vector3 targetDirection = Vector3.zero;     // Direction to the next pathpoint
    private Vector3 currentDirection = Vector3.zero;    // Current direction of movement
    private Pathfinding pathfinding;                    // To save the reference

    // Move timers
    private float moveTimer;                            // Failsafe if no targets have been found in moveTime seconds
    private const float moveTime = 2f;                  // Threshold for moveTimer

    // Damage over time done from contact
    private readonly List<BulletDamages> bulletDamages = new();         // Every bullet that hits this enemy (for dealing dot)
    private float damageTimer;                                          // Timer for when dot is applied
    private const float damageTime = 0.1f;                              // How often is dot applied 

    // Status elements
    private EnemyStatusElements statusElements;         // Reference to statusElements (created at start)

    // Ability variables
    private bool hasBeenActivated;                      // Ensures that ability is acitvated only once per lifetime
    protected float abilitySpeedModifier = 1f;          // Speed modifier for abilities

    /// <summary>
    /// Initialize max health, statuselement handler and add this to updatable objects
    /// </summary>
    private void Start()
    {
        MaxHealth = health;
        statusElements = new(this);
        GameObjectUpdateManager.Instance.AddObject(this);
        pathfinding = Pathfinding.Instance;
    }

    /// <summary>
    /// Update direction of movement
    /// </summary>
    private void MoveToNextTarget()
    {
        if (currentDirection != targetDirection)
        {
            // Calculate the difference between targetDirection and currentDirection
            Vector3 directionDifference = targetDirection - currentDirection;

            // Use turnSpeed to control the speed of turning
            currentDirection += Time.fixedDeltaTime * turnSpeed * directionDifference;

            // Ensure that currentDirection is normalized
            currentDirection.Normalize();
        }

        _rb.MovePosition(statusElements.SpeedModifier *
            abilitySpeedModifier *
            speed *
            Time.fixedDeltaTime *
            currentDirection +
            transform.position);
    }

    /// <summary>
    /// Set current direction towards current target
    /// </summary>c
    private void SetDirectionToTarget()
    {
        // Get a new target if its null or target has not been reached in x seconds
        if (currentTarget == null || moveTimer > moveTime)
        {
            moveTimer = 0f;
            currentTarget = pathfinding.GetClosestPathPoint(transform.position);
        }
        // Target has not been reached yet, continue moving
        else if (Vector2.Distance(transform.position, currentTarget.transform.position) > distanceToStop)
        {
            moveTimer += Time.deltaTime;
            targetDirection = currentTarget.transform.position - transform.position;
            targetDirection.Normalize();
        }
        // Target has been reached, get a new one
        else
        {
            moveTimer = 0f;
            currentTarget = pathfinding.GetNextPathpoint(currentTarget);
        }
    }

    /// <summary>
    /// Take damage for every damage over spawnTimer effect
    /// </summary>
    private void UpdateDamageOverTime()
    {

        // Update damage over spawnTimer effects based on waveTimer
        damageTimer += Time.fixedDeltaTime;
        if (damageTimer <= damageTime) return;

        damageTimer = 0f;
        for (int i = bulletDamages.Count - 1; i >= 0; i--)
        {
            if (bulletDamages.Count <= 0)
            {
                break;
            }

            BulletDamages damage = bulletDamages[i];
            // If damage is not null, take damage from it
            if (damage != null && damage.gameObject.activeSelf)
            {
                TakeDamage(damage.DamageOverTime * damageTime);
            }
            // Otherwise remove it from list
            else
            {
                bulletDamages.Remove(damage);
            }
        }
    }

    /// <summary>
    /// Reset stats after dying
    /// </summary>
    private void KillEnemy()
    {
        currentTarget = null;
        bulletDamages.Clear();
        health = MaxHealth;
        statusElements.ResetStatuses();
        gameObject.SetActive(false);
        hasBeenActivated = false;
    }

    /// <summary>
    /// Deal damage to this enemy
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="damage"></param>
    private void DealDamageToEnemy(BulletDamages bullet, float damage)
    {
        // Rare bug casues the enemy to take damage before start function is ran and statusElements is null
        if (statusElements == null)
        {
            return;
        }
        foreach (StatusElementClass status in bullet.StatusElements)
        {
            statusElements.CreateStatus(status);
        }
        TakeDamage(damage);
    }

    /// <summary>
    /// Collision check for initial hit
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out BulletDamages bullet) || !collision.gameObject.activeSelf)
            return;
        DealDamageToEnemy(bullet, bullet.GetBulletDamage());

        // Does the bullet contain damage over spawnTimer
        if (bullet.DamageOverTime <= 0)
        {
            return;
        }

        bulletDamages.Add(bullet);

    }

    /// <summary>
    /// Collision when trigger leaves (for dot)
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out BulletDamages bullet) || !collision.gameObject.activeSelf)
            return;

        // Set bullet null in list and remove it later
        if (bullet.DamageOverTime <= 0)
        {
            return;
        }

        int index = bulletDamages.IndexOf(bullet);
        if (index != -1)
        {
            bulletDamages[index] = null;
        }
    }

    /// <summary>
    /// Make damage calculations
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0f, MaxHealth);
        if (health <= 0)
        {
            KillEnemy();
        }
    }

    /// <summary>
    /// Returns enemy damage
    /// </summary>
    /// <returns></returns>
    public float GetEnemyDamage()
    {
        return damage;
    }

    /// <summary>
    /// Update enemy. From interface
    /// </summary>
    public void UpdateObject()
    {
        // Activate special ability if it's the first frame
        if (!hasBeenActivated)
        {
            hasBeenActivated = true;
            if (TryGetComponent(out ISpecialAbility ability))
            {
                ability.ActivateAbility();
            }
        }
        SetDirectionToTarget();
    }

    /// <summary>
    /// Fixedupdate enemy. From interface
    /// </summary>
    public void FixedUpdateGameobject()
    {
        MoveToNextTarget();
        UpdateDamageOverTime();
        statusElements.UpdateStatuses();
    }
}
