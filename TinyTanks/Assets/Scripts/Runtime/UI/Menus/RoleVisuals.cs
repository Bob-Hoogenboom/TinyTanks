using UnityEngine;
using TMPro;

public class RoleVisuals : MonoBehaviour
{
    [Tooltip("One GameObject per role index. Index 0 => role 0, etc.")]
    [SerializeField] private GameObject[] roleObjects;

    [Tooltip("Optional: owner name text per role (same length as roleObjects).")]
    [SerializeField] private TMP_Text[] ownerNameTexts;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        // listen for when the RoleManager singleton becomes available
        RoleManager.InstanceChanged += OnRoleManagerChanged;
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
    }

    private void OnRoleManagerChanged(RoleManager newInstance)
    {
        if (newInstance != null)
            RegisterWithRoleManager();
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

            // Toggle the visual for this role
            if (roleObjects[i] != null)
            {
                if (_mpb == null) _mpb = new MaterialPropertyBlock(); //'enter' local material settings

                MeshRenderer[] renderers = roleObjects[i].GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer r in renderers)
                {
                    if (taken)
                    {
                        // restore full color
                        r.GetPropertyBlock(_mpb);
                        _mpb.SetFloat("_Value", 1f);
                        r.SetPropertyBlock(_mpb);
                    }
                    else
                    {
                        // ghost: make it transparent / faded
                        r.GetPropertyBlock(_mpb);
                        _mpb.SetFloat("_Value", 0f);
                        r.SetPropertyBlock(_mpb);
                    }

                    // IMPORTANT: material must use a Transparent shader mode
                    // (e.g. "Standard" set to Rendering Mode: Transparent or Fade)
                }

                // Optional: update a name label for this role
                if (ownerNameTexts != null && i < ownerNameTexts.Length)
                {
                    ownerNameTexts[i].text = taken
                        ? RoleManager.Instance?.GetOwnerName(i) ?? "(resolving...)"
                        : "(free)";
                }
            }

            Debug.Log($"RoleVisuals: Updated visuals. RoleManager present={RoleManager.Instance != null}");
        }
    }

    void OnDestroy()
    {
        RoleManager.InstanceChanged -= OnRoleManagerChanged;
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
