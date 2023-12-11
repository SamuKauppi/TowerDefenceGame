using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // For detecting towers
    [SerializeField] private float checkDistance;           // How far are towers detected from cursor
    [SerializeField] private LayerMask towerLayer;          // Layer mask of the turrets
    [SerializeField] private Camera mainCamera;             // Main camera of the scene
    private readonly float maxDistanceFromLastPos = 0.1f;   // How far until lastMousePos is updated

    // Gameplay set in inspector
    [SerializeField] private int startingMoney;
    [SerializeField] private int towerCostIncrese = 50;     // How much does the cost increse after every tower
    [SerializeField] private int startingCost = 100;        // How much does the costs of tower start at
    [SerializeField] private float startingHealth = 100f;   // Satrting health

    // Tower related variables
    private readonly HashSet<Tower> towersPlaced = new();   // Tracking every created tower (to disable when game ends)
    private bool isAttached;                                // If a tower is attached to the cursor
    private Tower towerAttached;                            // Instance of a tower that is attached to cursor
    private Vector2 mousePosition;                          // The current position of mouse
    private Vector2 lastMousePos;                           // Position of the mouse in the last frame.
                                                            // Move attached tower to this pos
    private const GameEntity towerIdent = GameEntity.Tower; // Tower tag

    // Gameplay
    public float PlayerHealth { get; private set; }         // Players health
    public int PlayerMoney { get; private set; }            // Players money
    private int _costOfNextTower = 0;                       // Cost of next tower
    private UpgradePanel upgradePanel;                      // Reference for faster access

    // Stats
    public delegate
        void GameOverEvent(int money, int kills, int waves);// Delegate for detecting when new wave starts
    public static
        event GameOverEvent OnGameOver;                     // Event
    private int moneyEarned;                                // How much money did player earn during game
    private int enemiesKilled;                              // How many enemies player defeated
    private int wavesCompleted = -2;                        // Waves completed (-2 because first wave is delay and increses when wave starts)
    private bool playerIsDead;                              // To prevent calling the game over event multiple times

    // Background
    [SerializeField] private SpriteRenderer backgroundColorSr;  // Background sprite
    [SerializeField] private float smoothnessValue = 100f;      // How smooth is the transition
    [SerializeField] private float transitionTime;              // How long is the transition

    private void Start()
    {
        _costOfNextTower = startingCost;
        GameObjectUpdateManager.Instance.AddObject(this);
        upgradePanel = UpgradePanel.Instance;
        TakeDamage(-startingHealth);
        UpdateMoney(startingMoney);
        upgradePanel.UpdateMoneyText(PlayerMoney, _costOfNextTower, true);
        backgroundColorSr.color = Color.white;
    }

    /// <summary>
    /// Subscribe to delgates
    /// </summary>
    private void OnEnable()
    {
        // Ememy death
        Enemy.OnEnemyDeath += EnemyKilled;
        // Tower upgrade
        UpgradeButton.OnUpgrade += UpdateMoney;
        // Wave change
        EnemyManager.OnWaveChange += StartBackgroundChange;
    }

    /// <summary>
    /// Unsubscribe from delegates
    /// </summary>
    private void OnDisable()
    {
        // Enemy death
        Enemy.OnEnemyDeath -= EnemyKilled;
        // Upgrade tower
        UpgradeButton.OnUpgrade -= UpdateMoney;
        // Wave change
        EnemyManager.OnWaveChange -= StartBackgroundChange;
    }

    /// <summary>
    /// Start TransitionColor coroutine
    /// </summary>
    /// <param name="color"></param>
    private void StartBackgroundChange(Color color)
    {
        // Ensure that alpha is 1
        color.a = 1f;
        StaticFunctions.Instance.StartTransition(backgroundColorSr, color, transitionTime, smoothnessValue);

        // Gain one health on wave end
        TakeDamage(-1f);

        // Update stats
        wavesCompleted++;
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

        // Update stats
        if (amount > 0)
        {
            moneyEarned += amount;
        }
    }

    /// <summary>
    /// Increse enemies killed and gain money based on enemy
    /// </summary>
    /// <param name="moneyEarned"></param>
    private void EnemyKilled(int moneyEarned)
    {
        enemiesKilled++;
        UpdateMoney(moneyEarned);
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateMoney(1000);
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
            if (shortestDist != null && shortestDist.TryGetComponent(out Tower target))
            {
                upgradePanel.DisplayUpgrades(target, PlayerMoney);
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
    /// Handles turret placement. Called from button
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
        // Get every collider in range
        Collider2D[] collidersInRange = Physics2D.OverlapAreaAll(
            towerAttached.transform.position - (towerAttached.TowerSize * 0.4f),
            towerAttached.transform.position + (towerAttached.TowerSize * 0.4f),
            towerLayer);

        // Check if any of them is a non trigger and cancel building if so
        for (int i = 0; i < collidersInRange.Length; i++)
        {
            if (!collidersInRange[i].isTrigger)
            {
                StopPlacingTower();
                return;
            }
        }


        // Perform calculations for pathfinding. If the new tower won't block the path, build it
        if (Pathfinding.Instance.CheckIfPathIsValid(towerAttached.transform.position, towerAttached.TowerSize))
        {
            towersPlaced.Add(towerAttached);
            isAttached = false;
            towerAttached.SetFunctional(true);
            towerAttached.ShowTowerRange = false;
            int currentCost = -_costOfNextTower;
            _costOfNextTower += towerCostIncrese;
            UpdateMoney(currentCost);
            AudioManager.Instance.PlaySFX(SFXType.Placement);
        }
        else
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
    /// Take damage
    /// TODO: Handle death
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(float amount)
    {
        if (playerIsDead) return;

        PlayerHealth = Mathf.Clamp(PlayerHealth - amount, 0, startingHealth);
        upgradePanel.UpdateLivesText(PlayerHealth);

        if (amount > 0)
        {
            AudioManager.Instance.PlaySFX(SFXType.Damage);
        }

        if (PlayerHealth <= 0)
        {
            foreach (var tower in towersPlaced)
            {
                tower.SetFunctional(false);
            }

            OnGameOver?.Invoke(wavesCompleted, enemiesKilled, moneyEarned);
            playerIsDead = true;
        }
    }
}
