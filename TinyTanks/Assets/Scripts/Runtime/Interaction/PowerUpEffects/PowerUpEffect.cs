using UnityEngine;

/// <summary>
/// This is the abstract class every Power-Up Effect will derive from to add
/// global variables or functions in the future
/// </summary>
public abstract class PowerUpEffect : ScriptableObject
{
    [Tooltip("The duration of the Power-Up effect")]
    public float duration = 5f;

    public abstract void ApplyPowerUp(TankBrain tank);
    public abstract void RemovePowerUp(TankBrain tank);
}