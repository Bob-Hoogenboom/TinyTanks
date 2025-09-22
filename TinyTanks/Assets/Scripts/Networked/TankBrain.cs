using System;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
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
    [SerializeField] private GameObject serverShellPrefab;
    [SerializeField] private float shellSpeed = 10f;
    [SyncVar, SerializeField] private double reloadEndTime;
    [SerializeField] private float reloadTime = 5f;

    [Header("Health/Life")]
    [SyncVar, SerializeField] private int lives = 3;
    [SyncVar, SerializeField] private int currHealth;
    [SyncVar] private double respawnEndTime;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private Transform spawnLocation;
    private bool isDead;

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
    [SerializeField] private TMP_Text bulletStateText;
    [SerializeField] private Image bulletReloadImage;
    [SerializeField] private Image reloadTimerImage;

    [SyncVar] private bool isReloading = false;
    [SyncVar] private bool hasBullet = true;

    public override void OnStartServer()
    {
        rb = GetComponent<Rigidbody>();
        if (!tracks) tracks = GetComponent<TankTrackPhysics>();
        currHealth = maxHealth;
        isDead = false;
    }

    [Server]
    public void Server_RegisterSeat(CrewSeat s)
    {
        if (s.seatType == SeatType.Driver) driver = s;
        else if (s.seatType == SeatType.Gunner) gunner = s;
    }

    private void Update()
    {
        double respawnRemaining = respawnEndTime - NetworkTime.time;
        UpdateTimerDisplay(respawnRemaining, respawnTexts);

        if (respawnRemaining <= 0 && isDead) Server_RespawnTank();

        if(isReloading)
        {
            double reloadRemaining = reloadEndTime - NetworkTime.time;
            UpdateReloadDisplay(reloadRemaining);

            if (reloadRemaining <= 0)
                Server_FinishReload();
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!isServer || rb == null) return;
        if (isDead) return;
        if (tracks) tracks.SetInputs(leftTrack, rightTrack);
        if (turret) turret.SetInputs(yaw, pitch);
    }

    [Server]
    public void Server_SetGunnerInput(CrewSeat from, float yawDelta, float pitchDelta)
    {
        if (from != gunner) return;

        yaw = Mathf.Clamp(yawDelta, -1f, 1f);
        pitch = Mathf.Clamp(pitchDelta, -1f, 1f);
    }

    [Server]
    public void Server_SetOffGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (!hasBullet) return;

        var velocity = turretPitchPivot.transform.forward * shellSpeed;
        GameObject serverShellClone = Instantiate(serverShellPrefab, muzzle.transform.position, turretPitchPivot.transform.rotation);
        Rigidbody serverShellRB = serverShellClone.GetComponent<Rigidbody>();
        serverShellRB.velocity = velocity;

        NetworkedShell nShell = serverShellClone.GetComponent<NetworkedShell>();
        nShell.parent = this;
        NetworkServer.Spawn(serverShellClone);

        bulletStateText.text = "Not Ready";
        hasBullet = false;
        reloadGroup.alpha = 1;
    }

    [Server]
    public void Server_ReloadGun(CrewSeat from)
    {
        if (from != gunner) return;
        if (hasBullet) return;

        isReloading = true;
        reloadEndTime = NetworkTime.time + reloadTime;
    }

    [Server]
    public void Server_FinishReload()
    {
        bulletStateText.text = "Ready";
        isReloading = false;
        hasBullet = true;
        bulletReloadImage.fillAmount = 0;
        reloadTimerImage.fillAmount = 0;
        reloadGroup.alpha = 0;
    }

    [Server]
    public void Server_SetDriverInput(CrewSeat from, float _leftTrack, float _rightTrack)
    {
        if (from != driver) return;
        leftTrack = Mathf.Clamp(_leftTrack, -1f, 1f);
        rightTrack = Mathf.Clamp(_rightTrack, -1f, 1f);
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
        gameObject.transform.position = spawnLocation.transform.position;
        currHealth = maxHealth;
        lives -= 1;
        isDead = false;

        if(driverRespawn != null && gunnerRespawn != null)
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

    private void UpdateTimerDisplay(double timeRemaining, TMP_Text[] uiTexts)
    {
        if (timeRemaining <= 0) timeRemaining = 0;
        var ts = TimeSpan.FromSeconds(timeRemaining);

        foreach(var text in uiTexts)
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
        isDead = true;
        if (driverRespawn != null && gunnerRespawn != null)
        {
            driverRespawn.alpha = 1;
            gunnerRespawn.alpha = 1;
        }
        respawnEndTime = NetworkTime.time + +respawnTime;
    }

    public void TakeDamge(int dmg)
    {
        if (isDead) return;

        currHealth -= dmg;

        if (currHealth <= 0)
            Server_TankDeath();
    }

    #region WireCapsule
#if UNITY_EDITOR

    [SerializeField] bool gizmoDrawCapsuleCasts = true;
    [SerializeField] bool gizmoOnlyWhenSelected = true;
    [SerializeField] bool gizmoDrawHitAndNormal = true;

    // If your track runs along local RIGHT instead of FORWARD, flip this:
    [SerializeField] bool trackAxisIsRight = false;

    [ExecuteAlways]  // so it draws in Edit + Play
    void OnDrawGizmos()
    {
        if (!gizmoDrawCapsuleCasts || gizmoOnlyWhenSelected) return;
        DrawTrackCapsuleCasts();
    }
    void OnDrawGizmosSelected()
    {
        if (!gizmoDrawCapsuleCasts) return;
        DrawTrackCapsuleCasts();
    }

    void DrawTrackCapsuleCasts()
    {
        // Same math as your cast
        Vector3 leftBase = transform.TransformPoint(new Vector3(-trackSpacing * 0.5f, trackRayStartHeight, 0f));
        Vector3 rightBase = transform.TransformPoint(new Vector3(trackSpacing * 0.5f, trackRayStartHeight, 0f));

        Vector3 axisAlongTrack = (trackAxisIsRight ? transform.right : transform.forward) * contactCapsuleHalfLength;
        Vector3 castDir = -transform.up;
        float dist = trackRayLength;

        // Draw the swept capsule volumes
        DrawCapsuleCastGizmo(leftBase - axisAlongTrack, leftBase + axisAlongTrack,
                             contactRadius, castDir, dist, new Color(0f, 0.8f, 1f, 1f));  // left = cyan

        DrawCapsuleCastGizmo(rightBase - axisAlongTrack, rightBase + axisAlongTrack,
                             contactRadius, castDir, dist, new Color(1f, 0.85f, 0f, 1f)); // right = yellow

        // Optionally show the actual hit point & normal (uses current LayerMask)
        if (gizmoDrawHitAndNormal)
        {
            if (Physics.CapsuleCast(leftBase - axisAlongTrack, leftBase + axisAlongTrack, contactRadius,
                                    castDir, out RaycastHit L, dist, groundMask, QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(L.point, contactRadius * 0.15f);
                Gizmos.DrawLine(L.point, L.point + L.normal * 0.6f);
            }

            if (Physics.CapsuleCast(rightBase - axisAlongTrack, rightBase + axisAlongTrack, contactRadius,
                                    castDir, out RaycastHit R, dist, groundMask, QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(R.point, contactRadius * 0.15f);
                Gizmos.DrawLine(R.point, R.point + R.normal * 0.6f);
            }
        }
    }

    /// Draws start/end capsules and the swept rails between them (wireframe).
    void DrawCapsuleCastGizmo(Vector3 p1, Vector3 p2, float radius, Vector3 dir, float distance, Color color, int segments = 24)
    {
        if (radius <= 0f) { Gizmos.color = color; Gizmos.DrawLine(p1, p1 + dir.normalized * distance); return; }

        Gizmos.color = color;
        Vector3 n = dir.normalized;
        Vector3 off = n * distance;
        Vector3 a1 = p1, a2 = p2;           // start capsule endpoints
        Vector3 b1 = p1 + off, b2 = p2 + off; // end capsule endpoints

        // Draw the two capsules
        DrawCapsuleWire(a1, a2, radius, color, segments);
        DrawCapsuleWire(b1, b2, radius, color, segments);

        // Connect a few rails so the sweep volume reads clearly
        Vector3 axis = (a2 - a1).normalized;
        // orthonormal basis around the capsule axis
        Vector3 t = Vector3.Cross(axis, Vector3.up); if (t.sqrMagnitude < 1e-6f) t = Vector3.Cross(axis, Vector3.right);
        t.Normalize();
        Vector3 s = Vector3.Cross(axis, t);

        // Four evenly spaced rails
        for (int i = 0; i < 4; i++)
        {
            float ang = i * 0.5f * Mathf.PI; // 0, 90, 180, 270 deg
            Vector3 rim = (Mathf.Cos(ang) * t + Mathf.Sin(ang) * s) * radius;
            Gizmos.DrawLine(a1 + rim, b1 + rim);
            Gizmos.DrawLine(a2 + rim, b2 + rim);
        }
    }

    /// Wireframe capsule between endpoints a & b with radius r.
    void DrawCapsuleWire(Vector3 a, Vector3 b, float r, Color c, int segments = 24)
    {
        Gizmos.color = c;

        Vector3 axis = (b - a);
        float len = axis.magnitude;
        Vector3 n = (len > 1e-6f) ? axis / len : Vector3.up;

        // basis perpendicular to n
        Vector3 t = Vector3.Cross(n, Vector3.up); if (t.sqrMagnitude < 1e-6f) t = Vector3.Cross(n, Vector3.right);
        t.Normalize();
        Vector3 s = Vector3.Cross(n, t);

        // end rings + side lines
        float step = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * step, a1 = (i + 1) * step;
            Vector3 r0 = (Mathf.Cos(a0) * t + Mathf.Sin(a0) * s) * r;
            Vector3 r1 = (Mathf.Cos(a1) * t + Mathf.Sin(a1) * s) * r;

            Vector3 A0 = a + r0, A1 = a + r1;
            Vector3 B0 = b + r0, B1 = b + r1;

            Gizmos.DrawLine(A0, A1); // start ring
            Gizmos.DrawLine(B0, B1); // end ring
            Gizmos.DrawLine(A0, B0); // side
        }
    }
#endif
    #endregion
}
