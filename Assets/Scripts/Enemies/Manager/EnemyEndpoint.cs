using UnityEngine;

public class EnemyEndpoint : MonoBehaviour
{
    [SerializeField] private PlayerScript player;
    private void Start()
    {
        GetComponent<PathPoint>().SetDistanceToEnd(0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Enemy enemy))
        {
            // Player takes damage
            player.TakeDamage(enemy.GetEnemyDamage());
            // Reset enemy
            enemy.EnemyReachedEnd();
        }
    }

}
