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