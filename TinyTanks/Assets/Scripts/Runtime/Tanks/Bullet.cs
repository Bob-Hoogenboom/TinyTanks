using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject parent;

    [SerializeField] private GameObject _tankHitVFX;
    [SerializeField] private GameObject _enviormentHitVFX;
    [SerializeField] private GameObject _bulletTrailVFX;

    private GameObject _bulletTrail;

    private void Start()
    {
        _bulletTrail = Instantiate(_bulletTrailVFX, this.transform.position, this.transform.rotation, this.gameObject.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        

        if (other.gameObject.layer == 3)
        {
            Debug.Log(other.gameObject.name);
            if(other.gameObject != parent)
            {
                if (other.gameObject.GetComponent<Health>())
                    other.gameObject.GetComponent<Health>().TakeDamage(1);
                else if (other.gameObject.GetComponentInParent<Health>())
                    other.gameObject.GetComponentInParent<Health>().TakeDamage(1);
                Debug.Log("hit a player");
                var vxf = Instantiate(_tankHitVFX, this.transform.position, this.transform.rotation);
                Destroy(vxf, 3);
                Destroy(_bulletTrail, 1);
                Destroy(gameObject);
            }         
        }
        else if(other.gameObject.layer != 3 && other.gameObject.layer != 7)
        {
            var vxf = Instantiate(_enviormentHitVFX, this.transform.position, this.transform.rotation);
            Destroy(vxf, 3);
            Destroy(_bulletTrail, 1);
            Destroy(gameObject);      
        }

    }
}
