using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletExplosion : BulletProperties
{
    [SerializeField] private StatusElementClass[] explosionStatuses;
    [SerializeField] private Sprite ExplosionSprite;
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private float lifeTime;
    private const GameEntity explosionIdent = GameEntity.Explosion;

    public override void OnBulletDespawn()
    {
        Explosion exp = ObjectPooler.Instance.GetPooledObject(explosionIdent).GetComponent<Explosion>();
        exp.DetermineExplosion(transform.position, damage, radius, lifeTime, ExplosionSprite,
            explosionStatuses);
    }
}
