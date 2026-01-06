using UnityEngine;
using UnityEngine.AI;

public class TankPatrol : TankBaseState
{
    private Vector3 _raycastOffset = new Vector3(0f, .3f, 0f);
    private Vector3 _currentWaypoint;
    private float _patrolRange;

    public override void EnterState(TankStateManager tank)
    {
        if (tank.Agent.isOnNavMesh)
        {
            tank.Agent.SetDestination(_currentWaypoint);
        }
        _patrolRange = tank.DetectionRange / 0.75f;
        SetNewWaypoint(tank);
    }

    public override void UpdateState(TankStateManager tank)
    {
        Debug.Log("In Patrol State");
        if(GetDistanceTo(tank, _currentWaypoint) <= 0.1f)
        {
            //arrived at waypoint
            tank.SwitchState(tank.Idle);
        }

        if(GetDistanceTo(tank, tank.Player.transform.position) <= tank.DetectionRange)
        {
            RaycastHit hit;
            Vector3 dir = tank.Player.transform.position + _raycastOffset - tank.transform.position + _raycastOffset;
            if (Physics.Raycast(tank.transform.position, dir, out hit))
            {
                Debug.DrawRay(tank.transform.position + _raycastOffset, dir, Color.yellow, 10f);
                if(hit.transform.gameObject == tank.Player.gameObject)
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
        if (!NavMesh.SamplePosition(_currentWaypoint, out _, 0.1f, NavMesh.AllAreas))
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
