using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IFixedUpdate, IUpdate
{
    public float MaxHealth { get; private set; }        // Max health

    // References
    [SerializeField] private Rigidbody2D _rb;                       // Reference to rigidbody component
    [SerializeField] private PathPoint currentTarget;               // Current pathpoint target

    // Stats
    [SerializeField] private float health = 100f;       // Current health
    [SerializeField] private float damage;              // Damage to player when reaches end

    // Pathfinding
    [SerializeField] private float distanceToStop;      // Distance to when enemy reaches pathpoint
    [SerializeField] private float speed;               // How fast this enemy is
    [SerializeField] private float turnSpeed;           // How fast enemy can alter direction of movement
    private Vector3 targetdir = Vector3.zero;           // Direction to the next pathpoint
    private Vector3 currentDirection = Vector3.zero;    // Current direction of movement

    // Move timers
    private float moveTimer;                            // Failsafe if no targets have been found in moveTime seconds
    private const float moveTime = 2f;                  // Threshold for moveTimer

    // Damage over spawnTimer
    private readonly List<BulletDamages> bulletDamages = new();         // Every bullet that hits this enemy (for dealing dot)
    private float damageTimer;                                          // Timer for when dot is applied
    private const float damageTime = 0.1f;                              // How often is dot applied 

    // Status elements
    private EnemyStatusElements statusElements;    // Reference to statusElements

    // From interfaces
    public GameObject Object => gameObject;

    /// <summary>
    /// Initialize max health, statuselement handler and add this to updatable objects
    /// </summary>
    private void Start()
    {
        MaxHealth = health;
        statusElements = new(this);
        GameObjectUpdateManager.Instance.AddObject(this);
    }

    /// <summary>
    /// Update enemy. From interface
    /// </summary>
    public void UpdateObject()
    {
        GetTargetDirection();
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

    /// <summary>
    /// Update direction of movement
    /// </summary>
    private void MoveToNextTarget()
    {
        if (currentDirection != targetdir)
        {
            currentDirection += targetdir * turnSpeed;
            currentDirection.Normalize();
        }
        _rb.MovePosition(statusElements.SpeedModifier *
            speed *
            Time.fixedDeltaTime *
            currentDirection +
            transform.position);
    }

    /// <summary>
    /// Get the direction to current
    /// </summary>
    private void GetTargetDirection()
    {
        // Get a new target if its null or target has not been reached in x seconds
        if (currentTarget == null || moveTimer > moveTime)
        {
            moveTimer = 0f;
            currentTarget = Pathfinding.Instance.GetClosestPathPoint(transform.position);
        }
        // Target has not been reached yet, continue moving
        else if (Vector2.Distance(transform.position, currentTarget.transform.position) > distanceToStop)
        {
            moveTimer += Time.deltaTime;
            targetdir = currentTarget.transform.position - transform.position;
            targetdir.Normalize();
        }
        // Target has been reached, get a new one
        else
        {
            moveTimer = 0f;
            currentTarget = Pathfinding.Instance.GetNextPathpoint(currentTarget);
        }
    }

    /// <summary>
    /// Take damage for every damage over spawnTimer effect
    /// </summary>
    private void UpdateDamageOverTime()
    {

        // Update damage over spawnTimer effects based on timer
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
    private void ResetStats()
    {
        bulletDamages.Clear();
        health = MaxHealth;
        statusElements.ResetStatuses();
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
        health -= amount;
        if (health <= 0)
        {
            currentTarget = null;
            gameObject.SetActive(false);
            ResetStats();
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

}
