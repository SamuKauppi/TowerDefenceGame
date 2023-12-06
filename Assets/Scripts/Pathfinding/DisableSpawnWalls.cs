using UnityEngine;

public class DisableSpawnWalls : MonoBehaviour
{
    // For fincding the size
    [SerializeField] private SpriteRenderer sr;
    private void Start()
    {
        DisablePathpointsInArea();
    }

    private void DisablePathpointsInArea()
    {
        Vector2 size = new(sr.bounds.size.x, sr.bounds.size.y);
        Pathfinding.Instance.DisablePathpointsInArea(transform.position, size);
    }
}
