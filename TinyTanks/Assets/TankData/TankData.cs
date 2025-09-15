
using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Track Surface Variables")]
public class TankData : ScriptableObject
{
    [Header("Engine")]
    [Tooltip("Engine power in horsepower (HP). Higher number = stronger engine push at higher speeds.")]
    public float enginePowerHP;
    public float EnginePowerW => enginePowerHP * 735.5f;
    [Tooltip("Hard cap on engine push per track (Newtons). Limits how hard a track can shove the tank forward at very low speed.")]
    public float maxForcePerTrack;

    [Header("Grip & Resistance")]
    [Tooltip("Forward/back grip number (dimensionless). Higher = better traction for accelerating and braking.")]
    public float muLong;
    [Tooltip("Sideways grip number (dimensionless). Higher = less sliding sideways.")]
    public float muLat;
    [Tooltip("Always on slow down from tracks touching the ground (rolling drag). Higher = coasts less.")]
    public float rollingResistance;
    [Tooltip("Speed based slow down. The faster you go, the stronger this backward push.")]
    public float speedDrag;

    [Header("Contact Patch & Yaw Control")]
    [Tooltip("Distance from the track’s center to where we apply side slow down forces (front/back), in meters. Higher = more twist effect to resist spins.")]
    public float contactHalfLength;
    [Tooltip("Low speed side slow down factor. Helps stop gentle spins and sideways drift.")]
    public float linearLatDrag;
    [Tooltip("High speed side slow down factor. Gets much stronger when sliding fast; resists quick spins.")]
    public float quadLatDrag;

    [Header("Side Grip Tuning")]
    [Tooltip("Side grip strength. How quickly side force builds when you start to slide sideways.")]
    public float lateralStiffness;

    [Header("Braking & Steering")]
    [Tooltip("Maximum brake push per track (Newtons). Higher = stronger braking.")]
    public float trackBrakeForce;
    [Tooltip("Input threshold (0..1). Below this we treat it like no throttle and let the brakes stop the track.")]
    public float brakeDeadzone;
    [Tooltip("Extra braking when the other track is driving (helps pivot turns). 1 = off, >1 = stronger extra brake.")]
    public float steerBrakeMultiplier;
    [Tooltip("Extra engine twist when tracks spin opposite ways (pivot-in-place). 1 = off, >1 = stronger twist boost.")]
    public float neutralSteerTorqueBoost;
}