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

    [Header("Physics based movement")]
    [SerializeField] private TankTrackPhysics tracks;
    private float leftTrack;
    private float rightTrack;

    [Header("Gunner Controlls")]
    [SerializeField] private float turretYawSpeed = 1f;
    [SerializeField] private float turretPitchSpeed = 1f;
    [SerializeField] private float minPitch = -10;
    [SerializeField] private float maxPitch = 30;

    [SyncVar(hook = nameof(OnYawChanged))] private float _yaw;
    [SyncVar(hook = nameof(OnPitchChanged))] private float _pitch;

    [Header("Tank Parts")]
    [SerializeField] private GameObject tankBody;
    [SerializeField] private Transform turretYawPivot; // Y rotation
    [SerializeField] private Transform turretPitchPivot; // X rotation
    [SerializeField] private Transform muzzle; // shell spawn

    [Header("Firing")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellSpeed = 10f;
    [SerializeField] private float fireCooldownTime = 5f;
    [SyncVar] [SerializeField] private float fireCooldownTimer = 0f;
    private float _nextFireTime;

    [Header("Health/Life")]
    [SyncVar] [SerializeField] private int currHealth;
    [SyncVar] private float respawnTimer = 0f;
    [SerializeField] private int maxHealth = 5;    
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private Transform spawnLocation;

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody>();
        if (!tracks) tracks = GetComponent<TankTrackPhysics>();
        currHealth = maxHealth;
        respawnTimer = respawnTime;
    }

    [Server] public void Server_RegisterSeat(CrewSeat s)
    {
        if (s.seatType == SeatType.Driver) driver = s;
        else if (s.seatType == SeatType.Gunner) gunner = s;
    }

    private void Update()
    {
        if(fireCooldownTimer >= 0)
            fireCooldownTimer -= Time.deltaTime;

        if (currHealth <= 0)
            Server_TankDeath();

        if (respawnTimer <= 0)
            Server_RespawnTank();
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isServer || rb == null) return;
        if (tracks) tracks.SetInputs(leftTrack, rightTrack);
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

    [Server] public void Server_SetOffGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (fireCooldownTimer > 0) return;

        GameObject shellClone = Instantiate(shellPrefab, muzzle.transform.position, turretPitchPivot.transform.rotation * Quaternion.Euler(0,0,90));
        shellClone.GetComponent<Rigidbody>().velocity = turretPitchPivot.transform.right * shellSpeed;
        shellClone.GetComponent<NetworkedShell>().parent = this;
        NetworkServer.Spawn(shellClone);
        fireCooldownTimer = fireCooldownTime;
    }

    private void OnYawChanged(float _, float newYaw)
    {
        if (turretYawPivot) turretYawPivot.localRotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    private void OnPitchChanged(float _, float newPitch)
    {
        if (turretPitchPivot) turretPitchPivot.localRotation = Quaternion.Euler(0f, 0f, newPitch);
    }

    [Server] public void Server_SetDriverInput(CrewSeat from, float _leftTrack, float _rightTrack)
    {
        if (from != driver) return;
        leftTrack = Mathf.Clamp(_leftTrack,-1f,1f);
        rightTrack = Mathf.Clamp(_rightTrack, -1f, 1f);
    }

    [Server] private void Server_TankDeath()
    {
        respawnTimer -= Time.deltaTime;
        tankBody.SetActive(false);
        turretYawPivot.gameObject.SetActive(false);
        
    }
    [Server] private void Server_RespawnTank()
    {
        gameObject.transform.position = spawnLocation.transform.position;
        currHealth = maxHealth;
        respawnTimer = respawnTime;
        tankBody.SetActive(true);
        turretYawPivot.gameObject.SetActive(true);
    }

    public void TakeDamge(int dmg)
    {
        currHealth -= dmg;
    }
}
