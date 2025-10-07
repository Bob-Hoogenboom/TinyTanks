using UnityEngine;

public class TutorialTank : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TankTrackPhysics tankTrack;


    private void Update()
    {
        Driving();
    }

    private void Driving()
    {
        float leftInput = 0f;
        float rightInput = 0f;

        // --- Left track (W/S keys) ---
        if (Input.GetKey(KeyCode.W))
            leftInput = 1f;
        else if (Input.GetKey(KeyCode.S))
            leftInput = -1f;

        // --- Right track (Up/Down arrows) ---
        if (Input.GetKey(KeyCode.UpArrow))
            rightInput = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            rightInput = -1f;

        tankTrack.SetInputs(leftInput, rightInput);
    }
}