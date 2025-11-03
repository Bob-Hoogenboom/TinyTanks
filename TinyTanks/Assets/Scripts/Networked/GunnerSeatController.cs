using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GunnerSeatController : NetworkBehaviour
{
    [SerializeField] private CrewSeat seat;
    [SerializeField] private Camera seatCam;
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        if (!seat) seat = GetComponent<CrewSeat>();
    }
    public override void OnStartLocalPlayer()
    {
        CameraInit();

        if (seatCam) seatCam.gameObject.SetActive(true);
        if (canvas) canvas.gameObject.SetActive(true);
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

    [Client]
    private void CameraInit()
    {
        if (!seat) return;
        var t = seat.tank;
        if (!t) return;

        foreach (var cam in t.GetComponentsInChildren<Camera>(true))
            if (cam.CompareTag("gunnerCam")) { seatCam = cam; break; }

        foreach (var can in t.GetComponentsInChildren<Canvas>(true))
            if (can.CompareTag("gunnerCanvas")) { canvas = can; break; }
    }
}
