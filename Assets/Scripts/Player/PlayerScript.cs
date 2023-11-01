using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private Tower tower_prefab;        // Prefab
    [SerializeField] private bool isAttached;           // If a tower is attached to the cursor
    [SerializeField] private float checkDistance;       // How far are towers detected from cursor
    [SerializeField] private LayerMask towerLayer;      // Layer mask of the turrets
    [SerializeField] private Camera mainCamera;         // Main camera of the scene
    public float PlayerHealth { get; private set; }     // Players health
    // TODO: Create a way for player to get money from kills
    public int PlayerMoney { get; private set; }        // Players money

    private Tower tower_attached;                           // Instance of a tower that is attached to cursor

    private Vector2 mousePosition;                          // The current position of mouse
    private Vector2 lastMousePos;                           // Position of the mouse in the last frame.
                                                            // Move attached tower to this pos
    private readonly float maxDistanceFromLastPos = 0.1f;   // How far until lastMousePos is updated

    private void Start()
    {
        // TODO: make better way to asign player stats
        PlayerHealth = 100f;
        PlayerMoney = 200;
    }
    private void Update()
    {
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (!isAttached)
        {
            NoTurretAttached();
        }
        else if (PlayerMoney > 50)
        {
            TurretPlacement();
        }
    }
    /// <summary>
    /// Handles update when no turret is attached and player clicks
    /// </summary>
    private void NoTurretAttached()
    {
        if (!Input.GetMouseButtonDown(0) || UpgradePanel.Instance.IsMouseOver)
        {
            return;
        }

        // Get turret colliders in a range around the mouse
        Collider2D[] collidersInRange =
            Physics2D.OverlapCircleAll(mousePosition, checkDistance, towerLayer);

        // If colldiers were found in range, satrt upgrade panel for closest tower
        if (collidersInRange.Length > 0)
        {
            Collider2D shortestDist = collidersInRange[0];
            for (int i = 0; i < collidersInRange.Length; i++)
            {
                if (Vector2.Distance(mousePosition, collidersInRange[i].transform.position)
                    < Vector2.Distance(mousePosition, shortestDist.transform.position))
                {
                    shortestDist = collidersInRange[i];
                }
            }
            UpgradePanel.Instance.DisplayUpgrades(shortestDist.GetComponent<Tower>());
        }
        // Otherwise hide upgrades
        else
        {
            UpgradePanel.Instance.HideUpgrades();
        }
    }
    /// <summary>
    /// Handles turret placement
    /// </summary>
    private void TurretPlacement()
    {
        // Update the position of the attached turret if mouse has moved further than max distance
        if (Vector3.Distance(lastMousePos, mousePosition) > maxDistanceFromLastPos)
        {
            tower_attached.transform.position =
                Pathfinding.Instance.GetClosestPathPoint(mainCamera.ScreenToWorldPoint(Input.mousePosition)).transform.position;
            lastMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("os");
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
        if (isAttached) return;
        tower_attached = ObjectPooler.Instance.GetPooledObject("tower").GetComponent<Tower>();
        isAttached = true;
        UpgradePanel.Instance.HideUpgrades();
    }

    /// <summary>
    /// Stop placing tower
    /// </summary>
    private void StopPlacingTower()
    {
        if (!isAttached) return;
        isAttached = false;
        tower_attached.gameObject.SetActive(false);
        UpgradePanel.Instance.HideUpgrades();
    }
    /// <summary>
    /// If no towers are found within turret area range
    /// And it won't block path
    /// Place tower
    /// </summary>
    private void AttemptToPlaceTower()
    {
        Collider2D[] collidersInRange = Physics2D.OverlapAreaAll(
            tower_attached.transform.position - (tower_attached.TowerSize * 0.4f),
            tower_attached.transform.position + (tower_attached.TowerSize * 0.4f),
            towerLayer);

        for (int i = 0; i < collidersInRange.Length; i++)
        {
            if (!collidersInRange[i].isTrigger)
            {
                StopPlacingTower();
                return;
            }
        }

        if (Pathfinding.Instance.CheckIfPathIsValid(tower_attached.transform.position, tower_attached.TowerSize))
        {
            isAttached = false;
            tower_attached.SetFunctional(true);
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
            Debug.Log("dead");
        }
    }
}
