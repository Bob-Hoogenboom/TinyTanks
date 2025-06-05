using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TimedWait : CustomYieldInstruction
{
    private float _totalWaitTime;
    private float _startTime;

    public float ElapsedTime => Time.time - _startTime;
    public float RemainingTime => Mathf.Max(0, _totalWaitTime - ElapsedTime);
    public float Progress => Mathf.Clamp01(ElapsedTime / _totalWaitTime);

    public override bool keepWaiting => RemainingTime > 0;

    public TimedWait(float waitTime)
    {
        _totalWaitTime = waitTime;
        _startTime = Time.time;
    }
}
