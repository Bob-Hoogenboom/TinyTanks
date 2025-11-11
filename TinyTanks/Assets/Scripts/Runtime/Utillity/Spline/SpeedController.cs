using UnityEngine;
using Dreamteck.Splines;

[RequireComponent(typeof(SplineTrigger))]
public class SpeedController : MonoBehaviour
{
    public enum Mode { SetSpeed, AddSpeed }
    public Mode mode = Mode.SetSpeed;
    public float speedValue = 2f;

    private SplineTrigger trigger;

    void Awake()
    {
        trigger = GetComponent<SplineTrigger>();

        // Subscribe to trigger events
        trigger.onCross.AddListener(OnUserEnter);
    }

    void OnDestroy()
    {
        // Clean up listener when destroyed
        trigger.onCross.RemoveListener(OnUserEnter);
    }

    private void OnUserEnter(SplineUser user)
    {
        var controller = user.GetComponent<SplineDolly>();
        if (controller == null) return; // not a spline-controlled object

        if (mode == Mode.SetSpeed)
            controller.SetSpeed(speedValue);
        else
            controller.AddSpeed(speedValue);
    }
}