using CitrusFramework;
public class SwitchMachineTypeEvent : CitrusGameEvent
{
    public MapMachineType MachineType;
    public SwitchMachineTypeEvent(MapMachineType machineType)
    {
        MachineType = machineType;
    }
}
