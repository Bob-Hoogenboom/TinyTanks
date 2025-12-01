using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/MissileEffect")]
public class MissileEffect : PowerUpEffect
{
    public override void ApplyPowerUp(TankBrain tank)
    {
        tank.Server_LoadMissile();
    }

    public override void RemovePowerUp(TankBrain tank)
    {
        
    }
}
