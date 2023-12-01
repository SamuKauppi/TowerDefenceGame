using System;
using UnityEngine;

public class EnemyDifficultyData : MonoBehaviour
{
    // Singleton
    public static EnemyDifficultyData Instance { get; private set; }

    // Enemy data
    [SerializeField] private EnemyDifficulty[] enemyData;

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
        foreach (EnemyDifficulty enemy in enemyData)
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

}
