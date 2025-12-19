using UnityEngine;
using UnityEngine.AI;

public class TankStateManager : MonoBehaviour
{
    private TankBaseState _currentState;

    private TankIdle _idle = new TankIdle();
    private TankPatrol _patrol = new TankPatrol();
    private TankChase _chase = new TankChase();
    private TankShoot _shoot = new TankShoot();
    //private TankDeath _death = new TankDeath();

    private SinglePlayerTank _player;
    private NavMeshAgent _agent;

    [SerializeField] private GameObject cupola;
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;

    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canShoot = true;
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float shootingRange = 3f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color detectionColor = new Color (1f, 0.9f, 0.1f, 0.3f);
    [SerializeField] private Color attackColor = new Color (1f, 0f, 0f, 0.3f);


    //getters and setters
    public TankIdle Idle { get { return _idle; } }
    public TankPatrol Patrol { get { return _patrol; } }
    public TankChase Chase { get { return _chase; } }
    public TankShoot Shoot { get { return _shoot; } }

    public SinglePlayerTank Player { get { return _player; } }
    public NavMeshAgent Agent { get { return _agent; } }

    public GameObject Cupola { get { return cupola; } }
    public GameObject Barrel { get { return barrel; } }
    public GameObject BulletPrefab { get { return bulletPrefab; } }
    public Transform Muzzle { get { return muzzle; } }

    public bool CanMove { get { return canMove; } }
    public bool CanShoot { get { return canShoot; } }
    public float DetectionRange {  get { return detectionRange; } }
    public float ShootingRange {  get { return shootingRange; } }


    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = FindObjectOfType<SinglePlayerTank>();

        _currentState = _idle;

        _currentState.EnterState(this);
    }

    private void Update()
    {
        _currentState.UpdateState(this);
    }

    public void SwitchState(TankBaseState state)
    {
        _currentState.ExitState(this);

        _currentState = state;

        _currentState.EnterState(this);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = detectionColor;
        Gizmos.DrawSphere(transform.position, detectionRange);

        Gizmos.color = attackColor;
        Gizmos.DrawSphere(transform.position, shootingRange);

    }

}
