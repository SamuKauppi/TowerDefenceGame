using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private BulletProperties properties;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private float speed = 100;
    private float startingSpeed;
    [SerializeField] private float acceleration;
    private float initialAcc;
    [SerializeField] private bool containSpeed;
    [SerializeField] private float damage;
    private float initialDamage;
    [SerializeField] private float lifeTime;
    [SerializeField] private int health = 1;
    private int maxHealth;
    private float timeToDie = 0;
    private bool hasSpawned = true;

    private void Start()
    {
        maxHealth = health;
        startingSpeed = speed;
        initialAcc = acceleration;
        initialDamage = damage;
    }
    void FixedUpdate()
    {
        properties.OnBulletFixedUpdate();
        speed += (acceleration * Time.fixedDeltaTime);
        _rb.MovePosition(speed * Time.fixedDeltaTime * transform.up.normalized + transform.position);
    }
    private void Update()
    {
        if (hasSpawned)
        {
            properties.OnBulletSpawn();
            hasSpawned = false;
        }
        properties.OnBulletUpdate();

        if (lifeTime > 0)
        {
            timeToDie += Time.deltaTime;
            if (timeToDie > lifeTime)
            {
                KillBullet();
            }
        }

        if (containSpeed)
        {
            if (speed < 0)
            {
                speed = 0;
                acceleration = 0;
            }
            if (speed > startingSpeed)
            {
                speed = startingSpeed;
                acceleration = 0;
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            Enemy e = collision.GetComponent<Enemy>();
            for (int i = 0; i < properties.StatusElementOnHit.Length; i++)
            {
                e.CreateStatus(properties.StatusElementOnHit[i]);
            }
            e.TakeDamage(damage);
            health -= 1;
            damage *= 0.95f;
            if (health <= 0 && maxHealth > 0)
            {
                KillBullet();
            }
        }
    }

    private void KillBullet()
    {
        timeToDie = 0f;
        health = maxHealth;
        speed = startingSpeed;
        acceleration = initialAcc;
        damage = initialDamage;
        properties.OnBulletDespawn();
        gameObject.SetActive(false);
        hasSpawned = true;
    }

    public void ChangeSpeed(float amout)
    {
        speed += amout;
    }
}
