using System;
public class PropertyTrackFreeWinNode : PropertyTrackBaseNode
{
    public PropertyTrackFreeWinNode(uint id, FreeCreditsSource creditsSource, ulong initAmount, DateTime linkNodeBornTime)
    {
        Id = id;
        OriginalAmount = initAmount;
        RemainAmount = initAmount;
        NodeType = PropertyNodeType.FreeWin;
        CreditsSource = creditsSource;
        BornTime = NetworkTimeHelper.Instance.GetNowTime();
        LastDecreaseTime = NetworkTimeHelper.Instance.GetNowTime();
        LinkNodeBornTime = linkNodeBornTime;
    }

    public DateTime LinkNodeBornTime;
    public FreeCreditsSource CreditsSource;
    public int LinkNodeBornTotalSpinCount;
    public int EnQueueTotalSpinCount;
    public int DeQueueTotalSpinCount;
}
