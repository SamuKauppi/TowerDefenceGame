using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_sprite;
    private float Damage { get; set; }
    private float LifeTime { get; set; }
    private float aliveTImer;
    private StatusElementClass[] explosionStatus;
    public void DetermineExplosion(Vector3 pos, float damage, float radius, float lifeTime, Sprite s,
        StatusElementClass[] statuses)
    {
        explosionStatus = statuses;
        LifeTime = lifeTime;
        Damage = damage;
        m_sprite.sprite = s;
        transform.position = pos;
        transform.localScale = new Vector3(radius, radius, radius);
    }

    private void Update()
    {
        aliveTImer += Time.deltaTime;
        if (aliveTImer > LifeTime)
        {
            KillExplosion();
        }
    }

    private void KillExplosion()
    {
        aliveTImer = 0;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            Enemy e = collision.GetComponent<Enemy>();
            for (int i = 0; i < explosionStatus.Length; i++)
            {
                e.CreateStatus(explosionStatus[i]);
            }
            e.TakeDamage(Damage);
        }
    }
}
