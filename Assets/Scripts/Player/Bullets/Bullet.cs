using UnityEngine;

public class Bullet : BulletDamages, IUpdate, IFixedUpdate
{
    // References 
    [SerializeField] private BulletProperties properties;       // Properties
    [SerializeField] private Rigidbody2D _rb;                   // Rigidbody component

    // Public
    public GameObject Object => gameObject; // From interface

    // Stats
    [SerializeField] private bool isBeam;                       // Is the bullet a beam (overrides all speed related variables)
    [SerializeField] private float speed = 100;                 // Speed of the bullet
    [SerializeField] private float acceleration;                // Acceleration of the bullet
    [SerializeField] private bool containSpeed;                 // Will the speed be contained between 0 and staringspeed
    [SerializeField] private float lifeTime;                    // How long will the bullet be alive
    [SerializeField] private int health = 1;                    // How many hits can the bullet take

    // Variables
    private float startingSpeed;                                // Starting speed
    private float initialAccel;                                 // Initial acceleration
    private float initialDamage;                                // Initial damage (damage reduses after every hit)
    private int maxHealth;                                      // Max health
    private float lifeTimer = 0;                                // Timer to live                    
    private bool HasInitialized;
    /// <summary>
    /// Initilize bullets base stats 
    /// </summary>
    private void Start()
    {
        maxHealth = health;
        startingSpeed = speed;
        initialAccel = acceleration;
        GameObjectUpdateManager.Instance.AddObject(this);
        initialDamage = Damage;
    }
    /// <summary>
    /// Update bullet (From interface)
    /// </summary>
    public void UpdateObject()
    {
        if (!HasInitialized)
        {
            InitializeBullet();
        }
        properties.OnBulletUpdate();

        UpdateLifeTime();

        ContainSpeed();
    }

    /// <summary>
    /// Updates the lifetime of a bullet
    /// 0 == lifeTime = bullet does not die by time
    /// </summary>
    private void UpdateLifeTime()
    {
        if (lifeTime <= 0)
        {
            return;
        }
        lifeTimer += Time.deltaTime;
        if (lifeTimer > lifeTime)
        {
            KillBullet();
        }
    }
    /// <summary>
    /// Contain speed if true
    /// </summary>
    private void ContainSpeed()
    {
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
    /// <summary>
    /// Reduce bullet hp. If max hp <= 0, the bullet can't die from time
    /// </summary>
    private void ReduceBulletHits()
    {
        health--;
        if (health <= 0 && maxHealth > 0)
        {
            KillBullet();
        }
    }
    /// <summary>
    /// Kill bullet and reset it's variables
    /// </summary>
    private void KillBullet()
    {
        lifeTimer = 0f;
        health = maxHealth;
        speed = startingSpeed;
        acceleration = initialAccel;
        SetDamage(initialDamage);
        properties.OnBulletDespawn();
        gameObject.SetActive(false);
        HasInitialized = false;
    }
    /// <summary>
    /// Fixed update (From interface)
    /// </summary>
    public void FixedUpdateGameobject()
    {
        if (!_rb)
            return;
        speed += (acceleration * Time.fixedDeltaTime);
        _rb.MovePosition(speed * Time.fixedDeltaTime * transform.up.normalized + transform.position);
        properties.OnBulletFixedUpdate();
    }

    /// <summary>
    /// Initialize bullet
    /// </summary>
    public void InitializeBullet()
    {
        properties.OnBulletSpawn();
        HasInitialized = true;
    }

    /// <summary>
    /// Override function
    /// Normal bullets damage gets reduced after every hit
    /// </summary>
    /// <returns></returns>
    public override float GetBulletDamage()
    {
        ReduceBulletHits();
        float currentDamage = Damage;
        SetDamage(Damage * 0.95f);
        return currentDamage;
    }
}
