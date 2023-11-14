using System.Collections;
using UnityEngine;

public class TransitionOverTimeEffects : MonoBehaviour
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
        targetSpriteRenderer.enabled = false;
    }

    IEnumerator TransitionThrough()
    {
        foreach (TransitionState effect in transitions)
        {
            TransitionToNextState(effect);
            yield return new WaitForSeconds(effect.Time);
        }
    }

    private void TransitionToNextState(TransitionState effect)
    {
        Scale(effect);

        Rotate(effect);

        StartCoroutine(ChangeColor(effect));
    }

    private void Scale(TransitionState effect)
    {
        if (targetGameObject.transform.localScale == effect.Scale)
            return;

        if (LeanTween.isTweening(targetGameObject))
            LeanTween.cancel(targetGameObject);

        LeanTween.scale(targetGameObject, effect.Scale, effect.Time).setEase(effect.easeType);
    }
    private void Rotate(TransitionState effect)
    {
        if (targetSpriteRenderer.transform.localRotation.z == effect.Rotation)
            return;

        if (LeanTween.isTweening(targetSpriteRenderer.gameObject))
            LeanTween.cancel(targetSpriteRenderer.gameObject);

        LeanTween.rotateAround(targetSpriteRenderer.gameObject, 
            targetSpriteRenderer.transform.forward,
            effect.Rotation, 
            effect.Time).setEase(effect.easeType);
    }
    private IEnumerator ChangeColor(TransitionState effect)
    {
        if (effect.Time == 0f || effect.Color == targetSpriteRenderer.color)
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

    public void StartTransition()
    {
        LeanTween.scale(targetGameObject, OriginalScale, 0f);
        targetSpriteRenderer.color = OriginalColor;
        targetSpriteRenderer.enabled = true;
        StartCoroutine(TransitionThrough());
    }
}
