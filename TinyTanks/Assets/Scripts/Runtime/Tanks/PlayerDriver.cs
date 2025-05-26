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
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float reloadCooldown = 5f;
    [SerializeField] private float _bulletSpeed = 10f;

    [SerializeField] private InputDevice _driverInput;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform turretTransform;

    [Header("Input Values")]
    private Vector2 _moveVector; // left track = W/S && right track = ^/v
    private float _reloadTimer;
    private bool _isShooting;

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveVector = context.ReadValue<Vector2>();
    }

    public void OnShoot()
    {
        _isShooting = true;
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //driverInput = manager.inputDevices[0];
    }

    private void Update()
    {

        if (_reloadTimer >= 0)
            _reloadTimer -= Time.deltaTime;

        Move();
        Shoot();
    }

    private void Move()
    {
        rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        float moveAmount = (_moveVector.y + _moveVector.x) * 0.5f * _speed;
        Vector3 move = transform.forward * moveAmount * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        float rotationAmount = (_moveVector.y - _moveVector.x) * _rotationSpeed * Time.deltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, rotationAmount, 0);
        rb.MoveRotation(rb.rotation * turnOffset);
    }

    private void Shoot()
    {
        if(_isShooting == true)
        {
            if(_reloadTimer <= 0)
            {
                _reloadTimer = reloadCooldown;
                Debug.Log("Im shooting my load UwU");
                _isShooting = false;


                GameObject bullet = Instantiate(bulletPrefab, turretTransform.position + new Vector3(0,2,0), Quaternion.Euler(90, 0, 0));
                var brb = bullet.GetComponent<Rigidbody>();
                brb.AddRelativeForce(Vector3.up * _bulletSpeed, ForceMode.VelocityChange);
                Destroy(bullet, 5f);
            }
            else
            {
                Debug.Log("Im not ready to shoot my load Senpai");
                _isShooting = false;
            }
        }
    }
}
