using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private string[] enemyTypes;
    private float time = 0f;
    [SerializeField] private float spawnTimer = 0.5f;
    [SerializeField] private bool reduceSpawnTime;

    private void Start()
    {
        if (reduceSpawnTime)
            StartCoroutine(ReduceSpawn());
    }
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > spawnTimer)
        {
            Enemy latestEnemySpawned;
            latestEnemySpawned = ObjectPooler.Instance.
                GetPooledObject(enemyTypes[Random.Range(0, enemyTypes.Length)]).GetComponent<Enemy>();

            latestEnemySpawned.transform.position =
                new Vector3(transform.position.x, transform.position.y + Random.Range(-3f, 4f));

            time = 0;
        }

    }

    private IEnumerator ReduceSpawn()
    {
        while (spawnTimer > 0.0002f)
        {
            yield return new WaitForSeconds(1f);
            spawnTimer *= 0.97f;
        }
    }
}
