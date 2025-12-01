using System;
using UnityEngine;
using Mirror;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class NetworkGameTimer : NetworkBehaviour
{
    [SyncVar] private double _endTime;
    [SerializeField] private string timerTag = "gameTimer";
    [SerializeField] private List<TMP_Text> timerTexts = new List<TMP_Text>();
    [SerializeField] private float gameTime;

    [ClientCallback]
    private void Start()
    {
        TryBindTimerTexts();
        InvokeRepeating(nameof(TryBindTimerTexts), 0.25f, 0.25f);
    }

    [Server]
    public void Server_Initialize(float durationSeconds)
    {
        _endTime = NetworkTime.time + durationSeconds;
    }

    private void Update()
    {
        double remaining = _endTime - NetworkTime.time;
        UpdateTimerDisplay(remaining);

        if (isServer && remaining <= 0) ReturnToLobby();
    }

    [Client]
    private void TryBindTimerTexts()
    {
        if (timerTexts == null) timerTexts = new List<TMP_Text>();

        // collect from all objects tagged "gameTimer"
        var roots = GameObject.FindGameObjectsWithTag(timerTag);
        foreach (var go in roots)
        {
            // true => include inactive children (important when canvases are disabled initially)
            var txt = go.GetComponentInChildren<TMP_Text>(true);
            if (txt != null && !timerTexts.Contains(txt))
                timerTexts.Add(txt);
        }

        // stop retrying once we have at least one valid target
        if (timerTexts.Count > 0 && timerTexts.TrueForAll(t => t != null))
            CancelInvoke(nameof(TryBindTimerTexts));
    }

    private void UpdateTimerDisplay(double timeRemaining)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, timeRemaining));
        foreach (var text in timerTexts)
            text.text = $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
    }

    [Server] 
    private void ReturnToLobby()
    {
        var nm = NetworkManager.singleton;
        if (nm == null) return;

        if (NetworkServer.active)
        {
            nm.StopHost();
        }
    }
}
