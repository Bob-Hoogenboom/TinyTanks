using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Track Surface Variables")]
public class TankData : ScriptableObject
{
    [Header("Engine")]
    public float enginePowerHP;
    public float EnginePowerW => enginePowerHP * 735.5f;
    public float maxForcePerTrack;

    [Header("Grip & Resistance")]
    public float muLong;
    public float muLat;
    public float rollingResistance;
    public float speedDrag;

    public float contactHalfLength;   // half the track-ground footprint length (m)
    public float linearLatDrag; // N·s/m
    public float quadLatDrag;  // N·s²/m²

    [Header("Tuning")]
    public float lateralStiffness;

    [Header("Braking & Steer Assist")]
    public float trackBrakeForce;
    public float brakeDeadzone;
    public float steerBrakeMultiplier;
    public float neutralSteerTorqueBoost;
}
