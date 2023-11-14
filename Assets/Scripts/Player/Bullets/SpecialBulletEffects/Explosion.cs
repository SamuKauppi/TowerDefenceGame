using System.Collections;
using UnityEngine;

public class Explosion : BulletDamages
{
    [SerializeField] private SpriteRenderer m_sprite;
    private float LifeTime { get; set; }
    /// <summary>
    /// Deterimine explosions stats on spawn
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="damage"></param>
    /// <param name="radius"></param>
    /// <param name="lifeTime"></param>
    /// <param name="s"></param>
    /// <param name="statuses"></param>
    public void DetermineExplosion(Vector3 pos, float damage, float radius, float lifeTime, Sprite s,
        StatusElementClass[] statuses)
    {
        SetStatusElements(statuses);
        SetDamage(damage);
        LifeTime = lifeTime;
        m_sprite.sprite = s;
        transform.position = pos;
        transform.localScale = new Vector3(radius, radius, radius);
        StartCoroutine(KillExplosion());
    }

    IEnumerator KillExplosion()
    {
        yield return new WaitForSecondsRealtime(LifeTime);
        gameObject.SetActive(false);
    }

    public override float GetBulletDamage()
    {
        return Damage;
    }
}
