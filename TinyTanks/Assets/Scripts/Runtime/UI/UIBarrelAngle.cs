using UnityEngine;
using TMPro;

public class BarrelAngleDisplayTMP : MonoBehaviour
{
    public Transform barrelTransform;        // Assign the barrel Transform here
    public TextMeshProUGUI angleText;        // Assign the TMP Text element here

    void Update()
    {
        // Get the world pitch (X rotation)
        float pitch = barrelTransform.eulerAngles.x;

        // Convert from 0–360 to -180–180 range for easier interpretation
        if (pitch > 180f)
            pitch -= 360f;

        // Display the pitch as a rounded integer with degree symbol
        angleText.text = Mathf.RoundToInt(pitch) + "°";
    }
}
