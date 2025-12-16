using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum SeatType { Driver, Gunner}

public class CrewSeat : NetworkBehaviour
{
    public int roleKey;
    public SeatType seatType;

    [SyncVar(hook = nameof(OnTankChanged))] private GameObject tankObject;
    public TankBrain tank => tankObject ? tankObject.GetComponent<TankBrain>() : null;
    [SyncVar] public bool taken;

    public override void OnStartServer()
    {
        if (tank != null) tank.Server_RegisterSeat(this); // assign seat to tank
    }

    [Server]
    public void Server_AssignTank(TankBrain t)
    {
        if (!t) return;
        tankObject = t.gameObject; // SyncVar replication to clients
        t.Server_RegisterSeat(this); // keeps TankBrain’s driver/gunner pointers updated

        Debug.Log($"[Server] {seatType} seat {netIdentity.netId} assigned to tank {t.netIdentity.netId}");
    }
    private void OnTankChanged(GameObject _, GameObject __)
    {
        // no-op; seat controllers poll in FixedUpdate and will now see seat.tank on clients
    }
}
