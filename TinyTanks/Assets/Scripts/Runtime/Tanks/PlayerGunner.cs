using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerGunner : MonoBehaviour
{

    [Header("Events")]
    public UnityEvent OnReloadComplete;

    [Header("References")]
    private Rigidbody _rb;

    [Header("Variables")]
    [Tooltip("Rotation speed in angles/second")]
    [Range(10, 50)]
    [SerializeField] private float _rotationSpeed = 10f;
    [Tooltip("Reload cooldown in seconds")]
    [SerializeField] private float reloadCooldown = 5f;

    [SerializeField] private InputDevice _gunnerInput;


    [Header("Input Values")]
    private Vector2 _rotateVector; // only takes a/d -> y axis

    private bool isReloading = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        //gunnerInput = manager.inputDevices[1];
    }

    private void Update()
    {
        Rotate();
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotateVector = context.ReadValue<Vector2>();
    }

    public void OnReload()
    {
        if(isReloading == false)
        {
            isReloading = true;
            StartCoroutine(ReloadCoroutine());
        }
        
    }

    private void Rotate()
    {
        float rotationAmount = _rotateVector.x * _rotationSpeed * Time.deltaTime;
        this.transform.Rotate(0, rotationAmount, 0);
    }

    private IEnumerator ReloadCoroutine()
    {
        TimedWait reloadTimer = new TimedWait(reloadCooldown);

        while (reloadTimer.keepWaiting)
        {
            //Reload UI slider change etc.
            //reloadTimer.Progress displays how far the process is -> can be used for UI to give value to slider or something
            yield return null;
        }

        isReloading = false;
        OnReloadComplete.Invoke();
    }
}
