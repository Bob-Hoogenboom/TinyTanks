using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedMine : NetworkBehaviour
{
    const int TANK_LAYER = 9;
    const int BULLET_LAYER = 6;

    [Header("Behaviour")]
    [SerializeField] private int damage = 6;
    [SerializeField] private float armingTime = 4f;
    [SerializeField] private float despawnTime = 1f;
    [SyncVar] private double _endTime;
    [SerializeField, SyncVar] private bool _isArmed = false;

    [Header("VFX")]
    [SerializeField] private ParticleSystem particleEffect;
    [SerializeField] private GameObject mineVisual;

    [Header("Light")]
    [SerializeField] private Light armingBlinker;
    [SerializeField] private float cycleOn = 0.5f;
    [SerializeField] private float cycleOff = 4f;

    [Header("SFX")]
    [SerializeField] private AudioSource tankHitAudioSource;

    [Server]
    private void Server_Initialze(float durationSeconds)
    {
        _endTime = NetworkTime.time + durationSeconds;
    }

    [ServerCallback]
    private void Start()
    {
        Server_Initialze(armingTime);
        StartCoroutine(LightBlinker());
    }

    [ServerCallback]
    void Update()
    {
        if (_isArmed == false)
        {
            double remaining = _endTime - NetworkTime.time;
            if (remaining <= 0)
                _isArmed = true;
        }
    }

    [Server]
    void Server_DeleteSelfNow()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void Server_DeleteSelfIn(float delay)
    {
        if (!isServer) return;

        Invoke(nameof(Server_DeleteSelfNow), delay);
        RpcDestroyOnClient(delay);
    }

    [ClientRpc]
    public void RpcDestroyOnClient(float delay)
    {
        Destroy(gameObject, delay);
    }

    public IEnumerator LightBlinker()
    {
        while (true)
        {
            armingBlinker.enabled = true;
            yield return new WaitForSeconds(cycleOn);
            armingBlinker.enabled = false;
            yield return new WaitForSeconds(cycleOff);
        }
    }

    [ClientRpc]
    private void RpcExplode()
    {
        if (particleEffect != null) particleEffect.Play();
        if (tankHitAudioSource != null) tankHitAudioSource.Play();
        if (mineVisual != null) mineVisual.SetActive(false);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!_isArmed) return;

        if (other.gameObject.layer == TANK_LAYER)
        {
            _isArmed = false;

            var tankBrain = other.gameObject.GetComponentInParent<TankBrain>();
            if (tankBrain != null)
            {
                tankBrain.Server_TakeDamage(damage); // new server method, see below
            }

            RpcExplode();
            Server_DeleteSelfIn(despawnTime);       // despawn on server + clients
        }
        else if (other.gameObject.layer == BULLET_LAYER)
        {
            _isArmed = false;

            RpcExplode();
            Server_DeleteSelfIn(despawnTime);
        }
    }
}
