using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Tank Components")]
    [SerializeField] public GameObject tankBody;
    [SerializeField] public GameObject tankTurret;

    public PlayerInput input { get; private set; }
    private PlayerManager _manager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _manager = FindObjectOfType<PlayerManager>();
        //_manager.players.Add(this);
        input = GetComponent<PlayerInput>();
    }

    public void SetDriverControls(PlayerInput playerInput)
    {
        if (tankBody != null)
        {
            InputActionAsset actions = playerInput.actions;
            PlayerDriver driver = tankBody.GetComponent<PlayerDriver>();
            actions.FindAction("Move").performed += ctx => driver.OnMove(ctx);
            actions.FindAction("Shoot").performed += ctx => driver.OnShoot();
        }
    }

    public void SetGunnerControls(PlayerInput playerInput)
    {
        if (tankTurret != null)
        {
            InputActionAsset actions = playerInput.actions;
            PlayerGunner gunner = tankTurret.GetComponent<PlayerGunner>();
            actions.FindAction("Rotate").performed += ctx => gunner.OnRotate(ctx);
        }
    }
}
