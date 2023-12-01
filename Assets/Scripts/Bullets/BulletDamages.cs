using UnityEngine;

public abstract class BulletDamages : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float damageOverTime;
    [SerializeField] private StatusElementClass[] statusesOnHit;
    public float Damage { get { return damage; } private set { damage = value; } }
    public float DamageOverTime { get { return damageOverTime; } private set { damageOverTime = value; } }
    public StatusElementClass[] StatusElements { get { return statusesOnHit; } private set { statusesOnHit = value; } }

    public void SetDamage(float newDamage)
    {
        Damage = newDamage;
    }

    public void SetDamageOverTime(float newDamageOverTime)
    {
        DamageOverTime = newDamageOverTime;
    }

    public void SetStatusElements(StatusElementClass[] newStatusElements)
    {
        StatusElements = newStatusElements;
    }

    /// <summary>
    /// Returns bullet damage. Works differently with some bullets
    /// </summary>
    /// <returns></returns>
    public virtual float GetBulletDamage()
    {
        return 0;
    }
}
