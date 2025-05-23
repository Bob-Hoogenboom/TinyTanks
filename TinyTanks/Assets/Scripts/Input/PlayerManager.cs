using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [System.Serializable]
    public class RoleAssignmentEvent : UnityEvent<int, int> { } // playerIndex, roleIndex

    [Header("Role Configuration")]
    public string[] roleNames = { "Driver1", "Gunner1", "Driver2", "Gunner2" };
    public int[] maxPlayersPerRole = { 1, 1, 1, 1 };

    [Header("Input Configuratiom")]
    public InputActionAsset inputActions;

    [Header("Events")]
    public RoleAssignmentEvent onPlayerAssignedToRole;

    private List<PlayerInput> _connectedPlayers = new List<PlayerInput>();
    private Dictionary<int, int> _playerToRoleMap = new Dictionary<int, int>();
    private Dictionary<int, int> _roleToPlayerMap = new Dictionary<int, int>();

    //[Header("UI References")]
    //public GameObject playerInputPrefab;
    //public Transform playerContainer;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        InitializeRoleMaps();

        var playerInputManger = GetComponent<PlayerInputManager>();

        playerInputManger.EnableJoining();
        playerInputManger.onPlayerJoined += OnPlayerJoined;
        playerInputManger.onPlayerLeft += OnPlayerLeft;
    }

    void InitializeRoleMaps()
    {
        for (int i = 0; i < roleNames.Length; i++)
        {
            _roleToPlayerMap[i] = -1; // -1 means unassigned
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;
        _connectedPlayers.Add(playerInput);

        // Set up input actions for role selection
        SetupPlayerInputActions(playerInput);

        Debug.Log($"Player {playerIndex} joined with device: {playerInput.devices[0].displayName}");
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;
        _connectedPlayers.Remove(playerInput);

        // Remove from role assignments
        if (_playerToRoleMap.ContainsKey(playerIndex))
        {
            int roleIndex = _playerToRoleMap[playerIndex];
            _roleToPlayerMap[roleIndex] = -1; // Mark role as unassigned
            _playerToRoleMap.Remove(playerIndex);
        }

        Debug.Log($"Player {playerIndex} left");
    }

    public void SetupPlayerInputActions(PlayerInput playerInput)
    {
        var actions = playerInput.actions;

        // Bind role selection buttons
        actions.FindAction("SelectRole1").performed += ctx => AssignPlayerToRole(playerInput.playerIndex, 0);
        actions.FindAction("SelectRole2").performed += ctx => AssignPlayerToRole(playerInput.playerIndex, 1);
        actions.FindAction("SelectRole3").performed += ctx => AssignPlayerToRole(playerInput.playerIndex, 2);
        actions.FindAction("SelectRole4").performed += ctx => AssignPlayerToRole(playerInput.playerIndex, 3);

        // Alternative: Single button cycling through roles
        //actions.FindAction("CycleRole").performed += ctx => CyclePlayerRole(playerInput.playerIndex);
    }

    public void AssignPlayerToRole(int playerIndex, int roleIndex)
    {
        // Validate inputs
        if (roleIndex >= roleNames.Length || roleIndex < 0)
        {
            Debug.LogWarning($"Invalid role index: {roleIndex}");
            return;
        }

        // Check if role is already taken
        if (_roleToPlayerMap[roleIndex] != -1)
        {
            Debug.Log($"Role {roleNames[roleIndex]} is already taken by Player {_roleToPlayerMap[roleIndex]}!");
            return;
        }

        // Remove player from current role if assigned
        if (_playerToRoleMap.ContainsKey(playerIndex))
        {
            int currentRole = _playerToRoleMap[playerIndex];
            _roleToPlayerMap[currentRole] = -1; // Mark old role as unassigned
        }

        // Assign to new role
        _playerToRoleMap[playerIndex] = roleIndex;
        _roleToPlayerMap[roleIndex] = playerIndex;

        Debug.Log($"Player {playerIndex} assigned to role: {roleNames[roleIndex]}");

        // Trigger event
        onPlayerAssignedToRole?.Invoke(playerIndex, roleIndex);

        // Update UI or other systems
        UpdateRoleDisplay();
    }

    public int GetPlayerIndexInRole(int roleIndex)
    {
        return _roleToPlayerMap[roleIndex];
    }

    public GameObject GetPlayerInRole(int roleIndex)
    {
        int playerIndex = GetPlayerIndexInRole(roleIndex);
        if (playerIndex != -1)
        {
            // Find the PlayerInput component with matching player index
            PlayerInput playerInput = _connectedPlayers.Find(p => p.playerIndex == playerIndex);
            if (playerInput != null)
            {
                return playerInput.gameObject;
            }
        }
        return null; // Role unassigned or player not found
    }

    private void UpdateRoleDisplay()
    {
        // Update UI to show current assignments
        for (int roleIndex = 0; roleIndex < roleNames.Length; roleIndex++)
        {
            int assignedPlayer = _roleToPlayerMap[roleIndex];
            if (assignedPlayer != -1)
            {
                Debug.Log($"{roleNames[roleIndex]}: Player {assignedPlayer}");
            }
            else
            {
                Debug.Log($"{roleNames[roleIndex]}: [Unassigned]");
            }
        }
    }
}
