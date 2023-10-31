using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Tower : MonoBehaviour
{

    [SerializeField] private List<Transform> targets;               // Enemies inside circlecollider2d that istrigger
    [SerializeField] private Transform barrel;                      // Barrel of the turret (pointed towards enemies)
    private Vector3 endpoint;                                       // Last pathpoint (used to calculate enemy closest to exit)
    [SerializeField] private Collider2D towerHitbox;                // Hitbox of the turret
    [SerializeField] private CircleCollider2D attackRangeCollider;  // Collider for attack range

    public float rotationSpeed;         // how fast does the turret rotate
    private int accuracyAngle;          // how accurate turret is (max rotation offset after finding target)

    GameObject latestBulletShot;

    // Is the tower functional (has to be disabled while placing down)
    private bool IsFunctional { get; set; }
    // Current upgrade of the turret
    public TowerProperties currentUpgrade;
    // Size of the turret
    public Vector3 towerSize;
    // Timers and counters related to firing
    private float attackTimer;
    private float chargeTimer;
    private int numberOfBursts;

    // Sprite renderer
    [SerializeField] private SpriteRenderer barrelSpriterend;
    [SerializeField] public SpriteRenderer towerSpriterend;

    private void Start()
    {
        endpoint = Pathfinding.Instance.GetEndPoint();
        UpgradeTower("Normal");
    }
    void Update()
    {
        if (IsFunctional)
        {
            if (currentUpgrade.chargeTime > 0)
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

        if (attackTimer >= currentUpgrade.attackSpeed &&
                chargeTimer >= currentUpgrade.chargeTime)
        {
            FireABullet();
        }

    }
    private Vector2 ClostestTargetToEnd()
    {
        Vector2 closest = targets[0].position;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (!targets[i].gameObject.activeSelf)
            {
                targets.Remove(targets[i]);
                continue;
            }
            if (Vector2.Distance(endpoint, targets[i].position)
                < Vector2.Distance(endpoint, closest))
            {
                closest = targets[i].position;
            }
        }
        return closest;
    }

    private void FireABullet()
    {
        int shotsFired = 0;
        int currentShotAngle = 0;
        while (shotsFired < currentUpgrade.spreadShots)
        {
            latestBulletShot = ObjectPooler.Instance.GetPooledObject(currentUpgrade.bullet);
            latestBulletShot.transform.SetPositionAndRotation(barrel.position, barrel.rotation);
            latestBulletShot.transform.Rotate(0, 0, currentShotAngle);
            if (currentShotAngle > 0)
            {
                currentShotAngle *= -1;
            }
            else
            {
                currentShotAngle = math.abs(currentShotAngle) + currentUpgrade.degreePerShot;
            }
            shotsFired++;
        }

        accuracyAngle = Random.Range(-currentUpgrade.accuracy, currentUpgrade.accuracy + 1);

        attackTimer = 0;

        numberOfBursts++;
        if (numberOfBursts > currentUpgrade.burstShots)
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
        currentUpgrade = TowerTypes.Instance.GetTowerProperties(upgrade);
        attackRangeCollider.radius = currentUpgrade.attackRange;
        barrelSpriterend.sprite = currentUpgrade.barrelSprite;
        towerSpriterend.sprite = currentUpgrade.towerSprite;
        rotationSpeed = currentUpgrade.rotationSpeed;
    }
}
