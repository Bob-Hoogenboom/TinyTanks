using UnityEngine;
using TMPro;

public class BarrelAngleDisplayTMP : MonoBehaviour
{

    [Header("Scene refs")]
    [SerializeField] private Transform barrelTransform;        // Assign the barrel Transform here
    [SerializeField] private TextMeshProUGUI angleText;        // Assign the TMP Text element here
    [SerializeField] private RectTransform reticle;

    [Header("Mapping")]
    public float pixelsPerDegree = 8;
    public float minPitch = -10f;
    public float maxPitch = 10f;

    private Vector2 baseAnchor;
    private float baseY;

    private void Awake()
    {
        if(reticle != null)
        {
            baseAnchor = reticle.anchoredPosition;
            baseY = baseAnchor.y;
        }
    }

    void Update()
    {
        // Get the world pitch (X rotation)
        float pitch = barrelTransform.eulerAngles.x;

        // Convert from 0–360 to -180–180 range for easier interpretation
        if (pitch > 180f) pitch -= 360f;

        // Display the pitch as a rounded integer with degree symbol
        if(angleText != null)
        angleText.text = (Mathf.RoundToInt(pitch) * -1) + "°";

        if(reticle != null)
        {
            float clamped = Mathf.Clamp(pitch, minPitch, maxPitch);
            float dir = -1f;
            float targetY = baseY + dir * clamped * pixelsPerDegree;

            Vector2 pos = baseAnchor;
            pos.y = targetY;
            pos.x = baseAnchor.x;

            reticle.anchoredPosition = pos;
        }
    }


}
