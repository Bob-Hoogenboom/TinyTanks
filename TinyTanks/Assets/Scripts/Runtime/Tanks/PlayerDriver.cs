using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDriver : MonoBehaviour
{
    [Header("References")]
    private Rigidbody _rb;
    private PlayerGunner _playerGunner;

    [Header("Variables")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _rotationSpeed = 40f;
    [SerializeField] private float _bulletSpeed = 1f;

    [SerializeField] private InputDevice _driverInput;

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _bulletParent;
    [SerializeField] private Transform _bulletSpawnLocation;

    [Header("Input Values")]
    private Vector2 _moveVector;
    private float _reloadTimer;
    private bool _isLoaded;

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveVector = context.ReadValue<Vector2>();
    }

    public void OnShoot()
    {
        Shoot();
    }


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerGunner = GetComponentInChildren<PlayerGunner>();
        _playerGunner.OnReloadComplete.AddListener(HandleGunnerReloadComplete);

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
        _rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        float moveAmount = (_moveVector.y + _moveVector.x) * 0.5f * _speed;
        Vector3 move = transform.forward * moveAmount * Time.deltaTime;
        _rb.MovePosition(_rb.position + move);

        float rotationAmount = (_moveVector.y - _moveVector.x) * _rotationSpeed * Time.deltaTime;
        Quaternion turnOffset = Quaternion.Euler(0, rotationAmount, 0);
        _rb.MoveRotation(_rb.rotation * turnOffset);
    }

    private void Shoot()
    {
        if (_isLoaded == true)
        {
            _isLoaded = false;
            Quaternion rotation = _bulletSpawnLocation.rotation * Quaternion.Euler(-90, 0, 0);
            GameObject bulletObj = Instantiate(_bulletPrefab, _bulletSpawnLocation.position, rotation);
            Rigidbody brb = bulletObj.GetComponent<Rigidbody>();
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.parent = _bulletParent.gameObject;
            brb.AddForce(_bulletSpawnLocation.forward * _bulletSpeed, ForceMode.VelocityChange);
            Destroy(bulletObj, 5f);
        }
        else
        {
            Debug.Log("Im not ready to shoot my load Senpai");
        }
    }

    private void HandleGunnerReloadComplete()
    {
        _isLoaded = true;
        Debug.Log("Gunner finished reloading, Driver can shoot again!");
    }
}
