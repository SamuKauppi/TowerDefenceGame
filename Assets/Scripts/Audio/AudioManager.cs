using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private SFXAudio[] vfxTypes;

    private float originalMusicVolume;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        originalMusicVolume = musicSource.volume;
        musicSource.volume *= PersistentManager.Instance.musicVolume;
        foreach (var vfxSource in vfxTypes)
        {
            vfxSource.originalVolume = vfxSource.source.volume;
            vfxSource.source.volume *= PersistentManager.Instance.sfxVolume;
        }
    }

    private void Update()
    {
        foreach (var vfxType in vfxTypes)
        {
            if (vfxType.cooldownTimer <= vfxType.cooldown)
            {
                vfxType.cooldownTimer += Time.deltaTime;
            }
        }
    }

    public void PlaySFX(SFXType vfxType)
    {
        for (int i = 0; i < vfxTypes.Length; i++)
        {
            if (vfxTypes[i].type == vfxType && vfxTypes[i].cooldownTimer > vfxTypes[i].cooldown)
            {
                vfxTypes[i].source.Play();
                vfxTypes[i].cooldownTimer = 0f;
            }
        }
    }

    public void EditSound()
    {
        musicSource.volume = originalMusicVolume * PersistentManager.Instance.musicVolume;
        foreach (var sfx in vfxTypes)
        {
            sfx.source.volume = sfx.originalVolume * PersistentManager.Instance.sfxVolume;
        }
    }
}
