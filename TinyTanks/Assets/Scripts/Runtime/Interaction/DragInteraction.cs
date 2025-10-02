using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Interaction
{
    public class LocalDragMove : MonoBehaviour
    {
        [Header("Radius")]
        public float radius = 2f;

        [Header("Drag Plane")]
        [Tooltip("If true, use this transform's up as the drag plane normal. Otherwise use camera-facing plane.")]
        public bool useLocalPlane = false;

        [Header("Constraints")]
        [Tooltip("Prevent vertical (local Y) movement while dragging.")]
        public bool lockLocalY = true;

        Camera mainCam;
        bool isDragging = false;
        Vector3 startLocalPos;
        Plane dragPlane;      // plane used for raycast
        Vector3 worldCenter;  // center of allowed area in world space

        private void Start()
        {
            mainCam = Camera.main;
            startLocalPos = transform.localPosition;
            worldCenter = (transform.parent != null) ? transform.parent.TransformPoint(startLocalPos) : startLocalPos;
        }

        private void OnMouseDown()
        {
            if (mainCam == null) mainCam = Camera.main;
            // Recalculate center in case parent moved
            worldCenter = (transform.parent != null) ? transform.parent.TransformPoint(startLocalPos) : startLocalPos;

            // create drag plane (camera-facing by default)
            if (useLocalPlane)
                dragPlane = new Plane(transform.up, worldCenter); // local up as normal
            else
                dragPlane = new Plane(mainCam.transform.forward, worldCenter);

            isDragging = true;
        }

        private void OnMouseUp()
        {
            isDragging = false;
        }

        private void Update()
        {
            if (!isDragging) return;

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (!dragPlane.Raycast(ray, out float enter)) return;

            Vector3 hitPoint = ray.GetPoint(enter);

            // Convert the hit point into the parent's local space (so startLocalPos is comparable)
            Vector3 localHit = (transform.parent != null) ? transform.parent.InverseTransformPoint(hitPoint) : hitPoint;

            // Offset relative to starting local position
            Vector3 localOffset = localHit - startLocalPos;

            // Optionally remove vertical/local Y movement so object stays on local XZ plane
            if (lockLocalY) localOffset.y = 0f;

            // Clamp to radius
            if (localOffset.magnitude > radius)
                localOffset = localOffset.normalized * radius;

            // Final target local position
            Vector3 targetLocalPos = startLocalPos + localOffset;

            // Move the object
            transform.localPosition = targetLocalPos;

            // Make it look at the target position (in world space)
            Vector3 targetWorldPos = (transform.parent != null) ? transform.parent.TransformPoint(targetLocalPos) : targetLocalPos;
            Debug.Log(targetWorldPos);
            transform.LookAt(targetWorldPos, Vector3.up);
        }

        // Visualize allowed radius in scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = (transform.parent != null) ? transform.parent.TransformPoint(transform.localPosition) : transform.position;
#if UNITY_EDITOR
            if (Application.isPlaying)
                center = (transform.parent != null) ? transform.parent.TransformPoint(startLocalPos) : startLocalPos;
#endif

            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
