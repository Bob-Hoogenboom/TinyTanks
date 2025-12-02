using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    public const string MASTER_VOLUME = "MasterVolume";
    public const string SFX_VOLUME = "SoundFXVolume";
    public const string MUSIC_VOLUME = "MusicVolume";

    [SerializeField] private AudioMixer audioMixer;


    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat(MASTER_VOLUME, Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat(SFX_VOLUME, Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat(MUSIC_VOLUME, Mathf.Log10(level) * 20f);
    }
}
