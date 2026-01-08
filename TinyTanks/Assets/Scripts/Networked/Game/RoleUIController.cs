using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleUIController : MonoBehaviour
{
    [SerializeField] private RoleButton[] roleButtons;   // size 4
    [SerializeField] private TextMesh[] roleLabels;  // size 4

    private RoleManager _bound;

    private void OnEnable()
    {
        RoleManager.InstanceChanged += OnInstanceChanged;
        TryBind(RoleManager.Instance);   // bind immediately if already present
        WireButtons();
    }

    private void OnDisable()
    {
        RoleManager.InstanceChanged -= OnInstanceChanged;
        Unbind();
    }

    private void OnInstanceChanged(RoleManager mgr)
    {
        Unbind();
        TryBind(mgr);
    }

    private void TryBind(RoleManager mgr)
    {
        if (mgr == null) return;
        _bound = mgr;
        _bound.OnRolesUpdated += Refresh;
        // Force an initial draw (OnChange won't fire for existing contents)
        Refresh();
    }
    private void Unbind()
    {
        if (_bound != null)
        {
            _bound.OnRolesUpdated -= Refresh;
            _bound = null;
        }
    }

    private void WireButtons()
    {
        for (int i = 0; i < roleButtons.Length; i++)
        {
            int idx = i;
            roleButtons[i].onClick.AddListener(() =>
            {
                if (MyRoomPlayer.Local != null)
                    MyRoomPlayer.Local.OnClickToggleRole(idx);
            });
        }
    }

    private void Refresh()
    {
        if (_bound == null) return;

        for (int i = 0; i < roleButtons.Length; i++)
        {
            bool free = _bound.IsRoleFree(i);
            bool mine = _bound.IsRoleMine(i);
            string owner = _bound.GetOwnerName(i);

            roleButtons[i].interactable = free || mine;
            var btnText = roleButtons[i].GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = mine ? "Release" : "Claim";

            if (roleLabels != null && i < roleLabels.Length && roleLabels[i] != null)
                roleLabels[i].text = $"{(free ? "(free)" : owner)}";
        }
    }
}
