using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameTimer : MonoBehaviour
{
    private static Stopwatch stopwatch;
    
    //Keeps track of milliseconds since start
    private static Stopwatch _sinceStart;
    
    
    /// <summary>
    /// Time since framestart
    /// </summary>
    public static double FrameDuration
    {
        get
        {
            if (stopwatch == null)
                return 0;
            else
                return stopwatch.Elapsed.TotalMilliseconds;
        }
    }

    //Time since gamestart
    public static double sinceStart 
    {
        get 
        {
            if(_sinceStart == null) 
            {
                return 0d;
            }
            else 
            {
                return _sinceStart.Elapsed.TotalMilliseconds;
            }
        }
    }


    void Awake()
    {
        stopwatch = new Stopwatch();
        _sinceStart = new Stopwatch();
        _sinceStart.Start();
    }
    void Update()
    {
        // For whatever reason, .Restart() wasn't recognized.
        stopwatch.Reset();
        stopwatch.Start();
    }

    private void OnDestroy()
    {
        _sinceStart.Stop();
    }
}