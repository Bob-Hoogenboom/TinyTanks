using UnityEngine.UI;
using UnityEngine;
using Mirror;
using TMPro;

public class MyRoomPlayer : NetworkRoomPlayer
{
    public static MyRoomPlayer Local;
    public override void OnStartLocalPlayer() => Local = this;

    [Header("UI (Room Scene")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyStatus;

    [SyncVar(hook = nameof(OnRoleChanged))]
    public int roleIndex = -1;


    private void Awake()
    {
        readyButton = GameObject.FindWithTag("ReadyButton").GetComponent<Button>();
        readyStatus = GameObject.FindWithTag("ReadyText").GetComponent< TMP_Text>();
    }

    public override void OnClientEnterRoom()
    {
        if(readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyClicked);
        }

        UpdateUI();
    }

    public override void OnClientExitRoom()
    {
        Debug.Log("exited ready room");

        if (roleIndex == 0)
        {
            var tankBody = GameObject.FindGameObjectsWithTag("TankBody1");
        }
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
    void CmdRequestRole(int roleIndex)
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Server_TryClaimRole(netIdentity, roleIndex, allowSwitch: true);
    }

    [Command]
    void CmdReleaseRole(int roleIndex)
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Server_ReleaseRole(netIdentity, roleIndex);
    }

    // If this player disconnects, server clears the role if we owned it
    public override void OnStopServer()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.Server_ReleaseAllFor(netIdentity);
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
