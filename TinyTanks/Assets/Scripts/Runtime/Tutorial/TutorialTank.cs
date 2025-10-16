using Cinemachine;
using System;
using UnityEngine;

public enum TankRole
{
    TANK_DRIVER,
    TANK_OBSERVER,
    TANK_SPECTATOR
}

public class TutorialTank : MonoBehaviour, IDamagable
{
    [Header("References")]
    [SerializeField] private TankTrackPhysics tankTrack;
    [SerializeField] private TankTurretPhysics tankTurret;

    [Space]
    [SerializeField] private CinemachineVirtualCamera driverCam;
    [SerializeField] private CinemachineVirtualCamera observerCam;
    [SerializeField] private CinemachineVirtualCamera spectatorCam;

    [Header("Settings")]
    [SerializeField] private float hitpoints = 3f;
    public float HitPoints => hitpoints;

    [Header("Observer")]
    [SerializeField] private float shellSpeed = 10f;
    [SerializeField] private Transform shellSpawn;
    [SerializeField] private GameObject shellPrefab;

    [Header("State")]
    [SerializeField] private TankRole currentRole = TankRole.TANK_DRIVER;
    public static event Action <TankRole> OnUpdateRole;

    private float _leftInput = 0f;
    private float _rightInput = 0f;


    private void Start()
    {
        UpdateRoleState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentRole == TankRole.TANK_DRIVER)
                currentRole = TankRole.TANK_OBSERVER;
            else if (currentRole == TankRole.TANK_OBSERVER)
                currentRole = TankRole.TANK_DRIVER;

            UpdateRoleState();
        }

        // --- Toggle Overview Mode (map camera) ---
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (currentRole == TankRole.TANK_SPECTATOR)
            {
                // Return to previous role (default to Driver)
                currentRole = TankRole.TANK_DRIVER;
            }
            else
            {
                currentRole = TankRole.TANK_SPECTATOR;
            }

            UpdateRoleState();
        }

        if (currentRole == TankRole.TANK_OBSERVER)
        {
            //In Observer state
            Aiming();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Shoot();
            }
        }
        else
        {
            //In Driver and Spectator State
            Driving();
        }
    }

    private void UpdateRoleState()
    {
        bool isDriver = currentRole == TankRole.TANK_DRIVER;
        bool isShooter = currentRole == TankRole.TANK_OBSERVER;
        bool isOverview = currentRole == TankRole.TANK_SPECTATOR;

        if (driverCam) driverCam.enabled = isDriver;
        if (observerCam) observerCam.enabled = isShooter;
        if (spectatorCam) spectatorCam.enabled = isOverview;

        OnUpdateRole?.Invoke(currentRole);
    }


    #region Observer Controls
    private void Aiming()
    {
        // Reset
        _leftInput = 0f;
        _rightInput = 0f;

        // --- WASD Keys ---
        _leftInput = Input.GetAxisRaw("Horizontal");
        _rightInput = Input.GetAxisRaw("Vertical");

        tankTurret.SetInputs(_leftInput, _rightInput);
    }

    private void Shoot()
    {
        Quaternion rotation = shellSpawn.rotation;
        GameObject bulletObj = Instantiate(shellPrefab, shellSpawn.position, rotation);
        Rigidbody brb = bulletObj.GetComponent<Rigidbody>();
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        //bullet.parent = _bulletParent.gameObject; //Why set a parent?
        brb.AddForce(shellSpawn.forward * shellSpeed, ForceMode.VelocityChange);
        Destroy(bulletObj, 5f);
    }
    #endregion

    #region Driver Controls
    private void Driving()
    {
        // Reset
        _leftInput = 0f;
        _rightInput = 0f;

        // --- Left (W/S keys) ---
        if (Input.GetKey(KeyCode.W))
            _leftInput = 1f;
        else if (Input.GetKey(KeyCode.S))
            _leftInput = -1f;

        // --- Right (Up/Down arrows) ---
        if (Input.GetKey(KeyCode.UpArrow))
            _rightInput = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            _rightInput = -1f;

        tankTrack.SetInputs(_leftInput, _rightInput);
    }

    public void Damage(float damage)
    {
        Debug.Log("AUWW!");
        hitpoints -= damage;
        //do some damage effect here
    }
    #endregion
}