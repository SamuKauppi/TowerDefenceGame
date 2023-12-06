using System.Collections;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    private const GameEntity coinType = GameEntity.CoinVFX;
    public void StartSpawningCoins(int enemyValue)
    {
        StartCoroutine(SpawnCoins(enemyValue));
    }

    /// <summary>
    /// Spawn one coin for every 5 bucks earned
    /// </summary>
    /// <param name="enemyValue"></param>
    /// <returns></returns>
    private IEnumerator SpawnCoins(int enemyValue)
    {
        int costCounter = 0;
        while (costCounter < enemyValue)
        {
            GameObject coin = ObjectPooler.Instance.GetPooledObject(coinType);
            coin.transform.position = transform.position;
            costCounter += 15;
            yield return new WaitForSeconds(0.15f);
        }

        gameObject.SetActive(false);
    }
}
