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

    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float shootingRange = 3f;

    //getters and setters
    public TankIdle Idle { get { return _idle; } }
    public TankPatrol Patrol { get { return _patrol; } }
    public TankChase Chase { get { return _chase; } }
    public TankShoot Shoot { get { return _shoot; } }

    public SinglePlayerTank Player { get { return _player; } }
    public NavMeshAgent Agent { get { return _agent; } }

    public float DetectionRange {  get { return detectionRange; } }
    public float ShootingRange {  get { return shootingRange; } }


    private void Start()
    {
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



}
