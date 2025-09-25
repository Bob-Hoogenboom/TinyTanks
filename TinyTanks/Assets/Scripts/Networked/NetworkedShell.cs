using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkedShell : NetworkBehaviour
{

    const int TANK_LAYER = 9;

    [SyncVar] public TankBrain parent;
    [SerializeField] private float shellLifeTime = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int damage = 1;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        var startRotation = Random.Range(0, 360);
        Server_RotateSelf(startRotation);
    }

    private void FixedUpdate()
    {
        shellLifeTime -= Time.deltaTime;
        if (shellLifeTime <= 0)
            Server_DeleteSelf(gameObject);

        Server_RotateSelf(rotationSpeed);
    }

    [Server]
    public void Server_DeleteSelf(GameObject obj)
    {
        NetworkServer.Destroy(obj);
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
                var tankBrain = other.gameObject.GetComponentInParent<TankBrain>();
                tankBrain.TakeDamge(damage);
                Server_DeleteSelf(gameObject);
            }
        }
        else
            Server_DeleteSelf(gameObject);         
    }
}
