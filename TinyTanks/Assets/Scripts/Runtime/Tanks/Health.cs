using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    
    [SerializeField] public int hitPoints = 3;

    void Update()
    {
        if(hitPoints <= 0)
        {
            Destroy(gameObject);
        }
    }
}
