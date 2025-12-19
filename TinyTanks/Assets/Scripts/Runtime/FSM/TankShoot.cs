using UnityEngine;

public class TankShoot : TankBaseState
{
    private float _currentTimer = 3f;
    private float _coolDown = 3f;

    private float _cupolaRotateSpeed = 10f;
    private float _barrelRotateSpeed = 10f;

    public override void EnterState(TankStateManager tank)
    {
        tank.Agent.SetDestination(tank.transform.position);
    }

    public override void UpdateState(TankStateManager tank)
    {
        Debug.Log("In Shoot State");
        AimAtTarget(tank);

        //check if player is in line of sight for 3 seconds
        //Check player position every 3 seconds
        //if player was shoot
        _currentTimer -= Time.deltaTime;

        if (_currentTimer <= 0)
        {
            tank.SwitchState(tank.Idle);
        }

        //if the player leaves line of sight SetDestination to the last player position

        //arrived and no player in sight? go to patrol
        //see the player durring move? go to chase
    }
    public override void ExitState(TankStateManager tank)
    {

    }

    public override void OnCollisionEnter(TankStateManager tank)
    {

    }

    private void AimAtTarget(TankStateManager tank)
    {
        //Offset on the playertarget
        Vector3 target = tank.Player.transform.position + (Vector3.up * 0.5f);

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
}
