using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SeatInit : NetworkBehaviour
{
    [SerializeField] CrewSeat seat;

    public override void OnStartServer()
    {
        if (seat && seat.tank) seat.tank.RegisterSeat(seat);
    }
}
