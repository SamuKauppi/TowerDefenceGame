using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Tower : MonoBehaviour, IUpdate
{
    // Hitboxes
    [SerializeField] private Collider2D towerHitbox;                // Hitbox of the turret
    [SerializeField] private Transform attackRangeObj;              // Range of the attacks 

    // Sprites
    [SerializeField] private SpriteRenderer barrelSpriteRend;       // Sprite renderer of the barrel (asigned in inspector)
    [SerializeField] private SpriteRenderer towerSpriteRend;        // Sprite renderer of the tower (asigned in inspector)
    [SerializeField] private SpriteRenderer rangeSpriteRend;

    // Targeting
    [SerializeField] private Transform barrel;                      // Barrel of the turret (pointed towards enemies)
    private HashSet<Transform> targets = new();                     // Enemies inside circlecollider2d that istrigger

    // Properties
    private bool IsFunctional { get; set; }                                     // Is the tower functional (has to be disabled while placing down)
    public Vector3 TowerSize { get; private set; }                              // Size of the turret
    public TowerProperties CurrentUpgrade { get; private set; }                 // Current upgrade of the turret 
    public SpriteRenderer TowerBaseRend { get { return towerSpriteRend; } }     // Public renderer of the base
    public SpriteRenderer TowerBarrelRend { get { return barrelSpriteRend; } }  // Public renderer of the barrel
    public bool ShowTowerRange { set { rangeSpriteRend.enabled = value; } }     // Public access to set range indicator visible

    // Aiming
    private bool isAimingEnemy;         // Is the tower aiming towards an enemy
    private int accuracyAngle;          // How accurate the next shot will be
                                        // (randomized after every shot based on CurrenUpgrades.accuracyAngle)

    // Firing
    private bool isFiring;              // Is the tower firing
    private float attackTimer;          // Timer for firing speed
    private float chargeTimer;          // Timer for firing formationDelay
    private int numberOfBursts;         // Counter for how many bursts per charge
    private Vector3 endpoint;           // Last pathpoint (used to calculate enemy closest to exit)

    // From Interface
    public GameObject Object => gameObject;

    /// <summary>
    /// Start
    /// </summary>
    private void Start()
    {
        // Get the size of the tower
        Vector3 spriteRendSize = towerSpriteRend.bounds.size;
        TowerSize = new Vector3(spriteRendSize.x, spriteRendSize.y);

        // Get endpoint
        endpoint = Pathfinding.Instance.GetEndPoint();

        // Apply standard upgrade
        UpgradeTower(TowerUpgrade.Normal);

        // Add this to updatable objects
        GameObjectUpdateManager.Instance.AddObject(this);

        attackTimer = CurrentUpgrade.attackSpeed;
        chargeTimer = CurrentUpgrade.chargeTime;
    }

    /// <summary>
    /// Update gameobject
    /// From interface
    /// </summary>
    public void UpdateObject()
    {
        if (!IsFunctional)
            return;

        // Update chargetime only if its used
        if (CurrentUpgrade.chargeTime > 0)
        {
            chargeTimer += Time.deltaTime;
        }
        attackTimer += Time.deltaTime;

        // Handle aiming and firing
        AimAndFireAtClosestEnemy();

    }
    /// <summary>
    /// First aims then starts firing at target closest to endpoint
    /// </summary>
    private void AimAndFireAtClosestEnemy()
    {
        // Aim if a target enters in range
        switch (targets.Count)
        {
            case > 0:
                // Aim at the closest if it's allowed or IsReadyToFire
                if (CurrentUpgrade.aimWhileFiring || IsReadyToFire())
                {
                    AimAtClosest();
                }
                break;
            // If no targets are found then tower is no longer aiming at an enemy
            default:
                isAimingEnemy = false;
                break;
        }

        // Fire if:
        // 1. Tower is ready to fire
        // 2. Tower is aiming at a target or is currently firing a burst while aimWhileFiring is false
        // (aimWhileFiring locks rotation while firing a burst)
        if (IsReadyToFire() &&
            (isAimingEnemy || isFiring && !CurrentUpgrade.aimWhileFiring))
        {
            FireBullet();
        }
    }
    /// <summary>
    /// Check if both attackTimer and chargeTimer are ready
    /// </summary>
    /// <returns></returns>
    private bool IsReadyToFire()
    {
        return attackTimer >= CurrentUpgrade.attackSpeed && chargeTimer >= CurrentUpgrade.chargeTime;
    }

    /// <summary>
    /// Handles aiming logic
    /// </summary>
    private void AimAtClosest()
    {
        // Get the closest target from the list of enemies in range
        Vector3 closest = GetClosestTargetToEnd();

        // Check if tower is pointing towards enemy
        isAimingEnemy = IsBarrelPointingTowardsEnemy(closest);

        // If the tower is locked while firing, don't aim
        if (!IsAllowedToAim())
        {
            return;
        }

        // Smooth the aiming if rotation speed is > 0
        if (CurrentUpgrade.rotationSpeed > 0)
        {
            // Get the true direction form the barrel pos to closest
            Vector3 direction = closest - barrel.position;
            // Get the angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            // Rotate towards the angle
            Quaternion newRotation = Quaternion.RotateTowards(barrel.rotation,
                Quaternion.Euler(0, 0, angle),
                CurrentUpgrade.rotationSpeed * Time.deltaTime);

            // Set the new rotation
            barrel.rotation = newRotation;
        }
        // Otherwise set the rotation to look at target
        else
        {
            barrel.up = closest - barrel.position;
        }
    }

    /// <summary>
    /// Checks if the barrel is pointing at position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsBarrelPointingTowardsEnemy(Vector3 pos)
    {
        return Vector3.Dot(barrel.up.normalized, (pos - barrel.position).normalized)
            >= Mathf.Cos(Mathf.Deg2Rad * CurrentUpgrade.aimThreshold);
    }

    /// <summary>
    /// Returns the position of the closest enemy in range
    /// </summary>
    /// <returns>The position of the closest enemy in range or barrel.up if no targets are active</returns>
    private Vector2 GetClosestTargetToEnd()
    {
        if (!AreTargetsActive())
        {
            return -barrel.up;
        }

        Vector2 closest = -barrel.up;
        float shortestDist = 0f;

        foreach (Transform target in targets)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget > CurrentUpgrade.attackRange)
            {
                continue;
            }

            float targetToEndPoint = Vector2.Distance(target.position, endpoint);

            if (shortestDist == 0f || targetToEndPoint < shortestDist)
            {
                shortestDist = targetToEndPoint;
                closest = target.position;
            }
        }

        return closest;
    }

    /// <summary>
    /// Filter nonactive targets from HashSet
    /// </summary>
    /// <returns>Does the HashSet have elements</returns>
    private bool AreTargetsActive()
    {
        targets = targets.Where(targets => targets.gameObject.activeSelf).ToHashSet();
        if (targets.Count <= 0)
        {
            isAimingEnemy = false;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Fires a bullet towards the direction of the barrel
    /// </summary>
    private void FireBullet()
    {
        // Randomize accuracyAngle
        accuracyAngle = Random.Range(-CurrentUpgrade.accuracy, CurrentUpgrade.accuracy + 1);
        barrel.Rotate(0, 0, accuracyAngle);

        isFiring = true;
        int shotsFired = 0;
        int currentShotAngle = 0;

        // While loop for shooting spread shots
        while (shotsFired < CurrentUpgrade.spreadShots)
        {
            SpawnABullet(currentShotAngle);

            // Add rotation for next shot based on angle
            if (currentShotAngle <= 0)
            {
                currentShotAngle = math.abs(currentShotAngle) + CurrentUpgrade.degreePerShot;
            }
            // Swap sides for next shot
            else
            {
                currentShotAngle *= -1;
            }
            shotsFired++;
        }

        // A bullet was fired. Reset attack _waveTimer and increase burst counter
        attackTimer = 0;
        numberOfBursts++;

        // If all the bullets were fired from a burst:
        // Reset variables
        if (numberOfBursts >= CurrentUpgrade.burstShots)
        {
            chargeTimer = 0;
            numberOfBursts = 0;
            isFiring = false;
            accuracyAngle = 0;
        }
    }

    /// <summary>
    /// Spawns a bullet and rotates by parameter
    /// </summary>
    /// <param name="currentShotAngle"></param>
    private void SpawnABullet(int currentShotAngle)
    {
        // Get a pooled bullet object
        GameObject latestBulletShot = ObjectPooler.Instance.GetPooledObject(CurrentUpgrade.bulletIdent);
        // Align it with barrel
        latestBulletShot.transform.SetPositionAndRotation(barrel.position, barrel.rotation);
        // Rotate it equal to currentShotAngle
        latestBulletShot.transform.Rotate(0, 0, currentShotAngle);
        // Configure bullet properties
        ConfigureBulletProperties(latestBulletShot, currentShotAngle);
    }

    /// <summary>
    /// Configure properties for specific type of bullets that was shot
    /// </summary>
    /// <param name="bullet"></param>
    private void ConfigureBulletProperties(GameObject bullet, float angleOffset)
    {
        if (!bullet.TryGetComponent<BulletProperties>(out var properties))
        {
            return;
        }

        // Beam needs the barrel transform and the angle _moveOffset
        if (properties is BulletBeam beam)
        {
            beam.SetTowerBarrel(barrel, angleOffset);
        }

        // Add more conditions for other configurations as needed
    }

    /// <summary>
    /// Return if tower is allowed to aim
    /// Ignored if aimWhileFiring is true
    /// </summary>
    /// <returns></returns>
    private bool IsAllowedToAim()
    {
        if (CurrentUpgrade.aimWhileFiring)
            return true;

        if (isFiring)
            return false;
        else
            return true;
    }

    /// <summary>
    /// Set this towers functionality
    /// </summary>
    /// <param name="value"></param>
    public void SetFunctional(bool value)
    {
        IsFunctional = value;
        towerHitbox.enabled = value;
        attackRangeObj.gameObject.SetActive(value);
    }

    /// <summary>
    /// Upgrade tower
    /// Update upgrade and update relevant properties
    /// </summary>
    /// <param name="upgrade"></param>
    public void UpgradeTower(TowerUpgrade upgrade)
    {
        CurrentUpgrade = TowerTypes.Instance.GetTowerProperties(upgrade);
        attackRangeObj.localScale = new Vector3(CurrentUpgrade.attackRange, CurrentUpgrade.attackRange, 1f);
        TowerBarrelRend.sprite = CurrentUpgrade.barrelSprite;
        TowerBaseRend.sprite = CurrentUpgrade.towerSprite;
    }

    /// <summary>
    /// Adds or removes enemies from targets hashset
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isAdding"></param>
    public void DetectEnemy(Transform target, bool isAdding)
    {
        if (isAdding)
        {
            targets.Add(target);
        }
        else
        {
            targets.Remove(target);
        }
    }

}
