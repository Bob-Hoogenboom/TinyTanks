using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    [SerializeField] private GameObject cameraAnchor;

    private void FixedUpdate()
    {
        this.transform.position = cameraAnchor.transform.position;
    }
}
