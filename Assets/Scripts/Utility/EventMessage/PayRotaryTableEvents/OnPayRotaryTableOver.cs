using CitrusFramework;

public class OnPayRotaryTableOver : CitrusGameEvent {

    public PayRotaryTableData Result;

    public OnPayRotaryTableOver(PayRotaryTableData result)
    {
        Result = result;
    }
}
