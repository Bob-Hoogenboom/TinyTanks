using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Track Surface Variables")]
public class TankData : ScriptableObject
{
    [Header("Engine")]
    [Tooltip("Engine power in horsepower (HP). Converted at runtime to watts via EnginePowerW (1 HP = 735.5 W). Affects drive at medium–high speeds via power limit.")]
    public float enginePowerHP;
    public float EnginePowerW => enginePowerHP * 735.5f;
    [Tooltip("Hard cap on DRIVE FORCE per track (Newtons). Final drive limit uses: engineForceCap = min(maxForcePerTrack, EnginePowerW / max(0.5, |velocityFwd|)). Lower values limit launch/low-speed push; higher values shift limit to engine power.")]
    public float maxForcePerTrack;

    [Header("Grip & Resistance")]
    [Tooltip("Longitudinal friction coefficient μx (dimensionless). Caps forward drive & braking: tractionLong = μx * normalForce (normalForce ≈ m·g/2 per track).")]
    public float muLong;
    [Tooltip("Lateral friction coefficient μy (dimensionless). Caps lateral grip: tractionLat = μy * normalForce. Higher = stronger side grip (until capped).")]
    public float muLat;
    [Tooltip("Rolling-resistance coefficient c_rr (dimensionless). Speed-independent drag along forward: ForceRollingRessistance = c_rr * normalForce. Higher = coasts to a stop sooner.")]
    public float rollingResistance;
    [Tooltip("Linear speed-drag coefficient (N per m/s). Additional forward drag growing with speed: DragForce = speedDrag * velocityFwd. Higher = more high-speed resistance.")]
    public float speedDrag;

    [Tooltip("Half-length of the track/ground contact patch along the forward axis (meters). Forces are applied at hit.point ± contactHalfLength to create yaw-resisting lateral drag.")]
    public float contactHalfLength;
    [Tooltip("Linear coefficient for lateral drag at the front/rear application points (N·s/m). Used in: F = -(linearLatDrag * velocityLat + quadLatDrag * velocityLat * |velocityLat|). Dominates at low lateral speeds.")]
    public float linearLatDrag;
    [Tooltip("Quadratic coefficient for lateral drag at the front/rear application points (N·s²/m²). Same equation as above; dominates at higher lateral speeds to resist fast spins.")]
    public float quadLatDrag;

    [Header("Tuning")]
    [Tooltip("Cornering-stiffness–like coefficient (N·s/m). Maps lateral slip velocity to lateral grip before friction capping: ForceLat = clamp(-velocityLat * lateralStiffness, ±tractionLat). Higher = stronger lateral grip / less side-slip.")]
    public float lateralStiffness;

    [Header("Braking & Steer Assist")]
    [Tooltip("Max braking force per track (Newtons). Actual brake is min(trackBrakeForce, tractionLong). Applied when input is released or opposes motion; can be boosted for pivot turns.")]
    public float trackBrakeForce;
    [Tooltip("Input magnitude threshold (0..1). If |input| < brakeDeadzone the track is considered released and clutch-brake engages to stop residual motion.")]
    public float brakeDeadzone;
    [Tooltip("Multiplier applied to braking when the opposite track is being driven (|otherInput| > 0.2). >1 boosts pivot-turn braking; <1 softens it.")]
    public float steerBrakeMultiplier;
    [Tooltip("Multiplier applied to the engine force cap during neutral steer (tracks commanded in opposite directions; input*otherInput < -0.2). Boosts torque for pivot turns.")]
    public float neutralSteerTorqueBoost;
}