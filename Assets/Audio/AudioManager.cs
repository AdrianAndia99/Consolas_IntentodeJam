using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixerSO masterMixerSO;
    [SerializeField] private AudioMixerSO sfxMixerSO;
    [SerializeField] private AudioMixerSO musicMixerSO;

    private void Awake()
    {
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

    private void Start()
    {
        masterMixerSO.EnableSound();
        sfxMixerSO.EnableSound();
        musicMixerSO.EnableSound();
    }

    public void UpdateMasterVolume(float value)
    {
        masterMixerSO.UpdateVolume(value);
    }

    public void UpdateSFXVolume(float value)
    {
        sfxMixerSO.UpdateVolume(value);
    }

    public void UpdateMusicVolume(float value)
    {
        musicMixerSO.UpdateVolume(value);
    }

    public float GetMasterVolume()
    {
        return masterMixerSO.GetCurrentVolumeValue();
    }

    public float GetSFXVolume()
    {
        return sfxMixerSO.GetCurrentVolumeValue();
    }

    public float GetMusicVolume()
    {
        return musicMixerSO.GetCurrentVolumeValue();
    }
}