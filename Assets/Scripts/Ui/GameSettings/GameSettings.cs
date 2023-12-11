using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private Animator settingAnimator;

    private void Start()
    {
        settingAnimator.gameObject.SetActive(false);
    }
    public void OpenMenu()
    {
        settingAnimator.gameObject.SetActive(true);
        settingAnimator.SetBool("isOpen", true);
    }

    public void CloseMenu()
    {
        settingAnimator.SetBool("isOpen", false);
    }
}
