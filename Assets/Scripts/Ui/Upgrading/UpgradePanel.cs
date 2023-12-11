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
    [SerializeField] private TMP_Text upgradeName;              // Where upgrade name is displayed
    [SerializeField] private UpgradeButton[] upgradeButtons;    // Button objects
    private int isMouseOverCounter = 0;                         // Failsafe detecting if OnPointer-events happen multiple times
    private Tower targetToUpgrade;

    // Money related
    [SerializeField] private Button buildTowerButton;           // Button for buying towers
    [SerializeField] private TMP_Text buildTowerText;           // Displays next tower cost
    [SerializeField] private TMP_Text moneyText;                // Displays money
    private const string TOWERTEXT = "Build Tower!\n Cost: ";
    private int money;

    // Lives related
    [SerializeField] private TMP_Text livesText;

    private void Awake()
    {
        Instance = this;
        HideUpgrades();
    }

    /// <summary>
    /// Set tower visuals to match current state
    /// </summary>
    /// <param name="value"></param>
    private void SetTowerVisuals(bool value)
    {
        // Change the color based if it's being selected or deselected
        targetToUpgrade.TowerBaseRend.color = value ? Color.red : targetToUpgrade.CurrentUpgrade.towerBaseColor;
        // Show or hide tower range
        targetToUpgrade.ShowTowerRange = value;
        // Set name
        upgradeName.text = targetToUpgrade.CurrentUpgrade.name;
    }

    /// <summary>
    /// Display upgrades on panel
    /// </summary>
    /// <param name="target"></param>
    public void DisplayUpgrades(Tower target, int playerMoney)
    {
        HideUpgrades();
        // Hide previous tower visuals
        if (targetToUpgrade != null)
        {
            SetTowerVisuals(false);
        }
        // Show new tower visuals
        targetToUpgrade = target;
        SetTowerVisuals(true);

        for (int i = 0; i < target.CurrentUpgrade.upgradePaths.Length; i++)
        {
            upgradeButtons[i].gameObject.SetActive(true);
            upgradeButtons[i].DefineUpgrade(target.CurrentUpgrade.upgradePaths[i], playerMoney);
        }
        upgradeName.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the upgrade buttons
    /// </summary>
    public void HideUpgrades()
    {
        upgradeName.gameObject.SetActive(false);
        // Hide tower visuals
        if (targetToUpgrade != null)
        {
            SetTowerVisuals(false);
        }
        foreach (UpgradeButton button in upgradeButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Upgrade target tower
    /// </summary>
    /// <param name="upgrade"></param>
    public void SelectUpgrade(TowerUpgrade upgrade)
    {
        if (targetToUpgrade)
        {
            targetToUpgrade.UpgradeTower(upgrade);
            DisplayUpgrades(targetToUpgrade, money);
        }
        else
        {
            HideUpgrades();
        }

    }

    /// <summary>
    /// Updates the player money and health displayed on screen
    /// </summary>
    /// <param name="playerMoney"></param>
    /// <param name="nextTowerCost"></param>
    /// <param name="canBuildTower"></param>
    public void UpdateMoneyText(int playerMoney, int nextTowerCost, bool canBuildTower)
    {
        money = playerMoney;

        string moneyShortened = playerMoney.ToString();
        // If player has more than 1000, shorten it to 1k
        if (money >= 1000 && money < 1000000)
        {
            float floatMoney = playerMoney;
            floatMoney /= 1000.0f; // Divide by a float to keep decimals
            moneyShortened = floatMoney.ToString("#,##0.0") + "k";
        }
        // If player has more than 1 mil, shorten it to 1m
        else if (money >= 1000000)
        {
            float floatMoney = playerMoney;
            floatMoney /= 1000000.0f; // Divide by a float to keep decimals
            moneyShortened = floatMoney.ToString("#,##0.0") + "m";
        }


        moneyText.text = moneyShortened;
        buildTowerButton.interactable = canBuildTower;
        buildTowerText.text = TOWERTEXT + nextTowerCost;

        foreach (UpgradeButton button in upgradeButtons)
        {
            button.IsUpgradeReady(playerMoney);
        }
    }

    /// <summary>
    /// Updates the life text
    /// </summary>
    /// <param name="hp"></param>
    public void UpdateLivesText(float hp)
    {
        livesText.text = hp.ToString();
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
