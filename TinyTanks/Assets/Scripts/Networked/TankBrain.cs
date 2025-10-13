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
    [SyncVar] private CrewSeat driver;
    [SyncVar] private CrewSeat gunner;

    private Rigidbody _rb;
    private NetworkTransformReliable _netTrans;

    [Header("Physics based movement")]
    [SerializeField] private TankTrackPhysics tracks;
    private float _leftTrack;
    private float _rightTrack;
    [SerializeField] private TankTurretPhysics turret;
    private float _yaw;
    private float _pitch;

    [Header("Tank Parts")]
    [SerializeField] private GameObject tankBody;
    [SerializeField] private Transform turretYawPivot; // Y rotation
    [SerializeField] private Transform turretPitchPivot; // X rotation
    [SerializeField] private Transform muzzle; // shell spawn

    [Header("Firing")]
    [SerializeField] private GameObject serverShellPrefab;
    [SyncVar] private double _reloadEndTime;
    [SerializeField] private float reloadTime;
    [SerializeField] private float baseReloadTime = 5f;
    [SerializeField] private float noBatteryReloadTime = 10f;
    [SerializeField] private float shellSpeed = 10f;
    [SyncVar(hook = nameof(OnIsReloadingChanged))]
    private bool isReloading = false;
    [SyncVar(hook = nameof(OnHasBulletChanged))]
    [SerializeField] private bool hasBullet = true;

    [Header("Health/Life")]
    [SyncVar, SerializeField] private int lives = 3;
    [SyncVar, SerializeField] public int currHealth;
    public int maxHealth { get; private set; } = 5;
    [SyncVar] private double respawnEndTime;
    [SerializeField] private float respawnTime = 5f;
    private bool _isDead;

    [Header("Battery")]
    [SyncVar(hook = nameof(OnBatteryChanged))]
    public float currentBtry = 50f;
    [SerializeField] public float maxBtry { private set; get; } = 100f;
    [SerializeField] private bool hasBattery;
    [SerializeField] private float batteryDrainMove = 0.5f;
    [SerializeField] private float batteryDrainTurning = 0.3f;
    [SerializeField] private float batteryDrainNeutralSteer = 0.2f;
    [SerializeField] private float batteryDrainShot = 6f;
    [SerializeField] private float moveInputThreshold = 0.05f;

    [Header("RayCast")]
    [SerializeField] float contactRadius = 0.22f;
    [SerializeField] float contactCapsuleHalfLength = 0.45f;
    [SerializeField] private float trackSpacing = 2.6f;
    [SerializeField] private float trackRayStartHeight = 0.6f;
    [SerializeField] private float trackRayLength = 1.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("UI Canvas")]
    [SerializeField] CanvasGroup driverRespawn;
    [SerializeField] CanvasGroup gunnerRespawn;
    [SerializeField] TMP_Text[] respawnTexts;

    [Header("UI Bullet")]
    [SerializeField] private CanvasGroup reloadGroup;
    [SerializeField] private TMP_Text[] bulletStateTexts;
    [SerializeField] private Image bulletReloadImage;
    [SerializeField] private Image reloadTimerImage;

    [Header("UI Battery")]
    [SerializeField] private TMP_Text[] batteryTexts;

    [Header("Spawning")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private float minSpawnDistance = 20;

    public override void OnStartServer()
    {
        _rb = GetComponent<Rigidbody>();
        _netTrans = GetComponent<NetworkTransformReliable>();
        if (!tracks) tracks = GetComponent<TankTrackPhysics>();
        if (!turret) turret = GetComponent<TankTurretPhysics>();
        currHealth = maxHealth;
        _isDead = false;
        hasBattery = true;
        reloadTime = baseReloadTime;
        currentBtry = maxBtry;
    }

    [Server]
    public void Server_RegisterSeat(CrewSeat s)
    {
        if (s.seatType == SeatType.Driver) driver = s;
        else if (s.seatType == SeatType.Gunner) gunner = s;
    }

    private void Update()
    {
        if (_isDead)
        {
            double respawnRemaining = respawnEndTime - NetworkTime.time;
            UpdateTimerDisplay(respawnRemaining, respawnTexts);

            if (respawnRemaining <= 0 && _isDead) Server_RespawnTank();
        }

        if (isReloading)
        {
            double reloadRemaining = _reloadEndTime - NetworkTime.time;
            UpdateReloadDisplay(reloadRemaining);

            if (reloadRemaining <= 0)
                Server_FinishReload();
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isServer || _rb == null) return;
        if (_isDead) return;
        if (tracks) tracks.SetInputs(_leftTrack, _rightTrack, hasBattery);
        if (turret) turret.SetInputs(_yaw, _pitch);

        Server_ApplyBatteryMovementDrain(Time.fixedDeltaTime);
    }

    [Server]
    public void Server_SetGunnerInput(CrewSeat from, float yawDelta, float pitchDelta)
    {
        if (from != gunner) return;

        _yaw = Mathf.Clamp(yawDelta, -1f, 1f);
        _pitch = Mathf.Clamp(pitchDelta, -1f, 1f);
    }

    [Server]
    public void Server_SetOffGun(CrewSeat from)
    {
        if (from != driver) return;
        if (!hasBullet) return;

        var velocity = turretPitchPivot.transform.forward * shellSpeed;
        GameObject serverShellClone = Instantiate(serverShellPrefab, muzzle.transform.position, turretPitchPivot.transform.rotation);
        Rigidbody serverShellRB = serverShellClone.GetComponent<Rigidbody>();
        serverShellRB.velocity = velocity;

        NetworkedShell nShell = serverShellClone.GetComponent<NetworkedShell>();
        nShell.parent = this;
        NetworkServer.Spawn(serverShellClone);

        hasBullet = false;
        Server_ConsumeBattery(batteryDrainShot);
    }

    [Server]
    public void Server_ReloadGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (hasBullet) return;
        if (isReloading) return;

        isReloading = true;
        _reloadEndTime = NetworkTime.time + reloadTime;
    }

    [Server]
    public void Server_FinishReload()
    {
        isReloading = false;
        hasBullet = true;
    }

    [Server]
    private void Server_ApplyBatteryMovementDrain(float dt)
    {
        float aL = Mathf.Abs(_leftTrack);
        float aR = Mathf.Abs(_rightTrack);
        bool moving = (aL > moveInputThreshold) || (aR > moveInputThreshold);
        if (!moving) return;

        float drain = batteryDrainMove;
        float turnFactor = Mathf.Clamp01(Mathf.Abs(aL - aR));
        drain += batteryDrainTurning * turnFactor;

        bool neutralSteer = (_leftTrack * _rightTrack) < -0.2f;
        if (neutralSteer) drain += batteryDrainNeutralSteer;

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
            Debug.Log("Battery depleted -> disabling systems");
            reloadTime = noBatteryReloadTime;
        }
    }

    [Server]
    public void Server_RechargeBattery(float amount)
    {
        Debug.Log("Applied battery Effect");
        float newBtry = currentBtry + amount;

        if (newBtry > maxBtry)
        {
            newBtry = maxBtry;
        }

        currentBtry = newBtry;

        if(currentBtry > 0)
        {
            reloadTime = baseReloadTime;
            hasBattery = true;
        }    
    }

    [Server]
    public void Server_SetDriverInput(CrewSeat from, float _leftTrack, float _rightTrack)
    {
        if (from != driver) return;
        this._leftTrack = Mathf.Clamp(_leftTrack, -1f, 1f);
        this._rightTrack = Mathf.Clamp(_rightTrack, -1f, 1f);
    }

    [Server]
    private void Server_TankDeath()
    {
        if (lives <= 0) Server_ReturnToLobby();

        StarRespawnTimer();
    }

    [Server]
    private void Server_RespawnTank()
    {
        var players = FindObjectsOfType<TankBrain>().ToList();
        players.Remove(this);

        var possibleSpawnLocations = new List<Transform>();
        foreach (var player in players)
        {
            var go = player.gameObject;
            foreach (var location in spawnPoints)
            {
                if (Vector3.Distance(go.transform.position, location.position) > minSpawnDistance)
                    possibleSpawnLocations.Add(location);
            }
        }

        var idx = UnityEngine.Random.Range(0, possibleSpawnLocations.Count);
        spawnLocation = possibleSpawnLocations[idx];

        _netTrans.RpcTeleport(spawnLocation.position);

        currHealth = maxHealth;
        lives -= 1;
        _isDead = false;

        if (driverRespawn != null && gunnerRespawn != null)
        {
            driverRespawn.alpha = 0;
            gunnerRespawn.alpha = 0;
        }

        Debug.Log(this.name + $" respawned on location: " + spawnLocation);
    }

    [Server]
    private void Server_ReturnToLobby()
    {
        if (NetworkServer.active)
        {
            var roomMgr = (NetworkRoomManager)NetworkManager.singleton;
            NetworkManager.singleton.ServerChangeScene(roomMgr.RoomScene);
        }
    }

    [Client]
    private void OnHasBulletChanged(bool _, bool hasBullet)
    {
        foreach(var text in bulletStateTexts)
            if (text) text.text = hasBullet ? "Ready" : "Not Ready";
        
        if (reloadGroup) reloadGroup.alpha = hasBullet ? 0f : 1f;

        if (hasBullet)
        {
            if (bulletReloadImage) bulletReloadImage.fillAmount = 0f;
            if (reloadTimerImage) reloadTimerImage.fillAmount = 0f;
        }
    }

    [Client]
    private void OnIsReloadingChanged(bool _, bool reloading)
    {
        // For sound sfx etc.
    }

    [Client]
    private void OnBatteryChanged(float oldVal, float newVal)
    {
        foreach(var text in batteryTexts)
            text.text = $"Battery level: " + (float)Math.Round(newVal, 2) + "%";
    }

    private void UpdateTimerDisplay(double timeRemaining, TMP_Text[] uiTexts)
    {
        if (timeRemaining <= 0) timeRemaining = 0;
        var ts = TimeSpan.FromSeconds(timeRemaining);

        foreach (var text in uiTexts)
            text.text = $"{ts.Seconds:00}";
    }

    private void UpdateReloadDisplay(double timeRemaining)
    {
        float progress = 1f - Mathf.Clamp01((float)(timeRemaining / reloadTime));
        bulletReloadImage.fillAmount = progress;
        reloadTimerImage.fillAmount = progress;
    }

    private void StarRespawnTimer()
    {
        _isDead = true;
        if (driverRespawn != null && gunnerRespawn != null)
        {
            driverRespawn.alpha = 1;
            gunnerRespawn.alpha = 1;
        }
        respawnEndTime = NetworkTime.time + +respawnTime;
    }

    public void TakeDamge(int dmg)
    {
        if (_isDead) return;

        currHealth -= dmg;

        if (currHealth <= 0)
            Server_TankDeath();
    }

    #region TrackContactGizmos
#if UNITY_EDITOR
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

#if UNITY_EDITOR
            if (gizmoLabelPoints)
                UnityEditor.Handles.Label(hit.point + Vector3.up * 0.05f, label);
#endif
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

#if UNITY_EDITOR
            if (gizmoLabelPoints)
                UnityEditor.Handles.Label(end, label + " (miss)");
#endif
        }
    }
#endif
    #endregion
}
