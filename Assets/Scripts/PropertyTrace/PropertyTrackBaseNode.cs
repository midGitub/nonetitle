using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropertyNodeType
{
    Pay = 0,
    PayWin = 1,
    UnKnow = 2,
    PayAndWin = 3,
    Free = 4,
    FreeWin = 5,
    Misc = 6,
}

public class PropertyTrackBaseNode
{
    public uint Id;
    public PropertyNodeType NodeType;
    public ulong OriginalAmount;
    public ulong RemainAmount;
    public DateTime BornTime;
    public DateTime LastDecreaseTime;
}
