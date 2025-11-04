using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetMatchTimer : MonoBehaviour
{
    [Header("UI References")]
    public Button buttonLeft;
    public Button buttonRight;
    public TMP_InputField timeInput; // Single field for both presets + custom

    [Header("Preset Times (in seconds)")]
    private readonly int[] presetTimes = { 180, 300, 600, 900 }; // 3, 5, 10, 15 minutes
    private int currentIndex = 1; // Default = 5 minutes (index 1)

    private bool isCustom = false;
    public float selectedTime { get; private set; } // in seconds

    private void Start()
    {
        buttonLeft.onClick.AddListener(PreviousTime);
        buttonRight.onClick.AddListener(NextTime);
        timeInput.onValueChanged.AddListener(OnCustomTimeChanged);

        UpdateDisplay();
    }

    private void PreviousTime()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = 4; // wrap to "Custom"
        UpdateDisplay();
    }

    private void NextTime()
    {
        currentIndex++;
        if (currentIndex > 4)
            currentIndex = 0; // wrap to start
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentIndex < 4)
        {
            isCustom = false;
            selectedTime = presetTimes[currentIndex];
            Debug.Log(selectedTime);
            timeInput.text = FormatTime(selectedTime);
            Debug.Log(timeInput.text);
            timeInput.interactable = false; // disable typing
            Debug.Log("Klaar");
        }
        else
        {
            isCustom = true;
            timeInput.interactable = true; // enable manual input
            timeInput.text = ""; // blank until player types
        }

        timeInput.ForceLabelUpdate();
    }

    private void OnCustomTimeChanged(string input)
    {
        if (!isCustom) return; // ignore typing when on preset

        if (TryParseTime(input, out float seconds))
        {
            selectedTime = seconds;
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes}:{secs:00}";
    }

    private bool TryParseTime(string input, out float seconds)
    {
        seconds = 0f;
        string[] parts = input.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int minutes) &&
            int.TryParse(parts[1], out int secs))
        {
            seconds = minutes * 60 + secs;
            return true;
        }
        return false;
    }

    public float GetSelectedTime() => selectedTime;
}
