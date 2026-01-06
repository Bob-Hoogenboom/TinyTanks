using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class Shooter : NetworkBehaviour
{
    public Action OnMissileShoot;
    public Action OnMissileDestroy;
    public Action OnBulletShoot;
    public Action OnReloadBullet;
    public Action OnNoBattery;
    public Action OnRechargeBattery;
    public Action OnDeathReset;
    public Action OnPickupMissle;
    public Action SwapToMissile;
    public Action SwapToBullet;

    [Header("behaviour")]
    public NetworkedMissile missile;
    [SerializeField] private TankData data;
    [SerializeField] private GameObject bulletGO;
    [SerializeField] private GameObject missileGO;
    [SyncVar] private double _reloadEndTime;
    [SyncVar] private float _currReloadTime;
    [SyncVar] private bool _isShootingMissile;

    [SyncVar(hook = nameof(OnHasMissileChanged))]
    [SerializeField] private bool _hasMissile = false;
    [SyncVar(hook = nameof(OnIsReloadingChanged))]
    private bool _isReloading = false;
    [SyncVar(hook = nameof(OnHasBulletChanged))]
    [SerializeField] private bool _hasBullet = true;

    [Header("UI")]
    [SerializeField] private CanvasGroup reloadGroup;
    [SerializeField] private Image bulletReloadImage;
    [SerializeField] private Image reloadTimerImage;
    [SerializeField] private Image bulletImage;
    [SerializeField] private Image bulletFillDriver;
    [SerializeField] private Image missileImage;
    [SerializeField] private Image missileFillDriver;

    [Header("Aduio")]
    [SerializeField] private AudioSource reloadingAudio;
    [SerializeField] private AudioSource shootingAudio;

    [Header("VFX")]
    [SerializeField] private GameObject smokeVFX;

    public override void OnStartServer()
    {
        _currReloadTime = data.baseReloadTime;

        OnMissileShoot += Server_ShootMissile;
        OnBulletShoot += Server_ShootBullet;
        OnReloadBullet += Server_ReloadGun;
        OnNoBattery += Server_NoBattery;
        OnRechargeBattery += Server_RechargeBattery;
        OnDeathReset += Server_OnDeathReset;
        OnPickupMissle += Server_LoadMissile;
    }
    public override void OnStartClient()
    {
        SwapToMissile += ActivateMissileUI;
        SwapToBullet += ActivateBulletUI;

        ActivateBulletUI();
    }

    public override void OnStopClient()
    {
        SwapToMissile -= ActivateMissileUI;
        SwapToBullet -= ActivateBulletUI;
    }

    public override void OnStopServer()
    {
        OnMissileShoot -= Server_ShootMissile;
        OnBulletShoot -= Server_ShootBullet;
        OnReloadBullet -= Server_ReloadGun;
        OnNoBattery -= Server_NoBattery;
        OnRechargeBattery -= Server_RechargeBattery;
        OnDeathReset -= Server_OnDeathReset;
        OnPickupMissle -= Server_LoadMissile;
    }

    private void Update()
    {
        if (_isReloading)
        {
            double reloadRemaining = _reloadEndTime - NetworkTime.time;
            UpdateReloadDisplay(reloadRemaining);

            if (reloadRemaining <= 0)
                Server_FinishReload();
        }
    }

    [Server]
    private void Server_OnDeathReset()
    {
        _hasMissile = false;
        _hasBullet = true;
        _isReloading = false;
    }

    [Server]
    private void Server_NoBattery()
    {
        _currReloadTime = data.noBatteryReloadTime;
    }

    [Server]
    private void Server_RechargeBattery()
    {
        _currReloadTime = data.baseReloadTime;
    }

    [Server]
    public void Server_ShootMissile()
    {
        if (!_hasMissile) return;

        _isShootingMissile = true;
        var brain = GetComponent<TankBrain>();

        GameObject serverMissileClone = Instantiate(missileGO, brain.Muzzle.transform.position, brain.TurretPitchPivot.transform.rotation);

        NetworkedMissile nMissile = serverMissileClone.GetComponent<NetworkedMissile>();
        nMissile.parent = brain;
        NetworkServer.Spawn(serverMissileClone);
        missile = nMissile;
        brain.missile = nMissile;
        _hasMissile = false;
    }

    [Server]
    public void Server_ShootBullet()
    {
        if (!_hasBullet) return;

        var brain = GetComponent<TankBrain>();

        var velocity = brain.TurretPitchPivot.transform.forward * brain.tankData.shellSpeed;
        GameObject serverShellClone = Instantiate(bulletGO, brain.Muzzle.transform.position, brain.TurretPitchPivot.transform.rotation);
        Rigidbody serverShellRB = serverShellClone.GetComponent<Rigidbody>();
        serverShellRB.velocity = velocity;

        NetworkedShell nShell = serverShellClone.GetComponent<NetworkedShell>();
        nShell.parent = brain;
        NetworkServer.Spawn(serverShellClone);

        _hasBullet = false;
    }

    [Server]
    public void Server_ReloadGun()
    {
        if (_hasBullet) return;
        if (_isReloading) return;

        _isReloading = true;
        _reloadEndTime = NetworkTime.time + _currReloadTime;
    }

    [Server]
    public void Server_FinishReload()
    {
        _isReloading = false;
        _hasBullet = true;
    }

    [Server]
    public void Server_LoadMissile()
    {
        _hasMissile = true;
    }

    private void OnHasBulletChanged(bool _, bool hasBullet)
    {
        if (!hasBullet)
        {
            var brain = GetComponent<TankBrain>();
            var _barrelSmoke = Instantiate(smokeVFX, brain.Muzzle.transform.position, brain.Muzzle.transform.rotation);
            Destroy(_barrelSmoke, 3);
            shootingAudio.Play();
            bulletFillDriver.fillAmount = 0;
        }

        if (reloadGroup) reloadGroup.alpha = hasBullet ? 0f : 1f;

        if (hasBullet)
        {
            if (bulletReloadImage) bulletReloadImage.fillAmount = 0f;
            if (reloadTimerImage) reloadTimerImage.fillAmount = 0f;
            bulletFillDriver.fillAmount = 1;
        }
    }

    private void OnIsReloadingChanged(bool _, bool reloading)
    {
        if (reloading == true)
            reloadingAudio.Play();
    }

    private void OnHasMissileChanged(bool _, bool hasMissile)
    {
        if (hasMissile)
        {
            missileFillDriver.fillAmount = 1;
            return;
        }
        if (!hasMissile)
        {
            missileFillDriver.fillAmount = 0;
            return;
        }
    }

    [Client]
    private void ActivateBulletUI()
    {
        missileImage.enabled = false;
        missileFillDriver.enabled = false;
        bulletImage.enabled = true;
        bulletFillDriver.enabled = true;
    }

    [Client]
    private void ActivateMissileUI()
    {
        missileImage.enabled = true;
        missileFillDriver.enabled = true;
        bulletImage.enabled = false;
        bulletFillDriver.enabled = false;
    }

    private void UpdateReloadDisplay(double timeRemaining)
    {
        float progress = 1f - Mathf.Clamp01((float)(timeRemaining / _currReloadTime));
        bulletReloadImage.fillAmount = progress;
        reloadTimerImage.fillAmount = progress;
    }
}
