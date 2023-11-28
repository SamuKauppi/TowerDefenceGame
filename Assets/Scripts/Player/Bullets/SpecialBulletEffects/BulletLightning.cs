using System.Collections.Generic;
using UnityEngine;

public class BulletLightning : BulletProperties
{
    [SerializeField]
    private GameEntity _lightningIdent
    = GameEntity.LightningBolt;

    // Lightning stats
    [SerializeField] private float lightningLifetime;       // How long will lightning be visible
    [SerializeField] private float damage;                  // Damage of the lightningBolts

    // Lightning effect properties
    [SerializeField] private LayerMask _enemyLayer;         // Layer to detect enemies
    [SerializeField] private float _bounceTime;             // How often lightning is spawned
    [SerializeField] private int _numberOfBounces = 5;      // How many times lightning is spawned
    [SerializeField] private float _minTargetingDistance;   // How close can the lightning bounce
    [SerializeField] private float _maxTargetingDistance;   // How far can the lightning bounce
    [SerializeField] private float _targetingReduction;     // How much does the targeting reduces after every bounce
    [SerializeField] private float _lightningScaleFactor;   // Scales the lightning so that it reaches the next lightning

    // Lightning variables
    private const float _lightningLength = 2.5f;            // The physical length of the normal state of lightning
    private float _bounceTimer;                             // Timer for bounces
    private int _bounceCounter;                             // Counter for number of lightning have been spawned
    private float currentDamage;                            // Damage gets reduced after every hit

    // Targeting distances
    private float _currentMaxTargetingDistance;             // Current max targeting distance
    private float _currentMinTargetingDistance;             // Current min targeting distance
    private float _currentCheckRadius;                      // Current radius

    // Enemies hit to avoid retargeting same target
    private readonly HashSet<Transform> _objectsHit = new();

    /// <summary>
    /// Resets lightning
    /// </summary>
    private void ResetTargetingDistance()
    {
        _currentMaxTargetingDistance = _maxTargetingDistance;
        _currentMinTargetingDistance = _minTargetingDistance;
        _currentCheckRadius = (_maxTargetingDistance - _minTargetingDistance) * 0.5f;
    }

    /// <summary>
    /// Reduces the targeting distance
    /// </summary>
    public void ReduceTargetingDistance()
    {
        _currentMinTargetingDistance *= _targetingReduction;
        _currentMaxTargetingDistance *= _targetingReduction;
        _currentCheckRadius = (_currentMaxTargetingDistance - _currentMinTargetingDistance) * 0.5f;
    }

    /// <summary>
    /// Checks a sphere for enemies. If there are none, get a random position
    /// </summary>
    private void CheckEnemies()
    {
        // Get check position
        Vector2 checkPos =((_currentMinTargetingDistance + _currentCheckRadius) * 0.5f
            * transform.up.normalized) + transform.position;

        // Do collision check
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(
            checkPos,
            _currentCheckRadius,
            _enemyLayer);


        foreach (var enemy in enemiesHit)
        {
            // If enemy has not been hit, target that
            // Distance check is for a bug with collsion check (enemy is detected way too far away)
            if (!_objectsHit.Contains(enemy.transform) &&
                Vector3.Distance(checkPos, enemy.transform.position) <= _currentMaxTargetingDistance)
            {
                // Enemy found target it
                EnemyFound(enemy.transform);
                // Reduce targeting distance for the next lightning and end function
                ReduceTargetingDistance();
                return;
            }
        }

        // No enemies found, get a random position in check pos
        Vector2 randomPosition = StaticFunctions.GetRandomPointInCircle(_currentCheckRadius, checkPos);
        // Spawn lightning
        SpawnLightning(randomPosition);
        // Reduce targeting distance for next lightning
        ReduceTargetingDistance();
    }

    /// <summary>
    /// Add enemy to HashSet and SpawnLightning
    /// </summary>
    /// <param name="enemy"></param>
    private void EnemyFound(Transform enemy)
    {
        _objectsHit.Add(enemy);
        SpawnLightning(enemy.position);
    }

    /// <summary>
    /// Spawn lightning and make it face targetPos
    /// </summary>
    /// <param name="targetPos"></param>
    private void SpawnLightning(Vector3 targetPos)
    {
        // Get lightning
        GameObject lightning = ObjectPooler.Instance.GetPooledObject(_lightningIdent);

        // Initialize lightning explosion
        lightning.GetComponent<Lightning>().StartLightning(transform.position, lightningLifetime, currentDamage);
        currentDamage *= 0.9f;

        StretchLightning(targetPos, lightning.transform);

        transform.up = targetPos - transform.position;
        transform.position = targetPos;
    }

    /// <summary>
    /// Stretch lightning to size
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="projectile"></param>
    private void StretchLightning(Vector3 targetPos, Transform projectile)
    {
        projectile.up = targetPos - projectile.position;

        //Stretch lighting to hit target position
        float distance = Vector3.Distance(projectile.position, targetPos);
        float yScale = distance / _lightningLength * _lightningScaleFactor;
        float xScale = Mathf.Clamp(yScale, 0.4f, 1f);
        projectile.localScale = new(xScale, yScale);
    }

    /// <summary>
    /// Reset lightning variables on spawn
    /// </summary>
    public override void OnBulletSpawn()
    {
        _currentMaxTargetingDistance = _maxTargetingDistance;
        _bounceCounter = 0;
        _bounceTimer = 0;
        _objectsHit.Clear();
        currentDamage = damage;
        ResetTargetingDistance();
    }

    /// <summary>
    /// FixedUpdate lightning
    /// On waveTimer, SpawnLightning
    /// </summary>
    public override void OnBulletFixedUpdate()
    {
        _bounceTimer += Time.fixedDeltaTime;
        if (_bounceTimer > _bounceTime && _bounceCounter < _numberOfBounces)
        {
            _bounceCounter++;
            CheckEnemies();
            _bounceTimer = 0;
        }
    }
}
