using CitrusFramework;
using UnityEngine;

public class EnterMapRoomEvent : CitrusGameEvent
{
    public MapMachineRoom Room;

    public EnterMapRoomEvent(MapMachineRoom room)
    {
        Room = room;
    }
}
