using UnityEngine;

public class BulletExplosion : BulletProperties
{
    // References to assets
    [SerializeField] private StatusElementClass[] explosionStatuses;

    // Stats of the explosion
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private float lifeTime;
    [SerializeField] private GameEntity explosionIdent = GameEntity.BombExplosion;

    public override void OnBulletDespawn()
    {
        Explosion exp = ObjectPooler.Instance.GetPooledObject(explosionIdent).GetComponent<Explosion>();
        exp.DetermineExplosion(transform.position, damage, lifeTime, radius, explosionStatuses);
    }
}
