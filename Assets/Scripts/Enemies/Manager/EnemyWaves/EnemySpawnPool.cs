[System.Serializable]
public class EnemySpawnPool
{
    public GameEntity enemyType = GameEntity.NormalEnemy;
    public int count;
    public float delay;
    public int EnemiesSpawned { get; private set; } = 0;

    /// <summary>
    /// Returns GameEntity type and increases EnemiesSpawned
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
