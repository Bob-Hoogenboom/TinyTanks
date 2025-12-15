using UnityEngine;

/// <summary>
/// The concrete RepairEffect Power-Up
/// </summary>
[CreateAssetMenu(menuName = "PowerUps/RepairEffect")]
public class RepairEffect : PowerUpEffect
{
    public override void ApplyPowerUp(TankBrain brain)
    {
        brain.health.currHealth = brain.tankData.maxHealth;
    }

    public override void RemovePowerUp(TankBrain brain)
    {
        //Dont need to remove health*
    }
}