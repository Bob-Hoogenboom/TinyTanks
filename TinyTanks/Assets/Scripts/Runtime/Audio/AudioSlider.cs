using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public string audioGroup = new string("MasterVolume");

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider slider;

    public void Start()
    {
        float dB;
        audioMixer.GetFloat(audioGroup, out dB);

        float linearValue = Mathf.Pow(10f, dB / 20f);
        slider.value = linearValue;

        slider.onValueChanged.AddListener(AudioAdd);
    }


    public void AudioAdd(float value)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(AudioAdd);
    }
}
