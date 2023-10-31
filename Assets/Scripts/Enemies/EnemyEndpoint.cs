using System.Collections;
using System.Collections.Generic;
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
        if (collision.CompareTag("enemy"))
        {
            Enemy e = collision.GetComponent<Enemy>();
            e.TakeDamage(e.MaxHealth);
            player.TakeDamage(e.GetEnemyDamage());
        }
    }
}
