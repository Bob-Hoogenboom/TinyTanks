using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Tank Components")]
    [SerializeField] public GameObject tankBody;
    [SerializeField] public GameObject tankTurret;

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
        if (tankBody != null)
            _input.actions["Move"].performed += tankBody.GetComponent<PlayerDriver>().OnMove;
    }

    public void SetGunnerControls()
    {
        if (tankTurret != null)
        {
            _input.actions["Rotate"].performed += tankTurret.GetComponent<PlayerGunner>().OnRotate;
            //_input.actions["Shoot"].performed += _tankBody.GetComponent<PlayerGunner>().OnShoot;   // For when shooting gets implemented
        }
    }


}
