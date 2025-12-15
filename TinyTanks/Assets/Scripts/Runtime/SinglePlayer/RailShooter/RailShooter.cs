using Dreamteck.Splines;
using UnityEngine;

/// <summary>
/// A script that needs to be attached to the spline computer Gameobject
/// It holds all info for an OnRailShooter section like score, enemies and other events
/// UI is handled in a different section because of delegate events for all singleplayer instances
/// </summary>
public class RailShooter : MonoBehaviour
{
    [SerializeField] private SplineFollower follower;
    private float _initialSpeed;


    private void OnEnable() => SinglePlayer.Events.OnMatchRestart += RestartShooter;
    private void OnDisable() => SinglePlayer.Events.OnMatchRestart -= RestartShooter;


    private void Start()
    {
        follower.onEndReached += EndingShooter;
        _initialSpeed = follower.followSpeed;

        follower.followSpeed = 0f;
    }

    private void Update()
    {
        //For Debugging*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            follower.followSpeed = _initialSpeed;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            follower.followSpeed = 0f;
        }
    }



    private void EndingShooter(double percent)
    {
        SinglePlayer.Events.TrackFinished();
    }

    private void RestartShooter()
    {
        follower.SetPercent(0);
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