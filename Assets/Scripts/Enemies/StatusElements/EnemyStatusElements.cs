using System.Collections.Generic;
using UnityEngine;
public class EnemyStatusElements
{
    // References
    private Enemy HostEnemy { get; set; }                       // Reference to enemy this is attached to

    // Status elements
    public float SpeedModifier { get; private set; }            // When slowed, change this value

    private readonly List<StatusElementClass> statuses = new(); // Set of status elements applied to this 
    private float statusTimer;                                  // Timer for updating statuses
    private const float statusTime = 0.1f;                      // How often are statuses updated

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="hostEnemy"></param>
    public EnemyStatusElements(Enemy hostEnemy)
    {
        HostEnemy = hostEnemy;
        SpeedModifier = 1f;
    }

    /// <summary>
    /// Apply effects from status elements
    /// </summary>
    /// <param name="s"></param>
    private void ApplyStatusEffect(StatusElementClass s)
    {
        switch (s.statusEff)
        {
            case StatusEffects.Slow:
                if (s.strength < SpeedModifier)
                    SpeedModifier = s.strength;
                break;

            case StatusEffects.Dot:
                HostEnemy.TakeDamage(s.strength * statusTime);
                break;

            case StatusEffects.DotPrecent:
                HostEnemy.TakeDamage(s.strength * HostEnemy.MaxHealth * statusTime);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Update status element
    /// </summary>
    public void UpdateStatuses()
    {
        statusTimer += Time.deltaTime;
        if (statusTimer > statusTime)
        {
            for (int i = statuses.Count - 1; i >= 0; i--)
            {
                if (statuses.Count <= 0)
                {
                    break;
                }

                statuses[i].timer += 0.1f;
                if (statuses[i].timer >= statuses[i].duration)
                {
                    if (statuses[i].statusEff == StatusEffects.Slow)
                    {
                        SpeedModifier = 1f;
                    }

                    statuses.RemoveAt(i);
                    continue;
                }
                ApplyStatusEffect(statuses[i]);
            }
            statusTimer = 0f;
        }
    }

    /// <summary>
    /// Add a new status element
    /// </summary>
    /// <param name="s"></param>
    public void CreateStatus(StatusElementClass s)
    {
        foreach (StatusElementClass status in statuses)
        {
            // If the same status is already being applied
            // Reset duration but set the stronger effect
            if (s.bulletApplying == status.bulletApplying)
            {
                if (s.strength > status.strength)
                    status.strength = s.strength;
                status.timer = 0f;
                return;
            }
        }
        // Add new status
        statuses.Add(s);
    }

    /// <summary>
    /// Reset every status element
    /// </summary>
    public void ResetStatuses()
    {
        statuses.Clear();
        SpeedModifier = 1f;
    }

}
