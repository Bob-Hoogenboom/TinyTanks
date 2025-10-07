using UnityEngine;

/// <summary>
/// Adds battery power to your counter
/// </summary>
[CreateAssetMenu(menuName = "PowerUps/BatteryEffect")]
public class BatteryEffect : PowerUpEffect
{
    [SerializeField] private float battery;

    public override void ApplyPowerUp(TankBrain brain)
    {
        brain.Server_RechargeBattery(battery);
    }

    public override void RemovePowerUp(TankBrain brain)
    {
        //Dont need to remove battery*
    }
}
