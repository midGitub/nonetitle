using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyTrackFreeNode : PropertyTrackBaseNode {
    public PropertyTrackFreeNode(uint id, FreeCreditsSource creditsSource, ulong origAmount)
    {
        Id = id;
        NodeType = PropertyNodeType.Free;
        CreditsSource = creditsSource;
        OriginalAmount = origAmount;
        RemainAmount = origAmount;
        BornTime = NetworkTimeHelper.Instance.GetNowTime();
        LastDecreaseTime = NetworkTimeHelper.Instance.GetNowTime();
    }

    public PropertyTrackBaseNode LinkNode;
    public FreeCreditsSource CreditsSource;
    public int EnQueueTotalSpinCount;
    public int DeQueueTotalSpinCount;
}
