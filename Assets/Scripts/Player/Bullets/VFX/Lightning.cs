using System.Collections;
using UnityEngine;

public class Lightning : BulletDamages
{
    /// <summary>
    /// Start lightningbolt
    /// </summary>
    /// <param name="position"></param>
    /// <param name="lifeTime"></param>
    /// <param name="damage"></param>
    public void StartLightning(Vector3 position, float lifeTime, float damage)
    {
        transform.position = position;
        SetDamage(damage);
        StartCoroutine(KillLightning(lifeTime));
    }
    
    /// <summary>
    /// Kill lightningbolt after time
    /// </summary>
    /// <param name="lifeTime"></param>
    /// <returns></returns>
    private IEnumerator KillLightning(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Return damage
    /// </summary>
    /// <returns></returns>
    public override float GetBulletDamage()
    {
        float currentDamage = Damage;
        SetDamage(Damage * 0.95f);
        return currentDamage;
    }
}
