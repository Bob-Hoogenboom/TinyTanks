using UnityEngine;
using Mirror;
using System;

public class RoleManager : NetworkBehaviour
{

    public static RoleManager Instance { get; private set; }
    public static event Action<RoleManager> InstanceChanged;

    [Header("Config")]
    [SerializeField] private int roleCount = 4;

    public readonly SyncDictionary<int, uint> roleOwners = new SyncDictionary<int, uint>();
    public event Action OnRolesUpdated;


    private void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InstanceChanged?.Invoke(Instance);
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            InstanceChanged?.Invoke(null);
        }
    }

    public override void OnStartServer()
    {
        // initialize roles as free
        for (int i = 0; i < roleCount; i++)
            roleOwners[i] = 0;
    }

    public override void OnStartClient()
    {
        // refresh UI whenever server mutates the dictionary
        roleOwners.OnChange += OnRolesChanged;
        OnRolesUpdated?.Invoke();
    }

    public override void OnStopClient()
    {
        roleOwners.OnChange -= OnRolesChanged;
    }

    void OnRolesChanged(SyncDictionary<int, uint>.Operation op, int key, uint value)
    {
        OnRolesUpdated?.Invoke();
    }

    // ---------- SERVER API ----------
    [Server]
    public bool Server_TryClaimRole(NetworkIdentity requester, int roleIndex, bool allowSwitch = true)
    {
        if (!roleOwners.ContainsKey(roleIndex)) { RPC_TargetDenied(requester.connectionToClient, "Invalid role"); return false; }

        uint currentOwner = roleOwners[roleIndex];
        uint requesterId = requester.netId;

        // Already taken by someone else?
        if (currentOwner != 0 && currentOwner != requesterId)
        {
            RPC_TargetDenied(requester.connectionToClient, "Role already taken");
            return false;
        }

        // Enforce 1 role per player (switch if needed)
        int existing = GetRoleIndexOf(requesterId);
        if (existing != -1 && existing != roleIndex)
        {
            if (!allowSwitch)
            {
                RPC_TargetDenied(requester.connectionToClient, "You already have a role");
                return false;
            }
            roleOwners[existing] = 0;
        }

        roleOwners[roleIndex] = requesterId; // delta-synced to all clients
        var requestingPlayer = requester.GetComponent<MyRoomPlayer>();
        if (requestingPlayer != null) requestingPlayer.Server_SetRole(roleIndex);

        OnRolesUpdated?.Invoke();
        return true;
    }

    [Server]
    public void Server_ReleaseRole(NetworkIdentity requester, int roleIndex)
    {
        if (!roleOwners.TryGetValue(roleIndex, out uint owner)) return;
        if (owner == requester.netId)
        {
            roleOwners[roleIndex] = 0;
            var requestingPlayer = requester.GetComponent<MyRoomPlayer>();
            if (requestingPlayer != null) requestingPlayer.Server_SetRole(-1);

            OnRolesUpdated?.Invoke();
        }      
    }

    [Server]
    public void Server_ReleaseAllFor(NetworkIdentity identity)
    {
        uint id = identity.netId;
        foreach (var keyValue in roleOwners)
            if (keyValue.Value == id) roleOwners[keyValue.Key] = 0;

        var requestingPlayer = identity.GetComponent<MyRoomPlayer>();
        if (requestingPlayer != null) requestingPlayer.Server_SetRole(-1);

        OnRolesUpdated?.Invoke();
    }

    int GetRoleIndexOf(uint netId)
    {
        foreach (var keyValue in roleOwners)
            if (keyValue.Value == netId) return keyValue.Key;
        return -1;
    }

    [TargetRpc]
    void RPC_TargetDenied(NetworkConnectionToClient conn, string msg) =>
        Debug.Log($"[RoleManager] Denied: {msg}");

    // ---------- CLIENT HELPERS ----------
    public bool IsRoleFree(int roleIndex) =>
        roleOwners.TryGetValue(roleIndex, out uint owner) && owner == 0;

    public bool IsRoleMine(int roleIndex)
    {
        if (!roleOwners.TryGetValue(roleIndex, out uint owner) || owner == 0) return false;
        var my = NetworkClient.connection?.identity;
        return my != null && owner == my.netId;
    }

    public string GetOwnerName(int roleIndex)
    {
        if (!roleOwners.TryGetValue(roleIndex, out uint owner) || owner == 0) return "(free)";
        // Look up the owner NetworkIdentity; may be missing briefly during joins. :contentReference[oaicite:2]{index=2}
        return Mirror.NetworkClient.spawned.TryGetValue(owner, out var ni) ? /*ni.name*/ "Player" : "(resolving...)";
    }
}
