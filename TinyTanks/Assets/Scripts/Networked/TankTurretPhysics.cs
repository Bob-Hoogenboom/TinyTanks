using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankTurretPhysics : NetworkBehaviour
{
    const float EPS = 1e-4f;

    [Header("Axis Transforms")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;

    [Header("Control")]
    [Range(0f, 1f)] public float inputDeadzone = 0.05f;

    public enum IdleElectricalMode { CoastOpen, DynamicBrakeShort, HoldZero }

    [Header("Idle Electrical Mode (on stick release)")]
    public IdleElectricalMode idleModeYaw = IdleElectricalMode.DynamicBrakeShort;
    public IdleElectricalMode idleModePitch = IdleElectricalMode.DynamicBrakeShort;

    [Header("Axis Settings (ScriptableObjects)")]
    [Tooltip("Settings asset used for Yaw (azimuth) axis.")]
    public AxisParamaters yawSettings;

    [Tooltip("Settings asset used for Pitch (elevation) axis.")]
    public AxisParamaters pitchSettings;

    [Header("Yaw Limits")]
    [Tooltip("Optional hard yaw half-range about start [deg]. Set 0 for unlimited.")]
    [Range(0f, 180f)] public float yawHalfRange = 0f;
    [Tooltip("Soft zone width before hard yaw limit [deg].")]
    public float yawSoftZone = 8f;
    [Tooltip("Soft limit spring [N·m/rad].")]
    public float yawLimitK = 1200f;
    [Tooltip("Soft limit damper [N·m·s/rad].")]
    public float yawLimitC = 80f;

    [Header("Pitch Limits")]
    [Tooltip("Min / Max elevation [deg].")]
    public float pitchMin = -10f, pitchMax = 30f;
    [Tooltip("Soft zone width before hard pitch limit [deg].")]
    public float pitchSoftZone = 5f;
    [Tooltip("Soft limit spring [N·m/rad].")]
    public float pitchLimitK = 1500f;
    [Tooltip("Soft limit damper [N·m·s/rad].")]
    public float pitchLimitC = 100f;

    [Header("Pitch Gravity (optional)")]
    [Tooltip("Effective mass acting on elevation [kg].")]
    public float pitchMEff = 0f;
    [Tooltip("Lever arm from axis to CoM [m].")]
    public float pitchRM = 0f;

    // Runtime state
    private float yawRelDeg, yawRateDeg;     // yaw relative to start
    private float yawStartDeg;
    private float pitchDeg, pitchRateDeg;

    private float cmdYaw, cmdPitch; // [-1..1]

    // Input from your seat controller
    public void SetInputs(float yawAxis, float pitchAxis)
    {
        cmdYaw = Mathf.Abs(yawAxis) < inputDeadzone ? 0f : Mathf.Clamp(yawAxis, -1f, 1f);
        cmdPitch = Mathf.Abs(pitchAxis) < inputDeadzone ? 0f : Mathf.Clamp(pitchAxis, -1f, 1f);
    }

    void Awake()
    {
        if (!yawPivot) Debug.LogWarning("[TankTurretPhysics] Yaw pivot not assigned.");
        if (!pitchPivot) Debug.LogWarning("[TankTurretPhysics] Pitch pivot not assigned.");
        if (!yawSettings) Debug.LogError("[TankTurretPhysics] Missing yawSettings ScriptableObject.");
        if (!pitchSettings) Debug.LogError("[TankTurretPhysics] Missing pitchSettings ScriptableObject.");

        if (yawPivot) yawStartDeg = ToSigned(yawPivot.localEulerAngles.y);
        if (pitchPivot)
        {
            float x = pitchPivot.localEulerAngles.x;
            pitchDeg = (x > 180f) ? -(360f - x) : -x; // convert Unity X (right-handed) to our elevation sign
            pitchDeg = Mathf.Clamp(pitchDeg, pitchMin, pitchMax);
        }
    }

    void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        float dt = Time.fixedDeltaTime;

        // --- YAW ---
        {
            float w = Mathf.Deg2Rad * yawRateDeg;
            float motor = MotorTorque(yawSettings, cmdYaw, w, idleModeYaw);
            float fric = FrictionTorque(yawSettings, w);
            float tLimit = YawLimitTorque(w);

            float alpha = (motor - fric - tLimit) / Mathf.Max(EPS, yawSettings.inertia);
            w += alpha * dt;

            yawRateDeg = w * Mathf.Rad2Deg;
            yawRelDeg += yawRateDeg * dt;

            if (yawHalfRange > 0f)
            {
                float left = -yawHalfRange, right = yawHalfRange;
                if (yawRelDeg < left) { yawRelDeg = left; if (yawRateDeg < 0f) yawRateDeg = 0f; }
                if (yawRelDeg > right) { yawRelDeg = right; if (yawRateDeg > 0f) yawRateDeg = 0f; }
            }

            if (yawPivot)
                yawPivot.localRotation = Quaternion.Euler(0f, Wrap360(yawStartDeg + yawRelDeg), 0f);
        }

        // --- PITCH ---
        {
            float w = Mathf.Deg2Rad * pitchRateDeg;
            float theta = Mathf.Deg2Rad * pitchDeg;

            float motor = MotorTorque(pitchSettings, cmdPitch, w, idleModePitch);
            float fric = FrictionTorque(pitchSettings, w);
            float tGrav = (pitchMEff > 0f && pitchRM > 0f) ? pitchMEff * 9.81f * pitchRM * Mathf.Sin(theta) : 0f;
            float tLimit = PitchLimitTorque(w);

            float alpha = (motor - fric - tGrav - tLimit) / Mathf.Max(EPS, pitchSettings.inertia);
            w += alpha * dt;

            pitchRateDeg = w * Mathf.Rad2Deg;
            pitchDeg += pitchRateDeg * dt;

            if (pitchDeg < pitchMin) { pitchDeg = pitchMin; if (pitchRateDeg < 0f) pitchRateDeg = 0f; }
            if (pitchDeg > pitchMax) { pitchDeg = pitchMax; if (pitchRateDeg > 0f) pitchRateDeg = 0f; }

            if (pitchPivot)
                pitchPivot.localRotation = Quaternion.Euler(-pitchDeg, 0f, 0f);
        }
    }

    // ====== Physics pieces (read-only; all parameters from SO) ======
    static float MotorTorque(AxisParamaters s, float u, float w, IdleElectricalMode idleMode)
    {
        // Convert free speed to rad/s
        float wFree = Mathf.Deg2Rad * Mathf.Max(1e-3f, s.freeSpeedDegPerSec);

        if (Mathf.Abs(u) > 1e-3f)
        {
            // Linear motor model: tau = tau_stall * (u - w/w_free)
            return s.stallTorque * (Mathf.Clamp(u, -1f, 1f) - w / wFree);
        }

        // Idle behavior uses only data from SO
        switch (idleMode)
        {
            case IdleElectricalMode.DynamicBrakeShort: return -s.bElectricalShorted * w;
            case IdleElectricalMode.HoldZero: return -s.stallTorque * (w / wFree); // proportional to speed
            default: return 0f; // CoastOpen
        }
    }

    static float FrictionTorque(AxisParamaters s, float w)
    {
        float absw = Mathf.Abs(w);
        float sgn = Mathf.Sign(w);

        // Stribeck (static->coulomb) + Coulomb
        float wStr = Mathf.Deg2Rad * Mathf.Max(0.01f, s.stribeckDegPerSec);
        float strib = (s.tauStatic - s.tauCoulomb) * Mathf.Exp(-Mathf.Pow(absw / wStr, 2f));
        float dry = (s.tauCoulomb + strib) * sgn;

        // Viscous
        float visc = s.bMechanical * w;

        // Stick region clamp near zero speed
        if (absw < 0.01f) return Mathf.Clamp(dry + visc, -s.tauStatic, s.tauStatic);
        return dry + visc;
    }

    float YawLimitTorque(float w)
    {
        if (yawHalfRange <= 0f) return 0f;
        float soft = Mathf.Max(0.1f, yawSoftZone);
        float left = -yawHalfRange, right = yawHalfRange;

        float tau = 0f;
        if (yawRelDeg < left + soft)
        {
            float pen = Mathf.Deg2Rad * ((left + soft) - yawRelDeg);
            tau += yawLimitK * pen + yawLimitC * Mathf.Max(0f, -w);
        }
        else if (yawRelDeg > right - soft)
        {
            float pen = Mathf.Deg2Rad * (yawRelDeg - (right - soft));
            tau += -(yawLimitK * pen + yawLimitC * Mathf.Max(0f, w));
        }
        return tau;
    }

    float PitchLimitTorque(float w)
    {
        float soft = Mathf.Max(0.1f, pitchSoftZone);
        float tau = 0f;

        if (pitchDeg < pitchMin + soft)
        {
            float pen = Mathf.Deg2Rad * ((pitchMin + soft) - pitchDeg);
            tau += pitchLimitK * pen + pitchLimitC * Mathf.Max(0f, -w);
        }
        else if (pitchDeg > pitchMax - soft)
        {
            float pen = Mathf.Deg2Rad * (pitchDeg - (pitchMax - soft));
            tau += -(pitchLimitK * pen + pitchLimitC * Mathf.Max(0f, w));
        }
        return tau;
    }

    // ====== Helpers ======
    static float ToSigned(float deg) => Mathf.Repeat(deg + 180f, 360f) - 180f;
    static float Wrap360(float deg) => Mathf.Repeat(deg, 360f);
}

