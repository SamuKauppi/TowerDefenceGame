using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Tower : MonoBehaviour
{
    [SerializeField] private List<Transform> targets;               // Enemies inside circlecollider2d that istrigger
    [SerializeField] private Transform barrel;                      // Barrel of the turret (pointed towards enemies)

    [SerializeField] private Collider2D towerHitbox;                // Hitbox of the turret
    [SerializeField] private CircleCollider2D attackRangeCollider;  // Collider for attack range

    [SerializeField] private SpriteRenderer barrelSpriterend;       // Sprite renderer of the barrel
    [SerializeField] public SpriteRenderer towerSpriterend;         // Sprite renderer of the tower

    private Vector3 endpoint;           // Last pathpoint (used to calculate enemy closest to exit)
    public float rotationSpeed;         // How fast does the turret rotate
    private int accuracyAngle;          // How accurate turret is (max rotation offset after finding target)

    private bool IsFunctional { get; set; }                     // Is the tower functional (has to be disabled while placing down)
    public TowerProperties CurrentUpgrade { get; private set; } // Current upgrade of the turret 

    public Vector3 TowerSize { get; private set; }   // Size of the turret
    private float attackTimer;  // Timers and counters related to firing
    private float chargeTimer;
    private int numberOfBursts;

    private void Start()
    {
        Vector3 spriteRendSize = towerSpriterend.bounds.size;
        TowerSize = new Vector3(spriteRendSize.x, spriteRendSize.y);

        endpoint = Pathfinding.Instance.GetEndPoint();
        UpgradeTower("Normal");
    }
    void Update()
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
