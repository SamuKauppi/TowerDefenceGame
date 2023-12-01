using UnityEngine;

public class Lightning : BulletDamages
{
    [SerializeField] private KillEffectTimer _killTimer;
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
        _killTimer.StartKillCountdown(lifeTime);
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
