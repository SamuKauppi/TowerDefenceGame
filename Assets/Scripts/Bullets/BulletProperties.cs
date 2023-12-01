using UnityEngine;

public abstract class BulletProperties : MonoBehaviour
{
    // References
    [SerializeField] private TransitionOverTimeEffects scaler;
    public virtual void OnBulletSpawn()
    {
        if (scaler)
            scaler.StartTransition();
    }
    public virtual void OnBulletDespawn(){}
    public virtual void OnBulletUpdate(){}
    public virtual void OnBulletFixedUpdate(){}
}
