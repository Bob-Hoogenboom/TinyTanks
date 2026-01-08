using UnityEngine;
using Mirror;
using System;

public class TankSelector : NetworkBehaviour
{

    public static TankSelector Instance { get; private set; }
    public static event Action<TankSelector> InstanceChanged;
    public enum TankType : byte { Heavy = 0, Medium = 1, Light = 2 }

    private RoleVisuals _visuals;

    [Header("Prefabs")]
    [SerializeField] private GameObject heavyTankPrefab;
    [SerializeField] private GameObject mediumTankPrefab;
    [SerializeField] private GameObject lightTankPrefab;

    [SyncVar(hook = nameof(OnTeam1TypeChanged))] private TankType _team1Type = TankType.Light;
    [SyncVar(hook = nameof(OnTeam2TypeChanged))] private TankType _team2Type = TankType.Light;

    public TankType Team1Type => _team1Type;
    public TankType Team2Type => _team2Type;

    public event Action OnTankUpdated;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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
        base.OnStartServer();
        _team1Type = TankType.Light;
        _team2Type = TankType.Light;
        _visuals = FindObjectOfType<RoleVisuals>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _visuals = FindObjectOfType<RoleVisuals>();
        Client_ApplyAllTankTypeVisuals();
    }

    public override void OnStopClient()
    {
        Destroy(this);
        base.OnStopClient();
        CleanupSingleton();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        CleanupSingleton();
    }

    private void CleanupSingleton()
    {
        _visuals = null;

        if (Instance == this)
        {
            Instance = null;
            InstanceChanged?.Invoke(null);
        }

        // If you keep TankSelector as a scene object, destroying here prevents “old instance blocks new instance”
        Destroy(gameObject);
    }

    private bool TryResolveVisuals()
    {
        if (_visuals != null) return true;
        _visuals = FindObjectOfType<RoleVisuals>();
        return _visuals != null;
    }

    #region UI (client)
    [Client] public void UI_SelectHeavy() => UI_SelectType(TankType.Heavy);
    [Client] public void UI_SelectMedium() => UI_SelectType(TankType.Medium);
    [Client] public void UI_SelectLight() => UI_SelectType(TankType.Light);

    [Client]
    public void UI_SelectType(TankType type)
    {
        var me = MyRoomPlayer.Local;

        if (me == null && NetworkClient.localPlayer != null)
            me = NetworkClient.localPlayer.GetComponent<MyRoomPlayer>();

        if (me == null)
            return;

        if (me.roleIndex < 0)
        {
            Debug.Log("[TankSelector] Pick a role first, then select tank.");
            return;
        }

        CmdSelectTypeForRole(type, me.roleIndex);
    }

    [Client]
    public bool UI_IsTypeSelectedForMyTeam(TankType type)
    {
        var me = MyRoomPlayer.Local;
        if (me == null || me.roleIndex < 0) return false;
        int team = TeamFromRoleIndex(me.roleIndex);
        return (team == 1 ? _team1Type : _team2Type) == type;
    }

    [Client]
    public void Client_ApplyAllTankTypeVisuals()
    {
        if (!TryResolveVisuals()) return;

        // Apply absolute state (important for late join / scene load)
        _visuals.ApplyTeamType(0, _team1Type);
        _visuals.ApplyTeamType(1, _team2Type);
    }
    #endregion

    #region Server commands

    [Command(requiresAuthority = false)]
    private void CmdSelectTypeForRole(TankType type, int roleIndex, NetworkConnectionToClient sender = null)
    {
        if (!ValidateRoleOwnership(sender, roleIndex))
            return;

        int team = TeamFromRoleIndex(roleIndex);
        if (team == 1)
            _team1Type = type;
        else if (team == 2)
            _team2Type = type;
    }

    private bool ValidateRoleOwnership(NetworkConnectionToClient sender, int roleIndex)
    {
        if (sender == null || sender.identity == null)
        {
            Debug.LogWarning("[TankSelector] Sender has no identity.");
            return false;
        }
        if (RoleManager.Instance == null)
        {
            Debug.LogWarning("[TankSelector] No RoleManager available.");
            return false;
        }
        if (!RoleManager.Instance.roleOwners.TryGetValue(roleIndex, out uint ownerNetId))
        {
            Debug.LogWarning("[TankSelector] Invalid role index.");
            return false;
        }

        if (ownerNetId == 0 || ownerNetId != sender.identity.netId)
        {
            Debug.LogWarning("[TankSelector] Denied: you do not own this role.");
            return false;
        }

        return true;
    }

    [Server]
    public GameObject GetSelectedTankPrefabForRole(int roleIndex)
    {
        int team = TeamFromRoleIndex(roleIndex);
        return PrefabFor(team == 1 ? _team1Type : _team2Type);
    }

    [Server]
    public GameObject GetSelectedTankPrefabForPlayer(NetworkIdentity playerIdentity)
    {
        if (playerIdentity == null || RoleManager.Instance == null) return null;

        // Find the player’s role by scanning RoleManager.roleOwners. :contentReference[oaicite:4]{index=4}
        int roleIndex = -1;
        foreach (var kv in RoleManager.Instance.roleOwners)
            if (kv.Value == playerIdentity.netId) { roleIndex = kv.Key; break; }

        if (roleIndex == -1) return null;
        return GetSelectedTankPrefabForRole(roleIndex);
    }

    [Server]
    public GameObject Server_SpawnSelectedTankFor(NetworkConnectionToClient conn, Vector3 pos, Quaternion rot)
    {
        if (conn?.identity == null) return null;
        var prefab = GetSelectedTankPrefabForPlayer(conn.identity);
        if (prefab == null)
        {
            Debug.LogWarning("[TankSelector] No prefab resolved; defaulting to light.");
            prefab = lightTankPrefab;
        }

        var go = Instantiate(prefab, pos, rot);
        NetworkServer.Spawn(go, conn);
        return go;
    }
    #endregion

    #region SyncVar hooks & helpers
    private void OnTeam1TypeChanged(TankType oldVal, TankType newVal)
    {
        if (TryResolveVisuals())
            _visuals.ApplyTeamType(0, newVal);

        OnTankUpdated?.Invoke();
    }

    private void OnTeam2TypeChanged(TankType oldVal, TankType newVal)
    {
        if (TryResolveVisuals())
            _visuals.ApplyTeamType(1, newVal);

        OnTankUpdated?.Invoke();
    }

    private static int TeamFromRoleIndex(int roleIndex)
    {
        // Roles 0–1 = Team 1, Roles 2–3 = Team 2 (matches your existing room logic). :contentReference[oaicite:5]{index=5}
        if (roleIndex == 0 || roleIndex == 1) return 1;
        if (roleIndex == 2 || roleIndex == 3) return 2;
        return 0;
    }

    private GameObject PrefabFor(TankType type)
    {
        switch (type)
        {
            case TankType.Heavy: return heavyTankPrefab;
            case TankType.Medium: return mediumTankPrefab;
            case TankType.Light: return lightTankPrefab;
            default: return mediumTankPrefab;
        }
    }
    #endregion
}
