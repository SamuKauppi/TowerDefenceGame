using System.Collections;
using UnityEngine;

public class BulletFlame : BulletProperties
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color[] colors;
    [SerializeField] private Vector3[] scalesOverTime;
    private float delay;

    private void Start()
    {
        delay = 0.65f / colors.Length;
    }

    public override void OnBulletSpawn()
    {
        base.OnBulletSpawn();
        StartCoroutine(CycleColors());
        StartCoroutine(ScaleOverTime());
    }

    private IEnumerator CycleColors()
    {
        for (int i = 0; i < colors.Length - 1; i++)
        {
            StartCoroutine(SwapColor(colors[i], colors[i + 1], delay));
            yield return new WaitForSecondsRealtime(delay * 1.01f);
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

    private IEnumerator ScaleOverTime()
    {
        float delayBetween = delay / scalesOverTime.Length;
        for (int i = 0; i < scalesOverTime.Length; i++)
        {
            if (LeanTween.isTweening())
                LeanTween.cancel(gameObject);

            LeanTween.scale(gameObject, scalesOverTime[i], delayBetween);
            yield return new WaitForSecondsRealtime(delayBetween);
        }
    }

    public override void OnBulletDespawn()
    {
        LeanTween.scale(gameObject, scalesOverTime[0], 0f);
    }
}
