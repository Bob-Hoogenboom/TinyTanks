using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{

    private PlayerManager _playerManager;
    private Player driver1, driver2, gunner1, gunner2;


    private void Awake()
    {
        _playerManager = FindObjectOfType<PlayerManager>();
    }

    void Start()
    {
        GameObject playerObj = _playerManager.GetPlayerInRole(0);
        if (playerObj != null)
        {
            driver1 = playerObj.GetComponent<Player>();
        }
        playerObj = _playerManager.GetPlayerInRole(1);
        if (playerObj != null)
        {
            gunner1 = playerObj.GetComponent<Player>();
        }
        playerObj = _playerManager.GetPlayerInRole(2);
        if (playerObj != null)
        {
            driver2 = playerObj.GetComponent<Player>();
        }
        playerObj = _playerManager.GetPlayerInRole(3);
        if (playerObj != null)
        {
            gunner2 = playerObj.GetComponent<Player>();
        }

        if (driver1 != null)
        {
            driver1.tankBody = GameObject.FindGameObjectWithTag("TankBody1");
            driver1.SetDriverControls();
        }
        if (driver2 != null)
        {
            driver2.tankBody = GameObject.FindGameObjectWithTag("TankBody2");
            driver2.SetDriverControls();
        }
        if (gunner1 != null)
        {
            gunner1.tankTurret = GameObject.FindGameObjectWithTag("TankTurret1");
            gunner1.SetGunnerControls();
        }
        if (gunner2 != null)
        {
            gunner2.tankTurret = GameObject.FindGameObjectWithTag("TankTurret2");
            gunner2.SetGunnerControls();
        }
    }
}
