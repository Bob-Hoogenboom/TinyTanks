using UnityEngine;
using UnityEngine.AI;

public class TankShoot : TankBaseState
{
    private float _currentTimer = 3f;
    private float _coolDown = 3f;

    private float _cupolaRotateSpeed = 10f;
    private float _barrelRotateSpeed = 10f;

    private Vector3 _raycastOffset = new Vector3(0f, .3f, 0f);
    private Vector3 _lastPlayerPosition;

    private bool _isMoving = false;

    private float _bulletSpeed = 10f;

    public override void EnterState(TankStateManager tank)
    {
        if (tank.Agent.isOnNavMesh)
        {
            tank.Agent.SetDestination(tank.transform.position);
        }
        _lastPlayerPosition = tank.Player.transform.position;
        _currentTimer = _coolDown;

        _isMoving = false;
    }

    public override void UpdateState(TankStateManager tank)
    {
        Debug.Log("In Shoot State");

        //check if player is in line of sight for 3 seconds
        //Check player position every 3 seconds
        //if player is, shoot

        RaycastHit hit;
        Vector3 dir = tank.Player.transform.position + _raycastOffset - tank.transform.position + _raycastOffset;
        Debug.DrawRay(tank.transform.position + _raycastOffset, dir, Color.yellow, 0.1f);


        if (Physics.Raycast(tank.transform.position, dir, out hit))
        {
            //look for line of sight with the player
            if (hit.transform.gameObject == tank.Player.gameObject && !_isMoving)
            {
                //the player is in line of sight
                Debug.DrawRay(tank.transform.position + _raycastOffset, dir, Color.red, 0.5f);
                _currentTimer -= Time.deltaTime;
                _lastPlayerPosition = tank.Player.transform.position;

                //Offset on the playertarget
                Vector3 target = tank.Player.transform.position + (Vector3.up * 0.5f);
                AimAtTarget(tank, target);
            }
            else if(hit.transform.gameObject != tank.Player.gameObject && !_isMoving)
            {
                //the player broke line of sight and the enemy moves to the last location it has seen the player
                if (tank.CanMove)
                {
                    tank.Agent.SetDestination(_lastPlayerPosition);
                }

                _currentTimer = _coolDown;
                _isMoving = true;
            }
            else
            {
                Debug.Log("else::::::::");

                if (!tank.CanMove)
                {
                    Debug.Log("switch");
                    tank.SwitchState(tank.Idle);
                }

                Vector3 target = _lastPlayerPosition + _raycastOffset;
                AimAtTarget(tank, target);

                //the enemy did not see the player again and arrived at the last position
                if (GetDistanceTo(tank, _lastPlayerPosition) <= 0.1f)
                {
                    tank.SwitchState(tank.Idle);
                }

                //the enemy did see the player durring movin and gives chase
                if (hit.transform.gameObject == tank.Player.gameObject && tank.CanMove)
                {
                    tank.SwitchState(tank.Chase);
                }
            }

            //Debug.Log($"Moving: {_isMoving}, RaycastHit: {hit} ");
        }

        if(_currentTimer <= 0)
        {
            Shoot(tank);
        }
    }
    public override void ExitState(TankStateManager tank)
    {

    }
     
    public override void OnCollisionEnter(TankStateManager tank)
    {

    }

    private float GetDistanceTo(TankStateManager tank, Vector3 target)
    {
        float dist = Vector3.Distance(tank.transform.position, target);
        //Debug.Log(tank.transform.position + " => " + target + " = " + dist);
        return dist;
    }

    private void AimAtTarget(TankStateManager tank, Vector3 target)
    {
        // ---------------- CUPOLA (Yaw only, smoothed) ----------------
        Vector3 cupolaDir = target - tank.Cupola.transform.position;
        cupolaDir.y = 0f; // ignore vertical difference
        Quaternion cupolaTargetRot = Quaternion.LookRotation(cupolaDir, Vector3.up);

        // Smooth rotation
        tank.Cupola.transform.rotation = Quaternion.Slerp(
            tank.Cupola.transform.rotation,
            cupolaTargetRot,
            Time.deltaTime * _cupolaRotateSpeed
        );


        // ---------------- BARREL (Pitch only, smoothed) ----------------
        Vector3 barrelDir = target - tank.Barrel.transform.position;
        barrelDir = tank.Barrel.transform.parent.InverseTransformDirection(barrelDir);

        Quaternion barrelTargetRot = Quaternion.LookRotation(barrelDir);
        Vector3 e = barrelTargetRot.eulerAngles;

        // Only use X rotation (pitch)
        Quaternion onlyPitch = Quaternion.Euler(e.x, 0f, 0f);

        // Smooth local rotation
        tank.Barrel.transform.localRotation = Quaternion.Slerp(
            tank.Barrel.transform.localRotation,
            onlyPitch,
            Time.deltaTime * _barrelRotateSpeed
        );
    }

    private void Shoot(TankStateManager tank)
    {
        //shooting logic*
        Quaternion rotation = tank.Muzzle.rotation;
        GameObject bulletObj = Object.Instantiate(tank.BulletPrefab, tank.Muzzle.position, rotation);
        Rigidbody brb = bulletObj.GetComponent<Rigidbody>();

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.parent = tank.gameObject; //Set parent to check if you dont hit yourself and count a point if your bullet hits something

        brb.AddForce(tank.Muzzle.forward * _bulletSpeed, ForceMode.VelocityChange);
        Object.Destroy(bulletObj, 5f);


        _currentTimer = _coolDown;
        Debug.Log("Shoot");
    }
}
