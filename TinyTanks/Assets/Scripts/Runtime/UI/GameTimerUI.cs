using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void OnEnable()
    {
        LevelManager.OnTimerUpdate += UpdateTimerDisplay;
    }

    private void OnDisable()
    {
        LevelManager.OnTimerUpdate -= UpdateTimerDisplay;
    }

    private void UpdateTimerDisplay(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"{minutes}:{seconds}";
    }
}
