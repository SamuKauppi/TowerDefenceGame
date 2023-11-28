using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Refeneces
    [SerializeField] private WaveUiManager waveUiManager;

    // Waves from inspector
    [SerializeField] private List<EnemyWave> preDeterminedWaves;

    // Object pooler
    private ObjectPooler pooler;

    // Waves
    private readonly Queue<EnemyWave> enemyWaveQueue = new();       // Queue for waves used in runtime
    private EnemyWave currentWave = null;                           // Current wave 

    // private int waveNumber = 1;


    // Variables for current wave
    private float _beforeWaveTimer;             // Timer for starting wave
    private float _betweenFormationsTimer;      // Timer to formationDelay formations
    private float _betweenSpawnsTimer;          // Timer to formationDelay spawns
    private int _formationIndex;                // Counter keeping track which formations is used
    private float waveTimer;

    /// <summary>
    /// Set the ObjectPooler reference and add waves from inspector to queue
    /// </summary>
    private void Start()
    {
        pooler = ObjectPooler.Instance;
        for (int i = 0; i < preDeterminedWaves.Count; i++)
        {
            preDeterminedWaves[i].InitializeWave();
            enemyWaveQueue.Enqueue(preDeterminedWaves[i]);
            waveUiManager.AddWave(preDeterminedWaves[i].TotalWaveTime,
                preDeterminedWaves[i].waveName,
                preDeterminedWaves[i].waveColor);
        }

        StartCoroutine(SpawnWaves());
    }

    /// <summary>
    /// Handle wave spawning logic
    /// </summary>
    private void HandleWave()
    {
        // Check if timers are ready
        if (!CheckTimers())
        {
            return;
        }

        // Get enemy identifier
        GameEntity enemySpawnIdent = currentWave.GetNextEnemyFromWave(_formationIndex);

        // Check if the formation has enemies
        if (!currentWave.FormationHasEnemies)
        {
            ChangeFormations();
        }

        // Spawn enemy
        SpawnEnemy(enemySpawnIdent);
    }

    /// <summary>
    /// Check and update timers for a wave
    /// </summary>
    /// <returns></returns>
    private bool CheckTimers()
    {
        // Check that enough time has passed to start the wave
        if (_beforeWaveTimer < currentWave.DelayBeforeWave)
        {
            _beforeWaveTimer += Time.deltaTime;
            return false;
        }

        // Check that enough time has passed between formations
        if (_betweenFormationsTimer < currentWave.DelayBetweenFormation)
        {
            _betweenFormationsTimer += Time.deltaTime;
            return false;
        }

        // Check that enough time has passed to spawn the next unit
        if (_betweenSpawnsTimer < currentWave.DelayBetweenSpawn)
        {
            _betweenSpawnsTimer += Time.deltaTime;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Change to the next formation
    /// </summary>
    private void ChangeFormations()
    {
        _formationIndex++;
        _betweenFormationsTimer = 0f;
        currentWave.UpdateFormationDelay(_formationIndex);  // Update formationDelay between formations
    }

    /// <summary>
    /// Spawn enemy
    /// </summary>
    /// <param name="enemySpawnIdent"></param>
    private void SpawnEnemy(GameEntity enemySpawnIdent)
    {
        // Spawn enemy at position and reset spawn waveTimer
        GameObject lastestEnemySpawned = pooler.GetPooledObject(enemySpawnIdent);
        lastestEnemySpawned.transform.position =
            new Vector3(transform.position.x, transform.position.y + Random.Range(-3f, 4f));

        // Reset spawn waveTimer
        _betweenSpawnsTimer = 0f;
        // Update formationDelay between spawns
        currentWave.UpdateSpawnDelay();
    }

    /// <summary>
    /// Start the next wave
    /// </summary>
    private void StartNextWave()
    {
        currentWave = enemyWaveQueue.Dequeue();
        _beforeWaveTimer = 0f;
        _betweenFormationsTimer = 0f;
        _betweenSpawnsTimer = 0f;
        _formationIndex = 0;
        currentWave.UpdateFormationDelay(_formationIndex);  // Update formationDelay between formations
        Debug.Log("Wave lasted for: " + waveTimer);
        waveTimer = 0f;
        Debug.Log("Starting new wave. Expected time: " + currentWave.TotalWaveTime);
    }

    /// <summary>
    /// Spawn waves and manage them
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        StartCoroutine(WaveTimer());
        while (enemyWaveQueue.Count > 0)
        {
            // Get new wave
            StartNextWave();

            while (currentWave.WaveHasEnemies)
            {
                HandleWave();
                yield return null;
            }

            // Either delay the start of the new wave or stop wave Ui elements to fix desync between them
            if (currentWave.TotalWaveTime > waveTimer)
            {
                yield return new WaitForSeconds(currentWave.TotalWaveTime - waveTimer);
            }
            else
            {
                waveUiManager.FreezeUiElementsFor(waveTimer - currentWave.TotalWaveTime);
            }
        }
    }

    /// <summary>
    /// Time how long a wave lasts
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaveTimer()
    {
        while (true)
        {
            waveTimer += Time.deltaTime;
            yield return null;
        }
    }
}
