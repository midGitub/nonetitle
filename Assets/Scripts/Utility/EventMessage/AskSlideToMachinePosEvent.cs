using CitrusFramework;
using UnityEngine.Events;

public class AskSlideToMachinePosEvent : CitrusGameEvent
{
    public string MachineName;
    public UnityAction Callback;
    public bool AbandonIfNeedChangeMapRoom;
    public bool CancleSlideWhenEnterRoom;

    public AskSlideToMachinePosEvent(string machineName, UnityAction callback, bool abandonIfNeedChangeMapRoom = false, bool cancleSlideWhenEnterRoom = false)
    {
        MachineName = machineName;
        Callback = callback;
        AbandonIfNeedChangeMapRoom = abandonIfNeedChangeMapRoom;
        CancleSlideWhenEnterRoom = cancleSlideWhenEnterRoom;
    }
}
