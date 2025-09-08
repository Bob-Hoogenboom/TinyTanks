using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class DriverSeatController : NetworkBehaviour
{
    [SerializeField] CrewSeat seat;
    [SerializeField] private Camera seatCam;

    void Awake() => seatCam = GetComponentInChildren<Camera>(true);

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
