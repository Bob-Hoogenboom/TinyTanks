using UnityEngine;

namespace Utility 
{
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        [Header("Editor Mode")]
        [SerializeField] private bool updateInEditMode = false;

        private Camera _cam;

        private void Start()
        {
            if (Application.isPlaying) _cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying && !updateInEditMode) return;

            if (_cam == null)
            {
                // In edit mode, Camera.main doesn’t always exist — find any scene camera
                if (Camera.main != null)
                    _cam = Camera.main;
                else if (SceneViewCamera() != null)
                    _cam = SceneViewCamera();
                else
                    return;
            }

            transform.LookAt(_cam.transform);
        }

        // Try to get Scene View camera when in edit mode
        private Camera SceneViewCamera()
        {
#if UNITY_EDITOR
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView != null)
                return sceneView.camera;
#endif
            return null;
        }
    }
}