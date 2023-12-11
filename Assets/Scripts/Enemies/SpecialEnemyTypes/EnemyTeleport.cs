using System.Collections;
using UnityEngine;

public class EnemyTeleport : Enemy, ISpecialAbility
{
    [SerializeField] private float maxXPosition = 9f;
    [SerializeField] private float teleportDistance = 10f;
    [SerializeField] private float teleportFrequency = 3f;

    IEnumerator StartTeleporting()
    {
        while (true)
        {
            yield return new WaitForSeconds(teleportFrequency);

            if (transform.position.x >= maxXPosition)
            {
                continue;
            }
            Vector3 randomPos = StaticFunctions.Instance.GetRandomPointInCircle(teleportDistance, transform.position);

            if (Mathf.Abs(randomPos.y) < teleportDistance &&
                randomPos.x >= transform.position.x)
            {
                LeanTween.move(gameObject, randomPos, 0.1f).setEase(LeanTweenType.easeOutElastic);
            }

        }
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartTeleporting());
    }
}
