using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.WSA;

/// <summary>
/// Statemachine* and let the enemy be for freeroam and stationary stuff
/// </summary>
public class TutorialEnemy : MonoBehaviour, IDamagable
{
    [Header("References")]
    [SerializeField] private GameObject cupola;
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;

    private TutorialTank _playerTarget;

    [Header("Variables")]
    [SerializeField] private float hitPoints = 3f;
    public float HitPoints => hitPoints;

    [SerializeField] private bool isStationary = false;
    private bool _isActive;
    [Space]
    public UnityEvent onEnemyDefeat;

    [Header("Attack")]
    public bool canShoot = true;
    [SerializeField] private LayerMask hittable;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float attackRange;
    [SerializeField] private float vision;
    [SerializeField]private float _coolDown = 0f;
    [Space]
    public UnityEvent onShoot;
    private float time = 0f;

    [Header("Movement")]
    [SerializeField] private float waitTimeOnWayPoint = 1f;

    private NavMeshAgent _agent;

    private float _cupolaRotateSpeed = 10f;
    private float _barrelRotateSpeed = 10f;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerTarget = FindAnyObjectByType<TutorialTank>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        _isActive = GetDistanceToPlayer() <= attackRange ? true : false;

        if (_isActive)
        {
            AimAtPlayer();
            _isActive = GetDistanceToPlayer() <= vision ? true : false;
        }


        


    }




    private bool CheckIfPlayerRange()
    {
        bool playerInRange;
        float dist = Vector3.Distance(transform.position, _playerTarget.transform.position);
        Vector3 dir = _playerTarget.transform.position - transform.position;

        playerInRange = (dist < attackRange) ? true : false;

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

        if(dist < attackRange * 2.5) playerInRange = false;

        return playerInRange;
    }

    private float GetDistanceToPlayer()
    {
        float dist = Vector3.Distance(transform.position, _playerTarget.transform.position);
        return dist;
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

        if (!canShoot) return;

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
        hitPoints -= damage;

        if(hitPoints <= 0) 
        {
            Death();
        }


    }

    private void Death()
    {
        onEnemyDefeat.Invoke();
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, (attackRange * 2.5f));
    }
}
 