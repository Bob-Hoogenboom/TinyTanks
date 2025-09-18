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

        float leftTrack = Input.GetAxisRaw("Vertical");
        float rightTrack = Input.GetAxisRaw("Vertical2");
        CmdDriverInput(leftTrack, rightTrack);

        if (Input.GetKeyDown(KeyCode.Space))
            CmdDriverShooting();
    }

    [Command]
    private void CmdDriverInput(float leftTrack, float rightTrack)
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetDriverInput(seat, leftTrack, rightTrack);
    }

    [Command]
    private void CmdDriverShooting()
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetOffGun(seat, NetworkTime.time);
    }
}
