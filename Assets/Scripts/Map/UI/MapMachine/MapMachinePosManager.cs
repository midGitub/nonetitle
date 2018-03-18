using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.Events;

public struct MachinePosInfo
{
    private MapMachineRoom _roomType;
    private readonly float _pos;
    private string _machineName;
    public MachinePosInfo(MapMachineRoom room, string machineName, float pos)
    {
        _roomType = room;
        _machineName = machineName;
        _pos = pos;
    }

    public MapMachineRoom RoomType
    {
        get { return _roomType; }
    }

    public float Pos
    {
        get { return _pos;}
    }

    public string MachineName
    {
        get { return _machineName; }
    }
}

public class MapMachinePosManager : Singleton<MapMachinePosManager>
{
    public Dictionary<string, MachinePosInfo> MachinePosDic = new Dictionary<string, MachinePosInfo>();
    public float TinyMachineGroupWidth;
    public readonly float BillBoardWidth = 500;

    private float _curtainPos;
    private Transform _vipContentTrans;
    private Transform _customContentTrans;
    private Transform _tinyMapMachineParent;
    private Transform _tinyMapMachineRoot;
    public MapCurtainsUiController MapCurtainsUiCtrl;

    public void Init()
    {
    }

    public void CleanMachinePosDic()
    {
        MachinePosDic.Clear();
    }

    public void RegisterPosInfo(Transform vipMachineParent, Transform normalMachineRoot, Transform curtain, Transform tinyMachineRoot, Transform tinyMachineParent)
    {
        _vipContentTrans = vipMachineParent;
        _customContentTrans = normalMachineRoot;
        _tinyMapMachineRoot = tinyMachineRoot;
        _tinyMapMachineParent = tinyMachineParent;

        Canvas.ForceUpdateCanvases();
        MachinePosDic.Clear();
        _curtainPos = curtain != null
            ? curtain.GetComponent<RectTransform>().anchoredPosition3D.x
            : normalMachineRoot.GetComponent<RectTransform>().sizeDelta.x;
        TinyMachineGroupWidth = tinyMachineRoot != null ? tinyMachineRoot.GetComponent<RectTransform>().sizeDelta.x : 0;
        UpdateMachinePos(MapMachineRoom.CUSTOM);
        UpdateMachinePos(MapMachineRoom.VIP);
    }

    public void UpdateMachinePos(MapMachineRoom roomType)
    {
        switch (roomType)
        {
            case MapMachineRoom.CUSTOM:
                UpdateMachinePos(_customContentTrans, MapMachineRoom.CUSTOM, 0);
                float tinyMachineStartPos = _tinyMapMachineRoot != null
                    ? _tinyMapMachineRoot.GetComponent<RectTransform>().anchoredPosition3D.x
                    : 0;
                UpdateMachinePos(_tinyMapMachineParent, MapMachineRoom.CUSTOM, tinyMachineStartPos);
                break;
            case MapMachineRoom.VIP:
                UpdateMachinePos(_vipContentTrans, MapMachineRoom.VIP, 0);
                break;
        }
    }

    void UpdateMachinePos(Transform parent, MapMachineRoom roomType, float startPos)
    {
        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith("MapMachine_"))
                {
                    RectTransform rect = child.GetComponent<RectTransform>();
                    string key = child.name.Replace("MapMachine_", "");
                    key = key.Replace("(Clone)", "");
                    float value = rect.anchoredPosition3D.x + startPos;
                    MachinePosDic[key] = new MachinePosInfo(roomType, key, value);
                }
            }
        }
    }

    public void RecordMachinePos(MapMachineRoom room, string machineName)
    {
        if (room == MapMachineRoom.CUSTOM)
        {
            bool isTinyMachine = MachineUnlockSettingConfig.Instance.IsTinyMachine(machineName);
            UserDeviceLocalData.Instance.IsInTinyMachineRoom = isTinyMachine;
        }

        float result = CalculatePreferMachinePos(room, machineName);
        UserDeviceLocalData.Instance.SetMapRoomLastStayPos(room, -result);
    }

    //the pos this function calculate is used for scroll content to use to locate at machine
    public float CalculatePreferMachinePos(MapMachineRoom room, string machineName)
    {
        float startPos;
        float endPos;
        float curPos = GetMachinePos(machineName);

        if (room == MapMachineRoom.CUSTOM)
        {
            bool isTinyMachine = MachineUnlockSettingConfig.Instance.IsTinyMachine(machineName);
            startPos = isTinyMachine ? _curtainPos : 0;
            endPos = isTinyMachine ? _curtainPos + TinyMachineGroupWidth : _curtainPos;
        }
        else
        {
            startPos = 0;
            endPos = _vipContentTrans != null ? _vipContentTrans.GetComponent<RectTransform>().sizeDelta.x : 0;
        }

        int maxScreenIndex = (int)((endPos - startPos)/DeviceUtility.DesignWidth);
        int selectScreenIndex = (int)((curPos - startPos)/DeviceUtility.DesignWidth);
        bool hideByBillboard = (curPos - startPos)%DeviceUtility.DesignWidth < BillBoardWidth;
        float posWithinMaxIndex = startPos + selectScreenIndex*DeviceUtility.DesignWidth;
        float posOperateWithBillboard = hideByBillboard 
            ? posWithinMaxIndex - BillBoardWidth 
            : posWithinMaxIndex;

        float result = selectScreenIndex >= maxScreenIndex
            ? endPos - DeviceUtility.DesignWidth
            : posOperateWithBillboard;

        return result;
    }

    private float GetMachinePos(string machineName)
    {
        float result = 0;
        if (MachinePosDic.ContainsKey(machineName))
        {
            result = MachinePosDic[machineName].Pos;
        }

        return result;
    }

    public void RequstSwitchTinyAndNormalRoom(UnityAction callback)
    {
        if (MapCurtainsUiCtrl != null)
        {
            bool isInTinyMachineRoom = MapCurtainsUiCtrl.AlreadySwitched;
            MapCurtainsAnim anim = isInTinyMachineRoom
                ? MapCurtainsAnim.MoveInFromLeft
                : MapCurtainsAnim.MoveInFromRight;
            MapCurtainsUiCtrl.PlayMoreSlotsAnim(anim, false, callback);
        }
        else
        {
            if (callback != null)
            callback.Invoke();
        }
    }
}
