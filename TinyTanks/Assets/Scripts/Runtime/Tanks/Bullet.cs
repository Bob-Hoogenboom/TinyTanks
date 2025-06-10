using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject parent;
    public GameObject explosionVFX;

    private void OnTriggerEnter(Collider other)
    {
        

        if (other.gameObject.layer == 3)
        {
            if(other.gameObject != parent)
            {
                if (other.gameObject.GetComponent<Health>())
                    other.gameObject.GetComponent<Health>().TakeDamage(1);
                else if (other.gameObject.GetComponentInParent<Health>())
                    other.gameObject.GetComponentInParent<Health>().TakeDamage(1);
                Debug.Log("hit a player");
                var vxf = Instantiate(explosionVFX, this.transform.position, this.transform.rotation);
                Destroy(vxf, 3);
                Destroy(gameObject);
            }         
        }
        else if(other.gameObject.layer != 3)
        {
            var vxf = Instantiate(explosionVFX, this.transform.position, this.transform.rotation);
            Destroy(vxf, 3);
            Destroy(gameObject);      
        }

    }
}
