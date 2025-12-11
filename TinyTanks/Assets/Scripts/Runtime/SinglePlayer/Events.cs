using System;
using UnityEngine;

namespace SinglePlayer
{
    public static class Events
    {
        //RailShooterEvents
        public static event Action OnMatchRestart;
        public static event Action OnTrackFinished;

        // Methods to invoke the events
        public static void MatchRestart()
        {
            OnMatchRestart?.Invoke();
        }

        public static void TrackFinished()
        {
            OnTrackFinished?.Invoke();
        }
    }
}
