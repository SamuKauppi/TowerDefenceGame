using System.Collections;
using UnityEngine;

public class StaticFunctions : MonoBehaviour
{
    // Singleton
    public static StaticFunctions Instance {  get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Change color of the sprite
    /// </summary>
    /// <param name="sr"></param>
    /// <param name="targetColor"></param>
    /// <param name="duration"></param>
    /// <param name="smoothness"></param>
    /// <returns></returns>
    private IEnumerator TransitionColor(SpriteRenderer sr, Color targetColor, float duration, float smoothness)
    {
        if (sr.color != targetColor)
        {
            float timeBetweenSteps = duration / smoothness;
            Color transitionColor = (targetColor - sr.color) / smoothness;
            int counter = 0;
            while (counter < smoothness && sr != null)
            {
                sr.color += transitionColor;
                counter++;
                yield return new WaitForSeconds(timeBetweenSteps);
            }
        }

        if (sr != null)
            sr.color = targetColor;
    }

    /// <summary>
    /// Get random position in a circle
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="center"></param>
    /// <returns></returns>
    public Vector2 GetRandomPointInCircle(float radius, Vector2 center)
    {
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);

        return center + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * Random.Range(0f, radius);
    }

    /// <summary>
    /// Start changing color
    /// </summary>
    /// <param name="sr"></param>
    /// <param name="targetColor"></param>
    /// <param name="duration"></param>
    /// <param name="smoothness"></param>
    public void StartTransition(SpriteRenderer sr, Color targetColor, float duration, float smoothness = 100)
    {
        StartCoroutine(TransitionColor(sr, targetColor, duration, smoothness));
    }
}
