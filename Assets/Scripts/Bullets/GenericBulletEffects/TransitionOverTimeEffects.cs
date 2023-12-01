using System.Collections;
using UnityEngine;

public class TransitionOverTimeEffects : MonoBehaviour
{
    // References
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private GameObject targetGameObject;

    // States
    [SerializeField] private TransitionState[] transitions;     // Contains states for rotation, scale, color and spawnTimer between
    [SerializeField] private float colorChangeSmooth;           // How smooth color change will be
    [SerializeField] private bool resetRotation;                // Will rotation be reset on each spawn
    private float stepsBetweenMultiplier;                       // Multiplier for faster calculations

    // Original dimensions
    private Vector2 OriginalScale { get; set; }
    private Vector2 OriginalRotation { get; set; }
    private Color OriginalColor { get; set; }

    /// <summary>
    /// Initilize changes
    /// </summary>
    private void Start()
    {
        OriginalScale = transitions[0].Scale;
        OriginalColor = transitions[0].Color;
        OriginalRotation = targetSpriteRenderer.transform.rotation.eulerAngles;
        stepsBetweenMultiplier = 1 / colorChangeSmooth;
        targetSpriteRenderer.enabled = false;
    }
    /// <summary>
    /// Transition through every state
    /// </summary>
    /// <returns></returns>
    IEnumerator TransitionThrough()
    {
        foreach (TransitionState effect in transitions)
        {
            TransitionToNextState(effect);
            yield return new WaitForSeconds(effect.Time);
        }
    }
    /// <summary>
    /// Effects to be done
    /// </summary>
    /// <param name="effect"></param>
    private void TransitionToNextState(TransitionState effect)
    {
        Scale(effect);

        Rotate(effect);

        StartCoroutine(ChangeColor(effect));
    }

    /// <summary>
    /// Scale targetGameObject to effect size
    /// </summary>
    /// <param name="effect"></param>
    private void Scale(TransitionState effect)
    {
        if (targetGameObject.transform.localScale == effect.Scale)
            return;

        if (LeanTween.isTweening(targetGameObject))
            LeanTween.cancel(targetGameObject);

        LeanTween.scale(targetGameObject, effect.Scale, effect.Time).setEase(effect.easeType);
    }
    /// <summary>
    /// Add rotation to targetSpriteRenderer with effect angle
    /// </summary>
    /// <param name="effect"></param>
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
    /// <summary>
    /// Change color using the smoothness value
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
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
            while (counter < colorChangeSmooth)
            {
                targetSpriteRenderer.color += transitionColor;
                counter++;
                yield return new WaitForSecondsRealtime(timeBetweenSteps);
            }
        }
    }

    /// <summary>
    /// Start transition
    /// </summary>
    public void StartTransition()
    {
        if (resetRotation)
            LeanTween.rotate(targetSpriteRenderer.gameObject, OriginalRotation, 0f);

        LeanTween.scale(targetGameObject, OriginalScale, 0f);
        targetSpriteRenderer.color = OriginalColor;

        targetSpriteRenderer.enabled = true;
        StartCoroutine(TransitionThrough());
    }
}