using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IFixedUpdate, IUpdate
{
    public float MaxHealth { get; private set; }        // Max health

    // References
    [SerializeField] private Rigidbody2D _rb;           // Reference to rigidbody component
    [SerializeField] private PathPoint currentTarget;   // Current pathpoint target

    // Stats
    [SerializeField] private float health = 100f;       // Current health
    [SerializeField] private float damage;              // Damage to player when reaches end
    private bool isDead;                                // Is the enemy dead

    // Pathfinding
    [SerializeField] private float distanceToStop;      // Distance to when enemy reaches pathpoint
    [SerializeField] private float speed;               // How fast this enemy is
    [SerializeField] private float turnSpeed;           // How fast enemy can alter direction of movement
    private Vector3 targetdir = Vector3.zero;           // Direction to the next pathpoint
    private Vector3 currentDirection = Vector3.zero;    // Current direction of movement

    // Move timers
    private float moveTimer;                            // Failsafe if no targets have been found in moveTime seconds
    private const float moveTime = 2f;                  // Threshold for moveTimer

    // Status elements
    private readonly List<StatusElementClass> statuses = new();
    private float statusTimer;                          // Used to update statuses every 0.1s
    private float speedModifier = 1f;                   // When slowed, change this value

    // From interfaces
    public GameObject Object => gameObject;

    private void Start()
    {
        MaxHealth = health;
        GameObjectUpdateManager.Instance.AddObject(this);
    }

    private void OnEnable()
    {
        isDead = false;
    }

    /// <summary>
    /// Update enemy
    /// </summary>
    public void UpdateObject()
    {
        GetTargetDirection();
        UpdateStatuses();
    }

    /// <summary>
    /// Fixedupdate enemy
    /// </summary>
    public void FixedUpdateGameobject()
    {
        MoveToNextTarget();
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
        _rb.MovePosition(speedModifier * speed * Time.fixedDeltaTime * currentDirection + transform.position);
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
    /// Reset stats after dying
    /// </summary>
    private void ResetStats()
    {
        statuses.Clear();
        health = MaxHealth;
        speedModifier = 1f;
    }

    /// <summary>
    /// Returns enemy damage
    /// </summary>
    /// <returns></returns>
    public float GetEnemyDamage()
    {
        return damage;
    }

    public void CreateStatus(StatusElementClass s)
    {
        foreach (StatusElementClass status in statuses)
        {
            if (s.statusName == status.statusName)
            {
                status.timer = 0f;
                return;
            }
        }
        statuses.Add(s);
    }
    private void ApplyStatus(StatusElementClass s)
    {
        switch (s.statusEff)
        {
            case "slow":
                if (s.strength < speedModifier)
                    speedModifier = s.strength;
                break;
            case "dot":
                TakeDamage(s.strength);
                break;
            case "%dot":
                TakeDamage(s.strength * MaxHealth);
                break;
            default:
                break;
        }
    }
    private void UpdateStatuses()
    {
        statusTimer += Time.deltaTime;
        if (statusTimer > 0.1f)
        {
            for (int i = statuses.Count - 1; i >= 0; i--)
            {
                if (statuses[i].timer >= statuses[i].duration)
                {
                    if (statuses[i].statusEff.Equals("slow"))
                        speedModifier = 1f;
                    statuses.RemoveAt(i);
                    continue;
                }
                statuses[i].timer += 0.1f;
                ApplyStatus(statuses[i]);
            }
            statusTimer = 0f;
        }
    }
    /// <summary>
    /// Deal damage to this enemy
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="damage"></param>
    private void DealDamageToEnemy(BulletDamages bullet, float damage)
    {
        foreach (StatusElementClass status in bullet.StatusElements)
        {
            CreateStatus(status);
        }

        TakeDamage(damage);
    }
    /// <summary>
    /// Make damage calculations
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            ResetStats();
            return;
        }
        health -= amount;
        if (health <= 0)
        {
            currentTarget = null;
            gameObject.SetActive(false);
            isDead = true;
            ResetStats();
        }
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
    }
    /// <summary>
    /// Collision check for damage over time
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out BulletDamages bullet) || !collision.gameObject.activeSelf)
            return;
        DealDamageToEnemy(bullet, bullet.DamageOverTime * Time.fixedDeltaTime);
    }
}
