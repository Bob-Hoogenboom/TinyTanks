using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// Add to the same GameObject as the TankBrain
/// The powerup handler works as a middle man for the powerups and the accual tankbrain 
/// this way tehy donmt ened a reference to eachother.
/// </summary>
public class PowerUpHandler : NetworkBehaviour
{
    [SerializeField] private TankBrain tank;

    private void Start()
    {
        if (tank == null)
        {
            tank = GetComponent<TankBrain>();
        }
    }

    [ServerCallback]
    public void ActivatePowerUp(PowerUpEffect effect)
    {
        StartCoroutine(PowerUpRoutine(effect));
    }

    private IEnumerator PowerUpRoutine(PowerUpEffect effect) 
    {
        effect.ApplyPowerUp(tank);
        yield return new WaitForSeconds(effect.duration);
        effect.RemovePowerUp(tank);
    }
}
