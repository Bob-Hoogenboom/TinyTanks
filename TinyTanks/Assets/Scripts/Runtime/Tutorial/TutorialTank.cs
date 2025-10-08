using UnityEngine;

public class TutorialTank : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TankTrackPhysics tankTrack;
    [SerializeField] private TankTurretPhysics tankTurret;

    [SerializeField] private bool isDriver;

    private float _leftInput = 0f;
    private float _rightInput = 0f;



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) isDriver = !isDriver;

        if (!isDriver)
        {
            Aiming();
        }
        else
        {
            Driving();
        }
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