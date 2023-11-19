using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameEntity[] enemyTypes;
    [SerializeField] private float spawnTime = 3f;
    [SerializeField] private bool reduceSpawnTime;
    private float spawnTimer = 0f;

    private void Start()
    {
        if (reduceSpawnTime)
            StartCoroutine(ReduceSpawn());
    }

    void FixedUpdate()
    {
        spawnTimer += Time.fixedDeltaTime;
        if (spawnTimer > spawnTime)
        {
            for (int i = 0; i < Mathf.CeilToInt(Time.fixedDeltaTime); i++)
            {
                Enemy latestEnemySpawned;
                latestEnemySpawned = ObjectPooler.Instance.
                    GetPooledObject(enemyTypes[Random.Range(0, enemyTypes.Length)]).GetComponent<Enemy>();

                latestEnemySpawned.transform.position =
                    new Vector3(transform.position.x, transform.position.y + Random.Range(-3f, 4f));
            }
            spawnTimer = 0;
        }

    }

    private IEnumerator ReduceSpawn()
    {
        float waitTime = spawnTime;
        while (spawnTime > 0f)
        {
            yield return new WaitForSecondsRealtime(waitTime);
            spawnTime -= 0.05f;
            spawnTime *= 0.99f;
            waitTime = Mathf.Max(1f, spawnTime);
        }
    }

}
