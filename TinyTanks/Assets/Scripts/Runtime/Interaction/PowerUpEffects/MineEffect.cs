using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/MineEffect")]
public class MineEffect : PowerUpEffect
{
    public override void ApplyPowerUp(TankBrain tank)
    {
        tank.Server_RechargeMines();
    }

    public override void RemovePowerUp(TankBrain tank)
    {
        //Dont need to remove mines*
    }
}
