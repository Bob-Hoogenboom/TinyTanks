using UnityEngine;
using Fusion;

public class TankGunner : NetworkBehaviour
{
    [Networked] private PlayerRef Controller { get; set; }
    [Networked] private float TurretYRotation { get; set; }

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

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && Controller != PlayerRef.None)
        {
            if (Runner.TryGetInputForPlayer<NetworkInputData>(Controller, out var input))
            {
                // Apply turret rotation and write it to the networked property
                float deltaY = input.TurretRotationInput.x * turretRotationSpeed * Runner.DeltaTime;
                TurretYRotation += deltaY;
                TurretYRotation = Mathf.Repeat(TurretYRotation, 360f); // keep angle within 0-360

                Debug.Log(TurretYRotation);

                // Fire if needed
                if (input.FireInput)
                    Fire();
            }
        }
    }

    private void Fire()
    {
        if (HasStateAuthority)
        {
            // TODO: Spawn projectile here
            RPC_FireEffects();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_FireEffects()
    {
        // TODO: Play particle/sound effects
    }
}