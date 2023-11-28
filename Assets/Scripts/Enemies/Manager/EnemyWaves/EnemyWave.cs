using System.Linq;

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

    private int _formationIndex;
    private bool _waveIsActive = true;

    private int _enemyCount;
    private int _enemyCounter;

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
    /// <returns></returns>
    private int CalculateEnemies()
    {
        int count = 0;
        foreach (EnemyFormation formation in enemyFormations)
        {
            foreach (EnemySpawnPool pool in formation.enemiesToSpawn)
            {
                count += pool.count;
            }
        }
        return count;
    }
    /// <summary>
    /// Set parameters to initialize wave
    /// </summary>
    public void InitializeWave()
    {
        TotalWaveTime = CalculateTotalTime();
        _enemyCount = CalculateEnemies();
    }

    /// <summary>
    /// Gets an enemy from current formation
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameEntity GetNextEnemyFromWave(int index)
    {
        if (index >= enemyFormations.Length)
            return GameEntity.Null;

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
            _waveIsActive = false;
            DelayBetweenFormation = 0f;
            DelayBetweenSpawn = 0f;
            return;
        }

        DelayBetweenFormation = enemyFormations[index].formationDelay;
        _formationIndex = index;
    }

    /// <summary>
    /// Updates waveTimer between spawns
    /// </summary>
    public void UpdateSpawnDelay()
    {
        if (!_waveIsActive)
        {
            return;
        }
        int index = enemyFormations[_formationIndex].LastUsedIndex;
        DelayBetweenSpawn = enemyFormations[_formationIndex].enemiesToSpawn[index].delay;
    }
}
