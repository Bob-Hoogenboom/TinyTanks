using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// Statemachine* and let the enemy be for freeroam and stationary stuff
/// </summary>
public class StationaryEnemy : MonoBehaviour, IDamagable
{
    [Header("References")]
    private SinglePlayerTank _playerTarget;

    [Header("Detection")]
    [SerializeField] private Transform detectionOrigin;
    [SerializeField] private float attackRange = 8f;

    [Header("Aiming & Shooting")]
    public bool canShoot = true;
    [SerializeField] private LayerMask hittable;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float _coolDown = 0f;
    private float time = 0f;
    [Space]
    [SerializeField] private GameObject bulletPrefab;
    [Space]
    [SerializeField] private GameObject cupola;
    [SerializeField] private GameObject barrel;
    [SerializeField] private Transform muzzle;
    private float _cupolaRotateSpeed = 10f;
    private float _barrelRotateSpeed = 10f;

    [Header("Stats")]
    [SerializeField] private float hitPoints = 1f;
    public float HitPoints { get { return hitPoints; } }

    [Header("Effects")]
    public UnityEvent onShoot;
    public UnityEvent onEnemyDefeat;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = false;
    [SerializeField] private Color attackColor = new Color(1f, 0f, 0f, 0.2f);


    private void Awake()
    {
        _playerTarget = FindAnyObjectByType<SinglePlayerTank>();
    }


    private void Update()
    {
        float distance = GetDistanceToPlayer();

        if(distance < attackRange)
        {
            AimAtPlayer();
        }

    }

    private float GetDistanceToPlayer()
    {
        float dist = Vector3.Distance(detectionOrigin.position, _playerTarget.transform.position);
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
        time += Time.deltaTime;
        if (time >= _coolDown)
        {
            Vector3 dir = target - muzzle.transform.position;
            //Clean Shot
            RaycastHit hit;

            Debug.DrawRay(muzzle.transform.position, dir , Color.yellow, 10f);
            if (Physics.Raycast(muzzle.transform.position, dir, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject == _playerTarget.transform.gameObject)
                {
                    time = 0f;
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
        this.enabled = false;
    }

    public void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = attackColor;
        Gizmos.DrawSphere(detectionOrigin.position, attackRange);
    }
}
 