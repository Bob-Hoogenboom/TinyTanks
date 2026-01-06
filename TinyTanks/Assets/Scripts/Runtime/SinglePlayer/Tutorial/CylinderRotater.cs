using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderRotater : MonoBehaviour
{
    

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed;
    private float y = 90f;
    private float x;
    private float z = -90f;

    void Update()
    {
        x += rotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(x, y, z);
    }

}
