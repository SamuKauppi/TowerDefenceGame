using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Manages the gameobject updating
/// </summary>
public class GameObjectUpdateManager : MonoBehaviour
{
    // Singleton
    public static GameObjectUpdateManager Instance { get; private set; }

    // Updateable objects
    private readonly HashSet<IFixedUpdate> fixedUpdates = new();    // fixed update
    private readonly HashSet<IUpdate> updates = new();              // update

    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// Add an object to be updated and/or fixedupdated
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public void AddObject<T>(T obj) where T : class
    {
        if (obj is IFixedUpdate fixedObj && !fixedUpdates.Contains(fixedObj))
        {
            fixedUpdates.Add(fixedObj);
        }
        if (obj is IUpdate updateObj && !updates.Contains(updateObj))
        {
            updates.Add(updateObj);
        }
    }

    private void Update()
    {
        foreach (IUpdate updateObj in updates)
        {
            if (!updateObj.Object.activeInHierarchy)
            {
                continue;
            }
            updateObj.UpdateObject();
        }
    }

    private void FixedUpdate()
    {
        foreach (IFixedUpdate updateObj in fixedUpdates)
        {
            if (!updateObj.Object.activeInHierarchy)
            {
                continue;
            }
            updateObj.FixedUpdateGameobject();
        }
    }
}
