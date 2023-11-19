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
            // Check if it's an enemy, and if so, proceed with the logic
            enemy.TakeDamage(enemy.MaxHealth);
            player.TakeDamage(enemy.GetEnemyDamage());
        }
    }

}
