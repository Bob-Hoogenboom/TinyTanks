using UnityEngine;

public class CupolaUIRotation : MonoBehaviour
{
    public Transform cupolaTransform; // Assign the cupola Transform (e.g., the 3D model part that rotates)

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Get the Y-axis rotation of the cupola in world space
        float yaw = cupolaTransform.localEulerAngles.y;

        // Apply it to the UI rotation (Z axis, because UI rotates around Z)
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, -yaw);
    }
}
