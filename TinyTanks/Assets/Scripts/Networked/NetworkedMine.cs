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

    private void Start()
    {
        Server_Initialze(armingTime);
        StartCoroutine(LightBlinker());
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (!_isArmed) return;

        Debug.Log(other.gameObject.name);

        if (other.gameObject.layer == TANK_LAYER)
        {
            _isArmed = false;

            particleEffect.Play();
            tankHitAudioSource.Play();
            mineVisual.SetActive(false);

            var tankBrain = other.gameObject.GetComponentInParent<TankBrain>();
            tankBrain.TakeDamge(damage);

            Server_DeleteSelfIn(despawnTime);
        }
        else if (other.gameObject.layer == BULLET_LAYER)
        {
            _isArmed = false;

            particleEffect.Play();
            tankHitAudioSource.Play();

            mineVisual.SetActive(false);
            Server_DeleteSelfIn(despawnTime);
        }
    }
}
