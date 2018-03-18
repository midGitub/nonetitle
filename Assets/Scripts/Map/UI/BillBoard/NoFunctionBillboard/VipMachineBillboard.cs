
using CitrusFramework;
using UnityEngine.UI;

public class VipMachineBillboard : BillBoardBase
{
    public Button ResponseButton;

    void Start()
    {
        ResponseButton.onClick.AddListener(CallToEnterVipRoom);
    }

    void CallToEnterVipRoom()
    {
        CitrusEventManager.instance.Raise(new AskEnterMapRoomEvent(MapMachineRoom.VIP, null));
    }
}
