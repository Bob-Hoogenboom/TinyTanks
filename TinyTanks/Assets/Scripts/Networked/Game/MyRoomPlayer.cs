using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;

public class MyRoomPlayer : NetworkRoomPlayer
{
    public static MyRoomPlayer Local;

    [Header("UI (Room Scene")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyStatus;
    [SerializeField] private GameObject objTimerUI;

    [SyncVar(hook = nameof(OnRoleChanged))]
    public int roleIndex = -1;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Local = this;
    }

    public override void OnClientEnterRoom()
    {
        // IMPORTANT: don't bind UI from remote players
        if (!isLocalPlayer) return;

        ResolveUI();
        BindUI();

        CheckIfHost();
        UpdateUI();
    }

    private void CheckIfHost()
    {
        if (!this.isServer)
        {
            //Clients cannot change the durration of the match
            objTimerUI.SetActive(false);
            return;
        }
    }

    public override void OnClientExitRoom()
    {
        if (!isLocalPlayer) return;
        UnbindUI();
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        UpdateUI();
    }

    private void OnReadyClicked()
    {
        if (!isLocalPlayer) return;
        if (roleIndex == -1) { Debug.Log("Need to assign role first"); return; }

        CmdChangeReadyState(!readyToBegin);
    }

    private void UpdateUI()
    {
        if (!isLocalPlayer) return;

        if (readyStatus != null)
            readyStatus.text = readyToBegin ? "Ready" : "Not Ready";

        if(readyButton != null)
            readyButton.GetComponentInChildren<TMP_Text>().text = readyToBegin ? "Unready" : "Ready Up";
    }
    public void OnClickToggleRole(int roleIndex)
    {
        if (!isLocalPlayer) return; // fix for CS0103 when used outside NetworkBehaviour
        if (RoleManager.Instance == null) return;

        if (RoleManager.Instance.IsRoleMine(roleIndex))
            CmdReleaseRole(roleIndex);
        else
            CmdRequestRole(roleIndex);
    }

    [Command]
    private void CmdRequestRole(int roleIndex)
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Server_TryClaimRole(netIdentity, roleIndex, allowSwitch: true);
    }

    [Command]
    private void CmdReleaseRole(int roleIndex)
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Server_ReleaseRole(netIdentity, roleIndex);
    }

    [Command]
    private void CmdRequestType()
    {
        
    }

    public override void OnStopClient()
    {
        if (Local == this)
            Local = null;

        base.OnStopClient();
        UnbindUI();
    }

    // If this player disconnects, server clears the role if we owned it
    public override void OnStopServer()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Server_ReleaseAllFor(netIdentity);
    }

    private void BindUI()
    {
        if (readyButton == null) return;
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    private void UnbindUI()
    {
        if (readyButton == null) return;
        readyButton.onClick.RemoveListener(OnReadyClicked);
    }

    private void ResolveUI()
    {
        if (readyButton == null)
            readyButton = GameObject.FindWithTag("ReadyButton")?.GetComponent<Button>();

        if (readyStatus == null)
            readyStatus = GameObject.FindWithTag("ReadyText")?.GetComponent<TMP_Text>();

        if (objTimerUI == null)
            objTimerUI = FindAnyObjectByType<SetMatchTimer>()?.gameObject;
    }

    [Server]
    public void Server_SetRole(int idx)
    {
        roleIndex = idx;
    }

    private void OnRoleChanged(int oldIdx, int newIdx)
    {
        if (isLocalPlayer) UpdateUI();
    }
}
