using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.UI;

public class EnterRoom : MonoBehaviour
{
    public MapScene MapSceneScript;
    public Button EnterCustomRoom;
    public Button EnterVipRoom;
    private Dictionary<MapMachineRoom, GameObject> _roomButtons;
    void Start()
    {
        Init();
        EnterCustomRoom.onClick.AddListener(() => CitrusEventManager.instance.Raise(new AskEnterMapRoomEvent(MapMachineRoom.CUSTOM, null)));
        EnterVipRoom.onClick.AddListener(() => CitrusEventManager.instance.Raise(new AskEnterMapRoomEvent(MapMachineRoom.VIP, null)));
    }

    void OnDestroy()
    {
        EnterCustomRoom.onClick.RemoveListener(() => CitrusEventManager.instance.Raise(new AskEnterMapRoomEvent(MapMachineRoom.CUSTOM, null)));
        EnterVipRoom.onClick.RemoveListener(() => CitrusEventManager.instance.Raise(new AskEnterMapRoomEvent(MapMachineRoom.VIP, null)));
    }

    void Init()
    {
        _roomButtons = new Dictionary<MapMachineRoom, GameObject>
        {
            {MapMachineRoom.CUSTOM, EnterVipRoom.gameObject},
            {MapMachineRoom.VIP, EnterCustomRoom.gameObject},
        };
    }

    public void ShowRoomButton(MapMachineRoom roomType)
    {
        foreach (var kv in _roomButtons)
        {
            if (kv.Key != roomType)
            {
                kv.Value.SetActive(false);
            }
            else
            {
                kv.Value.SetActive(true);
            }
        }
    }
}
