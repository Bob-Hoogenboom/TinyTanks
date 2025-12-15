using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NetworkedMissile : NetworkBehaviour
{
    const int TANK_LAYER = 9;
    const int POWERUP_LAYER = 13;

    [Header("Behaviour")]
    [SyncVar] public TankBrain parent;
    [SerializeField] private float missileLifeTime = 10f;
    [SerializeField] private int missileSpeed = 7;

    [Header("Camera")]
    public Transform camAnchor;

    [Header("VFX")]
    [SerializeField] private GameObject bulletTrail;
    [SerializeField] private GameObject tankHitVFX;
    [SerializeField] private GameObject enviormentHitVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource bulletWhistle;
    [SerializeField] private AudioSource tankHitAudioSource;
    [SerializeField] private AudioSource enviormentHitAudioSource;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        bulletWhistle = GetComponentInChildren<AudioSource>();
    }

    private void FixedUpdate()
    {
        _rb.velocity = transform.forward * missileSpeed;

        missileLifeTime -= Time.deltaTime;
        if (missileLifeTime <= 0)
        {
            SpawnDeathVFX();
            Server_DeleteSelfNow();
        }         
    }

    public void MoveMissile(float yaw, float pitch)
    {
        var rotation = new Vector3(pitch, yaw, 0);
        _rb.transform.eulerAngles += rotation;
    }

    [Server]
    public void Server_DeleteSelfIn(float delay)
    {
        if (!isServer) return;
        Invoke(nameof(Server_DeleteSelfNow), delay);
    }

    [Server]
    private void Server_DeleteSelfNow()
    {
        parent.Server_NotifyMissileDestroyed();
        NetworkServer.Destroy(gameObject);
    }

    private void SpawnDeathVFX()
    {
        var vxf = Instantiate(enviormentHitVFX, this.transform.position, this.transform.rotation);
        vxf.transform.localScale *= 10;
        Destroy(vxf, 3);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == TANK_LAYER)
        {
            if (other.gameObject.GetComponentInParent<TankBrain>() != parent)
            {
                var vxf = Instantiate(tankHitVFX, this.transform.position, this.transform.rotation);
                Destroy(vxf, 3);
                Destroy(gameObject);
                bulletWhistle.Stop();
                var hitAudio = Instantiate(tankHitAudioSource, this.transform.position, this.transform.rotation);
                Destroy(hitAudio.gameObject, 4);

                var tankBrain = other.gameObject.GetComponentInParent<TankBrain>();
                tankBrain.health.Server_TakeDamage(parent.damage);
                Server_DeleteSelfNow();
            }
        }
        else if (other.gameObject.layer != POWERUP_LAYER)
        {
            var vxf = Instantiate(enviormentHitVFX, this.transform.position, this.transform.rotation);
            Destroy(vxf, 3);

            var hitAudio = Instantiate(enviormentHitAudioSource, this.transform.position, this.transform.rotation);
            Destroy(hitAudio.gameObject, 4);
            Destroy(bulletTrail);
            bulletWhistle.Stop();

            Server_DeleteSelfNow();
        }

    }
}