using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlls : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;

    [Header("Variables")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float turretRotationSpeed = 10f;

    [Header("Input Values")]
    private Vector2 _moveVector; // left track = W/S && right track = ^/v
    private Vector2 _rotateVector; // only takes a/d -> y axis

    [Header("Tank Components")]
    [SerializeField] private GameObject tankBody;
    [SerializeField] private GameObject tankTurret;

    private PlayerManager manager;
    private int index;

    private void Awake()
    {
        manager = FindObjectOfType<PlayerManager>();
        index = manager.index;

    }

    private void Start()
    {
        if(index == 1)
            tankBody = GameObject.FindGameObjectWithTag("TankBody1");
        if (index == 2)
            tankTurret = GameObject.FindGameObjectWithTag("TankTurret1");
        if (index == 3)
            tankBody = GameObject.FindGameObjectWithTag("TankBody2");
        if (index == 4)
            tankTurret = GameObject.FindGameObjectWithTag("TankTurret2");
    }

    private void Update()
    {
        if (tankBody != null)
        {
            if (rb != null)
                Move();
            else
                rb = tankBody.GetComponent<Rigidbody>();
        }

        if (tankTurret != null)
        {
            if (rb != null)
                Rotate();
            else
                rb = tankTurret.GetComponentInParent<Rigidbody>();
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveVector = context.ReadValue<Vector2>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotateVector = context.ReadValue<Vector2>();
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
    private void Rotate()
    {
        float rotationAmount = _rotateVector.x * turretRotationSpeed * Time.deltaTime;
        tankTurret.transform.Rotate(0, rotationAmount, 0);
    }

}
