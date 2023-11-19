using System.Collections;
using UnityEngine;

public class Lightning : BulletDamages
{
    public void StartLightning(Vector3 position, float lifeTime, float damage)
    {
        transform.position = position;
        SetDamage(damage);
        StartCoroutine(KillLightning(lifeTime));
    }

    private IEnumerator KillLightning(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }

    public override float GetBulletDamage()
    {
        float currentDamage = Damage;
        SetDamage(Damage * 0.95f);
        return currentDamage;
    }
}
