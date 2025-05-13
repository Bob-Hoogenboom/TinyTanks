using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDriver : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;

    [Header("Variables")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 10f;



    [Header("Input Values")]
    private Vector2 _moveVector; // left track = W/S && right track = ^/v


    public void OnMove(InputAction.CallbackContext context)
    {
        _moveVector = context.ReadValue<Vector2>();
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        float moveAmount = (_moveVector.y + _moveVector.x) * 0.5f * speed;
        Vector3 move = transform.forward * moveAmount * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        float rotationAmount = (_moveVector.y - _moveVector.x) * rotationSpeed * Time.deltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, rotationAmount, 0);
        rb.MoveRotation(rb.rotation * turnOffset);
    }
}
