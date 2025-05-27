using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlls : MonoBehaviour
{
    [Header("References")]
    private Rigidbody _rb;

    [Header("Variables")]
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _turretRotationSpeed = 10f;

    [Header("Input Values")]
    private Vector2 _moveVector; // left track = W/S && right track = ^/v
    private Vector2 _rotateVector; // only takes a/d -> y axis

    [Header("Tank Components")]
    [SerializeField] private GameObject _tankBody;
    [SerializeField] private GameObject _tankTurret;

    private PlayerManager _manager;
    private int _index;

    private void Awake()
    {
        _manager = FindObjectOfType<PlayerManager>();
    }

    private void Start()
    {
        if(_index == 1)
            _tankBody = GameObject.FindGameObjectWithTag("TankBody1");
        if (_index == 2)
            _tankTurret = GameObject.FindGameObjectWithTag("TankTurret1");
        if (_index == 3)
            _tankBody = GameObject.FindGameObjectWithTag("TankBody2");
        if (_index == 4)
            _tankTurret = GameObject.FindGameObjectWithTag("TankTurret2");
    }

    private void Update()
    {
        if (_tankBody != null)
        {
            if (_rb != null)
                Move();
            else
                _rb = _tankBody.GetComponent<Rigidbody>();
        }

        if (_tankTurret != null)
        {
            if (_rb != null)
                Rotate();
            else
                _rb = _tankTurret.GetComponentInParent<Rigidbody>();
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
        _rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        float moveAmount = (_moveVector.y + _moveVector.x) * 0.5f * _speed;
        Vector3 move = transform.forward * moveAmount * Time.deltaTime;
        _rb.MovePosition(_rb.position + move);

        float rotationAmount = (_moveVector.y - _moveVector.x) * _rotationSpeed * Time.deltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, rotationAmount, 0);
        _rb.MoveRotation(_rb.rotation * turnOffset);
    }
    private void Rotate()
    {
        float rotationAmount = _rotateVector.x * _turretRotationSpeed * Time.deltaTime;
        _tankTurret.transform.Rotate(0, rotationAmount, 0);
    }

}
