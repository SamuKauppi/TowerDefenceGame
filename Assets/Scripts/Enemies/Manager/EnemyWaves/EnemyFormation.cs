using UnityEngine;

[System.Serializable]
public class EnemyFormation
{
    public EnemyCount[] enemiesToSpawn;

    // Spawn order type
    public SpawnOrder spawnOrder = SpawnOrder.Random;

    // Varibles
    private int _currentIndex = 0;          // Used both in AllFromOne and Rotating spawn order
                                            // AllFromOne = +1 when everything is spawned from the current formation
                                            // Rotating = +1 after every spawn and loops

    /// <summary>
    /// Returns the GameEntity of the next enemy to be sapwned
    /// </summary>
    /// <returns></returns>
    public GameEntity GetEnemyTypeFromFormation()
    {
        return spawnOrder switch
        {
            SpawnOrder.AllFromOne => GetAllFromOneEnemyType(),
            SpawnOrder.Random => GetRandomEnemyType(),
            SpawnOrder.Rotating => GetRotatingEnemyType(),
            _ => GameEntity.Null,
        };
    }
    /// <summary>
    /// Random spawn order logic
    /// </summary>
    /// <returns></returns>
    private GameEntity GetRandomEnemyType()
    {
        // Set a default value
        GameEntity e = GameEntity.Null;

        // Ensure that formation contains enemies
        if (ContainsEnemies())
        {
            // Randomly select once a valid GameEntity type is selected
            while (e == GameEntity.Null)
            {
                int randomIndex = Random.Range(0, enemiesToSpawn.Length);
                e = enemiesToSpawn[randomIndex].GetEnemy();
            }
        }
        return e;
    }

    /// <summary>
    /// Get all from one spawn order logic
    /// </summary>
    /// <returns></returns>
    private GameEntity GetAllFromOneEnemyType()
    {
        GameEntity enemyType = GameEntity.Null;
        // While no enemy type has been found or the index has reached the end
        while (enemyType == GameEntity.Null && _currentIndex < enemiesToSpawn.Length)
        {
            // Get enemy type
            enemyType = enemiesToSpawn[_currentIndex].GetEnemy();

            // If the type is null, then no more of that type can be spawned
            // Move to next element in array
            if (enemyType == GameEntity.Null)
            {
                _currentIndex++;
            }
        }

        return enemyType;
    }

    /// <summary>
    /// Rotating spawn order logic
    /// </summary>
    /// <returns></returns>
    private GameEntity GetRotatingEnemyType()
    {
        GameEntity enemyType = GameEntity.Null;
        // If enemies remain
        if (ContainsEnemies())
        {
            while (enemyType == GameEntity.Null)
            {
                // Get enemy type (returns null if no more enemies remain
                enemyType = enemiesToSpawn[_currentIndex].GetEnemy();

                // Increase index if enemy type is null
                if (enemyType == GameEntity.Null)
                {
                    IncreaseRotatingIndex();
                }
            }
        }

        // Increase index but ensure it stays within bounds
        IncreaseRotatingIndex();

        return enemyType;
    }

    /// <summary>
    /// Ensures that the index loops
    /// </summary>
    private void IncreaseRotatingIndex()
    {
        _currentIndex++;
        if (_currentIndex >= enemiesToSpawn.Length)
        {
            _currentIndex = 0;
        }
    }

    /// <summary>
    /// Does the formation still have enemies to spawn
    /// </summary>
    /// <returns></returns>
    private bool ContainsEnemies()
    {
        foreach (EnemyCount enemy in enemiesToSpawn)
        {
            if (enemy.EnemiesSpawned < enemy.enemyCount)
            {
                return true;
            }
        }
        return false;
    }
}