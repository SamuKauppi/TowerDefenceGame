using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Singleton
    public static UpgradePanel Instance { get; private set; }
    public bool IsMouseOver { get; private set; }               // Is the players mouse over this this panel

    // Upgrading
    [SerializeField] private UpgradeButton[] upgradeButtons;    // Button objects
    private int isMouseOverCounter = 0;                         // Failsafe detecting if OnPointer-events happen multiple times

    // Money related
    [SerializeField] private Button buildTowerButton;           // Button for buying towers
    [SerializeField] private TMP_Text buildTowerText;           // Displays next tower cost
    [SerializeField] private TMP_Text moneyText;                // Displays money
    private const string BUGBUCKS = "BugBucks: ";
    private const string TOWERTEXT = "Build Tower!\n Cost: ";

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
        target.ShowTowerRange = true;
    }

    /// <summary>
    /// Hide the upgrade buttons
    /// </summary>
    public void HideUpgrades()
    {
        foreach (UpgradeButton button in upgradeButtons)
        {
            button.gameObject.SetActive(false);
            if (button.HasTarget)
            {
                button.SetTargetColor(Color.white);
                button.ShowTargetRange(false);
            }
        }
    }

    /// <summary>
    /// Updates the amount of money displayed on screen
    /// </summary>
    /// <param name="amount"></param>
    public void UpdateMoneyText(int amount, int nextTowerCost, bool canBuildTower)
    {
        moneyText.text = BUGBUCKS + amount;
        buildTowerButton.interactable = canBuildTower;
        buildTowerText.text = TOWERTEXT + nextTowerCost;
    }

    /// <summary>
    /// Keeps in track if mose is over ui element
    /// </summary>
    /// <param name="eventData"></param>
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
