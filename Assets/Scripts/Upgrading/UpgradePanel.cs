using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Singleton
    public static UpgradePanel Instance { get; private set; }

    // Variables
    public bool IsMouseOver { get; private set; }               // Is the players mouse over this this panel
    [SerializeField] private UpgradeButton[] upgradeButtons;    // Button objects
    private int isMouseOverCounter = 0;                         // Failsafe detecting if OnPointer-events happen multiple times
    private void Awake()
    {
        Instance = this;
        HideUpgrades();
    }

    /// <summary>
    /// Display upgrades on panel
    /// </summary>
    /// <param name="target"></param>
    public void DisplayUpgrades(Tower target)
    {
        for (int i = 0; i < target.CurrentUpgrade.upgradePaths.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].DefineUpgrade(target.CurrentUpgrade.upgradePaths[i], target);
        }
    }
    /// <summary>
    /// Hide the upgrade buttons
    /// </summary>
    public void HideUpgrades()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(false);
            if (upgradeButtons[i].HasTarget)
            {
                upgradeButtons[i].SetTargetColor(Color.white);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOverCounter++;
        IsMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOverCounter--;
        if (isMouseOverCounter <= 0)
        {
            isMouseOverCounter = 0;
            IsMouseOver = false;
        }
    }
}
