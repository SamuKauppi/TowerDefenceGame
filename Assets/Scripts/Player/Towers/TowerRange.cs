using UnityEngine;

public class TowerRange : MonoBehaviour
{
    // Reference for Tower to detect collisions
    [SerializeField] private Tower parentTower;
    private const string ENEMY_TAG = "enemy";


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(ENEMY_TAG))
        {
            parentTower.DetectEnemy(collision.transform, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(ENEMY_TAG))
        {
            parentTower.DetectEnemy(collision.transform, false);
        }
    }
}
