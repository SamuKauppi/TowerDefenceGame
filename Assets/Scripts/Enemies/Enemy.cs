using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IFixedUpdate, IUpdate
{
    // References
    [SerializeField] private Rigidbody2D _rb;           // Reference to rigidbody component
    [SerializeField] private PathPoint currentTarget;   // Current pathpoint target
    [SerializeField] private float moveTimer;           // Keeps the pathfinding from not breaking

    // Stats
    public float MaxHealth { get; private set; }        // Max health

    [SerializeField] private float health = 100f;       // Current health
    [SerializeField] private float damage;              // Damage to enemy

    // Pathfinding
    [SerializeField] private float distanceToStop;      // When a pathpoint is reached
    [SerializeField] private float speed;               // How fast this enemy is
    [SerializeField] private float turnSpeed;           // How fast enemy can alter direction of movement

    // Status elements
    [SerializeField] private List<StatusElementClass> statuses = new();
    private float statusTimer;                          // Used to update statuses every 0.1s
    private float speedModifier = 1f;                   // When slowed, change this value

    private Vector3 targetdir = Vector3.zero;           // Direction to the next pathpoint
    private Vector3 currentDirection = Vector3.zero;    // Current direction of movement

    // From interfaces
    public GameObject Object => gameObject;

    // Static
    private const string BULLET_TAG = "bullet";

    private void Start()
    {
        MaxHealth = health;
        GameObjectUpdateManager.Instance.AddObject(this);
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
        // Get a new target if none is found or target has not been reached in x seconds
        if (currentTarget == null || moveTimer > 2f)
        {
            moveTimer = 0f;
            currentTarget = Pathfinding.Instance.GetClosestPathPoint(transform.position);
        }
        // Target has not been reached yet
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
    /// Take damage
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            gameObject.SetActive(false);
            currentTarget = null;
            statuses.Clear();
            health = MaxHealth;
            speedModifier = 1f;
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
    //
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
                if (statuses.Count <= 0)
                    break;

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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(BULLET_TAG))
            return;

        Bullet b = collision.GetComponent<Bullet>();

        foreach (StatusElementClass status in b.GetBulletStatusElements())
        {
            CreateStatus(status);
        }

        TakeDamage(b.GetAndReduceBulletDamage());
    }
}
