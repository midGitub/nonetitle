using CitrusFramework;

public class OnStorePurchaseSucceed : CitrusGameEvent
{
    public IAPData Data;

    public OnStorePurchaseSucceed(IAPData data)
    {
        Data = data;
    }
}
