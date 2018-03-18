using CitrusFramework;
using CodeStage.AntiCheat.ObscuredTypes;

public class SetHourBonusMultiplierEvent : CitrusGameEvent
{
    public bool IsReset;
    public float Multiplier;
    public SetHourBonusMultiplierEvent(float multiplier, bool isReset = false)
    {
        Multiplier = multiplier;
        IsReset = isReset;
    }
}
