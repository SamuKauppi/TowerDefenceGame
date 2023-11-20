using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
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
    private float _betweenFormationsTimer;      // Timer to delay formations
    private float _betweenSpawnsTimer;          // Timer to delay spawns
    private int _formationIndex;                // Counter keeping track which formations is used

    /// <summary>
    /// Set the ObjectPooler reference and add waves from inspector to queue
    /// </summary>
    private void Start()
    {
        pooler = ObjectPooler.Instance;
        for (int i = preDeterminedWaves.Count - 1; i >= 0; i--)
        {
            enemyWaveQueue.Enqueue(preDeterminedWaves[i]);
        }
    }

    /// <summary>
    /// Update timers and Handle wave
    /// </summary>
    private void Update()
    {
        // Check that there are waves to spawn
        if (enemyWaveQueue.Count <= 0 && currentWave == null)
        {
            Debug.Log("No waves to spawn.");
            return;
        }

        _beforeWaveTimer += Time.deltaTime;
        _betweenFormationsTimer += Time.deltaTime;
        _betweenSpawnsTimer += Time.deltaTime;
        HandleWave();
    }

    /// <summary>
    /// Handle wave spawning logic
    /// </summary>
    private void HandleWave()
    {
        // Start a new wave if currentWave is null
        if (currentWave == null)
        {
            currentWave = enemyWaveQueue.Dequeue();
            _beforeWaveTimer = 0f;
            _betweenFormationsTimer = currentWave.delayBewteenFormations;
            _betweenSpawnsTimer = currentWave.delayBetweenSpawns;
            _formationIndex = 0;

            Debug.Log("Starting new wave.");
        }

        // Set the currentWave to null if no more formations remain in wave
        if (_formationIndex >= currentWave.enemyFormations.Length)
        {
            currentWave = null;
            _beforeWaveTimer = 0f;
            _betweenSpawnsTimer = 0f;
            _betweenFormationsTimer = 0f;
            _formationIndex = 0;
            Debug.Log("No more formations in the current wave. Wave completed.");
            return;
        }

        // Check that enough time has passed to start the wave
        if (_beforeWaveTimer < currentWave.delayBeforeWave)
        {
            return;
        }

        // Check that enough time has passed between formations
        if (_betweenFormationsTimer < currentWave.delayBewteenFormations)
        {
            return;
        }

        // Check that enough time has passed to spawn the next unit
        if (_betweenSpawnsTimer < currentWave.delayBetweenSpawns)
        {
            return;
        }

        // Get enemy identifier
        GameEntity enemySpawnIdent = currentWave.GetNextEnemyFromWave(_formationIndex);

        // If the enemy returned is null, the formation has run out of enemies
        // Move to the next formation
        if (enemySpawnIdent == GameEntity.Null)
        {
            _formationIndex++;
            _betweenFormationsTimer = 0f;
            Debug.Log("Formation completed. Moving to the next formation.");
            return;
        }

        // Spawn enemy at position and reset spawn timer
        GameObject lastestEnemySpawned = pooler.GetPooledObject(enemySpawnIdent);
        lastestEnemySpawned.transform.position =
            new Vector3(transform.position.x, transform.position.y + Random.Range(-3f, 4f));
        _betweenSpawnsTimer = 0f;

        Debug.Log("Enemy spawned successfully.");
    }

}
