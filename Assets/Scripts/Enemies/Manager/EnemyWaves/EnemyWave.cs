using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    // Wave determined variables (public to be visible in inspector)
    public string waveName;
    public UnityEngine.Color waveColor;
    public float delayBeforeWave = 1f;
    public EnemyFormation[] enemyFormations;

    // Auto-implemented properties
    public float DelayBeforeWave { get { return delayBeforeWave; } }    // How long until this wave starts
    public float DelayBetweenFormation { get; private set; }            // How long until next formation starts
    public float DelayBetweenSpawn { get; private set; }                // How long until next unit will be spawned
    public float TotalWaveTime { get; private set; }                    // How long will the wave last
    public bool WaveHasEnemies { get; private set; } = true;            // Does the wave contain enemies
    public bool FormationHasEnemies { get; private set; } = true;       // Does the current formation have enemies
    public int WaveDifficulty { get; private set; }                     // Total time

    private int _formationIndex;                                        // index of the formation last used (used for updating delays)

    private int _enemyCount;                                            // How many enemies can be spawned
    private int _enemyCounter;                                          // How many enemies have been spawned

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
    /// Calculate how many enemies does this wave has
    /// </summary>
    /// <returns>Total enemy count</returns>
    private int IterateThroughFormations()
    {
        // Create variables
        int totalEnemyCount = 0;            // How many enemies are there in total (return value)
        int totalEnemyDifficulty = 0;       // How difficult are enemies
        float totalEnemySpawnDifficulty = 0;// How difficult are spawns delays
        float totalFormationDifficulty = 0; // How difficult are formations delays  

        // Iterate through formations
        foreach (EnemyFormation formation in enemyFormations)
        {
            // If formation is random, then initialize the first spawn index
            formation.InitializeFirstRandomIndex();
            // Add to formation delay time
            totalFormationDifficulty += MapTimeToDifficulty(formation.formationDelay);

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

        // If only one formation exists, don't affect time
        if(enemyFormations.Length <= 0)
            totalFormationDifficulty = 0;

        // Calculate total wave time
        WaveDifficulty = totalEnemyDifficulty +
            Mathf.CeilToInt(totalEnemySpawnDifficulty / totalEnemyCount) +
            Mathf.CeilToInt(totalFormationDifficulty / enemyFormations.Length);

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
        time = Mathf.Clamp(time, 0.1f, 2.0f);

        // Map the time to a value between 0 and 1
        float normalizedTime = Mathf.InverseLerp(0.1f, 2.0f, time);

        // Map the normalized time to the range 0-30
        return Mathf.Lerp(30f, 0f, normalizedTime);
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
