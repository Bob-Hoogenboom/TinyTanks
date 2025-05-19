using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    private PlayerManager _playerManager;

    private void Awake()
    {
        _playerManager = FindObjectOfType<PlayerManager>();
    }

    void Start()
    {
        if(_playerManager.driver1 != null)
        {
            _playerManager.driver1.tankBody = GameObject.FindGameObjectWithTag("TankBody1");
            _playerManager.driver1.SetDriverControls();
        }
        if (_playerManager.driver2 != null)
        {
            _playerManager.driver2.tankBody = GameObject.FindGameObjectWithTag("TankBody2");
            _playerManager.driver2.SetDriverControls();
        }
        if (_playerManager.gunner1 != null)
        {
            _playerManager.gunner1.tankTurret = GameObject.FindGameObjectWithTag("TankTurret1");
            _playerManager.gunner1.SetGunnerControls();
        }
        if (_playerManager.gunner2 != null)
        {
            _playerManager.gunner2.tankTurret = GameObject.FindGameObjectWithTag("TankTurret2");
            _playerManager.gunner2.SetGunnerControls();
        }
    }
}
