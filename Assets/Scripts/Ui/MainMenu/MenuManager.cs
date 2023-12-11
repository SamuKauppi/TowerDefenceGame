using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        PersistentManager.Instance.LoadScene(1);
    }
}
