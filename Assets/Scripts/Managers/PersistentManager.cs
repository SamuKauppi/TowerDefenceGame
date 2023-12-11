using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }
    public string musicKeyValue;
    public float musicVolume;
    public string sfxKeyValue;
    public float sfxVolume;

    private void Awake()
    {
        musicVolume = PlayerPrefs.GetFloat(musicKeyValue, 0.4f);
        sfxVolume = PlayerPrefs.GetFloat(sfxKeyValue, 0.8f);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
