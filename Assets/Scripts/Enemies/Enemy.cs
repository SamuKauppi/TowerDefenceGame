using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // References
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PathPoint currentTarget;
    private Vector3 targetdir = Vector3.zero;
    private Vector3 newDir = Vector3.zero;
    [SerializeField] private float moveTimer;

    // Stats
    [SerializeField] private float health = 100f;
    public float MaxHealth { get; private set; }
    [SerializeField] private float damage;
    [SerializeField] private float distanceToStop;
    [SerializeField] private float speed;
    [SerializeField] private float turnSpeed;

    // Status elements
    [SerializeField] private List<StatusElementClass> statuses = new();
    private float statusTimer;                          // Used to update statuses every 0.1s
    private float speedModifier = 1f;                   // When slowed, change this value

    private void Start()
    {
        MaxHealth = health;
    }
    private void Update()
    {
        GetTargetDirection();
        UpdateStatuses();
    }
    private void FixedUpdate()
    {
        MoveToNextTarget();
    }

    private void MoveToNextTarget()
    {
        if (newDir != targetdir)
        {
            newDir += targetdir * turnSpeed;
            newDir.Normalize();
        }
        _rb.MovePosition(speedModifier * speed * Time.fixedDeltaTime * newDir + transform.position);
    }

    private void GetTargetDirection()
    {
        if (currentTarget == null || moveTimer > 2f)
        {
            moveTimer = 0f;
            currentTarget = Pathfinding.Instance.GetClosestPathPoint(transform.position);
        }
        else if (Vector2.Distance(transform.position, currentTarget.transform.position) > distanceToStop)
        {
            moveTimer += Time.deltaTime;
            targetdir = currentTarget.transform.position - transform.position;
            targetdir.Normalize();
        }
        else
        {
            moveTimer = 0f;
            currentTarget = Pathfinding.Instance.GetNextPathPoint(currentTarget);
        }
    }

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
}
