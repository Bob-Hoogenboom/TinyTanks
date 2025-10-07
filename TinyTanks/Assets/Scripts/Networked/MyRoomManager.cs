using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyRoomManager : NetworkRoomManager
{
    readonly Dictionary<int, CrewSeat> seatsByKey = new();
    private readonly Dictionary<int, TankBrain> _teamTank = new();

    public override void Awake()
    {
        playerSpawnMethod = PlayerSpawnMethod.Random;
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);
        seatsByKey.Clear();
        foreach (var seats in FindObjectsOfType<CrewSeat>(true))
            seatsByKey[seats.roleKey] = seats;

        _teamTank.Clear();
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        var rp = roomPlayer.GetComponent<MyRoomPlayer>();
        if (rp != null && seatsByKey.TryGetValue(rp.roleIndex, out var seat) && seat && !seat.taken)
        {
            int team = TeamFromRoleIndex(rp.roleIndex);
            EnsureTeamTankSpawned(team, rp.roleIndex);
            AssignTeamTankToAllSeats(team);

            seat.taken = true;
            Debug.Log($"Assigning seat {seat.roleKey} as player object for conn {conn.connectionId}");
            return seat.gameObject; // this becomes the player object; client gets isLocalPlayer on it
        }

        Debug.LogWarning($"No seat for {rp?.roleIndex}, falling back to default player prefab.");
        return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
    }

    private static int TeamFromRoleIndex(int roleIndex)
    {
        // Roles 0–1 => Team 1, Roles 2–3 => Team 2 (matches your existing room logic) :contentReference[oaicite:3]{index=3}
        if (roleIndex == 0 || roleIndex == 1) return 1;
        if (roleIndex == 2 || roleIndex == 3) return 2;
        return 0;
    }

    [Server]
    private void EnsureTeamTankSpawned(int team, int roleIndexForPrefabLookup)
    {
        if (team <= 0) return;
        if (_teamTank.ContainsKey(team)) return;

        // Decide spawn transform (use start positions in scene)
        Transform start = GetStartPosition();
        Vector3 pos = start ? start.position : Vector3.zero;
        Quaternion rot = start ? start.rotation : Quaternion.identity;

        // Resolve prefab via TankSelector (per-team selection)
        if (TankSelector.Instance == null)
        {
            Debug.LogError("[MyRoomManager] No TankSelector present in scene; cannot spawn team tank.");
            return;
        }

        GameObject prefab = TankSelector.Instance.GetSelectedTankPrefabForRole(roleIndexForPrefabLookup); // server-side helper
        if (prefab == null)
        {
            Debug.LogError("[MyRoomManager] Tank prefab not resolved; aborting spawn.");
            return;
        }

        var go = Instantiate(prefab, pos, rot);
        NetworkServer.Spawn(go); // no specific client authority; both seats command via their own player objects
        var tb = go.GetComponent<TankBrain>();
        if (tb == null)
        {
            Debug.LogError("[MyRoomManager] Spawned tank has no TankBrain component.");
            return;
        }

        _teamTank[team] = tb;
        Debug.Log($"[MyRoomManager] Spawned tank for Team {team} at {pos}.");
    }

    [Server]
    private void AssignTeamTankToAllSeats(int team)
    {
        if (!_teamTank.TryGetValue(team, out var tb) || tb == null) return;

        foreach (var kv in seatsByKey)
        {
            int roleKey = kv.Key;
            CrewSeat seat = kv.Value;
            if (seat == null) continue;

            if (TeamFromRoleIndex(roleKey) == team)
            {
                // set seat.tank and register on TankBrain
                seat.Server_AssignTank(tb);
            }
        }
    }
}
