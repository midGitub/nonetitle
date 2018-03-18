using CitrusFramework;

public class OutOfCreditsEvent : CitrusGameEvent
{
    public ulong UserCreditsNumWhenBackrupt;
    public OutOfCreditsEvent(ulong creditsNum)
    {
        UserCreditsNumWhenBackrupt = creditsNum;
    }
}
