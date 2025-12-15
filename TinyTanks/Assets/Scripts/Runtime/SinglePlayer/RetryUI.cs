using UnityEngine;

public class RetryUI : MonoBehaviour
{
    [SerializeField] private GameObject retryPanel;

    private void OnEnable()
    {
        SinglePlayer.Events.OnMatchRestart += HandleRestart;
        SinglePlayer.Events.OnTrackFinished += HandleTrackFinished;
    }

    private void OnDisable()
    {
        SinglePlayer.Events.OnMatchRestart -= HandleRestart;
        SinglePlayer.Events.OnTrackFinished -= HandleTrackFinished;
    }

    private void HandleRestart()
    {
        retryPanel.SetActive(false);
    }

    private void HandleTrackFinished()
    {
        retryPanel.SetActive(true);
    }

    //UnityEvent Methods
    public void RestartButton()
    {
        SinglePlayer.Events.MatchRestart();
    }
}
