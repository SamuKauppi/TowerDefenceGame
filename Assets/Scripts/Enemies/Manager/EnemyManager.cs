using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Refeneces
    [SerializeField] private WaveUiManager waveUiManager;
    private ObjectPooler pooler;

    // Waves from inspector
    [SerializeField] private List<EnemyWave> preDeterminedWaves;    // Waves set in inspector
    [SerializeField] private Vector2 spawnYPositions = new(-3f, 4f);// Spawn positions for enemies (randomly selected value between)

    // Waves
    private readonly Queue<EnemyWave> enemyWaveQueue = new();       // Queue for waves used in runtime
    private EnemyWave currentWave = null;                           // Current wave 
    private int _waveNumber = 0;                                    // Current wave number (starts at wave 0)
    private int _lastestDifficulty;                                 // Lastest difficulty (used to measure difficulty scale)
    private int _averageDifficultyIncrese;                          // How much difficulty increses over the predetermined waves

    // On wave change
    public delegate
    void NewWaveEventHandler(Color value);                  // Delegate for detecting when new wave starts
    public static
        event NewWaveEventHandler OnWaveChange;             // Event

    // Variables for current wave
    private float _beforeWaveTimer;             // Timer for starting wave
    private float _betweenFormationsTimer;      // Timer to formationDelay formations
    private float _betweenSpawnsTimer;          // Timer to formationDelay spawns
    private int _formationIndex;                // Counter keeping track which formations is used
    private float _waveTimer;                   // Timer to keep visuals and wave in sync

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

            // Calculate difficulties
            int waveAvgDifficulty = preDeterminedWaves[i].WaveAvgDifficulty;

            // Add difficutly to count avg difficulty increase
            _averageDifficultyIncrese += waveAvgDifficulty;
            // Save the avg difficulty as latest difficulty
            _lastestDifficulty = waveAvgDifficulty;
        }

        _averageDifficultyIncrese /= preDeterminedWaves.Count;
        Debug.Log(_averageDifficultyIncrese);

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
        // Confirm that the enemy ident is correct
        if (enemySpawnIdent != GameEntity.Null)
        {
            // Spawn enemy at position and reset spawn _waveTimer
            GameObject lastestEnemySpawned = pooler.GetPooledObject(enemySpawnIdent);
            lastestEnemySpawned.transform.position =
                new Vector3(transform.position.x, transform.position.y + Random.Range(spawnYPositions.x, spawnYPositions.y));
        }

        // Reset spawn _waveTimer
        _betweenSpawnsTimer = 0f;
        // Update formationDelay between spawns
        currentWave.UpdateSpawnDelay();
    }

    /// <summary>
    /// Start the next wave
    /// </summary>
    private void StartNextWave()
    {
        // Get a new wave
        currentWave = enemyWaveQueue.Dequeue();

        // Reset timers and index
        _beforeWaveTimer = 0f;
        _betweenFormationsTimer = 0f;
        _betweenSpawnsTimer = 0f;
        _formationIndex = 0;
        _waveTimer = 0f;

        // Update delays in the wave
        currentWave.UpdateFormationDelay(_formationIndex);  // Update formationDelay between formations
        currentWave.UpdateSpawnDelay();

        // Invoke wave change
        OnWaveChange?.Invoke(currentWave.waveColor);

        // Tell waveUi to continue moving
        waveUiManager.ContinueUiMovement();
    }

    /// <summary>
    /// Spawn waves and manage them
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        StartCoroutine(WaveTimer());
        while (true)
        {
            if (enemyWaveQueue.Count < 5)
            {
                CreateNewWave();
            }
            // Get new wave
            StartNextWave();

            while (currentWave.WaveHasEnemies)
            {
                HandleWave();
                yield return null;
            }

            // Either delay the start of the new wave or stop wave Ui elements to fix desync between them
            if (currentWave.TotalWaveTime > _waveTimer)
            {
                yield return new WaitForSeconds(currentWave.TotalWaveTime - _waveTimer);
            }
            _waveNumber++;
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
            _waveTimer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Create new wave
    /// </summary>
    private void CreateNewWave()
    {
        // Create new wave with 8 seconds delay
        EnemyWave newWave = new()
        {
            delayBeforeWave = 8f
        };

        // Calcualte new difficulty 
        _lastestDifficulty += _averageDifficultyIncrese;

        // Add formations to wave
        newWave.enemyFormations = CreateFormations();

        // Initialize wave properties and visuals
        newWave.InitializeWave();
        newWave.CreateWaveVisuals(_waveNumber);

        // Create wave ui element
        waveUiManager.AddWave(newWave.TotalWaveTime, newWave.waveName, newWave.waveColor);

        // Add wave to queue
        enemyWaveQueue.Enqueue(newWave);
    }

    /// <summary>
    /// Creates formations
    /// </summary>
    /// <param name="formationCount"></param>
    /// <param name="enemies"></param>
    /// <returns></returns>
    private EnemyFormation[] CreateFormations()
    {
        // Decide how many formations are created
        int formationCount = Random.Range(1, 4);

        // List of formations
        EnemyFormation[] formations = new EnemyFormation[formationCount];

        // Loop through formations
        for (int i = 0; i < formations.Length; i++)
        {
            // Create new formation
            formations[i] = new()
            {
                // Create spawn pools for the formation
                enemiesToSpawn = CreateSpawnPools(formationCount),
                // Set the formation delay
                formationDelay = 10f
            };
            // Set random index
            formations[i].InitializeFirstRandomIndex();
        }

        return formations;
    }

    /// <summary>
    /// Creates an array of EnemySpawnPool objects for a formation.
    /// </summary>
    /// <param name="formationCount">Number of formations.</param>
    /// <returns>Array of EnemySpawnPool objects.</returns>
    private EnemySpawnPool[] CreateSpawnPools(int formationCount)
    {
        // Fetch enemies for the formation based on the latest difficulty level
        GameEntity[] enemies = SelectEnemies(_lastestDifficulty);

        // Create an array of pools, each representing an enemy type
        EnemySpawnPool[] pools = new EnemySpawnPool[enemies.Length];

        // Initialize each pool with an enemy type, a count of 0 and random spawn delay
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new EnemySpawnPool
            {
                enemyType = enemies[i],
                count = 0,
                delay = Random.Range(0.01f, 0.50f)
            };
        }

        // Variables for tracking the pool index and used difficulty
        int poolIndex = 0;
        int usedDifficulty = 0;

        // Increase the pool's enemy count until the difficulty budget is reached
        while (usedDifficulty < _lastestDifficulty / formationCount)
        {
            // Increase enemy count, used difficulty, and total enemy count
            pools[poolIndex].count++;
            usedDifficulty += EnemyDifficultyData.Instance.GetDifficultyValue(pools[poolIndex].enemyType);

            // Move to the next pool or loop back to the first one
            poolIndex = (poolIndex + 1) % pools.Length;
        }

        // Return the array of created EnemySpawnPool objects
        return pools;
    }

    /// <summary>
    /// Returns random count of enemies based on difficulty
    /// </summary>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    private GameEntity[] SelectEnemies(int difficulty)
    {
        int count = Random.Range(3, 7);
        List<GameEntity> enemiesSelected = new();
        for (int i = 0; i < count; i++)
        {
            enemiesSelected.Add(EnemyDifficultyData.Instance.GetRandomEnemy(difficulty));
        }
        return enemiesSelected.ToArray();
    }
}
