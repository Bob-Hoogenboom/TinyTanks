using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyPlayers : MonoBehaviour
{
    [Tooltip("The button to start a macth. This button should only be clickable when all 4 players joined")]
    [SerializeField] private Button readyBTN;
    [SerializeField] private int playersToJoin = 4; //this variable is purely for testing 
    [SerializeField] private GameObject[] roleObjects;
    [SerializeField] private GameObject spherePrefab; // Original sphere prefab references
    [SerializeField] private GameObject assignObject; // The replacement (e.g. cube)

    private PlayerManager _playManager;
    private Dictionary<int, int> _lastRolesPerPlayer = new Dictionary<int, int>();


    private void Start()
    {
        _playManager = FindObjectOfType<PlayerManager>();
        readyBTN.interactable = false;

    }

    //TODO Instead of updating this every frame, make this an observer for when the players change*
    private void Update()
    {
        Dictionary<int, int> currentPlayers = _playManager.GetPlayers();
        readyBTN.interactable = currentPlayers.Count == playersToJoin;

        foreach (KeyValuePair<int, int> entry in currentPlayers)
        {
            int playerId = entry.Key;
            int currentRole = entry.Value;

            // If player switched roles, revert the old role object
            if (_lastRolesPerPlayer.TryGetValue(playerId, out int oldRole) && oldRole != currentRole)
            {
                RevertObjectForRole(oldRole);
            }

            // Only apply if it's a new or changed assignment
            if (!_lastRolesPerPlayer.ContainsKey(playerId) || _lastRolesPerPlayer[playerId] != currentRole)
            {
                ChangeObjectForRole(currentRole);
                _lastRolesPerPlayer[playerId] = currentRole;
            }
        }
    }

    private void ChangeObjectForRole(int roleId)
    {
        if (!IsValidRole(roleId)) return;

        GameObject oldObj = roleObjects[roleId];
        if (oldObj != null)
        {
            Destroy(oldObj);
        }

        Vector3 pos = oldObj.transform.position;
        Quaternion rot = oldObj.transform.rotation;

        GameObject newObj = Instantiate(assignObject, pos, rot,oldObj.transform.parent);
        roleObjects[roleId] = newObj;

        Debug.Log($"Changed role {roleId} to assigned object.");
    }

    private void RevertObjectForRole(int roleId)
    {
        if (!IsValidRole(roleId)) return;

        GameObject currentObj = roleObjects[roleId];
        if (currentObj != null)
        {
            Destroy(currentObj);
        }

        Vector3 pos = currentObj.transform.position;
        Quaternion rot = currentObj.transform.rotation;

        GameObject sphere = Instantiate(spherePrefab, pos, rot, currentObj.transform.parent);
        roleObjects[roleId] = sphere;

        Debug.Log($"Reverted role {roleId} back to sphere.");
    }

    private bool IsValidRole(int roleId)
    {
        if (roleId < 0 || roleId >= roleObjects.Length)
        {
            Debug.LogError($"Invalid roleId: {roleId}");
            return false;
        }

        return true;
    }
}
