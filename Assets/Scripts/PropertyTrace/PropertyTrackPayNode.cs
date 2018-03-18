using System;
using System.Collections.Generic;

public class PropertyTrackPayNode : PropertyTrackBaseNode
{
    public PropertyTrackPayNode(uint id, string paidOrderId, ulong origAmount)
    {
        Id = id;
        NodeType = PropertyNodeType.Pay;
        PaidOrderId = paidOrderId;
        OriginalAmount = origAmount;
        RemainAmount = origAmount;
        BornTime = NetworkTimeHelper.Instance.GetNowTime();
        LastDecreaseTime = NetworkTimeHelper.Instance.GetNowTime();
    }

    public PropertyTrackBaseNode LinkNode;
    public string PaidOrderId;
    public int EnQueueTotalSpinCount;
    public int DeQueueTotalSpinCount;
}
