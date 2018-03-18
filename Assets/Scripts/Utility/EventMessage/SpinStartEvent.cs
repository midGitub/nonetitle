using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class SpinStartEvent : CitrusGameEvent
{
    public string MachineName;
    public CoreSpinResult SpinResult;
    public SpinMode SpinMode;
    public SmallGameState SmallGameState;

    public SpinStartEvent(string machineName, CoreSpinResult spinResult, SpinMode spinMode, SmallGameState smallGameState)
    {
        MachineName = machineName;
        SpinResult = spinResult;
        SpinMode = spinMode;
        SmallGameState = smallGameState;
    }
}
