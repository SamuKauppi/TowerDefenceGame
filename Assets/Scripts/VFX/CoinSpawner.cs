using System.Collections;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private float originalVolume;
    [SerializeField] private AudioSource coinSource;
    [SerializeField] private AudioClip[] coinSounds;
    private const GameEntity coinType = GameEntity.CoinVFX;

    public void StartSpawningCoins(int enemyValue)
    {
        StartCoroutine(SpawnCoins(enemyValue));
    }

    /// <summary>
    /// Spawn one coin for every 15 bucks earned
    /// </summary>
    /// <param name="enemyValue"></param>
    /// <returns></returns>
    private IEnumerator SpawnCoins(int enemyValue)
    {
        int costCounter = 0;
        int coinSoundIndex = Random.Range(0, coinSounds.Length);

        coinSource.clip = coinSounds[coinSoundIndex];
        coinSource.volume = originalVolume * PersistentManager.Instance.sfxVolume;
        coinSource.Play();

        while (costCounter < enemyValue)
        {
            GameObject coin = ObjectPooler.Instance.GetPooledObject(coinType);
            coin.transform.position = transform.position;
            costCounter += 15;
            yield return new WaitForSeconds(0.15f);
        }

        while (coinSource.isPlaying)
        {
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
