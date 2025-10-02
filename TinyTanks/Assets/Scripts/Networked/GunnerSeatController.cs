using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GunnerSeatController : NetworkBehaviour
{
    [SerializeField] CrewSeat seat;
    [SerializeField] private Camera seatCam;
    [SerializeField] private Canvas canvas;

    public override void OnStartLocalPlayer()
    {
        if (seatCam) seatCam.gameObject.SetActive(true); // Set camera
        if (canvas) canvas.gameObject.SetActive(true); //Set Canvas
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        float pitch = Input.GetAxisRaw("Vertical");
        float yaw = Input.GetAxisRaw("Horizontal");

        CmdGunnerInput(yaw, pitch);

        if (Input.GetKeyDown(KeyCode.R))
            CmdGunnerReload();

        if (Input.GetKeyDown(KeyCode.Space))
            CmdGunnerShooting();
    }

    [Command]
    private void CmdGunnerInput(float yaw, float pitch)
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetGunnerInput(seat, yaw, pitch);
    }

    [Command]
    private void CmdGunnerReload()
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_ReloadGun(seat);
    }

    [Command]
    private void CmdGunnerShooting()
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetOffGun(seat);
    }
}
