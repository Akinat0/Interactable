
using System;
using UnityEngine;

public static class TimeManager
{
    public static Action Paused;
    public static Action Unpaused;
    
    public static bool IsPaused { get; private set; }
    
    public static void Pause()
    {
        Time.timeScale = 0;
        IsPaused = true;
        Paused?.Invoke();
    }

    public static void Unpause()
    {
        Time.timeScale = 1;
        IsPaused = false;
        Unpaused?.Invoke();
    }
}
