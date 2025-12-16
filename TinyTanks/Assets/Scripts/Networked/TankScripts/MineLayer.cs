using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

public class MineLayer : NetworkBehaviour
{
    public Action OnLayMines;
    public Action OnPickupMines;
    public Action OnDeathReset;

    [Header("Behaviour")]
    [SerializeField, SyncVar] private bool mineOnCooldown;
    [SyncVar(hook = nameof(OnMinesChanged))]
    private int _mineAmount = 0;
    [SerializeField] private GameObject mineGO;
    [SerializeField] private bool hasMines;
    [SyncVar] private double _mineCooldownEndTime;
    private int _maxMineAmount = 3;
    [SerializeField] private float mineCooldownTime = 4f;

    [Header("UI")]
    [SerializeField] private CanvasGroup mineUI;
    [SerializeField] private Image mineFillImage;
    [SerializeField] private TMP_Text mineTxt;

    [ServerCallback]
    private void Start()
    {
        OnLayMines += Server_LayMine;
        OnPickupMines += Server_ResetMines;
        OnDeathReset += Server_MineDeathReset;
    }

    private void Update()
    {
        if (mineOnCooldown)
        {
            double cooldownRemaining = _mineCooldownEndTime - NetworkTime.time;
            UpdateMineDisplay(cooldownRemaining);

            if (cooldownRemaining <= 0)
                Server_FinishMineCooldown();
        }
    }

    [Server]
    private void Server_LayMine()
    {
        if (!hasMines) return;
        if (mineOnCooldown) return;

        _mineAmount -= 1;

        if (_mineAmount <= 0)
            hasMines = false;

        GameObject mine = Instantiate(mineGO, transform.position, transform.rotation);
        NetworkServer.Spawn(mine);

        mineOnCooldown = true;
        _mineCooldownEndTime = NetworkTime.time + mineCooldownTime;
    }

    [Server]
    private void Server_ResetMines()
    {
        hasMines = true;
        mineUI.alpha = 1;
        _mineAmount = _maxMineAmount;
    }

    [Server]
    private void Server_MineDeathReset()
    {
        hasMines = false;
        _mineAmount = 0;
        mineOnCooldown = false;
    }

    [Server]
    public void Server_FinishMineCooldown()
    {
        mineOnCooldown = false;
    }

    [Client]
    private void OnMinesChanged(int oldVal, int newVal)
    {
        mineUI.alpha = (newVal == 0) ? 0 : 1;

        mineTxt.text = $"Mines left: {newVal}";
    }

    private void UpdateMineDisplay(double timeRemaining)
    {
        float progress = 1f - Mathf.Clamp01((float)(timeRemaining / mineCooldownTime));
        mineFillImage.fillAmount = progress;
    }
}
