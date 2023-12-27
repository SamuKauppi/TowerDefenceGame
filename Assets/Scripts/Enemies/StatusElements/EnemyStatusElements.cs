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
    /// <param name="status"></param>
    private void ApplyStatusEffect(StatusElementClass status)
    {
        switch (status.statusEff)
        {
            // Handle slow
            case StatusEffect.Slow:
                if ((1 - status.strength) < SpeedModifier)
                {
                    SpeedModifier = 1 - status.strength;
                }

                break;

            // Handle dot
            case StatusEffect.Dot:
                HostEnemy.TakeDamage(status.strength * statusTimer);
                break;

            // Handle %dot
            case StatusEffect.DotPrecent:
                HostEnemy.TakeDamage(status.strength * HostEnemy.MaxHealth * statusTimer);
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
        // Apply status at time
        statusTimer += Time.deltaTime;
        if (statusTimer > statusTime)
        {
            for (int i = statuses.Count - 1; i >= 0; i--)
            {
                // To prevent a rare bug
                if (statuses.Count <= 0)
                {
                    break;
                }

                // Check if the status has ended
                statuses[i].timer += statusTimer;
                if (statuses[i].timer >= statuses[i].duration)
                {
                    // Remove the slowing effects if it was slow
                    if (statuses[i].statusEff == StatusEffect.Slow)
                    {
                        SpeedModifier = 1f;
                    }

                    // Remove status and don't apply it
                    statuses.RemoveAt(i);
                    continue;
                }
                // Apply status
                ApplyStatusEffect(statuses[i]);
            }
            statusTimer = 0f;
        }
    }

    /// <summary>
    /// Add a new status element
    /// </summary>
    /// <param name="newStatus"></param>
    public void CreateStatus(StatusElementClass newStatus)
    {
        foreach (StatusElementClass status in statuses)
        {
            // If the same status is already being applied
            // Reset smoothnessValue but set the stronger effect
            if (newStatus.bulletApplying == status.bulletApplying)
            {
                if (newStatus.strength > status.strength)
                {
                    status.strength = newStatus.strength;
                }

                if (newStatus.duration > status.duration)
                {
                    status.duration = newStatus.duration;
                }

                status.timer = 0f;
                return;
            }
        }
        // Add new status
        statuses.Add(newStatus);
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
