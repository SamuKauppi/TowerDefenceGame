using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBoomerang : BulletProperties
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject rotatingSprite;
    public override void OnBulletSpawn()
    {
        base.OnBulletSpawn();
        BoomerangMovement();
    }
    void BoomerangMovement()
    {
        LeanTween.rotateAround(gameObject, Vector3.forward, 210f, 1.125f).setEase(LeanTweenType.easeInOutQuart);
        LeanTween.rotateZ(rotatingSprite, -4600, 3f).setEase(LeanTweenType.easeOutQuad);
    }

    public override void OnBulletDespawn()
    {
        LeanTween.cancel(rotatingSprite);
        LeanTween.cancel(gameObject);
    }
}
