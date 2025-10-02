using UnityEngine;

namespace Effect
{
    public class RotateEffect : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float rotationSpeed = 35f; 

        [Header("Float Settings")]
        [Tooltip("Changes the height of the floating effect")]
        public float floatAmplitude = 0.2f;
        [Tooltip("Changes the speed of the up and down bobbing")]
        public float floatFrequency = 0.2f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
        }

        private void LateUpdate()
        {
            // Rotate around Y-axis
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Floating up and down with easing (sine wave)
            float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency * Mathf.PI * 2) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
