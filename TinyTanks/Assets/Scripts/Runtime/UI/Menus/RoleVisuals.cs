using UnityEngine;

public class RoleVisuals : MonoBehaviour
{
    [SerializeField] private GameObject[] roleObjects;

    private MyRoomPlayer roomPlayer;

    private void Start()
    {
        RoleManager.InstanceChanged += OnRoleManagerChanged;
        roomPlayer = GetComponent<MyRoomPlayer>();

        if (RoleManager.Instance != null)
        { 
            RoleManager.Instance.OnRolesUpdated += UpdateVisuals;
            Debug.Log("Instance");
        }
    }

    private void OnDestroy()
    {
        RoleManager.InstanceChanged -= OnRoleManagerChanged;

        if (RoleManager.Instance != null)
            RoleManager.Instance.OnRolesUpdated -= UpdateVisuals;
    }

    private void OnRoleManagerChanged(RoleManager newManager)
    {
        if (newManager != null)
        {
            newManager.OnRolesUpdated += UpdateVisuals;
            Debug.Log("rolemanager is not null");
        }
    }

    private void UpdateVisuals()
    {
        // deactivate all roleObjects first
        foreach (var obj in roleObjects)
            if (obj != null) obj.SetActive(false);

        if (roomPlayer != null && roomPlayer.roleIndex >= 0)
        {
            int role = roomPlayer.roleIndex;
            if (role < roleObjects.Length && roleObjects[role] != null)
            {
                roleObjects[role].SetActive(true);
                Debug.Log($"roleObject {role} Turned on");
            }
        }
    }
}
