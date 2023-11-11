using UnityEngine;

public class Bullet : MonoBehaviour, IUpdate, IFixedUpdate
{
    // References 
    [SerializeField] private BulletProperties properties;       // Properties
    [SerializeField] private Rigidbody2D _rb;                   // Rigidbody component

    // Speeds
    [SerializeField] private float speed = 100;                 // Speed of the bullet
    [SerializeField] private float acceleration;                // Acceleration of the bullet
    [SerializeField] private bool containSpeed;                 // Will the speed be contained between 0 and staringspeed
    private float startingSpeed;                                // Starting speed
    private float initialAccel;                                 // Initial acceleration

    // Damage
    [SerializeField] private float damage;                      // Damage of the bullet
    private float initialDamage;
        
    // Bullet health
    [SerializeField] private float lifeTime;                    // How long will the bullet be alive
    [SerializeField] private int health = 1;                    // How many hits can the bullet take
    private int maxHealth;                                      // Max health
    private float lifeTimer = 0;                                // Timer to live                    
    private bool isInitialized = false;                         // Variable traking if this bullet has been initilized
                                                                // Used everytime a bullet is SetActive

    // From interface
    public GameObject Object => gameObject;

    /// <summary>
    /// Initilize bullets base stats for 
    /// </summary>
    private void Start()
    {
        maxHealth = health;
        startingSpeed = speed;
        initialAccel = acceleration;
        GameObjectUpdateManager.Instance.AddObject(this);
        initialDamage = damage;
    }
    private void InitializeBullet()
    {
        properties.OnBulletSpawn();
        isInitialized = true;
    }
    public void UpdateObject()
    {
        if (!isInitialized)
            InitializeBullet();

        properties.OnBulletUpdate();

        if (lifeTime > 0)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer > lifeTime)
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
    public void FixedUpdateGameobject()
    {
        properties.OnBulletFixedUpdate();
        speed += (acceleration * Time.fixedDeltaTime);
        _rb.MovePosition(speed * Time.fixedDeltaTime * transform.up.normalized + transform.position);
    }

    private void ReduceBulletHits()
    {
        health--;
        if (health <= 0 && maxHealth > 0)
        {
            KillBullet();
        }
    }
    private void KillBullet()
    {
        lifeTimer = 0f;
        health = maxHealth;
        speed = startingSpeed;
        acceleration = initialAccel;
        damage = initialDamage;
        properties.OnBulletDespawn();
        gameObject.SetActive(false);
        isInitialized = false;
    }

    public StatusElementClass[] GetBulletStatusElements()
    {
        return properties.StatusElementOnHit;
    }
    public float GetAndReduceBulletDamage()
    {
        ReduceBulletHits();
        float currentDamage = damage;
        damage *= 0.95f;
        return currentDamage;
    }
}
