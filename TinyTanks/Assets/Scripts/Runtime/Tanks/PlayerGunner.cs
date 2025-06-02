using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGunner : MonoBehaviour
{
    [Header("References")]
    private Rigidbody _rb;

    [Header("Variables")]
    [Tooltip("Rotation speed in angles/second")]
    [Range(10, 50)]
    [SerializeField] private float _rotationSpeed = 10f;

    [SerializeField] private InputDevice _gunnerInput;


    [Header("Input Values")]
    private Vector2 _rotateVector; // only takes a/d -> y axis

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        //gunnerInput = manager.inputDevices[1];
    }

    private void Update()
    {
        Rotate();
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotateVector = context.ReadValue<Vector2>();
        Debug.Log(_rotateVector);
    }

    private void Rotate()
    {
        float rotationAmount = _rotateVector.x * _rotationSpeed * Time.deltaTime;
        this.transform.Rotate(0, rotationAmount, 0);
    }

}
