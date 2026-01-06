using System;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class TankBrain : NetworkBehaviour
{
    public enum ammoTypes
    {
        normal = 0,
        missile = 1
    }

    public static Action OnReturnToLobby;
    public static Action OnTankDeath;
    public static Action OnTankRespawn;
    public Action OnSwapToMissileCam;
    public Action OnSwapToGunnerCam;

    [HideInInspector]
    public int damage { get; private set; }

    [SyncVar] public CrewSeat driver;
    [SyncVar] public CrewSeat gunner;

    private Rigidbody _rb;
    private NetworkTransformReliable _netTrans;

    public TankData tankData;

    [Header("Physics based movement and trackAnim")]
    [SerializeField] private TankTrackPhysics tracks;   
    [SyncVar(hook = nameof(OnLeftTrackChanged))] private float _leftTrack;
    [SyncVar(hook = nameof(OnRightTrackChanged))] private float _rightTrack;

    [SerializeField] private TankTurretPhysics turret;
    private float _yaw;
    private float _pitch;
    [SerializeField] private float impactThreshold = 5;
    [SyncVar(hook = nameof(OnIsDrivingChanged))]
    private bool isDriving = false;
    [SyncVar(hook = nameof(OnIsRotatingChanged))]
    private bool isRotating = false;

    [Header("Tank Parts")]
    [SerializeField] private GameObject tankBody;
    [SerializeField] private Vector3 gunnerCameraOffset;
    [SerializeField] private Transform turretYawPivot; // Y rotation
    [SerializeField] private Transform turretPitchPivot; // X rotation
    [SerializeField] private Transform muzzle; // shell spawn
    public Transform TurretYawPivot => turretYawPivot;
    public Transform TurretPitchPivot => turretPitchPivot;
    public Transform Muzzle => muzzle;
    public Vector3 GunnerCameraOffset => gunnerCameraOffset;

    [Header("Firing/Missile")]
    public Shooter shooter;
    private bool _isShootingMissile;
    [SyncVar(hook = nameof(OnMissileChanged))]
    public NetworkedMissile missile;
    [SyncVar(hook = nameof(OnAmmoTypeChanged))]
    [SerializeField] private ammoTypes currSelectedAmmo = ammoTypes.normal;

    [Header("Health/Life")]
    public NetworkedHealth health;

    [Header("Battery")]
    [SyncVar(hook = nameof(OnBatteryChanged))]
    public float currentBtry = 50f;
    [SerializeField] private bool hasBattery;
    [SerializeField] private float moveInputThreshold = 0.05f;

    [Header("Mines")]
    [SerializeField] private MineLayer mineLayer;

    [Header("UI Battery")]
    [SerializeField] private Sprite[] batteryImages;
    [SerializeField] private Image[] currImages;
    private int _lastSpriteIndex;

    [Header("UI Tracks")]
    [SerializeField] private Image leftTrackImage;
    [SerializeField] private Image rightTrackImage;
    [SerializeField] private Color orange;
    [SerializeField] private Color blue;

    [Header("Spawning")]
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private float minSpawnDistance = 20;

    [Header("VFX")]
    [SerializeField] private GameObject smokeVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource driveIntoEnviormentAudio;
    [SerializeField] private AudioSource rotateCapulaAudio;
    [SerializeField] private AudioSource startDrivingAudio;
    [SerializeField] private AudioSource duringDrivingAudio;
    [SerializeField] private AudioSource endinDrivingAudio;
    [SerializeField] private AudioSource idleAudio;

    public override void OnStartServer()
    {
        _rb = GetComponent<Rigidbody>();
        _netTrans = GetComponent<NetworkTransformReliable>();
        if (!tracks) tracks = GetComponent<TankTrackPhysics>();
        if (!turret) turret = GetComponent<TankTurretPhysics>();
        if (!mineLayer) mineLayer = GetComponent<MineLayer>();
        if (!shooter) shooter = GetComponent<Shooter>();

        OnTankDeath += Server_StarRespawnTimer;
        OnReturnToLobby += Server_ReturnToLobby;
        OnTankRespawn += Server_RespawnTank;

        hasBattery = true;
        currentBtry = tankData.maxBtry;
        damage = tankData.damage;
        idleAudio.Play();

        List<NetworkStartPosition> startPoints = FindObjectsOfType<NetworkStartPosition>().ToList();
        foreach (var point in startPoints)
            spawnPoints.Add(point.transform);

        shooter.OnMissileShoot += AssignMissile;
    }

    [Server]
    public void Server_RegisterSeat(CrewSeat s)
    {
        if (s.seatType == SeatType.Driver) driver = s;
        else if (s.seatType == SeatType.Gunner) gunner = s;
    }

    private void LateUpdate()
    {
        UpdateTrackColour(tracks.leftTrackGrounded, leftTrackImage, tracks.leftInput);
        UpdateTrackColour(tracks.rightTrackGrounded, rightTrackImage, tracks.rightInput);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isServer || _rb == null) return;
        if (health.IsDead) return;
        if (tracks) tracks.SetInputs(_leftTrack, _rightTrack, hasBattery);

        if (_isShootingMissile == true)
            missile.MoveMissile(_yaw, _pitch);
        else if (turret) turret.SetInputs(_yaw, _pitch);

        Server_ApplyBatteryMovementDrain(Time.fixedDeltaTime);
    }

    private void OnLeftTrackChanged(float oldVal, float newVal) => PushTrackInputsToTracks();
    private void OnRightTrackChanged(float oldVal, float newVal) => PushTrackInputsToTracks();

    private void PushTrackInputsToTracks()
    {
        if (!tracks) tracks = GetComponent<TankTrackPhysics>();
        if (tracks) tracks.SetInputs(_leftTrack, _rightTrack, currentBtry > 0f);
    }

    [Server]
    public void Server_SetGunnerInput(CrewSeat from, float yawDelta, float pitchDelta)
    {
        if (from != gunner) return;
        if (health.IsDead) return;

        _yaw = Mathf.Clamp(yawDelta, -1f, 1f);
        _pitch = Mathf.Clamp(pitchDelta, -1f, 1f);

        if (_yaw == 0f)
            isRotating = false;
        else
            isRotating = true;
    }

    [Server]
    public void AssignMissile()
    {
        _isShootingMissile = true;
    }

    [Server]
    public void Server_NotifyMissileDestroyed()
    {
        _isShootingMissile = false;
        missile = null;
    }

    [Server]
    public void Server_SelectShot(CrewSeat from)
    {
        if (from != driver) return;
        if (health.IsDead) return;

        currSelectedAmmo = (currSelectedAmmo == ammoTypes.normal) ? currSelectedAmmo = ammoTypes.missile : currSelectedAmmo = ammoTypes.normal;
    }

    [Server]
    public void Server_SetOffGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (health.IsDead) return;

        if (currSelectedAmmo == ammoTypes.normal) shooter.OnBulletShoot?.Invoke();
        else shooter.OnMissileShoot?.Invoke();
    }

    [Server]
    public void Server_ReloadGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (health.IsDead) return;

        shooter.OnReloadBullet?.Invoke();
    }

    [Server]
    public void Server_SetDriverInput(CrewSeat from, float _leftTrack, float _rightTrack)
    {
        if (from != driver) return;
        if (health.IsDead) return;

        this._leftTrack = Mathf.Clamp(_leftTrack, -1f, 1f);
        this._rightTrack = Mathf.Clamp(_rightTrack, -1f, 1f);

        if (this._leftTrack == 0f && this._rightTrack == 0f)
            isDriving = false;
        else
            isDriving = true;
    }

    [Server]
    private void Server_ApplyBatteryMovementDrain(float dt)
    {
        float aL = Mathf.Abs(_leftTrack);
        float aR = Mathf.Abs(_rightTrack);

        bool moving = (aL > moveInputThreshold) || (aR > moveInputThreshold);
        if (!moving) return;

        float drain = 0;

        float product = aL * aR;

        bool oneStoppedOneForward =
            (aR == 0f && aL > 0f) ||
            (aL == 0f && aR > 0f);

        if (oneStoppedOneForward)
        {
            drain = tankData.batteryDrainTurning;
        }
        else if (product > 0f)
        {
            drain = tankData.batteryDrainMove;
        }
        else if (product < -0.2f)
        {
            drain += tankData.batteryDrainNeutralSteer;
        }

        Server_ConsumeBattery(drain * dt);
    }

    [Server]
    private void Server_ConsumeBattery(float amount)
    {
        if (amount <= 0f) return;

        float prev = currentBtry;
        currentBtry = Mathf.Max(0f, prev - amount);

        if (prev > 0f && currentBtry <= 0f)
        {
            hasBattery = false;
            shooter.OnNoBattery?.Invoke();
        }
    }

    [Server]
    public void Server_RechargeBattery(float amount)
    {
        float newBtry = currentBtry + amount;

        if (newBtry > tankData.maxBtry)
            newBtry = tankData.maxBtry;

        currentBtry = newBtry;

        if (currentBtry > 0)
        {
            shooter.OnRechargeBattery?.Invoke();
            hasBattery = true;
        }
    }

    [Server]
    public void Server_LoadMissile()
    {
        shooter.OnPickupMissle?.Invoke();
    }

    [Server]
    public void Server_RechargeMines()
    {
        mineLayer.OnPickupMines?.Invoke();
    }

    [Server]
    public void Server_PlaceMine(CrewSeat from)
    {
        if (from != driver) return;
        if (health.IsDead) return;

        mineLayer.OnLayMines?.Invoke();
    }

    [Server]
    private void Server_RespawnTank()
    {
        var players = FindObjectsOfType<TankBrain>().ToList();
        players.Remove(this);

        var possibleSpawnLocations = new List<Transform>();
        if (players.Count != 0)
        {
            foreach (var player in players)
            {
                var go = player.gameObject;
                foreach (var location in spawnPoints)
                {
                    if (Vector3.Distance(go.transform.position, location.position) > minSpawnDistance)
                        possibleSpawnLocations.Add(location);
                }
            }
        }
        else
            possibleSpawnLocations = spawnPoints;

        var idx = UnityEngine.Random.Range(0, possibleSpawnLocations.Count);
        spawnLocation = possibleSpawnLocations[idx];

        transform.rotation = Quaternion.Euler(0, 0, 0);
        _netTrans.RpcTeleport(spawnLocation.position);

        health.OnHealthReset?.Invoke();
        mineLayer.OnDeathReset?.Invoke();

        currentBtry = tankData.maxBtry;
        driver.enabled = true;
        gunner.enabled = true;

        RpcTankBirth();
    }

    [Server]
    private void Server_ReturnToLobby()
    {
        var nm = NetworkManager.singleton;
        if (nm == null) return;

        if (NetworkServer.active)
        {
            nm.StopHost();
        }
    }

    [ClientRpc]
    private void RpcTankBirth()
    {
        health.OnStopRespawnTimer?.Invoke();
    }

    [Client]
    private void OnIsRotatingChanged(bool _, bool isRotating)
    {
        if (isRotating == true)
            rotateCapulaAudio.Play();
        else
            rotateCapulaAudio.Stop();
    }

    [Client]
    private void OnIsDrivingChanged(bool _, bool isDriving)
    {
        if (isDriving == false)
        {
            startDrivingAudio.Stop();
            if (duringDrivingAudio.isPlaying == true)
            {
                duringDrivingAudio.Stop();
                endinDrivingAudio.Play();
            }
            idleAudio.PlayDelayed(1);
        }
        else if (isDriving == true)
        {
            idleAudio.Stop();
            startDrivingAudio.Play();
            duringDrivingAudio.PlayDelayed(1);
        }
    }

    [Client]
    private void OnBatteryChanged(float oldVal, float newVal)
    {
        float clamped = Mathf.Clamp(newVal, 0f, 100f);
        int index;
        if (clamped > 80f) index = 0;
        else if (clamped > 60f) index = 1;
        else if (clamped > 40f) index = 2;
        else if (clamped > 20f) index = 3;
        else if (clamped > 0f) index = 4;
        else index = 5;

        var sprite = batteryImages[index];
        foreach (var image in currImages)
            image.sprite = sprite;
    }

    [Server]
    private void Server_StarRespawnTimer()
    {
        if (!health.IsDead) return;

        _leftTrack = 0;
        _rightTrack = 0;
        driver.enabled = false;
        gunner.enabled = false;
    }

    private void OnMissileChanged(NetworkedMissile oldMissile, NetworkedMissile newMissile)
    {
        if (newMissile != null)
        {
            OnSwapToMissileCam?.Invoke();
        }
        else
        {
            _isShootingMissile = false;
            OnSwapToGunnerCam?.Invoke();
        }
    }

    private void OnAmmoTypeChanged(ammoTypes oldVal, ammoTypes newVal)
    {
        if (newVal == ammoTypes.missile)
            shooter.SwapToMissile?.Invoke();
        else
            shooter.SwapToBullet?.Invoke();
    }

    private void UpdateTrackColour(bool grounded, Image sprite, float input) //Masterclass by Allan: how to be a maniac
    {
        sprite.color = (!grounded ? Color.red : (input == 0 ? Color.grey : (input > 0 ? blue : orange))); // dont be this guy, atleast not an if else ~ Allan
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed > impactThreshold)
            driveIntoEnviormentAudio.Play();
    }

    #region TrackContactGizmos
#if UNITY_EDITOR
    [Header("RayCast")]
    [SerializeField] float contactRadius = 0.22f;
    [SerializeField] float contactCapsuleHalfLength = 0.45f;
    [SerializeField] private float trackSpacing = 2.6f;
    [SerializeField] private float trackRayStartHeight = 0.6f;
    [SerializeField] private float trackRayLength = 1.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Track Contact Gizmos")]
    [SerializeField] bool gizmoDrawTrackCasts = true;
    [SerializeField] bool gizmoOnlyWhenSelected = true;
    [SerializeField] bool gizmoDrawNormals = true;
    [SerializeField] bool gizmoLabelPoints = false;

    void OnDrawGizmos()
    {
        if (!gizmoDrawTrackCasts || gizmoOnlyWhenSelected) return;
        DrawTrackContactGizmos();
    }

    void OnDrawGizmosSelected()
    {
        if (!gizmoDrawTrackCasts) return;
        DrawTrackContactGizmos();
    }

    void DrawTrackContactGizmos()
    {
        // Per-track midpoints at cast start height
        Vector3 leftMid = transform.TransformPoint(new Vector3(-trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 rightMid = transform.TransformPoint(new Vector3(trackSpacing * 0.5f, trackRayStartHeight, 0f));

        Vector3 fwd = transform.forward;      // along the track
        Vector3 down = -transform.up;          // cast direction
        float off = contactCapsuleHalfLength; // front/back offset along track
        float r = contactRadius;
        float len = trackRayLength;

        // LEFT track (front + rear)
        DrawTrackSphereCast(leftMid + fwd * off, down, r, len, new Color(0f, 0.75f, 1f, 1f), "L-F");
        DrawTrackSphereCast(leftMid - fwd * off, down, r, len, new Color(0f, 0.55f, 1f, 1f), "L-R");

        // RIGHT track (front + rear)
        DrawTrackSphereCast(rightMid + fwd * off, down, r, len, new Color(1f, 0.85f, 0f, 1f), "R-F");
        DrawTrackSphereCast(rightMid - fwd * off, down, r, len, new Color(1f, 0.65f, 0f, 1f), "R-R");
    }

    void DrawTrackSphereCast(Vector3 origin, Vector3 dir, float radius, float distance, Color c, string label)
    {
        Vector3 n = dir.normalized;
        Vector3 end = origin + n * distance;

        // start ring
        Gizmos.color = c;
        Gizmos.DrawWireSphere(origin, Mathf.Max(0.02f, radius * 0.9f));

        // cast + hit
        if (Physics.SphereCast(origin, radius, n, out RaycastHit hit, distance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Gizmos.DrawLine(origin, hit.point);
            Gizmos.DrawWireSphere(hit.point, Mathf.Max(0.01f, radius * 0.2f));

            if (gizmoDrawNormals)
                Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.6f);


            if (gizmoLabelPoints)
                UnityEditor.Handles.Label(hit.point + Vector3.up * 0.05f, label);

        }
        else
        {
            // miss: draw full ray and a small X at the end
            Gizmos.DrawLine(origin, end);

            Vector3 right = Vector3.Cross(n, Vector3.up);
            if (right.sqrMagnitude < 1e-6f) right = Vector3.Cross(n, Vector3.right);
            right.Normalize();
            Vector3 up = Vector3.Cross(n, right);
            float s = Mathf.Max(0.01f, radius * 0.25f);
            Gizmos.DrawLine(end - right * s, end + right * s);
            Gizmos.DrawLine(end - up * s, end + up * s);


            if (gizmoLabelPoints)
                UnityEditor.Handles.Label(end, label + " (miss)");

        }
    }
#endif
    #endregion
}
