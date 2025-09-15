using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tank/Turret Axis Parameters", fileName = "TurretAxisParameters")]
public class AxisParamaters : ScriptableObject
{
    [Header("Motor (at output)")]
    [Tooltip("Stall torque at the turret output [N·m].")]
    public float stallTorque = 400f;

    [Tooltip("No-load speed at full command [deg/s].")]
    public float freeSpeedDegPerSec = 35f;

    [Header("Inertia")]
    [Tooltip("Rotational inertia about this axis [kg·m²].")]
    public float inertia = 60f;

    [Header("Friction model")]
    [Tooltip("Static (breakaway) friction torque magnitude [N·m].")]
    public float tauStatic = 48f;

    [Tooltip("Coulomb (sliding) friction torque magnitude [N·m].")]
    public float tauCoulomb = 32f;

    [Tooltip("Stribeck corner speed [deg/s] (sets how quickly static drops to Coulomb).")]
    public float stribeckDegPerSec = 4f;

    [Tooltip("Mechanical viscous damping [N·m·s/rad] (bearings, grease, seals).")]
    public float bMechanical = 0.0f;

    [Header("Electrical damping when shorted (Dynamic Brake)")]
    [Tooltip("Equivalent electrical damping when the motor is shorted at idle [N·m·s/rad]. Set 0 if you don't use DynamicBrakeShort.")]
    public float bElectricalShorted = 0.0f;
}
