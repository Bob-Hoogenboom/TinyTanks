using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GunnerSeatController : NetworkBehaviour
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

        float pitch = Input.GetAxisRaw("Vertical");
        float yaw = Input.GetAxisRaw("Horizontal");

        CmdGunnerInput(yaw, pitch);

        if (Input.GetMouseButtonDown(0))
            CmdGunnerShooting();
            
    }

    [Command]
    private void CmdGunnerInput(float yaw, float pitch)
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetGunnerAim(seat, yaw, pitch);
    }

    [Command]
    private void CmdGunnerShooting()
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetOffGun(seat);
    }
}
