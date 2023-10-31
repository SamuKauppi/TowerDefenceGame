using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> objectPool = new();
    [SerializeField] private PooledObject[] objectsToBePooled;
    GameObject objectToCheck;
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

        for (int i = 0; i < objectPool[ident].Count; i++)
        {
            if (!objectPool[ident].Peek().activeSelf)
            {
                objectToCheck = objectPool[ident].Dequeue();
                objectToCheck.SetActive(true);
                objectPool[ident].Enqueue(objectToCheck);
                return objectToCheck;
            }
        }

        for (int i = 0; i < objectsToBePooled.Length; i++)
        {
            if (objectsToBePooled[i].ident == ident)
            {
                objectToCheck = Instantiate(objectsToBePooled[i].obj, objectsToBePooled[i].parent);
                objectToCheck.SetActive(true);
                objectPool[ident].Enqueue(objectToCheck);
                return objectToCheck;
            }
        }

        return null;
    }
}
