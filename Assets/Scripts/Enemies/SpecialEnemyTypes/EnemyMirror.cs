using System.Collections;
using UnityEngine;

public class EnemyMirror : Enemy, ISpecialAbility
{
    [SerializeField] private float copyTime = 2f;
    [SerializeField] private float copyRadius = 0.5f;
    [SerializeField] private float moveCopiedTime = 0.25f;
    [SerializeField] private GameEntity copyTarget = GameEntity.MirrorEnemy;

    private IEnumerator StartCopying()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            yield return new WaitForSeconds(copyTime);
            GameObject enemy = ObjectPooler.Instance.GetPooledObject(copyTarget);
            Vector3 randPos = StaticFunctions.GetRandomPointInCircle(copyRadius, transform.position);
            enemy.transform.position = transform.position;
            LeanTween.move(enemy, randPos, moveCopiedTime);
        }
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartCopying());
    }
}
