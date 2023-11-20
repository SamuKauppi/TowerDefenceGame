using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    public string waveName;
    public EnemyFormation[] enemyFormations;
    public float delayBeforeWave = 1f;
    public float delayBewteenFormations = 1f;
    public float delayBetweenSpawns = 0.5f;

    public GameEntity GetNextEnemyFromWave(int index)
    {
        if (index >= enemyFormations.Length)
        {
            Debug.LogWarning("Index higher than wave count");
            return GameEntity.Null;
        }

        return enemyFormations[index].GetEnemyTypeFromFormation();
    }
}
