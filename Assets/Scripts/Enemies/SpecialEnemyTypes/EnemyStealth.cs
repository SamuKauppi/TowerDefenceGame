using System.Collections;
using UnityEngine;
public class EnemyStealth : Enemy, ISpecialAbility
{
    // References
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private SpriteRenderer enemyRenderer;

    // Stealth variables
    [SerializeField] private bool startAsHidden;
    [SerializeField] private float timeHidden;
    [SerializeField] private float timeVisible;

    // Modifiers
    [SerializeField] private float speedModifierAsHidden;

    // Variables
    private Color enemyColor;

    /// <summary>
    /// Preodically hides and shows this enemy
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartStealth()
    {
        // Initialize variables
        enemyColor = enemyRenderer.color;
        bool isVisible = startAsHidden;
        float hiddenTimer = 0f;
        float visibleTimer = 0f;

        while (true)
        {
            // update timers
            if (!isVisible)
            {
                hiddenTimer += Time.deltaTime;
            }
            else
            {
                visibleTimer += Time.deltaTime;
            }

            // check if timers have passed
            if (hiddenTimer > timeHidden || visibleTimer > timeVisible)
            {
                // Reset timers and toggle isVisible
                hiddenTimer = 0f;
                visibleTimer = 0f;
                isVisible = !isVisible;

                // Apply conditions based if the enemy is be hidden or visible
                enemyCollider.enabled = isVisible;
                enemyColor.a = isVisible ? 1f : 0.5f;
                enemyRenderer.color = enemyColor;
                abilitySpeedModifier = isVisible ? 1f : speedModifierAsHidden;

            }

            yield return null;
        }
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartStealth());
    }
}
