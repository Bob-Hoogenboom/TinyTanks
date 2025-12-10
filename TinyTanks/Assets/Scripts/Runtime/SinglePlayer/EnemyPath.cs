using UnityEngine;

public enum PathType
{
    LOOP,
    PINGPONG,
    IDLE
}

public class EnemyPath : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public PathType pathType = PathType.LOOP;

    private int _direction = 1;
    private int _index;

    [Header("Debug")]
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private Color waypointColor = Color.red;
    [SerializeField] private float waypointSize = 0.2f;

    public Vector3 GetCurrentWayPoint()
    {
        return waypoints[_index].position;
    }

    public Vector3 GetNextWayPoint()
    {
        if(waypoints.Length == 0) return transform.position;

        _index = GetNextWayPointIndex();
        Vector3 nextWaypoint = waypoints[_index].position;

        return nextWaypoint;
    }

    private int GetNextWayPointIndex()
    {
        _index += _direction;

        switch (pathType)
        {
            case PathType.LOOP:
                _index %= waypoints.Length;
                break;

            case PathType.PINGPONG:
                if(_index >= waypoints.Length || _index < 0)
                {
                    _direction *= -1;
                    _index += _direction * 2;
                }
                break;

            case PathType.IDLE:
                _index = 0;
                break;

            default:
                break;
        }

        return _index;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = lineColor;

        if (pathType != PathType.IDLE)
        {
            for (int i = 0; i < waypoints.Length -1; i++) 
            {
           
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);        
            }

            if (pathType == PathType.LOOP)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }

        Gizmos.color = waypointColor;

        foreach (Transform waypoint in waypoints) 
        {
            Gizmos.DrawCube(waypoint.position, Vector3.one * waypointSize);        
        }
    }
}
