using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private PlayerManager _playerManager;
    private Player _driver1, _driver2, _gunner1, _gunner2;

    [Header("LevelTimer")]
    [Tooltip("Game time in seconds")] [SerializeField]
    private float gameDuration = 180f; //three minute Timer
    [Tooltip("Scene to load to after the timer has ended")] [SerializeField]
    private int exitScene;

    private float _timer;

    public delegate void TimerUpdateDelegate(float timeRemaining);
    public static event TimerUpdateDelegate OnTimerUpdate; //static event is fine here because there is usually one timer per match


    private void Awake()
    {
        _playerManager = FindObjectOfType<PlayerManager>();
    }

    void Start()
    {
        _timer = gameDuration;

        // assign players to tank objects
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


        // assign input if there is a player assigned to tankpart
        if (_driver1 != null)
        {
            _driver1.tankBody = GameObject.FindGameObjectWithTag("TankBody1");
            _driver1.SetDriverControls(_driver1.GetComponent<Player>().input);
        }
        if (_driver2 != null)
        {
            _driver2.tankBody = GameObject.FindGameObjectWithTag("TankBody2");
            _driver2.SetDriverControls(_driver2.GetComponent<Player>().input);
        }
        if (_gunner1 != null)
        {
            _gunner1.tankTurret = GameObject.FindGameObjectWithTag("TankTurret1");
            _gunner1.SetGunnerControls(_gunner1.GetComponent<Player>().input);
        }
        if (_gunner2 != null)
        {
            _gunner2.tankTurret = GameObject.FindGameObjectWithTag("TankTurret2");
            _gunner2.SetGunnerControls(_gunner2.GetComponent<Player>().input);
        }
    }

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            OnTimerUpdate?.Invoke(_timer);
        }
        else
        {
            SceneManager.LoadScene(exitScene);
        }
    }
}
