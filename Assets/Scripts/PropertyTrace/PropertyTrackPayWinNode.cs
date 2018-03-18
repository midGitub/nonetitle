using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyTrackPayWinNode : PropertyTrackBaseNode
{
    public PropertyTrackPayWinNode(uint id, ulong initAmount, DateTime linkNodeBornTime)
    {
        Id = id;
        OriginalAmount = initAmount;
        RemainAmount = initAmount;
        NodeType = PropertyNodeType.PayWin;
        BornTime = NetworkTimeHelper.Instance.GetNowTime();
        LastDecreaseTime = NetworkTimeHelper.Instance.GetNowTime();
        LinkNodeBornTime = linkNodeBornTime;
    }

    public DateTime LinkNodeBornTime;
    public int LinkNodeBornTotalSpinCount;
    public int EnQueueTotalSpinCount;
    public int DeQueueTotalSpinCount;
}
