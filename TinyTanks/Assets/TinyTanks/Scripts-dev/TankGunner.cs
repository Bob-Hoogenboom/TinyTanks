using UnityEngine;
using Fusion;

public class TankGunner : NetworkBehaviour
{
    [Networked] private PlayerRef Controller { get; set; }
    [SerializeField] private float turretRotationSpeed = 180f;
    [SerializeField] private Transform turretTransform;

    private Vector2 cachedTurretInput;
    private bool cachedFireInput;

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
        // Only collect input if this is the assigned gunner
        if (Controller == Runner.LocalPlayer)
        {
            // Cache the inputs to be used in OnInput callback
            cachedTurretInput = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
            cachedFireInput = Input.GetMouseButton(0);
        }
    }

    // This method should be called from the LobbyManager's OnInput callback
    public void ProcessInput(NetworkRunner runner, NetworkInput input)
    {
        // Only provide input if we're the gunner
        if (Controller == runner.LocalPlayer)
        {
            // Get existing data or create new
            var data = new NetworkInputData();

            // If the driver already set some input, we should retrieve it
            if (input.TryGet(out NetworkInputData existingData))
                data = existingData;

            // Set our gunner-specific inputs
            data.TurretRotationInput = cachedTurretInput;
            data.FireInput = cachedFireInput;

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
                // Apply turret rotation based on input
                Vector3 rotationChange = new Vector3(0, input.TurretRotationInput.x * turretRotationSpeed * Runner.DeltaTime, 0);
                turretTransform.localRotation *= Quaternion.Euler(rotationChange);

                // Handle firing logic
                if (input.FireInput)
                {
                    Fire();
                }
            }
        }
    }

    private void Fire()
    {
        // Handle firing logic (only on server)
        if (HasStateAuthority)
        {
            // Implement projectile spawning here
            // Runner.Spawn(projectilePrefab, turretTransform.position + turretTransform.forward, turretTransform.rotation);

            // Notify all clients about the firing effect
            RPC_FireEffects();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_FireEffects()
    {
        // Play firing effects on all clients
        // Implement particle systems, sound effects, etc.
    }
}
