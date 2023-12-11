using TMPro;
using UnityEngine;

public class GameOverUi : MonoBehaviour
{
    [SerializeField] private TMP_Text statText;
    private const string WAVE = "Waves Completed: ";
    private const string ENEMIES = "Enemies Defeated: ";
    private const string MONEY = "Money Earned: ";

    public void SetStats(int waves, int enemies, int money)
    {
        statText.text = WAVE + waves + "\n" + ENEMIES + enemies + "\n" + MONEY + money;
    }

    public void ReplayGame()
    {
        PersistentManager.Instance.LoadScene(1);
    }

    public void BackToMenu()
    {
        PersistentManager.Instance.LoadScene(0);
    }
}
