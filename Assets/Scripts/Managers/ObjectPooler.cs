using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [SerializeField] private PooledObject[] objectsToBePooled;

    private readonly Dictionary<string, Queue<GameObject>> objectPool = new();
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
            }
            objectPool.Add(objectsToBePooled[i].ident, tempQueue);
        }
    }

    public GameObject GetPooledObject(string ident)
    {
        if (!objectPool.ContainsKey(ident)) return null;

        if (!objectPool[ident].Peek().activeSelf)
        {
            GameObject objectToCheck = objectPool[ident].Dequeue();
            objectToCheck.SetActive(true);
            objectPool[ident].Enqueue(objectToCheck);
            return objectToCheck;
        }

        foreach (PooledObject pooledObj in objectsToBePooled)
        {
            if (pooledObj.ident == ident)
            {
                GameObject objectToCheck = Instantiate(pooledObj.obj, pooledObj.parent);
                objectToCheck.SetActive(true);
                objectPool[ident].Enqueue(objectToCheck);
                return objectToCheck;
            }
        }

        return null;
    }
}
