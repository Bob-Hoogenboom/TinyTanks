using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float delay = 5f;
    [SerializeField] private GameObject platform;

    private Vector3 _targetPos;

    private void Start()
    {
        platform.transform.position = pointA.transform.position;
        _targetPos = pointB.transform.position;
        StartCoroutine(MovePlatform());
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            while ((_targetPos - platform.transform.position).sqrMagnitude > 0.01f) 
            {
                platform.transform.position = Vector3.MoveTowards(platform.transform.position, _targetPos, speed * Time.deltaTime);
                yield return null;
            }

            _targetPos = _targetPos == pointA.position ? pointB.position : pointA.position;

            yield return new WaitForSeconds(delay);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(pointA.position, Vector3.one * 0.1f);
        Gizmos.DrawCube(pointB.position, Vector3.one * 0.1f);
    }
}
