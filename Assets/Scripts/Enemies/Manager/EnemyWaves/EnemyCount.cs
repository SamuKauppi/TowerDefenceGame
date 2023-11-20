[System.Serializable]
public class EnemyCount
{
    public GameEntity enemyType = GameEntity.NormalEnemy;
    public int enemyCount;
    public int EnemiesSpawned { get; private set; } = 0;

    /// <summary>
    /// Returns GameEntity type and increases EnemiesSpawned
    /// </summary>
    /// <returns></returns>
    public GameEntity GetEnemy()
    {
        if (EnemiesSpawned < enemyCount)
        {
            EnemiesSpawned++;
            return enemyType;
        }
        return GameEntity.Null;
    }
}
