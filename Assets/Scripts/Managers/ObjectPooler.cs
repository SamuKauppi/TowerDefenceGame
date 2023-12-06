using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Singleton
    public static ObjectPooler Instance { get; private set; }

    // Objects to be pooled (set in inspector)
    [SerializeField] private PooledObject[] objectsToBePooled;

    // Dictionary for pooled objects
    private readonly Dictionary<GameEntity, Queue<GameObject>> objectPool = new();
    

    /// <summary>
    /// Check if object is an enemy and asgin correct value to it
    /// </summary>
    /// <param name="g"></param>
    /// <param name="e"></param>
    private void SetValueIfEnemy(GameObject g, GameEntity e)
    {
        if (g.TryGetComponent(out Enemy enemy))
        {
            enemy.SetEnemyValue(EnemyDifficultyData.Instance.GetCurrencyValue(e));
        }
    }

    /// <summary>
    /// Set instance and populate dictionary
    /// </summary>
    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < objectsToBePooled.Length; i++)
        {
            Queue<GameObject> tempQueue = new();
            for (int j = 0; j < objectsToBePooled[i].amount; j++)
            {
                GameObject obj = Instantiate(objectsToBePooled[i].obj, objectsToBePooled[i].parent);
                obj.SetActive(false);
                tempQueue.Enqueue(obj);

                SetValueIfEnemy(obj, objectsToBePooled[i].gameIdent);
            }
            objectPool.Add(objectsToBePooled[i].gameIdent, tempQueue);
        }
    }

    /// <summary>
    /// Returns a pooled object
    /// </summary>
    /// <param name="ident"></param>
    /// <returns></returns>
    public GameObject GetPooledObject(GameEntity ident)
    {
        // Does the pool contain specified object
        if (!objectPool.ContainsKey(ident)) return null;

        // Get the first active object from set
        if (!objectPool[ident].Peek().activeSelf)
        {
            GameObject g = objectPool[ident].Dequeue();
            g.SetActive(true);
            objectPool[ident].Enqueue(g);
            return g;
        }

        // If no active objects are found, create one
        foreach (PooledObject pooledType in objectsToBePooled)
        {
            if (pooledType.gameIdent == ident)
            {
                GameObject objectToCheck = Instantiate(pooledType.obj, pooledType.parent);
                SetValueIfEnemy(objectToCheck, pooledType.gameIdent);
                objectToCheck.SetActive(true);
                objectPool[ident].Enqueue(objectToCheck);
                return objectToCheck;
            }
        }

        return null;
    }
}
