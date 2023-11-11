using UnityEngine;
/// <summary>
/// An Interface to update gameobjects
/// </summary>
public interface IUpdate
{
    GameObject Object { get; }
    void UpdateObject(); 
}
