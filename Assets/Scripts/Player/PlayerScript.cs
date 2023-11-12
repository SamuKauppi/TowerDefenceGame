using System.Collections.Generic;
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

    private Tower towerAttached;                            // Instance of a tower that is attached to cursor
    private int towersPlaced = 0;                           // Amount of towers placed

    private Vector2 mousePosition;                          // The current position of mouse
    private Vector2 lastMousePos;                           // Position of the mouse in the last frame.
                                                            // Move attached tower to this pos
    private readonly float maxDistanceFromLastPos = 0.1f;   // How far until lastMousePos is updated

    private const GameEntity towerIdent = GameEntity.Tower;

    private void Start()
    {
        // TODO: make better way to asign player stats
        PlayerHealth = 100f;
        PlayerMoney = 20000;
        GameObjectUpdateManager.Instance.AddObject(this);
    }

    public void Update()
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
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        // Get turret colliders in a range around the mouse
        Collider2D[] collidersInRange =
            Physics2D.OverlapCircleAll(mousePosition, checkDistance, towerLayer);

        // If colldiers were found in range, activate upgrade panel for closest tower
        if (collidersInRange.Length > 0 && !UpgradePanel.Instance.IsMouseOver)
        {
            // Get the closest collider to mouse
            Collider2D shortestDist = GetClosestColliderToMouse(collidersInRange);

            // if a shortest distance was found, diplay upgrades and return
            if (shortestDist != null)
            {
                UpgradePanel.Instance.DisplayUpgrades(shortestDist.GetComponent<Tower>());
                return;
            }
        }

        UpgradePanel.Instance.HideUpgrades();    
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
        if (isAttached) return;
        towerAttached = ObjectPooler.Instance.GetPooledObject(towerIdent).GetComponent<Tower>();
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
        towerAttached.gameObject.SetActive(false);
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

        }
    }
}
