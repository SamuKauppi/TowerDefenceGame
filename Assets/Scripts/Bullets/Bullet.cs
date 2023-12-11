using UnityEngine;

public class Bullet : BulletDamages, IUpdate, IFixedUpdate
{
    // References 
    [SerializeField] private BulletProperties properties;       // Properties
    [SerializeField] private Rigidbody2D _rb;                   // Rigidbody component

    // Public
    public GameObject Object => gameObject; // From interface

    // Stats
    [SerializeField] private SFXType audioType = SFXType.None;  // If this bullet has audio       
    [SerializeField] private bool isBeam;                       // Is the bullet a beam (overrides all speed related variables)
    [SerializeField] private float speed = 100;                 // Speed of the bullet
    [SerializeField] private float acceleration;                // Acceleration of the bullet
    [SerializeField] private bool containSpeed;                 // Will the speed be contained between 0 and staringspeed
    [SerializeField] private float lifeTime;                    // How long will the bullet be alive
    [SerializeField] private int health = 1;                    // How many hits can the bullet take

    // Variables
    private float _startingSpeed;                               // Starting speed
    private float _initialAccel;                                // Initial acceleration
    private float _initialDamage;                               // Initial damage (damage reduses after every hit)
    private int _maxHealth;                                     // Max health
    private float _lifeTimer = 0;                               // Timer to live                    
    private bool _hasInitialized;                               // Checks if the bullet has been Initialized

    /// <summary>
    /// Initilize bullets base stats 
    /// </summary>
    private void Start()
    {
        _maxHealth = health;
        _startingSpeed = speed;
        _initialAccel = acceleration;
        GameObjectUpdateManager.Instance.AddObject(this);
        _initialDamage = Damage;
    }

    /// <summary>
    /// Updates the lifetime of a bullet
    /// 0 == _animationLength = bullet does not die by lifeTime
    /// </summary>
    private void UpdateLifeTime()
    {
        if (lifeTime <= 0)
        {
            return;
        }
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer > lifeTime)
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
            if (speed > _startingSpeed)
            {
                speed = _startingSpeed;
                acceleration = 0;
            }
        }
    }

    /// <summary>
    /// Reduce bullet hp. If max hp <= 0, the bullet can't die from hits
    /// </summary>
    private void ReduceBulletHits()
    {
        health--;
        if (health <= 0 && _maxHealth > 0)
        {
            KillBullet();
        }
    }

    /// <summary>
    /// Kill bullet and reset it's variables
    /// </summary>
    private void KillBullet()
    {
        _lifeTimer = 0f;
        health = _maxHealth;
        speed = _startingSpeed;
        acceleration = _initialAccel;
        SetDamage(_initialDamage);
        properties.OnBulletDespawn();
        gameObject.SetActive(false);
        _hasInitialized = false;
    }

    /// <summary>
    /// Initialize bullet
    /// </summary>
    public void InitializeBullet()
    {
        properties.OnBulletSpawn();
        _hasInitialized = true;
        if (audioType != SFXType.None)
        {
            AudioManager.Instance.PlaySFX(audioType);
        }
    }

    /// <summary>
    /// Update bullet (From interface)
    /// </summary>
    public void UpdateObject()
    {
        if (!_hasInitialized)
        {
            InitializeBullet();
        }
        properties.OnBulletUpdate();

        UpdateLifeTime();

        ContainSpeed();
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
