using UnityEngine;

public class TankDefeat : TankBaseState
{
    private float _cupolaRotateSpeed = 10f;
    private float _barrelRotateSpeed = 10f;

    public override void EnterState(TankStateManager tank)
    {
        tank.Agent.SetDestination(tank.transform.position);
        AimAtTarget(tank);
    }

    public override void UpdateState(TankStateManager tank)
    {

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
        Vector3 target = tank.transform.position + (new Vector3(0f, -2f, 1f));

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
