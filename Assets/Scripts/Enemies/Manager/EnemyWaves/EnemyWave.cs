using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    // Wave determined variables (public to be visible in inspector)
    // Visuals
    public string waveName;                                             // Wave name displayed in Ui 
    public Color waveColor;                                             // Color of wave ui
    // Wave
    public float delayBeforeWave = 1f;                                  // Delay before wave starts
    public EnemyFormation[] enemyFormations;                            // Formations

    // Auto-implemented properties
    public float DelayBeforeWave { get { return delayBeforeWave; } }    // How long until this wave starts
    public float DelayBetweenFormation { get; private set; }            // How long until next formation starts
    public float DelayBetweenSpawn { get; private set; }                // How long until next unit will be spawned
    public float TotalWaveTime { get; private set; }                    // How long will the wave last
    public bool WaveHasEnemies { get; private set; } = true;            // Does the wave contain enemies
    public bool FormationHasEnemies { get; private set; } = true;       // Does the current formation have enemies
    public int WaveAvgDifficulty { get; private set; } = 0;             // Average difficulty between waves and spawns
    public int WaveEnemyDifficulty { get; private set; } = 0;           // Total difficulty of the enemies in wave
    public int WaveSpawnDifficulty {  get; private set; } = 0;          // Total difficulty of the spawn times in wave 

    // Private properties
    private int _formationIndex;                                        // Index of the formation last used spawn (used for updating delays)
    private int _enemyCount;                                            // How many enemies can be spawned
    private int _enemyCounter;                                          // How many enemies have been spawned

    // Const
    private const string WAVENAME = "Infinite wave: ";

    /// <summary>
    /// Calcualtes the total time it takes to complete this wave
    /// </summary>
    /// <returns></returns>
    private float CalculateTotalTime()
    {
        float totalTime = DelayBeforeWave +
                          enemyFormations.Sum(formation => formation.formationDelay) +
                          enemyFormations
                              .SelectMany(formation => formation.enemiesToSpawn)
                              .Sum(pool => pool.count * pool.delay);

        return totalTime;
    }

    /// <summary>
    /// Iterate through all formations
    /// Calculate difficulties and enemy count
    /// </summary>
    /// <returns>Total enemy count</returns>
    private int IterateThroughFormations()
    {
        // Create variables
        int totalEnemyCount = 0;            // How many enemies are there in total (return value)
        int totalEnemyDifficulty = 0;       // Sum of every enemy type difficulty * count
        float totalEnemySpawnDifficulty = 0;// Sum of every spawn delay (mapped to difficulty)

        // Iterate through formations
        foreach (EnemyFormation formation in enemyFormations)
        {
            // If formation is random, then initialize the first spawn index
            formation.InitializeFirstRandomIndex();

            // Iterate through spawn pools
            foreach (EnemySpawnPool pool in formation.enemiesToSpawn)
            {
                // Add to total wave count
                totalEnemyCount += pool.count;
                // Add to enemy time
                totalEnemyDifficulty += EnemyDifficultyData.Instance.GetDifficultyValue(pool.enemyType) * pool.count;
                // Add to enemy spawn delay time
                totalEnemySpawnDifficulty += MapTimeToDifficulty(pool.delay);
            }
        }

        // Save difficulties
        WaveEnemyDifficulty = totalEnemyDifficulty;
        WaveSpawnDifficulty = Mathf.CeilToInt(totalEnemySpawnDifficulty);
        WaveAvgDifficulty = (WaveEnemyDifficulty + WaveSpawnDifficulty) / 2;

        Debug.Log("average: " + WaveEnemyDifficulty + " and " + WaveSpawnDifficulty + " = " + WaveAvgDifficulty);

        // Return total enemy count
        return totalEnemyCount;
    }

    /// <summary>
    /// Scales time to difficulty value
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private float MapTimeToDifficulty(float time)
    {
        // Ensure the time is within the specified range
        time = Mathf.Clamp(time, 0f, 5f);

        // Map the time to a value between 0 and 5
        float normalizedTime = Mathf.InverseLerp(0f, 5f, time);

        // Map the normalized time to the range 0-100
        return Mathf.Lerp(100f, 0f, normalizedTime);
    }

    /// <summary>
    /// Set parameters to initialize wave
    /// </summary>
    public void InitializeWave()
    {
        TotalWaveTime = CalculateTotalTime();
        _enemyCount = IterateThroughFormations();
    }

    /// <summary>
    /// Gives value to name and color
    /// </summary>
    /// <param name="waveNumber"></param>
    public void CreateWaveVisuals(int waveNumber)
    {
        waveName = WAVENAME + waveNumber;
        waveColor = Random.ColorHSV();
    }

    /// <summary>
    /// Gets an enemy from current formation
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameEntity GetNextEnemyFromWave(int index)
    {
        // Confirm that index fits
        if (index >= enemyFormations.Length)
        {
            // If it does not, then the wave didn't have any enemies
            WaveHasEnemies = false;
            return GameEntity.Null;
        }

        GameEntity e = enemyFormations[index].GetEnemyTypeFromFormation();

        // Check if this formation still has enemies
        FormationHasEnemies = enemyFormations[index].ContainsEnemies();

        // Increase the enemy counter and check if every enemy have been spawned
        _enemyCounter++;
        if (_enemyCounter >= _enemyCount)
        {
            WaveHasEnemies = false;
        }

        return e;
    }

    /// <summary>
    /// Updates the delay between formations (called when formation changes)
    /// </summary>
    /// <param name="index"></param>
    public void UpdateFormationDelay(int index)
    {
        if (index >= enemyFormations.Length)
        {
            DelayBetweenFormation = 0f;
            DelayBetweenSpawn = 0f;
            return;
        }

        DelayBetweenFormation = enemyFormations[index].formationDelay;
        _formationIndex = index;
    }

    /// <summary>
    /// Updates _waveTimer between spawns
    /// </summary>
    public void UpdateSpawnDelay()
    {
        if (_formationIndex < enemyFormations.Length)
        {
            int index = enemyFormations[_formationIndex].LastUsedIndex;

            if (index < enemyFormations[_formationIndex].enemiesToSpawn.Length)
            {
                DelayBetweenSpawn = enemyFormations[_formationIndex].enemiesToSpawn[index].delay;
            }
        }
    }
}
