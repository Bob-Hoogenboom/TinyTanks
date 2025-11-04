using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class DriverSeatController : NetworkBehaviour
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
    private void Update()
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
        seat.tank.Server_SetOffGun(seat);
    }

    [Client]
    private void CameraInit()
    {
        if (!seat) return;
        var t = seat.tank;
        if (!t) return;

        foreach (var cam in t.GetComponentsInChildren<Camera>(true))
            if (cam.CompareTag("driverCam")) { seatCam = cam; break; }

        foreach (var can in t.GetComponentsInChildren<Canvas>(true))
            if (can.CompareTag("driverCanvas")) { canvas = can; break; }
    }
}
