using UnityEngine;
using UnityEngine.AI;

public class TankPatrol : TankBaseState
{
    private Vector3 _currentWaypoint;
    private float _patrolRange;

    public override void EnterState(TankStateManager tank)
    {
        SetNewWaypoint(tank);
        tank.Agent.SetDestination(_currentWaypoint);
    }

    public override void UpdateState(TankStateManager tank)
    {
        if(GetDistanceTo(tank, _currentWaypoint) <= 0.1f)
        {
            //arrived at waypoint
            tank.SwitchState(tank.Idle);
        }

        if(GetDistanceTo(tank, tank.Player.transform.position) <= tank.DetectionRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(tank.transform.position, tank.Player.transform.position, out hit))
            {
                if(hit.transform.gameObject == tank.Player)
                {
                    Debug.Log("Player hit!");
                    //the enemy will move towards this destination
                    tank.SwitchState(tank.Chase);
                }
                else
                {
                    Debug.Log(hit.transform.gameObject + "No Player Hit");
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

    //Searches for a new walkpoint when no walkpoint is assigned yet
    private void SetNewWaypoint(TankStateManager tank)
    {
        float randomZ = Random.Range(-_patrolRange, _patrolRange);
        float randomX = Random.Range(-_patrolRange, _patrolRange);

        _currentWaypoint = new Vector3(tank.transform.position.x + randomX, tank.transform.position.y, tank.transform.position.z + randomZ);

        //checks if the walkpoint is on the NavMesh 
        if (!NavMesh.SamplePosition(_currentWaypoint, out _, 1.0f, NavMesh.AllAreas))
        {
            SetNewWaypoint(tank);
        }
    }

    private float GetDistanceTo(TankStateManager tank, Vector3 target)
    {
        float dist = Vector3.Distance(tank.transform.position, target);
        return dist;
    }
}
