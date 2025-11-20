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
    [SerializeField] private LayerMask hittable;
    [SerializeField] private GameObject cupola;
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float range;
    [Space]
    public UnityEvent onShoot;

    private bool _isActive;

    [Header("Path")]
    [SerializeField] private float waitTimeOnWayPoint = 1f;
    [SerializeField] private EnemyPath path;

    private NavMeshAgent _agent;
    private float time = 0f;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerTarget = FindAnyObjectByType<TutorialTank>();
    }

    private void Start()
    {
        _agent.destination = path.GetCurrentWayPoint();
    }

    private void Update()
    {
        if (_isActive)
        {
            AimAtPlayer();
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

        CheckPlayerRange();
    }

    private bool CheckPlayerRange()
    {
        bool playerInRange;
        float dist = Vector3.Distance(transform.position, _playerTarget.transform.position);
        Vector3 dir = _playerTarget.transform.position - transform.position;

        playerInRange = (dist < range) ? true : false;

        //Player was never found before but is in range
        if (!_isActive && playerInRange)
        {
            Debug.Log(_isActive + " : " + playerInRange);
            //chack for a direct hit on the player
            RaycastHit hit;

            Debug.DrawRay(transform.position + Vector3.up, dir, Color.yellow, 10f);
            if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, Mathf.Infinity))
            {
                Debug.Log(hit);
                if(hit.transform.gameObject == _playerTarget.transform.gameObject)
                {
                    //activate tank
                    _isActive = true;
                }
            }
        }

        return playerInRange;
    }

    private void AimAtPlayer()
    {
        // --- CUPOLA (Yaw only: rotate around Y axis) ---
        Vector3 cupolaDir = _playerTarget.transform.position - cupola.transform.position;
        cupolaDir.y = 0f; // ignore vertical difference
        cupola.transform.rotation = Quaternion.LookRotation(cupolaDir, Vector3.up);

        // --- BARREL (Pitch only: rotate up/down but keep parent yaw) ---
        Vector3 barrelDir = _playerTarget.transform.position - barrel.transform.position;
        barrelDir = barrel.transform.parent.InverseTransformDirection(barrelDir); // convert to local space
        Quaternion targetRot = Quaternion.LookRotation(barrelDir);

        // Only pitch (local X)
        Vector3 e = targetRot.eulerAngles;
        barrel.transform.localEulerAngles = new Vector3(e.x, 0f, 0f);

        //TODO
        //timer + Clear Vision
        //Shoot()
    }

    public void Damage(float damage)
    {
        Debug.Log("AUWW!");
        hitpoints -= damage;
        //do some damage effect here
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
 