using System;
using UnityEngine;

[Serializable]
public class PooledObject
{
#if UNITY_EDITOR
    public string ident;        // Only in inspector
#endif
    public GameEntity gameIdent;
    public GameObject obj;
    public int amount;
    public Transform parent;
}
