using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class TutorialEnemy : MonoBehaviour, IDamagable
{
    [Header("Enemy")]
    [SerializeField] private float hitpoints = 3f;
    public float HitPoints => hitpoints;

    private TutorialTank _playerTarget;


    [Header("Attack")]
    [SerializeField] private GameObject cupola;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float range;
    [Space]
    public UnityEvent onShoot;

    [Header("Path")]
    [SerializeField] private float waitTimeOnWayPoint = 1f;
    [SerializeField] private EnemyPath path;

    private NavMeshAgent _agent;
    private float time = 0f;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _agent.destination = path.GetCurrentWayPoint();
    }

    private void Update()
    {
        if(_agent.remainingDistance <= 0.1f)
        {
            time += Time.deltaTime;
            if(time >= waitTimeOnWayPoint)
            {
                time = 0f;
                _agent.destination = path.GetNextWayPoint();
            }
        }
    }

    public void Damage(float damage)
    {
        Debug.Log("AUWW!");
        hitpoints -= damage;
        //do some damage effect here
    }
}
 