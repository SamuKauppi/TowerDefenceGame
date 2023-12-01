using UnityEngine;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    private Tower targetToUpgrade;
    private TowerUpgrade upgradeType;

    public bool HasTarget { get { return targetToUpgrade != null; } }

    public void DefineUpgrade(TowerUpgrade upgrade, Tower t)
    {
        if (HasTarget)
        {
            SetTargetColor(Color.white);
            ShowTargetRange(false);
        }

        upgradeType = upgrade;
        targetToUpgrade = t;

        SetTargetColor(Color.red);
        ShowTargetRange(true);
        buttonText.text = TowerTypes.Instance.GetTowerProperties(upgrade).name;
    }

    public void UpgradeTower()
    {
        if (!HasTarget)
            return;

        targetToUpgrade.UpgradeTower(upgradeType);
        UpgradePanel.Instance.HideUpgrades();
    }

    public void SetTargetColor(Color color)
    {
        targetToUpgrade.TowerBaseRend.color = color;
    }

    public void ShowTargetRange(bool value)
    {
        targetToUpgrade.ShowTowerRange = value;
    }
}
