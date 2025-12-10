using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedMine : NetworkBehaviour
{
    const int TANK_LAYER = 9;
    const int BULLET_LAYER = 6;
    [SerializeField] private Collider triggerCol;

    [Header("Behaviour")]
    [SerializeField] private int damage = 6;
    [SerializeField] private float armingTime = 4f;
    [SerializeField] private float despawnTime = 5f;
    [SyncVar] private double _endTime;
    private bool _isArmed = false;
    private bool _hasExploded = false;
    private Rigidbody rb;

    [Header("Launch settings")]
    [SerializeField] private float launchForce = 5f;
    [SerializeField] private ForceMode launchForceMode = ForceMode.Impulse;

    [Header("VFX")]
    [SerializeField] private ParticleSystem particleEffect;
    [SerializeField] private GameObject baseMesh;
    [SerializeField] private GameObject explodedMesh;

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

        rb = GetComponent<Rigidbody>();

        if (armingBlinker)
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
        if (particleEffect) particleEffect.Play();
        if (tankHitAudioSource) tankHitAudioSource.Play();
        if (baseMesh) baseMesh.SetActive(false);
        if (explodedMesh) explodedMesh.SetActive(true);
        if (rb) rb.AddForce(Vector3.up * launchForce, launchForceMode);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!_isArmed || _hasExploded)
            return;

        int otherLayer = other.gameObject.layer;
        if (otherLayer != TANK_LAYER && otherLayer != BULLET_LAYER)
            return;

        _hasExploded = true;
        _isArmed = false;

        triggerCol.enabled = false;

        if (otherLayer == TANK_LAYER)
        {
            var tankBrain = other.GetComponentInParent<TankBrain>();
            if (tankBrain != null)
            {
                tankBrain.Server_TakeDamage(damage);
            }
        }

        if (rb)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * launchForce, launchForceMode);
        }

        RpcExplode();
        Server_DeleteSelfIn(despawnTime);
    }
}
