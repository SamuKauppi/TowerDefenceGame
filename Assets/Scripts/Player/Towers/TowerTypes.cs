using UnityEngine;

public class TowerTypes : MonoBehaviour
{
    public static TowerTypes Instance { get; private set; }
    [SerializeField] private TowerProperties[] towerTypes;

    private void Awake()
    {
        Instance = this;
    }

    public TowerProperties GetTowerProperties(TowerUpgrade upgrade)
    {
        foreach (TowerProperties property in towerTypes)
        {
            if (property.upgradeIdent == upgrade)
            {
                return property;
            }
        }
        Debug.LogWarning("Tower type " + upgrade + " not found");
        return null;
    }
}
