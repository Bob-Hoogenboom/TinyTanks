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
        //if Player is close => tank.SwitchState(tank.Chase);

        _currentTimer -= Time.deltaTime;

        if(_currentTimer <= 0)
        {
            tank.SwitchState(tank.Patrol);
        }
    }

    public override void ExitState(TankStateManager tank)
    {
        
    }

    public override void OnCollisionEnter(TankStateManager tank)
    {
        
    }
}
