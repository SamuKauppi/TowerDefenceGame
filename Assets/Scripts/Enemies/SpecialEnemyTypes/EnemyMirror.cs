using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMirror : Enemy, ISpecialAbility
{
    [SerializeField] private float copyTime = 2f;
    [SerializeField] private float copyRadius = 0.5f;
    [SerializeField] private float moveCopiedTime = 0.25f;
    [SerializeField] private GameEntity copyTarget = GameEntity.MirrorEnemy;
    [SerializeField] private int spawnLimit = 10;
    [SerializeField] private float speedIncreseAfterSpawn;
    private int enemiesSpawned;

    private IEnumerator StartCopying()
    {
        yield return new WaitForEndOfFrame();
        while (enemiesSpawned < spawnLimit)
        {
            yield return new WaitForSeconds(copyTime);
            enemiesSpawned++;
            GameObject enemy = ObjectPooler.Instance.GetPooledObject(copyTarget);
            Vector3 randPos = StaticFunctions.Instance.GetRandomPointInCircle(copyRadius, transform.position);
            enemy.transform.position = transform.position;
            LeanTween.move(enemy, randPos, moveCopiedTime);
            abilitySpeedModifier = 1 + (speedIncreseAfterSpawn * enemiesSpawned);
        }
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartCopying());
    }
}
