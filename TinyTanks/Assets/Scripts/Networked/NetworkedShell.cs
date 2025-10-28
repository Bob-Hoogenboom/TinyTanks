using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkedShell : NetworkBehaviour
{

    const int TANK_LAYER = 9;

    [Header("Behaviour")]
    [SyncVar] public TankBrain parent;
    [SerializeField] private float shellLifeTime = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int damage = 1;

    [Header("VFX")]
    [SerializeField] private GameObject bulletTrail;
    [SerializeField] private GameObject tankHitVFX;
    [SerializeField] private GameObject enviormentHitVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource bulletWhistle;
    [SerializeField] private AudioSource tankHitAudioSource;
    [SerializeField] private AudioSource enviormentHitAudioSource;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        var startRotation = Random.Range(0, 360);
        Server_RotateSelf(startRotation);

        bulletWhistle = GetComponentInChildren<AudioSource>();
    }

    private void FixedUpdate()
    {
        shellLifeTime -= Time.deltaTime;
        if (shellLifeTime <= 0)
            Server_DeleteSelfNow();

        Server_RotateSelf(rotationSpeed);
    }

    [Server]
    public void Server_DeleteSelfIn(float delay)
    {
        if (!isServer) return;
        Invoke(nameof(Server_DeleteSelfNow), delay);
    }

    [Server]
    void Server_DeleteSelfNow()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void Server_RotateSelf(float newRotation)
    {
        var angles = transform.rotation.eulerAngles;
        angles.z += newRotation;
        Quaternion newRot = Quaternion.Euler(angles);
        rb.MoveRotation(newRot);       
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
                tankBrain.TakeDamge(damage);
                Server_DeleteSelfNow();
            }
        }
        else
        {
            var vxf = Instantiate(enviormentHitVFX, this.transform.position, this.transform.rotation);
            Destroy(vxf, 3);

            var hitAudio = Instantiate(enviormentHitAudioSource, this.transform.position, this.transform.rotation);
            Destroy(hitAudio.gameObject, 4);

            Collider col = GetComponent<Collider>();
            col.isTrigger = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Server_DeleteSelfIn(5);
            Destroy(bulletTrail);
            bulletWhistle.Stop();
            this.enabled = false;
        }
                   
    }


}
