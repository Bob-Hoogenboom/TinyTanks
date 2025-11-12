using Dreamteck.Splines;
using UnityEngine;

public class SpeedTriggerData : MonoBehaviour
{
    [SerializeField] private SplineDolly splineDolly;

    
    public void TimedStopDolly(float forSeconds)
    {

        splineDolly.PauseForSeconds(forSeconds);
    }

    public void InputStopDolly()
    {
        splineDolly.PauseUntilInput();
    }

    public void SlowDolly()
    {

        splineDolly.currentSpeed = splineDolly.dollySpeed / 2;
    }

    public void NormalDolly()
    {

        splineDolly.currentSpeed = splineDolly.dollySpeed;
    }

    public void FastDolly()
    {

        splineDolly.currentSpeed = splineDolly.dollySpeed * 2;
    }
}
