using UnityEngine;

namespace Utility 
{
    public class Billboard : MonoBehaviour
    {
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
        }

        private void LateUpdate()
        {
            transform.LookAt(_cam.transform);
        }
    }
}
