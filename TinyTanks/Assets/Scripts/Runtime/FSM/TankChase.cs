using UnityEngine;

public class TankChase : TankBaseState
{
    private float _refreshWaypoint = 2f;
    private float _currentTimer;
    private Vector3 _raycastOffset = new Vector3(0f, .5f, 0f);
    private Vector3 _waypoint;

    public override void EnterState(TankStateManager tank)
    {
        tank.Agent.SetDestination(tank.Player.transform.position);
        _currentTimer = _refreshWaypoint;
    }

    public override void UpdateState(TankStateManager tank)
    {
        Debug.Log("In Chase State");
        //if the enemy is in range of the player switch to shoot state
        if (GetDistanceTo(tank, tank.Player.transform.position) <= tank.ShootingRange * 0.75f)
        {
            tank.SwitchState(tank.Shoot);
        }

        //every second if the player is in sight the tank will update its position to the player
        _currentTimer -= Time.deltaTime;

        if(_currentTimer <= 0)
        {
            RaycastHit hit;
            Vector3 dir = tank.Player.transform.position + _raycastOffset - tank.transform.position + _raycastOffset;
            if (Physics.Raycast(tank.transform.position, dir, out hit))
            {
               Debug.DrawRay(tank.transform.position, dir, Color.yellow, 10f);
                if (hit.transform.gameObject == tank.Player.gameObject)
                {
                    Debug.Log("Player detected!");
                    //the enemy will move towards this destination
                    _waypoint = tank.Player.transform.position;
                    tank.Agent.SetDestination(_waypoint);
                }
                else
                {
                    Debug.Log(hit.transform.gameObject + "No Player detected");
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
        //Debug.Log(tank.transform.position + " => " + target + " = " + dist);
        return dist;
    }

}
