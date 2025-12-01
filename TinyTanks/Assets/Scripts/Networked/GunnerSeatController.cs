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
    [SerializeField] private Vector3 missileCamOffset;

    private void Awake()
    {
        if (!seat) seat = GetComponent<CrewSeat>();
    }
    public override void OnStartLocalPlayer()
    {
        CameraInit();

        if (seatCam) seatCam.gameObject.SetActive(true);
        if (canvas) canvas.gameObject.SetActive(true);

        seat.tank.OnMissileShoot.AddListener(InitializeMissileCam);
        seat.tank.OnMissileDestroy.AddListener(DeInitializeMissileCam);

        Debug.Log($"Adding missile listeners on {seat.tank.name} " +
          $"isServer={seat.tank.isServer} isClient={seat.tank.isClient} " +
          $"isLocalPlayer={seat.tank.isLocalPlayer}");
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float pitch = Input.GetAxisRaw("Vertical");
        float yaw = Input.GetAxisRaw("Horizontal");

        CmdGunnerInput(yaw, pitch);
        if (Input.GetKeyDown(KeyCode.R))
            CmdGunnerReload();

        if (Input.GetKeyDown(KeyCode.Space))
            CmdGunnerShooting();

        if (Input.GetKeyDown(KeyCode.M))
            CmdGunnerPlaceMine();
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

    [Command]
    private void CmdGunnerPlaceMine()
    {
        if (!seat || !seat.tank) return;
        seat.tank.Server_PlaceMine(seat);
    }

    [Client]
    public void InitializeMissileCam()
    {
        if (!seat) return;
        var t = seat.tank;
        if (!t) return;
        var m = t.missile;
        if (!m)
        {
            Debug.LogWarning("InitializeMissileCam: missile not set on client yet");
            return;
        }

        Debug.Log("set To MissileCam position");

        seatCam.transform.parent = m.camAnchor.transform;
        seatCam.transform.localPosition = new Vector3(0, 0, 0);
    }

    [Client]
    public void DeInitializeMissileCam()
    {
        if (!seat) return;
        var t = seat.tank;
        if (!t) return;

        Debug.Log("set MissileCam position");
        seatCam.transform.parent = t.TurretYawPivot;
        seatCam.transform.localRotation = Quaternion.Euler(0, 0, 0);
        seatCam.transform.localPosition = t.GunnerCameraOffset;        
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
