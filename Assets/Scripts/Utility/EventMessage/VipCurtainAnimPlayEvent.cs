using System;
using CitrusFramework;

public class VipCurtainAnimPlayEvent : CitrusGameEvent
{
    public Callback OnCurtainCoverScreen;
    public VipCurtainAnimPlayEvent(Callback callback)
    {
        OnCurtainCoverScreen = callback;
    }
}
