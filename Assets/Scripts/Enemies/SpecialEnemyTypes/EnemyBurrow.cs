using System.Collections;
using UnityEngine;

public class EnemyBurrow : Enemy, ISpecialAbility
{
    [SerializeField] private SpriteRenderer enemyRenderer;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private float burrowTime;
    [SerializeField] private float burrowSpeed;
    private const GameEntity burrowIdent = GameEntity.BurrowVfx;
    private bool isBurrowing;

    IEnumerator StartBurrowing()
    {
        SetVisibility(false);
        SetColliderState(false);
        SetSpeedModifier(burrowSpeed);
        isBurrowing = true;
        StartCoroutine(SpawnBurrowVfx());
        yield return new WaitForSeconds(burrowTime);
        isBurrowing = false;
        SetVisibility(true);
        SetColliderState(true);
        SetSpeedModifier(1f);
    }

    IEnumerator SpawnBurrowVfx()
    {
        while(isBurrowing)
        {
            GameObject burrow = ObjectPooler.Instance.GetPooledObject(burrowIdent);
            burrow.transform.position = transform.position;
            yield return new WaitForSeconds(0.2f);
        }
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
