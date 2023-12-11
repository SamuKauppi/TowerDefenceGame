using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicText;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxText;

    private const string MUSIC = "Music Volume: ";
    private const string SFX = "SFX Volume: ";
    private const string PRECENT = "%";

    private void Start()
    {
        SetSliders();
    }

    private void SetSliders()
    {
        musicVolumeSlider.value = PersistentManager.Instance.musicVolume;
        musicText.text = MUSIC + VolumeValueToString(musicVolumeSlider.value);

        sfxVolumeSlider.value = PersistentManager.Instance.sfxVolume;
        sfxText.text = SFX + VolumeValueToString(sfxVolumeSlider.value);
    }

    private void SetVolume(float musicVolume, float sfxVolume)
    {
        PersistentManager.Instance.musicVolume = musicVolume;
        PlayerPrefs.SetFloat(PersistentManager.Instance.musicKeyValue, musicVolume);
        musicText.text = MUSIC + VolumeValueToString(musicVolume);

        PersistentManager.Instance.sfxVolume = sfxVolume;
        PlayerPrefs.SetFloat(PersistentManager.Instance.sfxKeyValue, sfxVolume);
        sfxText.text = SFX + VolumeValueToString(sfxVolume);
    }

    private string VolumeValueToString(float original)
    {
        return (Math.Round(original, 2) * 100f).ToString() + PRECENT;
    }
    public void UpdateVolumes()
    {
        SetVolume(musicVolumeSlider.value, sfxVolumeSlider.value);
    }
}
