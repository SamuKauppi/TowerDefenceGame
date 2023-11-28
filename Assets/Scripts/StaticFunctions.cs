using UnityEngine;

public static class StaticFunctions
{
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
}

