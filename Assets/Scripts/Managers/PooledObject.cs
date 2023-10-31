using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PooledObject
{
    public string ident;
    public GameObject obj;
    public int amount;
    public Transform parent;
}
