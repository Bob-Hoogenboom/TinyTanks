using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankBrain : NetworkBehaviour
{
    [SyncVar] private CrewSeat driver;
    [SyncVar] private CrewSeat gunner;

    [Header("Driver controlls")]
    private Rigidbody rb;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float turnSpeed = 90f;

    [Header("Gunner Controlls")]
    [SerializeField] private float turretYawSpeed = 1f;
    [SerializeField] private float turretPitchSpeed = 1f;
    [SerializeField] private float minPitch = -10;
    [SerializeField] private float maxPitch = 30;

    [SyncVar(hook = nameof(OnYawChanged))] private float _yaw;
    [SyncVar(hook = nameof(OnPitchChanged))] private float _pitch;

    [Header("Turret Parts")]
    [SerializeField] private Transform turretYawPivot; // Y rotation
    [SerializeField] private Transform turretPitchPivot; // X rotation
    [SerializeField] private Transform muzzle; // shell spawn

    [Header("Firing")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellSpeed = 10f;
    [SerializeField] private float fireCooldown = 5f;
    private float _nextFireTime;

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Server] public void RegisterSeat(CrewSeat s)
    {
        if (s.seatType == SeatType.Driver) driver = s;
        else if (s.seatType == SeatType.Gunner) gunner = s;

        Debug.Log(gunner.name);
    }

    [Server] public void Server_SetGunnerAim(CrewSeat from, float yawDelta, float pitchDelta)
    {
        if (from != gunner) return;

        _yaw = Mathf.Repeat(_yaw + yawDelta, 360f);
        _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);

        // apply on server immediately
        if (turretYawPivot) turretYawPivot.localRotation = Quaternion.Euler(0f, _yaw, 0f);
        if (turretPitchPivot) turretPitchPivot.localRotation = Quaternion.Euler(0f, 0f, _pitch);
    }

    private void OnYawChanged(float _, float newYaw)
    {
        if (turretYawPivot) turretYawPivot.localRotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    private void OnPitchChanged(float _, float newPitch)
    {
        if (turretPitchPivot) turretPitchPivot.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
    }

    [Server] public void Server_SetDriverInput(CrewSeat from, float throttle, float steer)
    {
        if (from != driver) return;

        Debug.Log("im driver");

        Vector3 fwd = transform.forward * (throttle * moveSpeed * Time.fixedDeltaTime);
        Quaternion turn = Quaternion.Euler(0f, steer * turnSpeed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(rb.position + fwd);
        rb.MoveRotation(rb.rotation * turn);
    }
}
