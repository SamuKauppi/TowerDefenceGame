using UnityEngine;
using System.Threading.Tasks;

public static class StaticFunctions
{
    /// <summary>
    /// Change the color of sprite renderer to target color
    /// </summary>
    /// <param name="sr"></param>
    /// <param name="targetColor"></param>
    /// <param name="duration"></param>
    /// <param name="smoothness"></param>
    /// <returns></returns>
    private static async Task TransitionColor(SpriteRenderer sr, Color targetColor, float duration, float smoothness)
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
                await Task.Delay(Mathf.FloorToInt(timeBetweenSteps * 1000)); // Convert seconds to milliseconds
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
    public static Vector2 GetRandomPointInCircle(float radius, Vector2 center)
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
    public static async void StartTransition(SpriteRenderer sr, Color targetColor, float duration, float smoothness = 100)
    {
        await TransitionColor(sr, targetColor, duration, smoothness);
    }
}
