using UnityEngine;
using Fusion;

public class SharedTank : NetworkBehaviour
{
    // References to components
    [SerializeField] private TankDriver tankDriver;
    [SerializeField] private TankGunner TankGunner;

    // Network properties to track roles
    [Networked] private PlayerRef DriverPlayer { get; set; }
    [Networked] private PlayerRef GunnerPlayer { get; set; }
    [Networked] private NetworkBool HasDriver { get; set; }
    [Networked] private NetworkBool HasGunner { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            // Initialize with no roles assigned
            HasDriver = false;
            HasGunner = false;
            DriverPlayer = PlayerRef.None;
            GunnerPlayer = PlayerRef.None;
        }
    }

    // RPC from server to assign a role to a player
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_AssignRole(PlayerRef player, bool isDriver)
    {
        if (isDriver)
        {
            DriverPlayer = player;
            HasDriver = true;
            tankDriver.AssignControl(player);

            // Notify player via UI
            if (player == Runner.LocalPlayer)
            {
                Debug.Log("You are now the driver!");
                //UIManager.Instance.ShowRoleAssignment("Driver");
            }
        }
        else
        {
            GunnerPlayer = player;
            HasGunner = true;
            TankGunner.AssignControl(player);

            // Notify player via UI
            if (player == Runner.LocalPlayer)
            {
                Debug.Log("You are now the gunner!");
                //UIManager.Instance.ShowRoleAssignment("Gunner");
            }
        }
    }

    // RPC from server when a player with a role leaves
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayerLeftRole(bool wasDriver)
    {
        if (wasDriver)
        {
            HasDriver = false;
            DriverPlayer = PlayerRef.None;
            tankDriver.ClearControl();
        }
        else
        {
            HasGunner = false;
            GunnerPlayer = PlayerRef.None;
            TankGunner.ClearControl();
        }
    }

    // Helper method to check if local player has a role
    public bool IsLocalPlayerInRole(bool checkForDriver)
    {
        if (checkForDriver)
            return HasDriver && DriverPlayer == Runner.LocalPlayer;
        else
            return HasGunner && GunnerPlayer == Runner.LocalPlayer;
    }
}