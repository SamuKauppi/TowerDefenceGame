using UnityEngine;

public class BulletSword : BulletProperties
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BoxCollider2D bx;
    [SerializeField] private float angleToRotate;
    public override void OnBulletSpawn()
    {
        sr.enabled = true;
        bx.enabled = true;
        RotateSword();
    }
    public override void OnBulletDespawn()
    {
        sr.enabled = false;
        bx.enabled = false;
    }
    private void RotateSword()
    {
        float random = Random.value < 0.5f ? -1 : 1; 
        LeanTween.rotateAroundLocal(gameObject, transform.forward, angleToRotate * random, 0f);
        LeanTween.rotateAroundLocal(gameObject, transform.forward, angleToRotate * random * -2.2f, 0.45f).setEase(LeanTweenType.easeInBack);
    }
}
