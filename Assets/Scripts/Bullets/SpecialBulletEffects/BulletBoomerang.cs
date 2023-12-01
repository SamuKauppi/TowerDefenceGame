using UnityEngine;

public class BulletBoomerang : BulletProperties
{
    [SerializeField] private GameObject rotatingSprite;

    public override void OnBulletSpawn()
    {
        if (LeanTween.isTweening(gameObject))
        {
            LeanTween.cancel(gameObject);
        }

        if (LeanTween.isTweening(rotatingSprite))
        {
            LeanTween.cancel(rotatingSprite);
        }

        LeanTween.rotateAround(gameObject, Vector3.forward, 220f, 1f).setEase(LeanTweenType.easeInOutQuart);
        LeanTween.rotateZ(rotatingSprite, -3600, 1.1f).setEase(LeanTweenType.easeOutSine);
    }

}
