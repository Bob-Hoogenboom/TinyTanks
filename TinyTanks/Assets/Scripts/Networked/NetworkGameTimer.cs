using System;
using UnityEngine;
using Mirror;
using TMPro;

public class NetworkGameTimer : NetworkBehaviour
{
    [SyncVar] private double endTime;
    [SerializeField] private TMP_Text[] timerTexts;
    [SerializeField] private float gameTime;

    private void Start()
    {
        StarTimer();
    }

    private void Update()
    {
        double remaining = endTime - NetworkTime.time;
        UpdateTimerDisplay(remaining);
    }

    private void StarTimer()
    {
        endTime = NetworkTime.time + gameTime;
    }

    private void UpdateTimerDisplay(double timeRemaining)
    {
        if (timeRemaining <= 0) ReturnToLobby();
        var ts = TimeSpan.FromSeconds(timeRemaining);

        foreach(var text in timerTexts)
            text.text = $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
    }

    [Server] private void ReturnToLobby()
    {
        if(NetworkServer.active)
        {
            var roomMgr = (NetworkRoomManager)NetworkManager.singleton;
            NetworkManager.singleton.ServerChangeScene(roomMgr.RoomScene);
        }
    }
}
