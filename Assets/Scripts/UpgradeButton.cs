using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    private Tower targetToUpgrade;
    private string upgradeType;


    public void DefineUpgrade(string upgrade, Tower t)
    {
        if(targetToUpgrade != null)
            targetToUpgrade.towerSpriterend.color = Color.white;

        upgradeType = upgrade;
        targetToUpgrade = t;

        targetToUpgrade.towerSpriterend.color = Color.red;

        buttonText.text = TowerTypes.Instance.GetTowerProperties(upgrade).name;
    }

    public void UpgradeTower()
    {
        targetToUpgrade.UpgradeTower(upgradeType);
        UpgradePanel.Instance.HideUpgrades();
        targetToUpgrade.towerSpriterend.color = Color.white;
    }
}
