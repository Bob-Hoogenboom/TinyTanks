using UnityEngine;
using Fusion;

public class PlayerCameraController : NetworkBehaviour
{
    private NetworkObject targetTank;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 5, -8);
    [SerializeField] private float smoothSpeed = 0.125f;

    private Camera playerCamera;

    public override void Spawned()
    {
        // Only the local player should have an active camera
        if (Object.HasInputAuthority)
        {
            // Create a camera for this player
            playerCamera = new GameObject("PlayerCamera").AddComponent<Camera>();
            playerCamera.transform.parent = transform;

            transform.position += cameraOffset;
            // Set as main camera
            playerCamera.tag = "MainCamera";

            // Disable any other cameras
            foreach (Camera cam in FindObjectsOfType<Camera>())
            {
                if (cam != playerCamera)
                    cam.enabled = false;
            }
        }
    }

    // RPC to set which tank this camera should follow
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetTargetTank(NetworkObject tank)
    {
        targetTank = tank;
    }

    private void LateUpdate()
    {
        // Only run for the local player's camera
        if (!Object.HasInputAuthority || playerCamera == null)
            return;

        // Follow the tank if we have one
        if (targetTank != null)
        {
            // Calculate desired position
            Vector3 desiredPosition = targetTank.transform.position + targetTank.transform.TransformDirection(cameraOffset);

            // Smoothly move camera
            Vector3 smoothedPosition = Vector3.Lerp(playerCamera.transform.position, desiredPosition, smoothSpeed);
            playerCamera.transform.position = smoothedPosition;

            // Look at tank
            playerCamera.transform.LookAt(targetTank.transform);
        }
    }
}