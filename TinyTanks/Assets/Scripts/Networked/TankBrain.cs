using System;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

[DefaultExecutionOrder(-100)]
public class TankBrain : NetworkBehaviour
{
    public enum ammoTypes
    {
        normal = 0,
        missile = 1
    }

    public UnityEvent OnMissileShoot;
    public UnityEvent OnMissileDestroy;

    [HideInInspector]
    public int damage { get; private set; }

    [SyncVar] public CrewSeat driver;
    [SyncVar] public CrewSeat gunner;

    private Rigidbody _rb;
    private NetworkTransformReliable _netTrans;

    public TankData tankData;

    [Header("Physics based movement")]
    [SerializeField] private TankTrackPhysics tracks;
    private float _leftTrack;
    private float _rightTrack;
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
    public Vector3 GunnerCameraOffset => gunnerCameraOffset;

    [Header("Firing")]
    [SerializeField] private GameObject serverShellPrefab;
    [SyncVar] private double _reloadEndTime;
    [SyncVar] private float currReloadTime;
    [SyncVar(hook = nameof(OnAmmoTypeChanged))]
    [SerializeField] private ammoTypes currSelectedAmmo = ammoTypes.normal;

    [SyncVar(hook = nameof(OnIsReloadingChanged))]
    private bool isReloading = false;
    [SyncVar(hook = nameof(OnHasBulletChanged))]
    [SerializeField] private bool hasBullet = true;

    [Header("Health/Life")]
    [SyncVar(hook = nameof(OnLivesChanged))]
    [SerializeField] private int lives = 3;
    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField] public float currHealth;
    [SyncVar] private double respawnEndTime;
    [SerializeField] private float respawnTime = 5f;
    private bool _isDead;

    [Header("Battery")]
    [SyncVar(hook = nameof(OnBatteryChanged))]
    public float currentBtry = 50f;
    [SerializeField] private bool hasBattery;
    [SerializeField] private float moveInputThreshold = 0.05f;

    [Header("Mines")]
    [SerializeField] private GameObject mine;
    [SerializeField] private bool hasMines;
    [SerializeField] private float mineCooldownTime = 4f;
    [SyncVar(hook = nameof(OnMineCooldownChanged))]
    [SerializeField] private bool mineOnCooldown;
    private int _maxMineAmount = 3;
    [SyncVar(hook = nameof(OnMinesChanged))]
    private int _mineAmount = 0;
    [SyncVar] private double _mineCooldownEndTime;

    [Header("Homing Missile")]
    [SerializeField] private GameObject missileGO;
    [SyncVar] private bool _isShootingMissile;
    [SyncVar(hook = nameof(OnHasMissileChanged))]
    [SerializeField] private bool _hasMissile = false;
    [SyncVar(hook = nameof(OnMissileChanged))]
    public NetworkedMissile missile;

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
    [SerializeField] private Image bulletReloadImage;
    [SerializeField] private Image reloadTimerImage;
    [SerializeField] private Image bulletFillDriver;

    [Header("UI Battery")]
    [SerializeField] private Sprite[] batteryImages;
    [SerializeField] private Image[] currImages;
    private int _lastSpriteIndex;

    [Header("UI Mine")]
    [SerializeField] private CanvasGroup mineUI;
    [SerializeField] private Image mineFillImage;
    [SerializeField] private TMP_Text mineTxt;

    [Header("UI Missile")]
    [SerializeField] private Image indicatorImage;
    [SerializeField] private Image missileFillDriver;
    [SerializeField] private Sprite missileSprite;
    [SerializeField] private Sprite bulletSprite;

    [Header("UI Health")]
    [SerializeField] private Image[] healthImage;
    [SerializeField] private TMP_Text[] livesText;

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
    [SerializeField] private AudioSource shootingAudio;
    [SerializeField] private AudioSource driveIntoEnviormentAudio;
    [SerializeField] private AudioSource rotateCapulaAudio;
    [SerializeField] private AudioSource startDrivingAudio;
    [SerializeField] private AudioSource duringDrivingAudio;
    [SerializeField] private AudioSource endinDrivingAudio;
    [SerializeField] private AudioSource idleAudio;
    [SerializeField] private AudioSource reloadingAudio;

    public override void OnStartServer()
    {
        _rb = GetComponent<Rigidbody>();
        _netTrans = GetComponent<NetworkTransformReliable>();
        if (!tracks) tracks = GetComponent<TankTrackPhysics>();
        if (!turret) turret = GetComponent<TankTurretPhysics>();
        currHealth = tankData.maxHealth;
        _isDead = false;
        hasBattery = true;
        currReloadTime = tankData.baseReloadTime;
        currentBtry = tankData.maxBtry;
        damage = tankData.damage;
        idleAudio.Play();

        List<NetworkStartPosition> startPoints = FindObjectsOfType<NetworkStartPosition>().ToList();
        foreach (var point in startPoints)
            spawnPoints.Add(point.transform);
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

        if (mineOnCooldown)
        {
            double cooldownRemaining = _mineCooldownEndTime - NetworkTime.time;
            UpdateMineDisplay(cooldownRemaining);

            if (cooldownRemaining <= 0)
                Server_FinishMineCooldown();
        }
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
        if (_isDead) return;
        if (tracks) tracks.SetInputs(_leftTrack, _rightTrack, hasBattery);

        if (_isShootingMissile == true)
            missile.MoveMissile(_yaw, _pitch);
        else if (turret) turret.SetInputs(_yaw, _pitch);

        Server_ApplyBatteryMovementDrain(Time.fixedDeltaTime);
    }

    [Server]
    public void Server_SetGunnerInput(CrewSeat from, float yawDelta, float pitchDelta)
    {
        if (from != gunner) return;

        _yaw = Mathf.Clamp(yawDelta, -1f, 1f);
        _pitch = Mathf.Clamp(pitchDelta, -1f, 1f);

        if (_yaw == 0f)
            isRotating = false;
        else
            isRotating = true;
    }

    [Server]
    public void Server_NotifyMissileDestroyed()
    {
        missile = null;
    }

    [Server]
    public void Server_SelectShot(CrewSeat from)
    {
        if (from != driver) return;

        currSelectedAmmo = (currSelectedAmmo == ammoTypes.normal) ? currSelectedAmmo = ammoTypes.missile : currSelectedAmmo = ammoTypes.normal;
    }

    [Server]
    public void Server_ShootMissile()
    {
        _isShootingMissile = true;

        GameObject serverMissileClone = Instantiate(missileGO, muzzle.transform.position, turretPitchPivot.transform.rotation);

        NetworkedMissile nMissile = serverMissileClone.GetComponent<NetworkedMissile>();
        nMissile.parent = this;
        NetworkServer.Spawn(serverMissileClone);
        missile = nMissile;
        _hasMissile = false;
    }

    [Server]
    public void Server_ShootBullet()
    {
        var velocity = turretPitchPivot.transform.forward * tankData.shellSpeed;
        GameObject serverShellClone = Instantiate(serverShellPrefab, muzzle.transform.position, turretPitchPivot.transform.rotation);
        Rigidbody serverShellRB = serverShellClone.GetComponent<Rigidbody>();
        serverShellRB.velocity = velocity;

        NetworkedShell nShell = serverShellClone.GetComponent<NetworkedShell>();
        nShell.parent = this;
        NetworkServer.Spawn(serverShellClone);

        hasBullet = false;
        Server_ConsumeBattery(tankData.batteryDrainShot);
    }

    [Server]
    public void Server_SetOffGun(CrewSeat from)
    {
        if (from != driver) return; //revert to driver when done testing

        if (currSelectedAmmo == ammoTypes.normal)
        {
            if (hasBullet)
                Server_ShootBullet();
            return;
        }

        if (currSelectedAmmo == ammoTypes.missile)
        {
            if (_hasMissile)
                Server_ShootMissile();
            return;

        }
    }

    [Server]
    public void Server_ReloadGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (hasBullet) return;
        if (isReloading) return;

        isReloading = true;
        _reloadEndTime = NetworkTime.time + currReloadTime;
    }

    [Server]
    public void Server_SetDriverInput(CrewSeat from, float _leftTrack, float _rightTrack)
    {
        if (from != driver) return;
        this._leftTrack = Mathf.Clamp(_leftTrack, -1f, 1f);
        this._rightTrack = Mathf.Clamp(_rightTrack, -1f, 1f);

        if (this._leftTrack == 0f && this._rightTrack == 0f)
            isDriving = false;
        else
            isDriving = true;
    }

    [Server]
    private void Server_TankDeath()
    {
        lives -= 1;

        if (lives == 0) Server_ReturnToLobby();
        else StarRespawnTimer();
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

        float drain = 0;

        if (aR == 0 && aL > 0 || (aR == 0 && aL > 0))
            drain = tankData.batteryDrainTurning;
        else if (aL == 0 && aR > 0 || (aL == 0 && aR > 0))
            drain = tankData.batteryDrainTurning;
        else if (aL > 0 && aR > 0 || aL < 0 && aR < 0)
            drain = tankData.batteryDrainMove;
        else if ((_leftTrack * _rightTrack) < -0.2f)
            drain += tankData.batteryDrainNeutralSteer;

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
            currReloadTime = tankData.noBatteryReloadTime;
        }
    }

    [Server]
    public void Server_RechargeBattery(float amount)
    {
        Debug.Log("Applied battery Effect");
        float newBtry = currentBtry + amount;

        if (newBtry > tankData.maxBtry)
        {
            newBtry = tankData.maxBtry;
        }

        currentBtry = newBtry;

        if (currentBtry > 0)
        {
            currReloadTime = tankData.baseReloadTime;
            hasBattery = true;
        }
    }

    [Server]
    public void Server_LoadMissile()
    {
        _hasMissile = true;
    }

    [Server]
    public void Server_RechargeMines()
    {
        hasMines = true;
        mineUI.alpha = 1;
        _mineAmount = _maxMineAmount;
    }

    [Server]
    public void Server_PlaceMine(CrewSeat from)
    {
        if (from != gunner) return; //Make sure this is gunner when live
        if (!hasMines) return;
        if (mineOnCooldown) return;

        _mineAmount -= 1;

        if (_mineAmount <= 0)
            hasMines = false;

        GameObject mineGO = Instantiate(mine, transform.position, transform.rotation);
        NetworkServer.Spawn(mineGO);

        mineOnCooldown = true;
        _mineCooldownEndTime = NetworkTime.time + mineCooldownTime;
    }

    [Server]
    public void Server_FinishMineCooldown()
    {
        mineOnCooldown = false;
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

        _netTrans.RpcTeleport(spawnLocation.position);

        currHealth = tankData.maxHealth;
        currentBtry = tankData.maxBtry;
        if (_mineAmount > 0)
            _mineAmount = 0;
        driver.enabled = true;
        gunner.enabled = true;
        _isDead = false;

        if (driverRespawn != null && gunnerRespawn != null)
        {
            driverRespawn.alpha = 0;
            gunnerRespawn.alpha = 0;
        }
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
    private void OnLivesChanged(int oldVal, int newVal)
    {
        foreach (var text in livesText)
            text.text = $"{newVal}";
    }

    [Client]
    private void OnHealthChanged(float oldVal, float newVal)
    {
        foreach (var image in healthImage)
            image.fillAmount = newVal / tankData.maxHealth;

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
    private void OnHasBulletChanged(bool _, bool hasBullet)
    {
        if (!hasBullet)
        {
            var _barrelSmoke = Instantiate(smokeVFX, muzzle.transform.position, muzzle.transform.rotation);
            Destroy(_barrelSmoke, 3);
            shootingAudio.Play();
            bulletFillDriver.fillAmount = 0;
        }

        if (reloadGroup) reloadGroup.alpha = hasBullet ? 0f : 1f;

        if (hasBullet)
        {
            if (bulletReloadImage) bulletReloadImage.fillAmount = 0f;
            if (reloadTimerImage) reloadTimerImage.fillAmount = 0f;
            bulletFillDriver.fillAmount = 1;
        }
    }

    [Client]
    private void OnIsReloadingChanged(bool _, bool reloading)
    {
        if (reloading == true)
            reloadingAudio.Play();
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

    [Client]
    private void OnMinesChanged(int oldVal, int newVal)
    {
        mineUI.alpha = (newVal == 0) ? 0 : 1;

        mineTxt.text = $"Mines left: {newVal}";
    }

    [Client]
    private void OnMineCooldownChanged(bool _, bool onCooldown)
    {
        if (onCooldown == true)
        {
            // change UI to on cooldown
        }
        else if (onCooldown == false)
        {
            //change UI to off cooldown
        }
    }

    private void OnAmmoTypeChanged(ammoTypes oldVal, ammoTypes newVal)
    {
        if(newVal == ammoTypes.missile)
        {
            indicatorImage.sprite = missileSprite;
            missileFillDriver.enabled = true;
            bulletFillDriver.enabled = false;
        }
        else
        {
            indicatorImage.sprite = bulletSprite;
            missileFillDriver.enabled = false;
            bulletFillDriver.enabled = true;
        }
    }

    [Client]
    private void StarRespawnTimer()
    {
        _isDead = true;
        _leftTrack = 0;
        _rightTrack = 0;
        driver.enabled = false;
        gunner.enabled = false;

        if (driverRespawn != null && gunnerRespawn != null)
        {
            driverRespawn.alpha = 1;
            gunnerRespawn.alpha = 1;
        }
        respawnEndTime = NetworkTime.time + +respawnTime;
    }

    private void OnHasMissileChanged(bool _, bool hasMissile)
    {
        if (hasMissile)
        {
            missileFillDriver.fillAmount = 1;
            return;
        }
        if (!hasMissile)
        {
            missileFillDriver.fillAmount = 0;
            return;
        }
    }

    private void OnMissileChanged(NetworkedMissile oldMissile, NetworkedMissile newMissile)
    {
        if (newMissile != null)
        {
            OnMissileShoot?.Invoke();
        }
        else
        {
            _isShootingMissile = false;
            OnMissileDestroy?.Invoke();
        }
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
        float progress = 1f - Mathf.Clamp01((float)(timeRemaining / currReloadTime));
        bulletReloadImage.fillAmount = progress;
        reloadTimerImage.fillAmount = progress;
    }

    private void UpdateMineDisplay(double timeRemaining)
    {
        float progress = 1f - Mathf.Clamp01((float)(timeRemaining / currReloadTime));
        mineFillImage.fillAmount = progress;
    }

    private void UpdateTrackColour(bool grounded, Image sprite, float input) //Masterclass by Allan: how to be a maniac
    {
        sprite.color = (!grounded ? Color.red : (input == 0 ? Color.grey : (input > 0 ? blue : orange))); // dont be this guy, atleast not an if else ~ Allan
    }


    public void TakeDamge(int dmg)
    {
        if (_isDead) return;

        currHealth -= dmg;

        if (currHealth <= 0)
            Server_TankDeath();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed > impactThreshold)
            driveIntoEnviormentAudio.Play();
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
