using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class DriverSeatController : NetworkBehaviour
{
    [SerializeField] CrewSeat seat;
    [SerializeField] private Camera seatCam;

    public override void OnStartLocalPlayer()
    {
        if (seatCam) seatCam.gameObject.SetActive(true); // Set camera
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float throttle = Input.GetAxisRaw("Vertical");
        float steer = Input.GetAxisRaw("Horizontal");
        CmdDriverInput(throttle, steer);
    }

    [Command]
    private void CmdDriverInput(float throttle, float steer)
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetDriverInput(seat, throttle, steer);
    }
}
