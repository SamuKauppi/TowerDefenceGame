using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // For detecting towers
    [SerializeField] private float checkDistance;           // How far are towers detected from cursor
    [SerializeField] private LayerMask towerLayer;          // Layer mask of the turrets
    [SerializeField] private Camera mainCamera;             // Main camera of the scene
    private readonly float maxDistanceFromLastPos = 0.2f;   // How far until lastMousePos is updated (should match pathfinding positionStep)

    // Costs
    [SerializeField] private int startingMoney;
    [SerializeField] private int towerCostIncrese = 50;     // How much does the cost increse after every tower
    [SerializeField] private int startingCost = 100;        // How much does the costs of tower start at
    private UpgradePanel upgradePanel;                      // Reference for faster access

    // Tower related variables
    private bool isAttached;                                // If a tower is attached to the cursor
    private Tower towerAttached;                            // Instance of a tower that is attached to cursor
    private int towersPlaced = 0;                           // Amount of towers placed
    private Vector2 mousePosition;                          // The current position of mouse
    private Vector2 lastMousePos;                           // Position of the mouse in the last frame.
                                                            // Move attached tower to this pos

    private const GameEntity towerIdent = GameEntity.Tower; // Tower tag

    // Gameplay
    public float PlayerHealth { get; private set; } = 100f; // Players health
    public int PlayerMoney { get; private set; }            // Players money
    private int _costOfNextTower = 0;

    private void Start()
    {
        _costOfNextTower = startingCost;
        GameObjectUpdateManager.Instance.AddObject(this);
        upgradePanel = UpgradePanel.Instance;
        UpdateMoney(startingMoney);
        upgradePanel.UpdateMoneyText(PlayerMoney, _costOfNextTower, true);
    }

    /// <summary>
    /// Subscribe parentTower to enemy death delegate
    /// </summary>
    private void OnEnable()
    {
        Enemy.OnEnemyDeath += UpdateMoney;
    }

    /// <summary>
    /// Unsubscribe parentTower from enemy death delegate
    /// </summary>
    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= UpdateMoney;
    }

    /// <summary>
    /// Increases parentTower money by amount
    /// </summary>
    /// <param name="amount"></param>
    private void UpdateMoney(int amount)
    {
        PlayerMoney += amount;
        bool canBuildNew = PlayerMoney >= _costOfNextTower;
        upgradePanel.UpdateMoneyText(PlayerMoney, _costOfNextTower, canBuildNew);
    }

    /// <summary>
    /// Update parentTower
    /// </summary>
    private void Update()
    {
        // Get mouse pos
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Handle when no turret is attached
        if (!isAttached)
        {
            NoTurretAttached();
        }
        // Move the attached tower with the mouse if one exists
        else if (towerAttached != null)
        {
            TurretPlacement();
        }
    }

    /// <summary>
    /// Handles update when no turret is attached and parentTower clicks
    /// </summary>
    private void NoTurretAttached()
    {
        if (!Input.GetMouseButtonDown(0) || upgradePanel.IsMouseOver)
        {
            return;
        }

        // Get turret colliders in a range around the mouse
        Collider2D[] collidersInRange =
            Physics2D.OverlapCircleAll(mousePosition, checkDistance, towerLayer);

        // If colldiers were found in range, activate upgrade panel for closest tower
        if (collidersInRange.Length > 0)
        {
            // Get the closest collider to mouse
            Collider2D shortestDist = GetClosestColliderToMouse(collidersInRange);

            // if a shortest distance was found, diplay upgrades and return
            if (shortestDist != null)
            {
                upgradePanel.DisplayUpgrades(shortestDist.GetComponent<Tower>());
                return;
            }
        }

        upgradePanel.HideUpgrades();    
    }

    /// <summary>
    /// Find the closest collider to mouse position
    /// </summary>
    /// <param name="collidersInRange"></param>
    /// <returns></returns>
    private Collider2D GetClosestColliderToMouse(Collider2D[] collidersInRange)
    {
        Collider2D shortestDist = null;
        foreach (Collider2D collider in collidersInRange)
        {
            // Confirm that the collider is not the range indicator of a tower
            if (collider.isTrigger)
                continue;
            // Give shortest distance a base value
            else if (shortestDist == null)
            {
                shortestDist = collider;
                continue;
            }

            // Compare distances
            if (Vector2.Distance(mousePosition, collider.transform.position)
                < Vector2.Distance(mousePosition, shortestDist.transform.position))
            {
                shortestDist = collider;
            }
        }

        return shortestDist;
    }

    /// <summary>
    /// Handles turret placement
    /// </summary>
    private void TurretPlacement()
    {
        // Update the position of the attached turret if mouse has moved further than max distance
        if (Vector3.Distance(lastMousePos, mousePosition) > maxDistanceFromLastPos)
        {
            towerAttached.transform.position =
                Pathfinding.Instance.GetClosestPathPoint(mainCamera.ScreenToWorldPoint(Input.mousePosition)).transform.position;
            lastMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        // Check input
        if (Input.GetMouseButtonDown(0))
        {
            AttemptToPlaceTower();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            StopPlacingTower();
        }
    }

    /// <summary>
    /// Starts placing tower
    /// Activated from a button
    /// </summary>
    public void StartPlacingTower()
    {
        if (isAttached || PlayerMoney < _costOfNextTower) return;
        towerAttached = ObjectPooler.Instance.GetPooledObject(towerIdent).GetComponent<Tower>();
        isAttached = true;
        upgradePanel.HideUpgrades();
    }

    /// <summary>
    /// Stop placing tower
    /// </summary>
    private void StopPlacingTower()
    {
        if (!isAttached) return;
        isAttached = false;
        towerAttached.gameObject.SetActive(false);
        upgradePanel.HideUpgrades();
    }
    /// <summary>
    /// If no towers are found within turret area range
    /// And it won't block path
    /// Place tower
    /// </summary>
    private void AttemptToPlaceTower()
    {
        Collider2D[] collidersInRange = Physics2D.OverlapAreaAll(
            towerAttached.transform.position - (towerAttached.TowerSize * 0.4f),
            towerAttached.transform.position + (towerAttached.TowerSize * 0.4f),
            towerLayer);

        for (int i = 0; i < collidersInRange.Length; i++)
        {
            if (!collidersInRange[i].isTrigger)
            {
                StopPlacingTower();
                return;
            }
        }

        towersPlaced++;
        if (Pathfinding.Instance.CheckIfPathIsValid(towerAttached.transform.position, towerAttached.TowerSize, towersPlaced))
        {
            isAttached = false;
            towerAttached.SetFunctional(true);
            int currentCost = -_costOfNextTower;
            _costOfNextTower += towerCostIncrese;
            UpdateMoney(currentCost);
        }
        else
        {
            StopPlacingTower();
        }
    }

    /// <summary>
    /// Take damage
    /// TODO: Handle death
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        PlayerHealth -= amount;
        if (PlayerHealth <= 0)
        {
            Debug.Log("deth");
        }
    }
}
