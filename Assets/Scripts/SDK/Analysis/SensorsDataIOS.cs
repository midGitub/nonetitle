using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using CitrusFramework;
using System.Collections.Generic;
using SimpleJson;

//#if UNITY_IOS
public class SensorsDataIOS : ISensorsData 
{
    private bool m_InitOk = false;

    public void InitSensorsData(string serverurl, string configurl, bool isdebug)
    {
        _InitSensorsData(serverurl, configurl, isdebug);
        m_InitOk = true;
    }

    public void SetUserId(string udid)
    {
        if(m_InitOk)
        {
            _SetUserId(udid);
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
            _TrackEvent(eventName, json.ToString());
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
            Dictionary<string, object> nullparm = new Dictionary<string, object>();
            nullparm.Add("a", 1);//xcode always need nsdic
//                _TrackEvent(eventName);
            TrackEvent(eventName, nullparm);
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
            _SetGameInfo(Channel, codename);
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
            _SetProfile(name, value);
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
            _SetDicProfile(json.ToString());
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
            _TrackInstallation();
        }
        else
        {
            InitNotOk();
        }
    }

    private void InitNotOk()
    {
        GameDebug.Log("sd ios init not ok!");
    }


    [DllImport("__Internal")] 
    private static extern void _InitSensorsData(string serverurl, string configurl, bool isdebug); 
    [DllImport("__Internal")] 
    private static extern void _TrackEvent(string eventname, string jsonstring); 
//  [DllImport("__Internal")] 
//  private static extern bool _TrackEvent(string eventname);
    [DllImport("__Internal")] 
    private static extern bool _SetUserId(string userid);
    [DllImport("__Internal")] 
    private static extern bool _SetGameInfo(string channel, string codename);
    [DllImport("__Internal")] 
    private static extern bool _SetProfile(string name, string value);
    [DllImport("__Internal")] 
    private static extern bool _SetDicProfile(string jsonstring);
    [DllImport("__Internal")] 
    private static extern bool _TrackInstallation();
}
//    #endif