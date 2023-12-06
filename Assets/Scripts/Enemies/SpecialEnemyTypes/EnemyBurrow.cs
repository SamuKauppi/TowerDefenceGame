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
        // Set burrowing true
        ToggleBurrowing(true);
        StartCoroutine(SpawnBurrowVfx());

        // Wait and then stop burrowing
        yield return new WaitForSeconds(burrowTime);
        ToggleBurrowing(false);
    }

    /// <summary>
    /// Sets values when burrowing and unburrowing
    /// </summary>
    /// <param name="value"></param>
    private void ToggleBurrowing(bool value)
    {
        // Set variables to value
        isBurrowing = value;
        flyToEnd = value;

        // Set visiblity to opposite value
        enemyRenderer.enabled = !value;
        enemyCollider.enabled = !value;

        // Set float variables
        abilitySpeedModifier = value ? burrowSpeed : 1f;
    }

    IEnumerator SpawnBurrowVfx()
    {
        while (isBurrowing)
        {
            GameObject burrow = ObjectPooler.Instance.GetPooledObject(burrowIdent);
            burrow.transform.position = transform.position;
            yield return new WaitForSeconds(1f / burrowSpeed);
        }
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartBurrowing());
    }
}
