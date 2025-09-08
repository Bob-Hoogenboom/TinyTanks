using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum SeatType { Driver, Gunner}

public class CrewSeat : NetworkBehaviour
{
    public int roleKey;
    public SeatType seatType;
    public TankBrain tank;
    [SyncVar] public bool taken;

    public override void OnStartServer()
    {
        if (tank != null) tank.RegisterSeat(this); // assign seat to tank
        else Debug.LogError($"{name} has no TankBrain reference!");
    }
}
