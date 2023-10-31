using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTypes : MonoBehaviour
{
    public static TowerTypes Instance { get; private set; }
    [SerializeField] private TowerProperties[] towerTypes;

    private void Awake()
    {
        Instance = this;
    }

    public TowerProperties GetTowerProperties(string name)
    {
        for (int i = 0; i < towerTypes.Length; i++)
        {
            if (towerTypes[i].name == name)
            {
                return towerTypes[i];
            }
        }

        return null;
    }

}
