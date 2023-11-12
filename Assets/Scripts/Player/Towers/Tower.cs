using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Tower : MonoBehaviour, IUpdate
{
    // Hitboxes
    [SerializeField] private Collider2D towerHitbox;                // Hitbox of the turret
    [SerializeField] private CircleCollider2D attackRangeCollider;  // Collider for attack range

    // Sprites
    [SerializeField] private SpriteRenderer barrelSpriterend;       // Sprite renderer of the barrel (asigned in inspector)
    [SerializeField] private SpriteRenderer towerSpriterend;        // Sprite renderer of the tower (asigned in inspector)

    // Targeting
    [SerializeField] private HashSet<Transform> targets = new();    // Enemies inside circlecollider2d that istrigger
    [SerializeField] private Transform barrel;                      // Barrel of the turret (pointed towards enemies)

    // Properties
    private bool IsFunctional { get; set; }                                     // Is the tower functional (has to be disabled while placing down)
    public Vector3 TowerSize { get; private set; }                              // Size of the turret
    public TowerProperties CurrentUpgrade { get; private set; }                 // Current upgrade of the turret 
    public SpriteRenderer TowerBaseRend { get { return towerSpriterend; } }     // Public renderer of the base
    public SpriteRenderer TowerBarrelRend { get { return barrelSpriterend; } }  // Public renderer of the barrel

    // Aiming
    private bool isAimingEnemy;         // Is the tower aiming towards an enemy
    private int accuracyAngle;          // How accurate the next shot will be
                                        // (randomized after every shot based on CurrenUpgrades.accuracyAngle)

    // Firing
    private bool isFiring;              // Is the tower firing
    private float attackTimer;          // Timer for firing speed
    private float chargeTimer;          // Timer for firing delay
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
        Vector3 spriteRendSize = towerSpriterend.bounds.size;
        TowerSize = new Vector3(spriteRendSize.x, spriteRendSize.y);

        // Get endpoint
        endpoint = Pathfinding.Instance.GetEndPoint();

        // Apply standard upgrade
        UpgradeTower(TowerUpgrade.Normal);

        // Add this to updatable objects
        GameObjectUpdateManager.Instance.AddObject(this);
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
        if (targets.Count > 0)
            AimAtClosest();
        // If no targets are found then tower is no longer aiming at an enemy
        else
            isAimingEnemy = false;


        // Fire if:
        // 1. Firing rate allows it
        // 2. Tower is not charging next burst of shots
        // 3. Tower is aiming at a target or is currently firing a burst while aimWhileFiring is false
        // (aimWhileFiring locks rotation while firing a burst)
        if (attackTimer >= CurrentUpgrade.attackSpeed &&
            chargeTimer >= CurrentUpgrade.chargeTime &&
            (isAimingEnemy || isFiring && !CurrentUpgrade.aimWhileFiring))
        {
            FireBullet();
        }
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
    /// <returns></returns>
    private Vector2 GetClosestTargetToEnd()
    {
        targets = targets.Where(targets => targets.gameObject.activeInHierarchy).ToHashSet();
        if (targets.Count <= 0)
            return barrel.up;

        // Take the first avaliable targets position
        Vector2 closest = targets.First().position;

        foreach (Transform target in targets)
        {
            if (Vector2.Distance(endpoint, target.position) < Vector2.Distance(endpoint, closest))
            {
                closest = target.position;
            }
        }

        return closest;
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

        // A bullet was fired. Reset attack timer and increase burst counter
        attackTimer = 0;
        numberOfBursts++;

        // If all the bullets were fired from a burst:
        // Reset variables
        if (numberOfBursts > CurrentUpgrade.burstShots)
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
        ConfigureBulletProperties(latestBulletShot);
    }
    /// <summary>
    /// Configure properties for specific type of bullets
    /// </summary>
    /// <param name="bullet"></param>
    private void ConfigureBulletProperties(GameObject bullet)
    {
        if (!bullet.TryGetComponent<BulletProperties>(out var properties))
        {
            return;
        }

        // Beam needs the barrel transform
        if (properties is BulletBeam beam)
        {
            beam.SetTowerBarrel(barrel);
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
    /// To detect enemies entering range
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            targets.Add(collision.transform);
        }
    }
    /// <summary>
    /// To detect enemies leaving range
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            targets.Remove(collision.transform);
        }
    }

    /// <summary>
    /// Set this towers functionality
    /// </summary>
    /// <param name="value"></param>
    public void SetFunctional(bool value)
    {
        IsFunctional = value;
        towerHitbox.enabled = value;
        attackRangeCollider.enabled = value;
    }

    /// <summary>
    /// Upgrade tower
    /// Update upgrade and update relevant properties
    /// </summary>
    /// <param name="upgrade"></param>
    public void UpgradeTower(TowerUpgrade upgrade)
    {
        CurrentUpgrade = TowerTypes.Instance.GetTowerProperties(upgrade);
        attackRangeCollider.radius = CurrentUpgrade.attackRange;
        TowerBarrelRend.sprite = CurrentUpgrade.barrelSprite;
        TowerBaseRend.sprite = CurrentUpgrade.towerSprite;
    }
}
