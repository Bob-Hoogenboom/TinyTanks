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
    [SerializeField] private TankTurretPhysics turret;
    private float yaw;
    private float pitch;

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
        if (turret) turret.SetInputs(yaw, pitch);
    }

    [Server] public void Server_SetGunnerInput(CrewSeat from, float yawDelta, float pitchDelta)
    {
        if (from != gunner) return;

        yaw = Mathf.Clamp(yawDelta, -1f, 1f);
        pitch = Mathf.Clamp(pitchDelta, -1f, 1f);
    }

    [Server] public void Server_SetOffGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (fireCooldownTimer > 0) return;

        GameObject shellClone = Instantiate(shellPrefab, muzzle.transform.position, turretPitchPivot.transform.rotation * Quaternion.Euler(90,0,0));
        shellClone.GetComponent<Rigidbody>().velocity = turretPitchPivot.transform.forward * shellSpeed;
        shellClone.GetComponent<NetworkedShell>().parent = this;
        NetworkServer.Spawn(shellClone);
        fireCooldownTimer = fireCooldownTime;
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
