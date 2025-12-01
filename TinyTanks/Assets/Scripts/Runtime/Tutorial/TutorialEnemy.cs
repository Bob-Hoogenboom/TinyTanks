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
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float range;
    [SerializeField]private float _coolDown = 0f;
    [Space]
    public UnityEvent onShoot;

    private bool _isActive;
    private float _cupolaRotateSpeed = 10f;
    private float _barrelRotateSpeed = 10f;

    [Header("Path")]
    [SerializeField] private float waitTimeOnWayPoint = 1f;
    [SerializeField] private EnemyPath path;

    private NavMeshAgent _agent;
    private float time = 0f;

    [Space]
    public UnityEvent onEnemyDefeat;


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

        if(dist < range * 2.5) playerInRange = false;

        return playerInRange;
    }

    private void AimAtPlayer()
    {
        //Offset on the playertarget
        Vector3 target = _playerTarget.transform.position + (Vector3.up * 0.5f);

        // ---------------- CUPOLA (Yaw only, smoothed) ----------------
        Vector3 cupolaDir = target - cupola.transform.position;
        cupolaDir.y = 0f; // ignore vertical difference
        Quaternion cupolaTargetRot = Quaternion.LookRotation(cupolaDir, Vector3.up);

        // Smooth rotation
        cupola.transform.rotation = Quaternion.Slerp(
            cupola.transform.rotation,
            cupolaTargetRot,
            Time.deltaTime * _cupolaRotateSpeed
        );


        // ---------------- BARREL (Pitch only, smoothed) ----------------
        Vector3 barrelDir = target - barrel.transform.position;
        barrelDir = barrel.transform.parent.InverseTransformDirection(barrelDir);

        Quaternion barrelTargetRot = Quaternion.LookRotation(barrelDir);
        Vector3 e = barrelTargetRot.eulerAngles;

        // Only use X rotation (pitch)
        Quaternion onlyPitch = Quaternion.Euler(e.x, 0f, 0f);

        // Smooth local rotation
        barrel.transform.localRotation = Quaternion.Slerp(
            barrel.transform.localRotation,
            onlyPitch,
            Time.deltaTime * _barrelRotateSpeed
        );

        //Timer
        _coolDown += Time.deltaTime;
        if (_coolDown >= 3f)
        {
            Vector3 dir = target - muzzle.transform.position;
            //Clean Shot
            RaycastHit hit;

            Debug.DrawRay(muzzle.transform.position, dir , Color.yellow, 10f);
            if (Physics.Raycast(muzzle.transform.position, dir, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject == _playerTarget.transform.gameObject)
                {
                    _coolDown = 0f;
                    Shoot();
                }
            }
        }
    }

    private void Shoot()
    {
        Quaternion rotation = muzzle.rotation;
        GameObject bulletObj = Instantiate(bulletPrefab, muzzle.position, rotation);
        Rigidbody brb = bulletObj.GetComponent<Rigidbody>();

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.parent = gameObject; //Set parent to check if you dont hit yourself and count a point if your bullet hits something

        brb.AddForce(muzzle.forward * bulletSpeed, ForceMode.VelocityChange);
        Destroy(bulletObj, 5f);

        onShoot.Invoke();
    }

    public void Damage(float damage)
    {
        Debug.Log("AUWW!");
        hitpoints -= damage;
        if(hitpoints <= 0)
        {
            Death();
        }
        //do some damage effect here
    }

    private void Death()
    {
        onEnemyDefeat.Invoke();
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, (range * 2.5f));
    }
}
 