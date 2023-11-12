using UnityEngine;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    private Tower targetToUpgrade;
    private TowerUpgrade upgradeType;

    public void DefineUpgrade(TowerUpgrade upgrade, Tower t)
    {
        if(targetToUpgrade != null)
            targetToUpgrade.TowerBaseRend.color = Color.white;

        upgradeType = upgrade;
        targetToUpgrade = t;

        targetToUpgrade.TowerBaseRend.color = Color.red;

        buttonText.text = TowerTypes.Instance.GetTowerProperties(upgrade).name;
    }

    public void UpgradeTower()
    {
        targetToUpgrade.UpgradeTower(upgradeType);
        UpgradePanel.Instance.HideUpgrades();
        targetToUpgrade.TowerBaseRend.color = Color.white;
    }
}
