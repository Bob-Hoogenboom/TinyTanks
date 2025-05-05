using Fusion;
using UnityEngine;

// Define network input structure for both players
public struct NetworkInputData : INetworkInput
{
    public Vector2 TankMoveInput;
    public Vector2 TurretRotationInput;
    public NetworkBool FireInput;
}
