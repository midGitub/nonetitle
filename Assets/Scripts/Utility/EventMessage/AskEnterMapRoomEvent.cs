
using CitrusFramework;
using UnityEngine.Events;

public class AskEnterMapRoomEvent : CitrusGameEvent
{
    public MapMachineRoom Room;
    public UnityAction Callback;
    public string LocateAtMachine;
    public AskEnterMapRoomEvent(MapMachineRoom roomType, UnityAction callback, string locateAtMachine = null)
    {
        Room = roomType;
        Callback = callback;
        LocateAtMachine = locateAtMachine;
    }
}
