using UnityEngine;
using Fusion;

public class TankDriver : NetworkBehaviour
{
    [Networked] private PlayerRef Controller { get; set; }
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 120f;

    private Vector2 cachedMoveInput;

    public void AssignControl(PlayerRef player)
    {
        Controller = player;
    }

    public void ClearControl()
    {
        Controller = PlayerRef.None;
    }

    private void Update()
    {
        // Only collect input if this is the assigned driver
        if (Controller == Runner.LocalPlayer)
        {
            // Cache the inputs to be used in OnInput callback
            cachedMoveInput = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );
        }
    }

    // This method should be called from the LobbyManager's OnInput callback
    public void ProcessInput(NetworkRunner runner, NetworkInput input)
    {
        // Only provide input if we're the driver
        if (Controller == runner.LocalPlayer)
        {
            var data = new NetworkInputData();
            data.TankMoveInput = cachedMoveInput;

            // Set the input data through the provided NetworkInput parameter
            input.Set(data);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Process inputs on the server/host
        if (HasStateAuthority && Controller != PlayerRef.None)
        {
            // Get input for the controller player
            if (Runner.TryGetInputForPlayer<NetworkInputData>(Controller, out var input))
            {
                // Apply movement based on input
                transform.position += transform.forward * input.TankMoveInput.y * moveSpeed * Runner.DeltaTime;
                transform.Rotate(0, input.TankMoveInput.x * rotationSpeed * Runner.DeltaTime, 0);
            }
        }
    }
}