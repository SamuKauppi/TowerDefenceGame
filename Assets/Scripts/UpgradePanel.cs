using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static UpgradePanel Instance { get; private set; }
    [SerializeField] private UpgradeButton[] upgradeButtons;
    public int IsMouseOver;
    private void Awake()
    {
        Instance = this;
        IsMouseOver = 0;
    }

    public void DisplayUpgrades(Tower target)
    {
        for (int i = 0; i < target.currentUpgrade.upgradePaths.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].DefineUpgrade(target.currentUpgrade.upgradePaths[i], target);
        }
    }

    public void HideUpgrades()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsMouseOver++;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsMouseOver--;
        if (IsMouseOver < 0)
            IsMouseOver = 0;
    }
}
