using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class AskUnlockMachineEvent : CitrusGameEvent
{
    public List<string> UnlockNameList;

    public AskUnlockMachineEvent(List<string> unlockNameList)
    {
        UnlockNameList = unlockNameList;
    }
}
