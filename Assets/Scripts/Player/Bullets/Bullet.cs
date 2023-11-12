using UnityEngine;

public class Bullet : MonoBehaviour, IUpdate, IFixedUpdate
{
    // References 
    [SerializeField] private BulletProperties properties;       // Properties
    [SerializeField] private Rigidbody2D _rb;                   // Rigidbody component

    // Public
    public StatusElementClass[] StatusElements { get { return statusElementsOnHit; } }
    public GameObject Object => gameObject; // From interface

    // Stats
    [SerializeField] private StatusElementClass[] statusElementsOnHit;  // Status elements
    [SerializeField] private bool isBeam;                       // Is the bullet a beam (overrides all speed related variables)
    [SerializeField] private float speed = 100;                 // Speed of the bullet
    [SerializeField] private float acceleration;                // Acceleration of the bullet
    [SerializeField] private bool containSpeed;                 // Will the speed be contained between 0 and staringspeed
    [SerializeField] private float damage;                      // Damage of the bullet
    [SerializeField] private float lifeTime;                    // How long will the bullet be alive
    [SerializeField] private int health = 1;                    // How many hits can the bullet take

    // Variables
    private float startingSpeed;                                // Starting speed
    private float initialAccel;                                 // Initial acceleration
    private float initialDamage;                                // Initial damage (damage reduses after every hit)
    private int maxHealth;                                      // Max health
    private float lifeTimer = 0;                                // Timer to live                    

    /// <summary>
    /// Initilize bullets base stats 
    /// </summary>
    private void Start()
    {
        maxHealth = health;
        startingSpeed = speed;
        initialAccel = acceleration;
        GameObjectUpdateManager.Instance.AddObject(this);
        initialDamage = damage;
    }
    /// <summary>
    /// Update bullet (From interface)
    /// </summary>
    public void UpdateObject()
    {
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
    /// Fixed update (From interface)
    /// </summary>
    public void FixedUpdateGameobject()
    {
        properties.OnBulletFixedUpdate();
        speed += (acceleration * Time.fixedDeltaTime);
        _rb.MovePosition(speed * Time.fixedDeltaTime * transform.up.normalized + transform.position);
    }
    /// <summary>
    /// Bullet hit enemy, reduce health
    /// If maxhealth <= 0, bullet does not die from hits
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
    /// Initialize bullet
    /// </summary>
    public BulletProperties InitializeAndGetBulletProperties()
    {
        properties.OnBulletSpawn();
        return properties;
    }
    /// <summary>
    /// Set bullet inactive and reset variables
    /// </summary>
    private void KillBullet()
    {
        lifeTimer = 0f;
        health = maxHealth;
        speed = startingSpeed;
        acceleration = initialAccel;
        damage = initialDamage;
        properties.OnBulletDespawn();
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Returns bullet damage before reducing it
    /// </summary>
    /// <returns></returns>
    public float GetAndReduceBulletDamage()
    {
        ReduceBulletHits();
        float currentDamage = damage;
        damage *= 0.95f;
        return currentDamage;
    }
}
