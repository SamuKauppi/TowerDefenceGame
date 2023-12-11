using UnityEngine;

[System.Serializable]
public class SFXAudio
{
    public AudioSource source;
    public float originalVolume;
    public SFXType type;
    public float cooldown;
    public float cooldownTimer;
}
