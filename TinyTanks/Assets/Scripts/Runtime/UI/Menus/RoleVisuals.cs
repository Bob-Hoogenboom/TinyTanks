using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RoleVisuals : MonoBehaviour
{
    [Tooltip("One GameObject per role index. Index 0 => role 0, etc.")]
    [SerializeField] private GameObject[] roleObjects;
    [Tooltip("Holds visuals of GameObject for all tanktypes")]
    [SerializeField] private GameObject[] team1GOs;
    [SerializeField] private GameObject[] team2GOs;

    [Tooltip("Optional: owner name text per role (same length as roleObjects).")]
    [SerializeField] private TMP_Text[] ownerNameTexts;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        // listen for when the RoleManager singleton becomes available
        RoleManager.InstanceChanged += OnRoleManagerChanged;
        TankSelector.InstanceChanged += OnTankSelectorChanged;
    }

    private void Start()
    {
        // If RoleManager already exists, register immediately
        if (RoleManager.Instance != null)
        {
            RegisterWithRoleManager();
        }
        else
        {
            UpdateVisuals(); // still run a pass (all free)
        }

        if (TankSelector.Instance != null)
            ApplyFromTankSelector(TankSelector.Instance);
    }

    private void OnRoleManagerChanged(RoleManager newInstance)
    {
        if (newInstance != null)
            RegisterWithRoleManager();
    }

    private void OnTankSelectorChanged(TankSelector ts)
    {
        if (ts != null)
            ApplyFromTankSelector(ts);
    }

    private void RegisterWithRoleManager()
    {
        // subscribe to updates and also immediately refresh visuals
        RoleManager.Instance.OnRolesUpdated += UpdateVisuals;
        UpdateVisuals();
    }

    // Called whenever roles are updated on the RoleManager (runs on clients)
    private void UpdateVisuals()
    {
        // sanity: nothing to do if no role objects assigned
        if (roleObjects == null || roleObjects.Length == 0) return;

        for (int i = 0; i < roleObjects.Length; i++)
        {
            bool taken = false;
            uint ownerId = 0;

            // If RoleManager exists and has an entry for this index, check owner
            if (RoleManager.Instance != null && RoleManager.Instance.roleOwners.TryGetValue(i, out ownerId) && ownerId != 0)
            {
                taken = true;
            }

            // Optional: update a name label for this role
            if (ownerNameTexts != null && i < ownerNameTexts.Length)
            {
                ownerNameTexts[i].text = taken
                    ? RoleManager.Instance?.GetOwnerName(i) ?? "(resolving...)"
                    : "(free)";
            }
            Debug.Log($"RoleVisuals: Updated visuals. RoleManager present={RoleManager.Instance != null}");
        }
    }

    private void ApplyFromTankSelector(TankSelector ts)
    {
        // Force visuals to match current synced values (works for late joiners)
        ApplyTeamType(0, ts.Team1Type);
        ApplyTeamType(1, ts.Team2Type);
    }

    public void ApplyTeamType(int tankID, TankSelector.TankType type)
    {
        GameObject[] gos = tankID switch
        {
            0 => team1GOs,
            1 => team2GOs,
            _ => null
        };

        if (gos == null) return;

        int n = Mathf.Min(6, gos.Length);

        // Turn off all 6 first (heavy/medium/light in both rows)
        for (int i = 0; i < n; i++)
            if (gos[i] != null) gos[i].SetActive(false);

        // Turn on the chosen type (0/1/2 and 3/4/5)
        switch (type)
        {
            case TankSelector.TankType.Heavy:
                if (gos[0] != null) gos[0].SetActive(true);
                if (gos[3] != null) gos[3].SetActive(true);
                break;

            case TankSelector.TankType.Medium:
                if (gos[1] != null) gos[1].SetActive(true);
                if (gos[4] != null) gos[4].SetActive(true);
                break;

            case TankSelector.TankType.Light:
                if (gos[2] != null) gos[2].SetActive(true);
                if (gos[5] != null) gos[5].SetActive(true);
                break;
        }
    }

    void OnDestroy()
    {
        RoleManager.InstanceChanged -= OnRoleManagerChanged;
        TankSelector.InstanceChanged -= OnTankSelectorChanged;

        if (RoleManager.Instance != null)
        {
            RoleManager.Instance.OnRolesUpdated -= UpdateVisuals;
        }

        ClearHueOverrides();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        // In Editor, removing a component triggers OnDisable before OnDestroy
        ClearHueOverrides();
#endif
    }

    private void ClearHueOverrides()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            // Passing null removes all overrides
            r.SetPropertyBlock(null);
        }
    }
}
