using System.Collections;
using UnityEngine;

public class TransitionOverTimeEffects: MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private TransitionState[] transitions;
    [SerializeField] private float stepsBetween;
    private float stepsBetweenMultiplier;

    Vector2 OriginalScale { get; set; }
    Color OriginalColor { get; set; }
    private void Start()
    {
        OriginalScale = targetGameObject.transform.localScale;
        OriginalColor = targetSpriteRenderer.color;
        stepsBetweenMultiplier = 1 / stepsBetween;
    }

    public void StartTransition()
    {
        LeanTween.scale(targetGameObject, OriginalScale, 0f);
        targetSpriteRenderer.color = OriginalColor;
        StartCoroutine(TransitionThrough());
    }

    IEnumerator TransitionThrough()
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            StartCoroutine(TransitionToNextState(transitions[i]));
            yield return new WaitForSecondsRealtime(transitions[i].Time);
        }
    }

    IEnumerator TransitionToNextState(TransitionState effect)
    {

        if (LeanTween.isTweening(targetGameObject))
            LeanTween.cancel(targetGameObject);

        LeanTween.scale(targetGameObject, effect.Scale, effect.Time);

        if (effect.Time == 0f)
        {
            targetSpriteRenderer.color = effect.Color;
        }
        else
        {
            float timeBetweenSteps = effect.Time * stepsBetweenMultiplier;
            Color transitionColor = (effect.Color - targetSpriteRenderer.color) * stepsBetweenMultiplier;
            int counter = 0;
            while (counter < stepsBetween)
            {
                targetSpriteRenderer.color += transitionColor;
                counter++;
                yield return new WaitForSecondsRealtime(timeBetweenSteps);
            }
        }
    }
}
