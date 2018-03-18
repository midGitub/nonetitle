using CitrusFramework;

public class RegisterWinAmountEvent : CitrusGameEvent
{

    public ulong WinAmount;
    public RegisterWinAmountEvent(ulong winAmount)
    {
        WinAmount = winAmount;
    }
}
