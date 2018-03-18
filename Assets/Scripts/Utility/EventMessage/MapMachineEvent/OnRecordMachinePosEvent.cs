using CitrusFramework;

public class OnRecordMachinePosEvent : CitrusGameEvent
{
    public MapMachineRoom CurStayRoom;

    public OnRecordMachinePosEvent(MapMachineRoom curStayRoom)
    {
        CurStayRoom = curStayRoom;
    }
}
