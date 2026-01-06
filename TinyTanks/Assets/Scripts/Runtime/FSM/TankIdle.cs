using UnityEngine;

public class TankIdle : TankBaseState
{
    private Vector2 _timerRange = new Vector2(3f, 6f);
    private Vector3 _raycastOffset = new Vector3(0f, .3f, 0f);
    private float _currentTimer;

    public override void EnterState(TankStateManager tank)
    {
        if (tank.Agent.isOnNavMesh)
        {
            tank.Agent.SetDestination(tank.transform.position);
        }
        _currentTimer = Random.Range(_timerRange.x, _timerRange.y);
    }

    public override void UpdateState(TankStateManager tank)
    {
        Debug.Log("In Idle State");
        //if Player is close => tank.SwitchState(tank.Chase);

        if (tank.CanMove)
        {
            _currentTimer -= Time.deltaTime;

            if(_currentTimer <= 0)
            {
                tank.SwitchState(tank.Patrol);
            }

            if (GetDistanceTo(tank, tank.Player.transform.position) <= tank.DetectionRange)
            {
                RaycastHit hit;
                Vector3 dir = tank.Player.transform.position + _raycastOffset - tank.transform.position + _raycastOffset;
                if (Physics.Raycast(tank.transform.position, dir, out hit))
                {
                    Debug.DrawRay(tank.transform.position + _raycastOffset, dir, Color.yellow, 1f);

                    if (hit.transform.gameObject == tank.Player.gameObject)
                    {
                        //Debug.Log("Player detected!");
                        tank.SwitchState(tank.Chase);
                    }
                    else
                    {

                    }
                }
            }
        }

        if (GetDistanceTo(tank, tank.Player.transform.position) <= tank.DetectionRange)
        {
            tank.SwitchState(tank.Shoot);
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
        return dist;
    }
}
