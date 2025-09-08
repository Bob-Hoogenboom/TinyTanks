using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyRoomManager : NetworkRoomManager
{
    readonly Dictionary<int, CrewSeat> seatsByKey = new();
    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);
        seatsByKey.Clear();
        foreach (var seats in FindObjectsOfType<CrewSeat>(true))
            seatsByKey[seats.roleKey] = seats;
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        var rp = roomPlayer.GetComponent<MyRoomPlayer>();
        if (rp != null && seatsByKey.TryGetValue(rp.roleIndex, out var seat) && seat && !seat.taken)
        {
            seat.taken = true;
            Debug.Log($"Assigning seat {seat.roleKey} as player object for conn {conn.connectionId}");
            return seat.gameObject; // this becomes the player object; client gets isLocalPlayer on it
        }

        Debug.LogWarning($"No seat for {rp?.roleIndex}, falling back to default player prefab.");
        return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
    }
}
