using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int _index { get; private set; }

    [Header("Tank Components")]
    [SerializeField] public GameObject _tankBody;
    [SerializeField] public GameObject _tankTurret;

    private PlayerInput _input;

    private PlayerManager _manager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _manager = FindObjectOfType<PlayerManager>();
        //_manager.players.Add(this);
        _input = GetComponent<PlayerInput>();
    }

    public void SetDriverControls()
    {
        if (_tankBody != null)
            _input.actions["Move"].performed += _tankBody.GetComponent<PlayerDriver>().OnMove;
    }

    public void SetGunnerControls()
    {
        if (_tankTurret != null)
        {
            _input.actions["Rotate"].performed += _tankTurret.GetComponent<PlayerGunner>().OnRotate;
            //_input.actions["Shoot"].performed += _tankBody.GetComponent<PlayerGunner>().OnShoot;   // For when shooting gets implemented
        }
    }


}
