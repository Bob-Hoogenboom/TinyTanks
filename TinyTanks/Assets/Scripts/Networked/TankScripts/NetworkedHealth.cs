using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System;

public class NetworkedHealth : NetworkBehaviour
{
    public Action OnHealthReset;
    public TankData tankData;

    [Header("Behaviour")]
    [SyncVar(hook = nameof(OnLivesChanged))]
    [SerializeField] private int lives = 3;
    [SyncVar(hook = nameof(OnHealthChanged))]
    [SerializeField] public float currHealth;
    [SyncVar] private double respawnEndTime;
    [SerializeField] private float respawnTime = 4f;
    [SyncVar(hook = nameof(OnDeadChanged))]
    private bool _isDead;
    public bool IsDead => _isDead;

    [Header("UI")]
    [SerializeField] private Image[] healthImage;
    [SerializeField] private TMP_Text[] livesText;
    [SerializeField] private CanvasGroup driverRespawn;
    [SerializeField] private CanvasGroup gunnerRespawn;
    [SerializeField] private TMP_Text[] respawnTexts;

    private void Start()
    {
        OnHealthReset += Server_ResetTankHealth;
        currHealth = tankData.maxHealth;
    }

    private void Update()
    {
        if (_isDead)
        {
            double respawnRemaining = respawnEndTime - NetworkTime.time;
            UpdateTimerDisplay(respawnRemaining, respawnTexts);

            if (respawnRemaining <= 0 && _isDead && isServer) TankBrain.OnTankRespawn?.Invoke();
        }
    }

    [Server]
    private void Server_ResetTankHealth()
    {
        currHealth = tankData.maxHealth;
        _isDead = false;
    }

    [Server]
    public void Server_TakeDamage(int dmg)
    {
        if (_isDead) return;

        currHealth -= dmg;

        if (currHealth <= 0)
        {
            _isDead = true;
            lives -= 1;

            if (lives <= 0)
            {
                TankBrain.OnReturnToLobby?.Invoke();
            }
            else
            {
                respawnEndTime = NetworkTime.time + respawnTime;
                TankBrain.OnTankDeath?.Invoke();
            }
        }
    }

    [Client]
    private void OnLivesChanged(int oldVal, int newVal)
    {
        foreach (var text in livesText)
            text.text = $"{newVal}";
    }

    private void OnHealthChanged(float oldVal, float newVal)
    {
        foreach (var image in healthImage)
            image.fillAmount = newVal / tankData.maxHealth;
    }

    private void OnDeadChanged(bool oldVal, bool newVal)
    {
        if (driverRespawn != null) driverRespawn.alpha = newVal ? 1 : 0;
        if (gunnerRespawn != null) gunnerRespawn.alpha = newVal ? 1 : 0;
    }

    private void UpdateTimerDisplay(double timeRemaining, TMP_Text[] uiTexts)
    {
        if (timeRemaining <= 0) timeRemaining = 0;
        var ts = TimeSpan.FromSeconds(timeRemaining);

        foreach (var text in uiTexts)
            text.text = $"{ts.Seconds:00}";
    }
}
