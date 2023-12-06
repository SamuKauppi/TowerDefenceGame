using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyDifficultyData : MonoBehaviour
{
    // Singleton
    public static EnemyDifficultyData Instance { get; private set; }

    // Selecting enemies
    [SerializeField] private int tier1DifficultyThreshold;              // For selecting tier 1 enemies
    [SerializeField] private int tier2DifficultyThreshold;              // For selecting tier 2 enemies

    // Enemy data
    [SerializeField] private EnemyDifficulty[] tier1Enemies;
    [SerializeField] private EnemyDifficulty[] tier2Enemies;
    [SerializeField] private EnemyDifficulty[] tier3Enemies;


    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Get the desired value
    /// </summary>
    /// <param name="enemyType"></param>
    /// <param name="valueSelector"></param>
    /// <returns></returns>
    private int GetValueByType(GameEntity enemyType, Func<EnemyDifficulty, int> valueSelector)
    {
        foreach (EnemyDifficulty enemy in tier1Enemies)
        {
            if (enemy.enemyType == enemyType)
            {
                return valueSelector(enemy);
            }
        }

        foreach (EnemyDifficulty enemy in tier2Enemies)
        {
            if (enemy.enemyType == enemyType)
            {
                return valueSelector(enemy);
            }
        }

        foreach (EnemyDifficulty enemy in tier3Enemies)
        {
            if (enemy.enemyType == enemyType)
            {
                return valueSelector(enemy);
            }
        }

        return 0;
    }

    /// <summary>
    /// Get difficulty value
    /// </summary>
    /// <param name="enemyType"></param>
    /// <returns></returns>
    public int GetDifficultyValue(GameEntity enemyType)
    {
        return GetValueByType(enemyType, enemy => enemy.difficultyValue);
    }

    /// <summary>
    /// Get the money value of enemy
    /// </summary>
    /// <param name="enemyType"></param>
    /// <returns></returns>
    public int GetCurrencyValue(GameEntity enemyType)
    {
        return GetValueByType(enemyType, enemy => enemy.currencyEarned);
    }

    /// <summary>
    /// Gives enemy type based on difficulty
    /// </summary>
    /// <param name="difficulty"></param>
    /// <returns></returns>
    public GameEntity GetRandomEnemy(int difficulty)
    {
        difficulty = Random.Range(difficulty / 2, difficulty);
        if (difficulty < tier1DifficultyThreshold)
        {
            return tier1Enemies[Random.Range(0, tier1Enemies.Length)].enemyType;
        }
        else if (difficulty < tier2DifficultyThreshold)
        {
            return tier2Enemies[Random.Range(0, tier2Enemies.Length)].enemyType;
        }
        else
        {
            return tier3Enemies[Random.Range(0, tier3Enemies.Length)].enemyType;
        }
    }


}
