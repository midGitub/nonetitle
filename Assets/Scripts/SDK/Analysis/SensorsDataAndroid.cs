using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;
using CitrusFramework;

public class SensorsDataAndroid : ISensorsData
{
    private bool m_InitOk = false;
    private AndroidJavaClass UnityBridge;

    public void InitSensorsData(string serverurl, string configurl, bool isdebug)
    {
        using(AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using(AndroidJavaObject curActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                UnityBridge = new AndroidJavaClass("com.sensorsdata.Unity.sensorsdataUnityBridge");
                UnityBridge.CallStatic("initSensorsDataAPI", curActivity, serverurl, configurl, isdebug ? 1 : 0);
                m_InitOk = true;
            }
        }
    }

    public void SetUserId(string udid)
    {
        if(m_InitOk)
        {
            UnityBridge.CallStatic("SetUserId", udid);
        }
        else
        {
            InitNotOk();
        }
    }

    public void TrackEvent(string eventName, Dictionary<string, object> parm)
    {
        if(m_InitOk)
        {
            JsonObject json = new JsonObject(parm);
            UnityBridge.CallStatic("TrackEvent", eventName, json.ToString());
        }
        else
        {
            InitNotOk();
        }
    }

    public void TrackEvent(string eventName)
    {
        if(m_InitOk)
        {
            UnityBridge.CallStatic("TrackEvent", eventName);
        }
        else
        {
            InitNotOk();
        }
    }

    public void SetGameInfo(string Channel, string codename)
    {
        if(m_InitOk)
        {
            UnityBridge.CallStatic("SetGameInfo", Channel, codename);
        }
        else
        {
            InitNotOk();
        }
    }

    public void SetProfile(string name, string value)
    {
        if(m_InitOk)
        {
            UnityBridge.CallStatic("SetProfile", name, value);
        }
        else
        {
            InitNotOk();
        }
    }

    public void SetDicProfile(Dictionary<string, object> parm)
    {
        if(m_InitOk)
        {
            JsonObject json = new JsonObject(parm);
            UnityBridge.CallStatic("SetDicProfile", json.ToString());
        }
        else
        {
            InitNotOk();
        }
    }

    public void TrackInstallation()
    {
        if(m_InitOk)
        {
            UnityBridge.CallStatic("TrackInstallation");
        }
        else
        {
            InitNotOk();
        }
    }

    private void InitNotOk()
    {
        GameDebug.Log("sd android init not ok!");
    }
}

