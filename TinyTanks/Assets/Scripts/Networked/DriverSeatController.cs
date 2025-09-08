using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class DriverSeatController : NetworkBehaviour
{
    [SerializeField] CrewSeat seat;
    Camera seatCam;

    void Awake() => seatCam = GetComponentInChildren<Camera>(true);

    public override void OnStartLocalPlayer()
    {
        Debug.Log($"[DriverSeat] I am local player: netId={netId}");
        if (seatCam) seatCam.gameObject.SetActive(true);   // enable seat camera for local user
        enabled = true;                                    // in case you keep controllers disabled by default
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
