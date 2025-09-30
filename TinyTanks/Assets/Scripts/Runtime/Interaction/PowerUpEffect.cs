using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpEffect : ScriptableObject
{
    [Tooltip("The duration of the Power-Up effect")]
    public float duration = 5f;

    public abstract void ApplyPowerUp(TankBrain tank);
    public abstract void RemovePowerUp(TankBrain tank);
    
}
