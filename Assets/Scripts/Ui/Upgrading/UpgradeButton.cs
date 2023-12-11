using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // OnUpgradeEvent
    public delegate
    void UpgradeTowerHandler(int cost);                 // Delegate for when upgrade happens
    public static
        event UpgradeTowerHandler OnUpgrade;            // Event

    // Ui elements
    [SerializeField] private Button button;             // Button that upgrades tower
    [SerializeField] private TMP_Text buttonText;       // Text displayed in button
    [SerializeField] private TMP_Text hoverText;        // Description displayed on hover object
    [SerializeField] private GameObject hoverObject;    // HoverObject

    // Upgrading
    private int upgradeCost;                            // Cost of this upgrade
    private TowerUpgrade upgradeType;                   // Upgrade type that this button has
    private const string UPGRADETEXT = ":\n";           // Text put between the name and cost

    // References
    private UpgradePanel upgradePanel;

    private void Start()
    {
        upgradePanel = UpgradePanel.Instance;
    }

    /// <summary>
    /// Define what will be upgraded when button is pressed
    /// </summary>
    /// <param name="upgrade"></param>
    /// <param name="t"></param>
    public void DefineUpgrade(TowerUpgrade upgrade, int playerMoney)
    {
        // Ensure that the hover object is inactive
        hoverObject.SetActive(false);

        // Define upgrade
        upgradeType = upgrade;

        // Update UI
        TowerProperties properties = TowerTypes.Instance.GetTowerProperties(upgrade);
        upgradeCost = properties.cost;
        buttonText.text = properties.name + UPGRADETEXT + upgradeCost;
        hoverText.text = properties.description;

        IsUpgradeReady(playerMoney);
    }

    /// <summary>
    /// Upgrade tower
    /// </summary>
    public void UpgradeTower()
    {
        OnUpgrade?.Invoke(-upgradeCost);
        upgradePanel.SelectUpgrade(upgradeType);
    }

    /// <summary>
    /// Is upgrade able to be purchased
    /// </summary>
    /// <param name="playerMoney"></param>
    public void IsUpgradeReady(int playerMoney)
    {
        // Disable button if player does not have enough money
        if (playerMoney < upgradeCost)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }

    /// <summary>
    /// Toggle hover object on exit and entry
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        hoverObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverObject.SetActive(true);
    }
}
