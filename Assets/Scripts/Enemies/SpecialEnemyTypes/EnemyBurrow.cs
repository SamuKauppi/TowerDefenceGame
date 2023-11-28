using System.Collections;
using UnityEngine;

public class EnemyBurrow : Enemy, ISpecialAbility
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private float burrowTime;
    [SerializeField] private float burrowSpeed;

    IEnumerator StartBurrowing()
    {
        SetVisibility(false);
        SetColliderState(false);
        SetSpeedModifier(burrowSpeed);
        yield return new WaitForSeconds(burrowTime);
        SetVisibility(true);
        SetColliderState(true);
        SetSpeedModifier(1f);
    }

    void SetVisibility(bool isVisible)
    {
        enemyRenderer.enabled = isVisible;
    }

    void SetColliderState(bool isEnabled)
    {
        enemyCollider.enabled = isEnabled;
    }

    void SetSpeedModifier(float speed)
    {
        abilitySpeedModifier = speed;
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartBurrowing());
    }
}
