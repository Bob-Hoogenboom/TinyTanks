using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

/// <summary>
/// This script accually goesd on the power-up gameObject
/// When the player drives against this, it will activate the effect given to this object
/// </summary>
public class PowerUp : NetworkBehaviour
{
    [Header("Variables")]

    [Tooltip("Add the Power-Up Effect you wish this object to give the TankBrain Object")]
    [SerializeField] private PowerUpEffect effect;

    [SerializeField] private float coolDownTimer = 20f;

    [Header("References")]

    [SerializeField] private Collider col;
    [SerializeField] private GameObject visual;

    [Space]
    [Tooltip("Add a power-up effect or sound to this event to play when its grabbed")]
    [SerializeField] private UnityEvent onGrabPowerUp;


    private void OnTriggerEnter(Collider other)
    {
        PowerUpHandler tank = other.GetComponentInParent<PowerUpHandler>();
        if (tank != null) StartPowerUp(tank);
    }

    [Server]
    private void StartPowerUp(PowerUpHandler tank)
    {
        onGrabPowerUp.Invoke();
        tank.ActivatePowerUp(effect);

        StartCoroutine(CoolDown());
    }

    [Server]
    private IEnumerator CoolDown()
    {
        col.enabled = false;
        visual.SetActive(false);
        Debug.Log("Cooldown start");

        yield return new WaitForSeconds(coolDownTimer);

        Debug.Log("Cooldown end");
        col.enabled = true;
        visual.SetActive(true);
    }
}