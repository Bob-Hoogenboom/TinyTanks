using Cinemachine;
using System;
using UnityEngine;

public enum TankRole
{
    TANK_DRIVER,
    TANK_OBSERVER
}

public class TutorialTank : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TankTrackPhysics tankTrack;
    [SerializeField] private TankTurretPhysics tankTurret;

    [SerializeField] private CinemachineVirtualCamera driverCam;
    [SerializeField] private CinemachineVirtualCamera observerCam;

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
        // --- Switch roles ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentRole = currentRole == TankRole.TANK_DRIVER ? TankRole.TANK_OBSERVER : TankRole.TANK_DRIVER;
            UpdateRoleState();
        }

        if (currentRole == TankRole.TANK_OBSERVER)
        {
            //Everything Observer related
            Aiming();
        }
        else
        {
            //everything Driver Related
            Driving();
        }
    }

    private void UpdateRoleState()
    {
        bool isDriver = currentRole == TankRole.TANK_DRIVER;

        // Toggle cameras
        if (driverCam) driverCam.enabled = isDriver;
        if (observerCam) observerCam.enabled = !isDriver;

        OnUpdateRole?.Invoke(currentRole);

        Debug.Log($"Switched to role: {currentRole}");
    }


    #region Gunner Controls
    private void Aiming()
    {
        // Reset
        _leftInput = 0f;
        _rightInput = 0f;

        // --- WASD Keys ---
        _leftInput = Input.GetAxisRaw("Vertical");
        _rightInput = Input.GetAxisRaw("Horizontal");

        tankTurret.SetInputs(_leftInput, _rightInput);
    }
    #endregion

    #region Runner Controls
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
}