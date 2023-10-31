using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private Tower tower_prefab;
    private Tower tower_attached;
    [SerializeField] private bool isAttached;
    [SerializeField] private float checkDistance;
    [SerializeField] private LayerMask towerLayer;
    [SerializeField] private Camera cam;
    public float PlayerHealth { get; private set; }
    public int PlayerMoney { get; private set; }
    private float moneyTimer;

    private Vector3 lastMousePos;
    private readonly float maxDistanceFromLastPos = 0.1f;

    private void Start()
    {
        PlayerHealth = 100f;
        PlayerMoney = 200;
    }
    private void Update()
    {
        if (!isAttached)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Collider2D[] collidersInRange =
                    Physics2D.OverlapCircleAll(mousPos, 0.15f, towerLayer);
                if (collidersInRange.Length > 0 && UpgradePanel.Instance.IsMouseOver == 0)
                {
                    Collider2D shortestDist = collidersInRange[0];
                    for (int i = 0; i < collidersInRange.Length; i++)
                    {
                        if (Vector2.Distance(mousPos, collidersInRange[i].transform.position)
                            < Vector2.Distance(mousPos, shortestDist.transform.position))
                        {
                            shortestDist = collidersInRange[i];
                        }
                    }
                    UpgradePanel.Instance.DisplayUpgrades(shortestDist.GetComponent<Tower>());
                }
                else if (UpgradePanel.Instance.IsMouseOver == 0)
                {
                    UpgradePanel.Instance.HideUpgrades();
                }
            }
        }
        else if (PlayerMoney > 50)
        {
            if (Vector3.Distance(lastMousePos, cam.ScreenToWorldPoint(Input.mousePosition)) > maxDistanceFromLastPos)
            {
                tower_attached.transform.position =
                    Pathfinding.Instance.GetClosestPathPoint(cam.ScreenToWorldPoint(Input.mousePosition)).transform.position;
                lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            }


            if (Input.GetMouseButtonDown(0))
            {
                AttemptToPlaceTower();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                StopPlacingTower();
            }
        }

        moneyTimer += Time.deltaTime;
        if (moneyTimer > 2f)
        {
            moneyTimer = 0f;
            PlayerMoney += 20;
        }
    }

    public void StartPlacingTower()
    {
        if (isAttached) return;
        tower_attached = ObjectPooler.Instance.GetPooledObject("tower").GetComponent<Tower>();
        isAttached = true;
        UpgradePanel.Instance.HideUpgrades();
    }

    private void StopPlacingTower()
    {
        if (!isAttached) return;
        isAttached = false;
        tower_attached.gameObject.SetActive(false);
        UpgradePanel.Instance.HideUpgrades();
    }

    private void AttemptToPlaceTower()
    {
        Collider2D[] collidersInRange = Physics2D.OverlapAreaAll(
            tower_attached.transform.position - (tower_attached.towerSize * 0.4f),
            tower_attached.transform.position + (tower_attached.towerSize * 0.4f),
            towerLayer);

        for (int i = 0; i < collidersInRange.Length; i++)
        {
            if (!collidersInRange[i].isTrigger)
            {
                StopPlacingTower();
                return;
            }
        }

        if (Pathfinding.Instance.CheckIfPathIsValid(tower_attached.transform.position, tower_attached.towerSize))
        {
            isAttached = false;
            tower_attached.SetFunctional(true);
        }
        else
        {
            StopPlacingTower();
        }
    }

    public void TakeDamage(float amount)
    {
        PlayerHealth -= amount;
        if (PlayerHealth <= 0)
        {
            Debug.Log("dead");
        }
    }
}
