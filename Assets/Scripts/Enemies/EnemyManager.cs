using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameEntity[] enemyTypes;
    private float time = 0f;
    [SerializeField] private float spawnTimer = 0.5f;
    [SerializeField] private bool reduceSpawnTime;

    private void Start()
    {
        if (reduceSpawnTime)
            StartCoroutine(ReduceSpawn());
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time > spawnTimer)
        {
            for (int i = 0; i < Mathf.CeilToInt(Time.fixedDeltaTime); i++)
            {
                Enemy latestEnemySpawned;
                latestEnemySpawned = ObjectPooler.Instance.
                    GetPooledObject(enemyTypes[Random.Range(0, enemyTypes.Length)]).GetComponent<Enemy>();

                latestEnemySpawned.transform.position =
                    new Vector3(transform.position.x, transform.position.y + Random.Range(-3f, 4f));
            }
            time = 0;
        }

    }

    private IEnumerator ReduceSpawn()
    {
        float waitTime = spawnTimer;
        while (spawnTimer > 0f)
        {
            yield return new WaitForSecondsRealtime(waitTime);
            spawnTimer -= 0.05f;
            spawnTimer *= 0.99f;
            waitTime = Mathf.Max(1f, spawnTimer);
        }
    }

}
