using Dreamteck.Splines;
using UnityEngine;

/// <summary>
/// A script that needs to be attached to the spline computer Gameobject
/// It holds all info for an OnRailShooter section
/// </summary>
public class RailShooter : MonoBehaviour
{
    [SerializeField] private SplineFollower follower;

    private float _initialSpeed;


    // Start is called before the first frame update
    private void Start()
    {
        follower.onEndReached += RestartWindow;
        _initialSpeed = follower.followSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RestartWindow(double percent)
    {
        //restartLogic here*
    }


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //Trigger Group Methods
    public void TimedStopFollower(float forSeconds)
    {
        //#TODO Make a Time Prompt
        //#TODO Stop follower for X* amount of time
        
    }

    public void InputStopFollower()
    {
        //#TODO Make an Input Prompt
        //#TODO Stop follower until INPUT*
        
    }

    public void SlowFollower()
    {
        //#TODO let the player known they slow down
        follower.followSpeed = _initialSpeed/ 2;
    }

    public void NormalFollower()
    {
        //#TODO let the player known they return to normal
        follower.followSpeed = _initialSpeed;
    }

    public void FastFollower()
    {
        //#TODO let the player known they speed up
        follower.followSpeed = _initialSpeed * 2;
    }
}