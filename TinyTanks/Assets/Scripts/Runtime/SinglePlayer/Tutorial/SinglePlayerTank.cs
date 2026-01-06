using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.Events;

public enum TankRole
{
    TANK_DRIVER,
    TANK_OBSERVER,
    TANK_SPECTATOR
}

public class SinglePlayerTank : MonoBehaviour, IDamagable
{
    [Header("References")]
    [SerializeField] private TankTrackPhysics tankTrack;
    [SerializeField] private TankTurretPhysics tankTurret;

    [Space]
    [SerializeField] private CinemachineVirtualCamera driverCam;
    [SerializeField] private CinemachineVirtualCamera observerCam;
    [SerializeField] private CinemachineVirtualCamera spectatorCam;

    [Header("Observer")]
    [SerializeField] private float shellSpeed = 10f;
    [SerializeField] private Transform shellSpawn;
    [SerializeField] private GameObject shellPrefab;

    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private GameObject hitIndicatorPrefab;
    [SerializeField] private float cooldown = 2f;
    private GameObject _hitIndicatorInstance;

    [Header("State")]
    public TankRole currentRole = TankRole.TANK_DRIVER;
    public static event Action <TankRole> OnUpdateRole;

    [Header("Effects and Actions")]
    public UnityEvent onShoot;

    [Header("Settings")]
    public bool canSpectate = false;
    public bool onlyObserver = false;
    [SerializeField] private float hitpoints = 3f;
    public float HitPoints => hitpoints;
     
    private float _leftInput = 0f;
    private float _rightInput = 0f;


    private void Start()
    {
        if (hitIndicatorPrefab != null)
        {
            // Create one instance to reuse (so we’re not constantly instantiating)
            _hitIndicatorInstance = Instantiate(hitIndicatorPrefab);
            _hitIndicatorInstance.SetActive(false);
        }
        UpdateRoleState();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentRole == TankRole.TANK_DRIVER)
            {
                currentRole = TankRole.TANK_OBSERVER;
            }
            else if (currentRole == TankRole.TANK_OBSERVER && !onlyObserver)
            {
                currentRole = TankRole.TANK_DRIVER;
            }

            UpdateRoleState();
        }

        // --- Toggle Overview Mode (Spectator camera) ---
        if (Input.GetKeyDown(KeyCode.M) && canSpectate)
        {
            if (currentRole == TankRole.TANK_SPECTATOR)
            {
                // Return to previous role (default to Driver)
                if (onlyObserver)
                {
                    currentRole = TankRole.TANK_OBSERVER;
                }
                else
                {
                    currentRole = TankRole.TANK_DRIVER;
                }
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
            AimIndicator();

            if (Input.GetKeyDown(KeyCode.Space) && cooldown <= 0f)
            {
                Shoot();
            }
        }
        else
        {
            //In Driver and Spectator State
            Driving();
            _hitIndicatorInstance.SetActive(false);
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
        bullet.parent = gameObject; //Set parent to check if you dont hit yourself and count a point if your bullet hits something

        brb.AddForce(shellSpawn.forward * shellSpeed, ForceMode.VelocityChange);
        Destroy(bulletObj, 5f);

        cooldown = 2f;

        onShoot.Invoke();
    }

    private void AimIndicator()
    {
        Ray ray = new Ray(shellSpawn.position, shellSpawn.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (_hitIndicatorInstance != null)
            {
                _hitIndicatorInstance.SetActive(true);
                _hitIndicatorInstance.transform.position = hit.point;
                _hitIndicatorInstance.transform.rotation = Quaternion.identity; // optional: align with hit.normal
            }
            else
            {
                // Debug sphere if no prefab is assigned
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                Debug.DrawLine(hit.point, hit.point + hit.normal * 0.3f, Color.yellow);
            }
        }
        else
        {
            if (_hitIndicatorInstance != null)
                _hitIndicatorInstance.SetActive(false);
        }

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

    #endregion

    public void Damage(float damage)
    {
        Debug.Log("AUWW!");
        hitpoints -= damage;
        //do some damage effect here
    }
}