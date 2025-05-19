using UnityEngine;


/// <summary>
/// Source: https://www.youtube.com/watch?v=jt1bQX4sUoA
/// </summary>
public class CamOrbit : MonoBehaviour
{
    [SerializeField] private Transform lookAt;
    [SerializeField] private Vector3 offSet;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float maxOrbitDistance = 10f;
    [SerializeField] private float minOrbitDistance = 2f;

    private float _orbitRadius = 5f;

    private bool _isCamActive = false;
    private float mouseX;
    private float mouseY;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.C)) 
        {
            _isCamActive = !_isCamActive;
            _orbitRadius = 5f;
        }

        if (_isCamActive)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 targetPosition = lookAt.position + offSet;
                transform.LookAt(targetPosition);

                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");

                Debug.Log("X: " + mouseX + " /Y: " + mouseY);
                transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
            }

            _orbitRadius -= Input.mouseScrollDelta.y / sensitivity;
            _orbitRadius = Mathf.Clamp(_orbitRadius, minOrbitDistance, maxOrbitDistance);

            Vector3 targetPositionWithOffset = lookAt.position + offSet;
            transform.position = targetPositionWithOffset - transform.forward * _orbitRadius;
        }
    }
}
