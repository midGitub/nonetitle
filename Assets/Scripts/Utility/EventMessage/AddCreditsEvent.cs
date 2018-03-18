using CitrusFramework;

public class AddCreditsEvent : CitrusGameEvent
{
    public FreeCreditsSource Source;
    public ulong Amount;

    public AddCreditsEvent(FreeCreditsSource source, ulong amount)
    {
        Source = source;
        Amount = amount;
    }
}
