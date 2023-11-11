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
    private bool IsFunctional { get; set; }                     // Is the tower functional (has to be disabled while placing down)
    public Vector3 TowerSize { get; private set; }              // Size of the turret
    public TowerProperties CurrentUpgrade { get; private set; } // Current upgrade of the turret 
    public SpriteRenderer TowerBaseRend { get; private set; }   // Public renderer of the base
    public SpriteRenderer TowerBarrelRend { get; private set; } // Public renderer of the barrel

    // Firing properties
    public float rotationSpeed;         // How fast does the turret rotate
    private int accuracyAngle;          // How accurate turret is (max rotation offset after finding target)

    // Firing variables
    private bool isFiring;              // Is the tower firing
    private bool isAimingEnemy;         // Is the tower aiming towards an enemy
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
        // Save renderers to public variables
        TowerBaseRend = towerSpriterend;
        TowerBarrelRend = barrelSpriterend;

        // Get the size of the tower
        Vector3 spriteRendSize = towerSpriterend.bounds.size;
        TowerSize = new Vector3(spriteRendSize.x, spriteRendSize.y);

        // Get endpoint
        endpoint = Pathfinding.Instance.GetEndPoint();

        // Apply standard upgrade
        UpgradeTower("Normal");

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
        // If no targets are found then reset áiming towards a target
        else
            isAimingEnemy = false;


        // Fire if:
        // 1. Firing rate allows it
        // 2. Tower is not charging next shot
        // 3. The barrel points towards an enemy, or isFiring is true
        if (attackTimer >= CurrentUpgrade.attackSpeed &&
            chargeTimer >= CurrentUpgrade.chargeTime &&
            (isAimingEnemy || isFiring))
        {
            FireABullet();
        }
    }
    /// <summary>
    /// Handles aiming logic
    /// </summary>
    private void AimAtClosest()
    {
        // Get the closest target from the list of enemies in range
        Vector3 closest = ClostestTargetToEnd();

        // Check if tower is pointing towards enemy
        isAimingEnemy = IsBarrelPointingTowardsEnemy(closest);
        
        // If the tower is locked while firing, don't aim
        if (!IsAllowedToAim())
        {
            return;
        }

        // Smooth the aiming if rotation speed is > 0
        if (rotationSpeed > 0)
        {
            Vector3 direction = closest - barrel.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + accuracyAngle;

            Quaternion desiredRotation = Quaternion.Euler(0, 0, angle);
            Quaternion newRotation = Quaternion.RotateTowards(barrel.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            barrel.rotation = newRotation;
        }
        else
        {
            barrel.up = closest - barrel.position;
            barrel.Rotate(0, 0, accuracyAngle);
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


    private Vector2 ClostestTargetToEnd()
    {
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


    private void FireABullet()
    {
        isFiring = true;
        int shotsFired = 0;
        int currentShotAngle = 0;
        while (shotsFired < CurrentUpgrade.spreadShots)
        {
            GameObject latestBulletShot = ObjectPooler.Instance.GetPooledObject(CurrentUpgrade.bullet);
            latestBulletShot.transform.SetPositionAndRotation(barrel.position, barrel.rotation);
            latestBulletShot.transform.Rotate(0, 0, currentShotAngle);
            if (currentShotAngle > 0)
            {
                currentShotAngle *= -1;
            }
            else
            {
                currentShotAngle = math.abs(currentShotAngle) + CurrentUpgrade.degreePerShot;
            }
            shotsFired++;
        }

        accuracyAngle = Random.Range(-CurrentUpgrade.accuracy, CurrentUpgrade.accuracy + 1);

        attackTimer = 0;

        numberOfBursts++;
        if (numberOfBursts > CurrentUpgrade.burstShots)
        {
            chargeTimer = 0;
            numberOfBursts = 0;
            isFiring = false;
        }
    }

    private bool IsAllowedToAim()
    {
        if (CurrentUpgrade.aimWhileFiring)
            return true;

        if (isFiring)
            return false;
        else
            return true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            targets.Add(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("enemy"))
        {
            targets.Remove(collision.transform);
        }
    }
    public void SetFunctional(bool value)
    {
        IsFunctional = value;
        towerHitbox.enabled = value;
        attackRangeCollider.enabled = value;
    }


    public void UpgradeTower(string upgrade)
    {
        CurrentUpgrade = TowerTypes.Instance.GetTowerProperties(upgrade);
        attackRangeCollider.radius = CurrentUpgrade.attackRange;
        TowerBarrelRend.sprite = CurrentUpgrade.barrelSprite;
        TowerBaseRend.sprite = CurrentUpgrade.towerSprite;
        rotationSpeed = CurrentUpgrade.rotationSpeed;
    }
}
