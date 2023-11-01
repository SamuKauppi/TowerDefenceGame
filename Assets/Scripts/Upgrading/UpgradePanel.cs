using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static UpgradePanel Instance { get; private set; }
    [SerializeField] private UpgradeButton[] upgradeButtons;
    public bool IsMouseOver { get; private set; }
    private int isMouseOverCounter = 0;
    private void Awake()
    {
        Instance = this;
    }

    public void DisplayUpgrades(Tower target)
    {
        for (int i = 0; i < target.CurrentUpgrade.upgradePaths.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].DefineUpgrade(target.CurrentUpgrade.upgradePaths[i], target);
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
        isMouseOverCounter++;
        IsMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOverCounter--;
        if (isMouseOverCounter < 0)
        {
            isMouseOverCounter = 0;
            IsMouseOver = false;
        }
    }
}
