using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletProperties : MonoBehaviour
{
    public StatusElementClass[] StatusElementOnHit;
    public Vector3 StartingSize { get; set; }
    private void Start()
    {
       StartingSize = transform.localScale;
    }
    public virtual void OnBulletSpawn()
    { 
        transform.localScale = StartingSize * 0.65f;
        StartCoroutine(ScaleToNormal());
    }
    public virtual void OnBulletDespawn()
    { return; }
    public virtual void OnBulletUpdate() 
    { return; }
    public virtual void OnBulletFixedUpdate() 
    { return; }

    IEnumerator ScaleToNormal()
    {
        while (transform.localScale.magnitude < StartingSize.magnitude) 
        {
            transform.localScale *= 1.1f;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
