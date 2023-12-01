[System.Serializable]
public class EnemySpawnPool
{
    public GameEntity enemyType = GameEntity.NormalEnemy;
    public int count = 1;
    public float delay = 0.1f;
    public int EnemiesSpawned { get; private set; } = 0;

    /// <summary>
    /// Returns GameEntity type and increases EnemiesSpawned
    /// Returns Null if count >= EnemiesSpawned
    /// </summary>
    /// <returns></returns>
    public GameEntity GetEnemy()
    {
        if (EnemiesSpawned < count)
        {
            EnemiesSpawned++;
            return enemyType;
        }
        return GameEntity.Null;
    }
}
