using UnityEngine;

public class TankChase : TankBaseState
{
    private float _refreshWaypoint = 1f;
    private float _currentTimer;
    //private Vector3 _raycastOrigin = new Vector3(0f, 1f, 0f);
    private Vector3 _waypoint;

    public override void EnterState(TankStateManager tank)
    {
        tank.Agent.SetDestination(tank.Player.transform.position);
        _currentTimer = _refreshWaypoint;
    }

    public override void UpdateState(TankStateManager tank)
    {
        //if the enemy is in range of the player switch to shoot state
        if(GetDistanceTo(tank, _waypoint) <= tank.ShootingRange)
        {
            tank.SwitchState(tank.Shoot);
        }

        //every second if the player is in sight the tank will update its position to the player
        _currentTimer -= Time.deltaTime;

        if(_currentTimer <= 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(tank.transform.position, tank.Player.transform.position, out hit))
            {
                if (hit.transform.gameObject == tank.Player)
                {
                    Debug.Log("Player hit!");
                    //the enemy will move towards this destination
                    tank.Agent.SetDestination(tank.Player.transform.position);
                }
                else
                {
                    Debug.Log(hit.transform.gameObject + "No Player Hit");
                }
            }
            _currentTimer = _refreshWaypoint;
        }

        if(GetDistanceTo(tank, _waypoint) <= 0.1f)
        {
            tank.SwitchState(tank.Idle);
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
