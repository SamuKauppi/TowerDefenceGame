using UnityEngine;

public class Explosion : BulletDamages
{
    [SerializeField] private Animator _animator;
    [SerializeField] private KillEffectTimer _killTimer;
    /// <summary>
    /// Deterimine explosions stats on spawn
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="damage"></param>
    /// <param name="radius"></param>
    /// <param name="lifeTime"></param>
    /// <param name="anim"></param>
    /// <param name="statuses"></param>
    public void DetermineExplosion(Vector3 pos,
                                    float damage,
                                    float lifeTime,
                                    float radius,
                                    StatusElementClass[] statuses)
    {
        // Set Damage and statuses
        SetDamage(damage);
        SetStatusElements(statuses);

        // Set Animation
        _animator.SetFloat("ExplosionSpeed", 1f / lifeTime);

        // Set postion
        transform.position = pos;

        // Adjust radius if applicable
        if (radius > 0)
            transform.localScale = new Vector3(radius, radius, radius);

        // Start kill _waveTimer
        _killTimer.StartKillCountdown(lifeTime);
    }

    /// <summary>
    /// Inherited. Returns damage
    /// </summary>
    /// <returns></returns>
    public override float GetBulletDamage()
    {
        return Damage;
    }
}
