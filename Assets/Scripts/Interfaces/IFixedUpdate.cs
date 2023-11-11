using UnityEngine;
/// <summary>
/// An Interface to fixed update gameobjects
/// </summary>
public interface IFixedUpdate 
{
    GameObject Object { get; }
    void FixedUpdateGameobject();
}
