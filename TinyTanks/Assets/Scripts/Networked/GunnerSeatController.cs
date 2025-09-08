using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GunnerSeatController : NetworkBehaviour
{
    [SerializeField] CrewSeat seat;
    Camera seatCam;

    void Awake() => seatCam = GetComponentInChildren<Camera>(true);

    public override void OnStartLocalPlayer()
    {
        Debug.Log($"[GunnerSeat] I am local player: netId={netId}");
        if (seatCam) seatCam.gameObject.SetActive(true);
        enabled = true;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float pitch = Input.GetAxisRaw("Vertical");
        float yaw = Input.GetAxisRaw("Horizontal");

        CmdGunnerInput(yaw, pitch);
    }

    [Command]
    void CmdGunnerInput(float yaw, float pitch)
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_SetGunnerAim(seat, yaw, pitch);
    }
}
