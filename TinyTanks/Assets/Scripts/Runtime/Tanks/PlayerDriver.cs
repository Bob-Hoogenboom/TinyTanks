using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerDriver : MonoBehaviour
{

    [Header("Events")]
    public UnityEvent OnShootComplete;

    [Header("References")]
    private Rigidbody _rb;
    private PlayerGunner _playerGunner;

    [Header("Physics Layers")]
    public LayerMask enviormentLayer;

    [Header("Audio")]
    public AudioSource driveIntoEnviornmentAudio;
    public float minImpactSpeed;
    public AudioSource startDrivingAudio;
    public AudioSource duringDrivingAudio;
    public AudioSource stopDrivingAudio;
    public AudioSource idleAudio;
    float playThreshold = 0.1f;
    private bool isDriving = false;

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
    private bool _isLoaded = true;

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
    private void Start()
    {
        idleAudio.Play();
    }

    private void Update()
    {

        if (_reloadTimer >= 0)
            _reloadTimer -= Time.deltaTime;

        Move();
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

        if (Mathf.Abs(moveAmount) > playThreshold)
        {
            if (!isDriving)
            {
                idleAudio.Stop();
                startDrivingAudio.Play();
                duringDrivingAudio.PlayDelayed(1);
                isDriving = true;
            }
        }
        else if (isDriving)
        {
            startDrivingAudio.Stop();
            duringDrivingAudio.Stop();
            stopDrivingAudio.Play();
            idleAudio.PlayDelayed(1);
            isDriving = false;
        }

    }

    private void Shoot()
    {
        if (_isLoaded == true)
        {
            _isLoaded = false;
            Quaternion rotation = _bulletSpawnLocation.rotation;
            GameObject bulletObj = Instantiate(_bulletPrefab, _bulletSpawnLocation.position, rotation);
            Rigidbody brb = bulletObj.GetComponent<Rigidbody>();
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.parent = _bulletParent.gameObject;
            brb.AddForce(_bulletSpawnLocation.forward * _bulletSpeed, ForceMode.VelocityChange);
            Destroy(bulletObj, 5f);
            OnShootComplete.Invoke();
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

    private void OnCollisionEnter(Collision collision)
    {
        if(((1<<collision.collider.gameObject.layer) & enviormentLayer) == 0)
            return;

        // Would be nice to add this, but need better hitboxes and physics for this to work
        //float speed = collision.relativeVelocity.magnitude; 
        //Debug.Log(speed);
        //if (speed < minImpactSpeed)
        //    return;

        driveIntoEnviornmentAudio.Play();
    }
}
