using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunner : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;

    [Header("Variables")]
    [Tooltip("Rotation speed in angles/second")]
    [Range(10, 50)]
    [SerializeField] private float rotationSpeed = 10f;



    [Header("Input Values")]
    private Vector2 _rotateVector; // only takes a/d -> y axis

    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotateVector = context.ReadValue<Vector2>();
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        float rotationAmount = _rotateVector.x * rotationSpeed * Time.deltaTime;
        this.transform.Rotate(0, rotationAmount, 0);
    }

}
