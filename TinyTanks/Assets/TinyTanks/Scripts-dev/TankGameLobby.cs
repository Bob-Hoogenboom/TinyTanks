using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private TMP_InputField sessionNameInput;

    private NetworkRunner _runner;

    // Called when the host button is clicked in the UI
    public void OnHostButtonClicked()
    {
        if (sessionNameInput == null)
            sessionNameInput = FindFirstObjectByType<TMP_InputField>();

        if (connectingPanel != null)
            connectingPanel.SetActive(true);

        if (lobbyUI != null)
            lobbyUI.SetActive(false);

        // Get session name from input field or use default
        string sessionName = string.IsNullOrEmpty(sessionNameInput.text)
            ? "TankCoopGame" + UnityEngine.Random.Range(0, 10000)
            : sessionNameInput.text;

        StartGameInMode(GameMode.Host, sessionName);
    }

    // Called when the join button is clicked in the UI
    public void OnJoinButtonClicked()
    {
        if (sessionNameInput == null)
            sessionNameInput = FindFirstObjectByType<TMP_InputField>();

        if (connectingPanel != null)
            connectingPanel.SetActive(true);

        if (lobbyUI != null)
            lobbyUI.SetActive(false);

        // Must have a session name to join
        if (string.IsNullOrEmpty(sessionNameInput.text))
        {
            Debug.LogError("Session name is required to join a game");
            connectingPanel.SetActive(false);
            lobbyUI.SetActive(true);
            return;
        }

        StartGameInMode(GameMode.Client, sessionNameInput.text);
    }

    private async void StartGameInMode(GameMode mode, string sessionName)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Add the INetworkRunnerCallbacks handler
        _runner.AddCallbacks(this);

        // Start the game
        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        // Start or join the game
        var result = await _runner.StartGame(startGameArgs);

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            connectingPanel.SetActive(false);
            lobbyUI.SetActive(true);
        }
    }

    // INetworkRunnerCallbacks implementation
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} joined");

        // If we're the server, we should handle spawning the tank when players join
        if (runner.IsServer)
        {
            // Find or spawn the tank on the server
            GameManager.Instance.HandlePlayerJoined(runner, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left");

        // Handle player leaving
        if (runner.IsServer)
        {
            GameManager.Instance.HandlePlayerLeft(runner, player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Find the tank
        SharedTank tank = FindObjectOfType<SharedTank>();
        if (tank == null)
            return;

        // Find the driver and gunner controllers
        TankDriver driver = tank.GetComponent<TankDriver>();
        TankGunner gunner = tank.GetComponent<TankGunner>();

        // Process input for both components
        // We first process driver then gunner - order matters because both will use the same NetworkInput
        if (driver != null)
            driver.ProcessInput(runner, input);

        if (gunner != null)
            gunner.ProcessInput(runner, input);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
}