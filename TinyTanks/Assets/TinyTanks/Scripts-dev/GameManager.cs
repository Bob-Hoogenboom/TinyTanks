using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    [SerializeField] private SharedTank tankPrefab;
    [SerializeField] private NetworkObject playerCameraPrefab;

    // Track the tank and player assignments
    private SharedTank spawnedTank;
    private Dictionary<PlayerRef, PlayerRoleType> playerRoles = new Dictionary<PlayerRef, PlayerRoleType>();

    public enum PlayerRoleType
    {
        None,
        Driver,
        Gunner
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Called when a player joins
    public void HandlePlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Spawn player's camera controller (follows the tank)
        var playerCamera = runner.Spawn(playerCameraPrefab, Vector3.zero, Quaternion.identity, player);

        // On server, handle tank spawning and role assignment
        if (runner.IsServer)
        {
            // Create tank if it doesn't exist
            if (spawnedTank == null)
            {
                spawnedTank = runner.Spawn(tankPrefab, new Vector3(0, 1, 0), Quaternion.identity);
            }

            // Assign a role based on availability
            AssignRoleToPlayer(runner, player);

            // Tell the player camera what to follow
            //playerCamera.GetComponent<PlayerCameraController>().RPC_SetTargetTank(player, spawnedTank.Object);
        }
    }

    // Assign a role to the player
    private void AssignRoleToPlayer(NetworkRunner runner, PlayerRef player)
    {
        // Check available roles
        bool hasDriver = false;
        bool hasGunner = false;

        foreach (var role in playerRoles.Values)
        {
            if (role == PlayerRoleType.Driver) hasDriver = true;
            if (role == PlayerRoleType.Gunner) hasGunner = true;
        }

        // Assign first available role
        if (!hasDriver)
        {
            playerRoles[player] = PlayerRoleType.Driver;
            spawnedTank.RPC_AssignRole(player, true);
            Debug.Log($"Player {player} assigned as Driver");
        }
        else if (!hasGunner)
        {
            playerRoles[player] = PlayerRoleType.Gunner;
            spawnedTank.RPC_AssignRole(player, false);
            Debug.Log($"Player {player} assigned as Gunner");
        }
        else
        {
            // No roles available - could implement spectator mode
            playerRoles[player] = PlayerRoleType.None;
            Debug.Log($"Player {player} has no role (spectator)");
        }
    }

    // Handle player leaving
    public void HandlePlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Check if player had a role and make it available again
        if (playerRoles.TryGetValue(player, out PlayerRoleType role))
        {
            Debug.Log($"Player {player} with role {role} left");
            playerRoles.Remove(player);

            // Notify tank that player left
            if (spawnedTank != null)
            {
                if (role == PlayerRoleType.Driver)
                    spawnedTank.RPC_PlayerLeftRole(true);
                else if (role == PlayerRoleType.Gunner)
                    spawnedTank.RPC_PlayerLeftRole(false);
            }
        }
    }
}