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
    [Range(0f, 180f)] [SerializeField] private float yawHalfRange = 0f;
    [Tooltip("Soft zone width before hard yaw limit [deg].")]
    [SerializeField] private float yawSoftZone = 8f;
    [Tooltip("Soft limit spring [N·m/rad].")]
    [SerializeField] private float yawLimitK = 1200f;
    [Tooltip("Soft limit damper [N·m·s/rad].")]
    [SerializeField] private float yawLimitC = 80f;

    [Header("Pitch Limits")]
    [Tooltip("Min / Max elevation [deg].")]
    [SerializeField] private float pitchMin = -10f, pitchMax = 30f;
    [Tooltip("Soft zone width before hard pitch limit [deg].")]
    [SerializeField] private float pitchSoftZone = 5f;
    [Tooltip("Soft limit spring [N·m/rad].")]
    [SerializeField] private float pitchLimitK = 1500f;
    [Tooltip("Soft limit damper [N·m·s/rad].")]
    [SerializeField] private float pitchLimitC = 100f;

    [Header("Pitch Gravity (optional)")]
    [Tooltip("Effective mass acting on elevation [kg].")]
    [SerializeField] private float pitchMEff = 0f;
    [Tooltip("Lever arm from axis to CoM [m].")]
    [SerializeField] private float pitchRM = 0f;

    [Header("Anti-Jitter Sleep")]
    [Tooltip("If |angular speed| is below this (deg/s) with no input, axis can sleep.")]
    [SerializeField] private float sleepSpeedDeg = 0.05f;
    [Tooltip("If |net torque| is below this (N·m) with no input, axis can sleep.")]
    [SerializeField] private float sleepTorque = 0.25f;
    [Tooltip("Input needed to wake the yaw from sleep.")]
    [SerializeField] private float yawWakeInput = 0.02f;
    [Tooltip("Snap grid for yaw when sleeping or near rest (deg).")]
    [SerializeField] private float yawAngleSnapDeg = 0.005f;

    // Runtime state
    private float _yawRelDeg, _yawRateDeg;     // yaw relative to start
    private float _yawStartDeg;
    private float _pitchDeg, _pitchRateDeg;
    private bool _yawSleeping = false;
    private float _yawSleepAngleDeg = 0f;

    private float _cmdYaw, _cmdPitch; // [-1..1]

    // Input from your seat controller
    public void SetInputs(float yawAxis, float pitchAxis)
    {
        _cmdYaw = Mathf.Abs(yawAxis) < inputDeadzone ? 0f : Mathf.Clamp(yawAxis, -1f, 1f);
        _cmdPitch = Mathf.Abs(pitchAxis) < inputDeadzone ? 0f : Mathf.Clamp(pitchAxis, -1f, 1f);
    }

    void Awake()
    {
        if (yawPivot) _yawStartDeg = ToSigned(yawPivot.localEulerAngles.y);
        if (pitchPivot)
        {
            float x = pitchPivot.localEulerAngles.x;
            _pitchDeg = (x > 180f) ? -(360f - x) : -x; // convert Unity X (right-handed) to our elevation sign
            _pitchDeg = Mathf.Clamp(_pitchDeg, pitchMin, pitchMax);
        }
    }

    void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        float dt = Time.fixedDeltaTime;

        // --- YAW ---
        {
            float angularVelocity = Mathf.Deg2Rad * _yawRateDeg;
            float motor = MotorTorque(yawSettings, _cmdYaw, angularVelocity, idleModeYaw);
            float fric = FrictionTorque(yawSettings, angularVelocity);
            float tLimit = YawLimitTorque(angularVelocity);
            float netTau = motor - fric - tLimit;

            // Wake condition: real stick movement or actual torque
            bool wantWake = Mathf.Abs(_cmdYaw) > yawWakeInput || Mathf.Abs(netTau) >= sleepTorque;

            // Sleep condition: tiny speed, tiny torque, and no input
            bool canSleep =
                Mathf.Abs(_cmdYaw) <= yawWakeInput &&
                Mathf.Abs(angularVelocity) < Mathf.Deg2Rad * Mathf.Max(0.001f, sleepSpeedDeg) &&
                Mathf.Abs(netTau) < Mathf.Max(0f, sleepTorque);

            // Latch sleeping
            if (_yawSleeping)
            {
                if (wantWake)
                    _yawSleeping = false;
            }
            else if (canSleep)
            {
                _yawSleeping = true;
                // Snap to a stable angle when entering sleep
                float worldYawDeg = _yawStartDeg + _yawRelDeg;
                if (yawAngleSnapDeg > 0f)
                    worldYawDeg = Mathf.Round(worldYawDeg / yawAngleSnapDeg) * yawAngleSnapDeg;
                _yawSleepAngleDeg = worldYawDeg;
            }

            if (_yawSleeping)
            {
                // Hold perfectly still at the snapped angle
                _yawRateDeg = 0f;
                _yawRelDeg = _yawSleepAngleDeg - _yawStartDeg;
            }
            else
            {
                // Normal integrate
                float alpha = netTau / Mathf.Max(EPS, yawSettings.inertia);
                angularVelocity += alpha * dt;
                _yawRateDeg = angularVelocity * Mathf.Rad2Deg;
                _yawRelDeg += _yawRateDeg * dt;

                // Limits: stop overshoot from re-waking the axis
                if (yawHalfRange > 0f)
                {
                    float left = -yawHalfRange, right = yawHalfRange;
                    if (_yawRelDeg < left) { _yawRelDeg = left; if (_yawRateDeg < 0f) _yawRateDeg = 0f; }
                    if (_yawRelDeg > right) { _yawRelDeg = right; if (_yawRateDeg > 0f) _yawRateDeg = 0f; }
                }

                // Gentle snap even when awake near rest to avoid micro dithering
                if (Mathf.Abs(_yawRateDeg) < sleepSpeedDeg * 1.5f && Mathf.Abs(_cmdYaw) <= yawWakeInput && yawAngleSnapDeg > 0f)
                {
                    float worldYawDeg = _yawStartDeg + _yawRelDeg;
                    worldYawDeg = Mathf.Round(worldYawDeg / yawAngleSnapDeg) * yawAngleSnapDeg;
                    _yawRelDeg = worldYawDeg - _yawStartDeg;
                }
            }

            if (yawPivot)
            {
                float wrapped = Wrap360(_yawStartDeg + _yawRelDeg);
                yawPivot.localRotation = Quaternion.Euler(0f, wrapped, 0f);
            }
        }

        // --- PITCH ---
        {
            float angularVelocity = Mathf.Deg2Rad * _pitchRateDeg;
            float theta = Mathf.Deg2Rad * _pitchDeg;

            float motor = MotorTorque(pitchSettings, _cmdPitch, angularVelocity, idleModePitch);
            float fric = FrictionTorque(pitchSettings, angularVelocity);
            float tGrav = (pitchMEff > 0f && pitchRM > 0f) ? pitchMEff * 9.81f * pitchRM * Mathf.Sin(theta) : 0f;
            float tLimit = PitchLimitTorque(angularVelocity);

            // NEW: compute net torque and sleep if near-zero
            float netTau = motor - fric - tGrav - tLimit;
            bool wantSleep =
                Mathf.Approximately(_cmdPitch, 0f) &&
                Mathf.Abs(angularVelocity) < Mathf.Deg2Rad * Mathf.Max(0.001f, sleepSpeedDeg) &&
                Mathf.Abs(netTau) < Mathf.Max(0f, sleepTorque);

            if (wantSleep)
            {
                angularVelocity = 0f;
                _pitchRateDeg = 0f;
            }
            else
            {
                float alpha = netTau / Mathf.Max(EPS, pitchSettings.inertia);
                angularVelocity += alpha * dt;
                _pitchRateDeg = angularVelocity * Mathf.Rad2Deg;
            }

            _pitchDeg += _pitchRateDeg * dt;

            if (_pitchDeg < pitchMin) { _pitchDeg = pitchMin; if (_pitchRateDeg < 0f) _pitchRateDeg = 0f; }
            if (_pitchDeg > pitchMax) { _pitchDeg = pitchMax; if (_pitchRateDeg > 0f) _pitchRateDeg = 0f; }

            if (pitchPivot)
                pitchPivot.localRotation = Quaternion.Euler(-_pitchDeg, 0f, 0f);
        }
    }

    static float MotorTorque(AxisParamaters _params, float u, float angularVelocity, IdleElectricalMode idleMode)
    {
        // Convert free speed to rad/s
        float wFree = Mathf.Deg2Rad * Mathf.Max(1e-3f, _params.freeSpeedDegPerSec);

        if (Mathf.Abs(u) > 1e-3f)
        {
            // Linear motor model: tau = tau_stall * (u - w/w_free)
            return _params.stallTorque * (Mathf.Clamp(u, -1f, 1f) - angularVelocity / wFree);
        }

        // Idle behavior uses only data from SO
        switch (idleMode)
        {
            case IdleElectricalMode.DynamicBrakeShort: return -_params.dynamicBrake * angularVelocity;
            case IdleElectricalMode.HoldZero: return -_params.stallTorque * (angularVelocity / wFree); // proportional to speed
            default: return 0f; // CoastOpen
        }
    }

    static float FrictionTorque(AxisParamaters _params, float angularVelocity)
    {
        float absw = Mathf.Abs(angularVelocity);
        float sgn = Mathf.Sign(angularVelocity);

        // Stribeck (static->coulomb) + Coulomb
        float wStr = Mathf.Deg2Rad * Mathf.Max(0.01f, _params.stribeckDegPerSec);
        float strib = (_params.tauStatic - _params.tauCoulomb) * Mathf.Exp(-Mathf.Pow(absw / wStr, 2f));
        float dry = (_params.tauCoulomb + strib) * sgn;

        // Viscous
        float visc = _params.bMechanical * angularVelocity;

        // Stick region clamp near zero speed
        if (absw < 0.01f) return Mathf.Clamp(dry + visc, -_params.tauStatic, _params.tauStatic);
        return dry + visc;
    }

    float YawLimitTorque(float angularVelocity)
    {
        if (yawHalfRange <= 0f) return 0f;
        float soft = Mathf.Max(0.1f, yawSoftZone);
        float left = -yawHalfRange, right = yawHalfRange;

        float tau = 0f;
        if (_yawRelDeg < left + soft)
        {
            float pen = Mathf.Deg2Rad * ((left + soft) - _yawRelDeg);
            tau += yawLimitK * pen + yawLimitC * Mathf.Max(0f, -angularVelocity);
        }
        else if (_yawRelDeg > right - soft)
        {
            float pen = Mathf.Deg2Rad * (_yawRelDeg - (right - soft));
            tau += -(yawLimitK * pen + yawLimitC * Mathf.Max(0f, angularVelocity));
        }
        return tau;
    }

    float PitchLimitTorque(float angularVelocity)
    {
        float soft = Mathf.Max(0.1f, pitchSoftZone);
        float tau = 0f;

        if (_pitchDeg < pitchMin + soft)
        {
            float pen = Mathf.Deg2Rad * ((pitchMin + soft) - _pitchDeg);
            tau += pitchLimitK * pen + pitchLimitC * Mathf.Max(0f, -angularVelocity);
        }
        else if (_pitchDeg > pitchMax - soft)
        {
            float pen = Mathf.Deg2Rad * (_pitchDeg - (pitchMax - soft));
            tau += -(pitchLimitK * pen + pitchLimitC * Mathf.Max(0f, angularVelocity));
        }
        return tau;
    }

    // ====== Helpers ======
    static float ToSigned(float deg) => Mathf.Repeat(deg + 180f, 360f) - 180f;
    static float Wrap360(float deg) => Mathf.Repeat(deg, 360f);
}

