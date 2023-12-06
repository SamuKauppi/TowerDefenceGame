using System.Collections;
using UnityEngine;

public class StartVisuals : MonoBehaviour
{
    // Arrow visuals
    [SerializeField] private Transform endPoint;            // Endpoint
    [SerializeField] private float startDelay;              // Delay before starting visuals
    [SerializeField] private int count;                     // How many arrows will be spawned
    [SerializeField] private int rowCount;                  // How many rows are spawned
    [SerializeField] private float delay;                   // How long delay is between arrows
    [SerializeField] private float spacing;                 // The distance between arrows in a row
    [SerializeField] private float animationSpeed;          // How fast do the arrows move
    private const GameEntity arrowIdent = GameEntity.Arrow; // Arrow ident for objectpooler

    private void Start()
    {
        Vector3 thisPos = transform.position;
        transform.position = new Vector3(thisPos.x - 10f, thisPos.y, thisPos.z);
        LeanTween.move(gameObject, thisPos, startDelay).setEase(LeanTweenType.easeOutBack);
        StartCoroutine(SpawnArrows());
    }

    /// <summary>
    /// Spawn arrow visuals
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnArrows()
    {
        yield return new WaitForSeconds(startDelay);

        // Get position with the position.x and endpos.y
        Vector3 startPos = new(transform.position.x, endPoint.position.y);


        for (int i = 0; i < count; i++)
        {
            Vector3 arrowPos = transform.position;
            for (int j = 0; j < rowCount; j++)
            {
                GameObject g = ObjectPooler.Instance.GetPooledObject(arrowIdent);
                g.transform.position = arrowPos;
                g.transform.right = startPos - arrowPos;

                LeanTween.moveX(g, endPoint.position.x, animationSpeed).setEase(LeanTweenType.easeInSine);
                LeanTween.moveY(g, endPoint.position.y, animationSpeed * 0.5f).setEase(LeanTweenType.easeInSine);
                LeanTween.rotate(g, endPoint.position - arrowPos, animationSpeed).setEase(LeanTweenType.easeOutCubic);

                if (arrowPos.y <= 0)
                {
                    arrowPos.y = Mathf.Abs(arrowPos.y) + spacing;
                }
                else
                {
                    arrowPos.y *= -1f;
                }
            }

            yield return new WaitForSeconds(delay);
        }
    }
}
