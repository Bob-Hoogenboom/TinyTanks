using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{

    private PlayerManager _playerManager;
    private Player _driver1, _driver2, _gunner1, _gunner2;


    private void Awake()
    {
        _playerManager = FindObjectOfType<PlayerManager>();
    }

    void Start()
    {
        GameObject playerObj = _playerManager.GetPlayerInRole(0);
        if (playerObj != null)
        {
            _driver1 = playerObj.GetComponent<Player>();
        }
        playerObj = _playerManager.GetPlayerInRole(1);
        if (playerObj != null)
        {
            _gunner1 = playerObj.GetComponent<Player>();
        }
        playerObj = _playerManager.GetPlayerInRole(2);
        if (playerObj != null)
        {
            _driver2 = playerObj.GetComponent<Player>();
        }
        playerObj = _playerManager.GetPlayerInRole(3);
        if (playerObj != null)
        {
            _gunner2 = playerObj.GetComponent<Player>();
        }

        if (_driver1 != null)
        {
            _driver1.tankBody = GameObject.FindGameObjectWithTag("TankBody1");
            _driver1.SetDriverControls();
        }
        if (_driver2 != null)
        {
            _driver2.tankBody = GameObject.FindGameObjectWithTag("TankBody2");
            _driver2.SetDriverControls();
        }
        if (_gunner1 != null)
        {
            _gunner1.tankTurret = GameObject.FindGameObjectWithTag("TankTurret1");
            _gunner1.SetGunnerControls();
        }
        if (_gunner2 != null)
        {
            _gunner2.tankTurret = GameObject.FindGameObjectWithTag("TankTurret2");
            _gunner2.SetGunnerControls();
        }
    }
}
