using UnityEngine;

public class TankIdle : TankBaseState
{
    private Vector2 _timerRange = new Vector2(3f, 6f);
    private float _currentTimer;

    public override void EnterState(TankStateManager tank)
    {
        tank.Agent.SetDestination(tank.transform.position);
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
                if (Physics.Raycast(tank.transform.position, tank.Player.transform.position, out hit))
                {
                    if (hit.transform.gameObject == tank.Player)
                    {
                        Debug.Log("Player detected!");
                        //the enemy will move towards this destination
                        tank.SwitchState(tank.Chase);
                    }
                    else
                    {
                        Debug.Log(hit.transform.gameObject + "No Player detected");
                    }
                }
            }
        
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
