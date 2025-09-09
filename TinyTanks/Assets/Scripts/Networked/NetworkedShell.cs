using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkedShell : NetworkBehaviour
{

    const int TANK_LAYER = 9;

    [SyncVar] public TankBrain parent;
    [SerializeField] private float shellLifeTime = 5f;
    [SerializeField] private int damage = 1;

    private void FixedUpdate()
    {
        shellLifeTime -= Time.deltaTime;
        if (shellLifeTime <= 0)
            Server_DeleteSelf(gameObject);
    }

    [Server]
    public void Server_DeleteSelf(GameObject obj)
    {
        NetworkServer.Destroy(obj);
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
