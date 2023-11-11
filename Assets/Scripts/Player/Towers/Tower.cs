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
    [SerializeField] private SpriteRenderer barrelSpriterend;       // Sprite renderer of the barrel
    [SerializeField] public SpriteRenderer towerSpriterend;         // Sprite renderer of the tower

    // Targeting
    [SerializeField] private HashSet<Transform> targets = new();    // Enemies inside circlecollider2d that istrigger
    [SerializeField] private Transform barrel;                      // Barrel of the turret (pointed towards enemies)

    // Properties
    private bool IsFunctional { get; set; }                     // Is the tower functional (has to be disabled while placing down)
    public Vector3 TowerSize { get; private set; }              // Size of the turret
    public TowerProperties CurrentUpgrade { get; private set; } // Current upgrade of the turret 

    public GameObject Object => gameObject;

    // firing properties
    private Vector3 endpoint;           // Last pathpoint (used to calculate enemy closest to exit)
    public float rotationSpeed;         // How fast does the turret rotate
    private int accuracyAngle;          // How accurate turret is (max rotation offset after finding target)
    private float attackTimer;          // Timers and counters related to firing
    private float chargeTimer;          // How long does the tower take to charge a shot
    private int numberOfBursts;         // How many shots can the tower shoot

    private void Start()
    {
        Vector3 spriteRendSize = towerSpriterend.bounds.size;
        TowerSize = new Vector3(spriteRendSize.x, spriteRendSize.y);

        endpoint = Pathfinding.Instance.GetEndPoint();
        UpgradeTower("Normal");
        GameObjectUpdateManager.Instance.AddObject(this);
    }

    public void UpdateObject()
    {
        if (IsFunctional)
        {
            if (CurrentUpgrade.chargeTime > 0)
            {
                chargeTimer += Time.deltaTime;
            }
            attackTimer += Time.deltaTime;

            if (targets.Count > 0)
            {
                AimAndFireAtClosestEnemy();
            }
        }
    }

    private void AimAndFireAtClosestEnemy()
    {
        Vector3 closest = ClostestTargetToEnd();

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

        if (attackTimer >= CurrentUpgrade.attackSpeed &&
                chargeTimer >= CurrentUpgrade.chargeTime)
        {
            FireABullet();
        }

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
        }
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
        barrelSpriterend.sprite = CurrentUpgrade.barrelSprite;
        towerSpriterend.sprite = CurrentUpgrade.towerSprite;
        rotationSpeed = CurrentUpgrade.rotationSpeed;
    }
}
