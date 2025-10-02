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
        float newBtry = brain.currentBtry + battery;

        if(newBtry > brain.maxBtry)
        {
            newBtry = brain.maxBtry;
        }

        brain.currentBtry = newBtry;
    }

    public override void RemovePowerUp(TankBrain brain)
    {
        //Dont need to remove battery*
    }
}
