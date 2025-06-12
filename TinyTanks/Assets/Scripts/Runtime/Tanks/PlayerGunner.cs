using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PlayerGunner : MonoBehaviour
{

    [Header("Events")]
    public UnityEvent OnReloadStarted;
    public UnityEvent OnReloadComplete;

    [Header("References")]
    private Rigidbody _rb;
    private PlayerDriver _playerDriver;

    [Header("Audio")]
    public AudioSource rotateAudio;
    float playThreshold = 0.1f;
    private bool isRotating = false;

    [Header("Variables")]
    [Tooltip("Rotation speed in angles/second")]
    [Range(10, 50)]
    [SerializeField] private float _rotationSpeed = 10f;
    [Tooltip("Reload cooldown in seconds")]
    [SerializeField] private float reloadCooldown = 5f;

    [SerializeField] private InputDevice _gunnerInput;

    [Header("UI")]
    [Tooltip("Bullet state UI")]
    [SerializeField] private TMP_Text bulletStateText;
    [SerializeField] private GameObject reloadTimerPrefab;
    [SerializeField] private Image bulletBGImage;
    [SerializeField] private Image reloadTimerImage;


    [Header("Input Values")]
    private Vector2 _rotateVector; // only takes a/d -> y axis

    private bool isReloading = false;
    

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerDriver = GetComponentInParent<PlayerDriver>();
        _playerDriver.OnShootComplete.AddListener(HandleDriverShoot);
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

        if (Mathf.Abs(rotationAmount) > playThreshold)
        {
            if (!isRotating)
            {
                rotateAudio.Play();
                isRotating = true;
            }
        }
        else if (isRotating)
        {
            rotateAudio.Stop();
            isRotating = false;
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        TimedWait reloadTimer = new TimedWait(reloadCooldown);
        OnReloadStarted.Invoke();


        while (reloadTimer.keepWaiting)
        {
            //Reload UI slider change etc.
            bulletBGImage.fillAmount = reloadTimer.Progress;
            reloadTimerImage.fillAmount = reloadTimer.Progress;

            yield return null;
        }

        bulletStateText.text = "Ready";
        isReloading = false;
        reloadTimerPrefab.SetActive(false);
        bulletBGImage.fillAmount = 0;
        reloadTimerImage.fillAmount = 0;
        OnReloadComplete.Invoke();
    }

    private void HandleDriverShoot()
    {
        bulletStateText.text = "Not Ready";
        reloadTimerPrefab.SetActive(true);
    }
}
