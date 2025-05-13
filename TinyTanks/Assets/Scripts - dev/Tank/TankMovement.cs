using UnityEngine;
using UnityEngine.InputSystem;


public class TankMovement : MonoBehaviour
{
    public const float SmoothMovement = 0.5f;
    public const float SmoothTurning = 2f;

    [Range(0.25f, 1f)]
    public float speed = 0.5f;
    [Range(2, 10)]
    public int turnSpeed = 20;

    private PlayerInput playerInput;
    private Rigidbody tankRig;

    private float leftTrack;
    private float rightTrack;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        tankRig = GetComponent<Rigidbody>();

        // Subscribe to input events
        playerInput.onActionTriggered += HandleInput;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerInput != null)
            playerInput.onActionTriggered -= HandleInput;
    }

    private void FixedUpdate()
    {
        TankTrackMovement(leftTrack, rightTrack);
        TurnTank(leftTrack, rightTrack);
    }

    private void TankTrackMovement(float _leftValue, float _rightValue)
    {
        this.transform.position += new Vector3(0, 0, _leftValue + _rightValue);    
        //tankRig.AddForce(new Vector3(rightTrack * 0.5f + -leftTrack * 0.5f, 0, rightTrack + leftTrack) * speed, ForceMode.Impulse);
    }

    private void TurnTank(float _leftValue, float _rightValue)
    {

        this.transform.rotation *= Quaternion.Euler(0, _rightValue * 0.5f - _leftValue * 0.5f, 0);
        //float turn = (_leftValue - _rightValue) * turnSpeed * SmoothTurning * 1 / 60;
        //Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        //tankRig.MoveRotation(tankRig.rotation * turnRotation);
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        // Get the action name
        string actionName = context.action.name;

        if (actionName == "LeftTrack")
        {
            if (context.performed || context.started)
            {
                // Read the movement input value
                leftTrack = context.ReadValue<float>();
                Debug.Log($"LeftTrackMovement: {leftTrack}");
                // Your movement code here
            }
            else if (context.canceled)
            {
                leftTrack = 0;
                Debug.Log("LeftTrackMovement stopped");
                // Your code for when movement stops
            }
        }
        else if(actionName == "RightTrack")
        {
            if (context.performed || context.started)
            {
                // Read the movement input value
                rightTrack  = context.ReadValue<float>();
                Debug.Log($"RightTrackMovement: {rightTrack}");
                // Your movement code here
            }
            else if (context.canceled)
            {
                rightTrack = 0;
                Debug.Log("RightTrackMovement stopped");
                // Your code for when movement stops
            }

        }
    }

}

