using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBeam : BulletProperties
{
    [SerializeField] private SpriteRenderer _sp;
    Color OriginalColor { get; set; }
    bool fstTime = true;

    public override void OnBulletSpawn()
    {
        base.OnBulletSpawn();
        if(fstTime)
        {
            OriginalColor = _sp.color;
            fstTime = false;
        }
        StartCoroutine(FadeToAlpha());
    }
    public override void OnBulletDespawn()
    {
        _sp.color = OriginalColor;
    }
    IEnumerator FadeToAlpha()
    {
        Color alphacolor = OriginalColor;
        while (alphacolor.a > 0.05f)
        {
            yield return new WaitForSeconds(0.01f);
            alphacolor.a *= 0.95f;
            _sp.color = alphacolor;
        }
    }
}
