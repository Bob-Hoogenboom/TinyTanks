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
        if (!isServer) return;

        PowerUpHandler tank = other.GetComponentInParent<PowerUpHandler>();
        if (tank != null) Server_StartPowerUp(tank);
    }

    [Server]
    private void Server_StartPowerUp(PowerUpHandler tank)
    {
        RpcOnGrabVFX();
        tank.ActivatePowerUp(effect);
        StartCoroutine(CoolDown());
    }

    [ClientRpc]
    private void RpcOnGrabVFX()
    {
        onGrabPowerUp?.Invoke();
    }

    [Server]
    private IEnumerator CoolDown()
    {
        RpcSetPickupActive(false);
        SetPickupActive(false);
        

        yield return new WaitForSeconds(coolDownTimer);

        SetPickupActive(true);
        RpcSetPickupActive(true);
    }

    [Server]
    private void SetPickupActive(bool active)
    {
        if (col) col.enabled = active;
        if (visual) visual.SetActive(active);
    }

    [ClientRpc]
    private void RpcSetPickupActive(bool active)
    {
        if (visual) visual.SetActive(active);
    }
}