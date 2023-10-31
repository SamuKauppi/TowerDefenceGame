using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFlame : BulletProperties
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color[] colors;

    public override void OnBulletSpawn()
    {
        base.OnBulletSpawn();
        StartCoroutine(CycleColors());
    }

    private IEnumerator CycleColors()
    {
        float delay = 0.65f / colors.Length;
        for (int i = 0; i < colors.Length - 1; i++)
        {
            StartCoroutine(SwapColor(colors[i], colors[i + 1], delay));
            yield return new WaitForSecondsRealtime(delay);
        }
    }

    private IEnumerator SwapColor(Color original, Color target, float time)
    {
        float timeDelay = time * 0.1f;
        while (original != target) 
        {
            original = Color.Lerp(original, target, 0.1f);
            sr.color = original;
            yield return new WaitForSecondsRealtime(timeDelay);
        }
    }
}
