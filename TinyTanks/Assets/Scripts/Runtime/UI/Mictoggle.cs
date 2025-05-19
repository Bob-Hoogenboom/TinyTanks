using UnityEngine;
using UnityEngine.UI; // Required to work with UI elements

public class MicHold : MonoBehaviour
{
    public Image micImage; // Reference to the UI image you want to show/hide
    public KeyCode holdKey = KeyCode.Alpha1; // The key to hold down for toggling the image

    private bool isMicActive = false; // Whether the mic (or image) is active or not

    // Start is called before the first frame update
    void Start()
    {
        if (micImage != null)
        {
            micImage.enabled = isMicActive; // Set the initial state of the mic image (off)
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the hold key is being pressed
        if (Input.GetKey(holdKey))
        {
            // If the key is held down, activate the mic image
            if (!isMicActive)
            {
                isMicActive = true;
                micImage.enabled = true;
            }
        }
        else
        {
            // If the key is not held down, deactivate the mic image
            if (isMicActive)
            {
                isMicActive = false;
                micImage.enabled = false;
            }
        }
    }
}
