using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using System;

public class SensorsData 
{
    private static ISensorsData m_Instance;
	//bind to 120.92.16.53 in GoDaddy.com
	private const string SERVER_URL = "http://sensor.citrusjoy.com:8006/sa?project=trojan&token=******";
	private const string CONFIG_URL = "http://sensor.citrusjoy.com:8006/config/?project=trojan";

    public static void SetUserId(string udid)
    {
        if(m_Instance != null)
        {
            m_Instance.SetUserId(udid);
        }
        else
        {
            NullInstance();   
        }
    }

    public static void TrackEvent(string name, Dictionary<string, object> parm)
    {
        if(m_Instance != null)
        {
            m_Instance.TrackEvent(name, parm);
        }
        else
        {
            NullInstance();   
        }
    }

    public static void TrackEvent(string name)
    {
        if(m_Instance != null)
        {
            m_Instance.TrackEvent(name);
        }
        else
        {
            NullInstance();   
        }
    }

    public static void Purchase(string EventName, double price)
    {
        Dictionary<string, object> newdic = new Dictionary<string, object>();
        newdic.Add(EventName, price);
        TrackEvent("Purchase", newdic);
    }

    public static void Reward(int count, string reason)
    {
        Dictionary<string, object> newdic = new Dictionary<string, object>();
        newdic.Add("reason", reason);
        newdic.Add("count", count);
        TrackEvent("Reward", newdic);
    }

    private static void NullInstance()
    {
        //GameDebug.Log("SensorData instance is null!");
    }

    public static void SetProfile(string name, string value)
    {
        if(m_Instance != null)
        {
            m_Instance.SetProfile(name, value);
        }
        else
        {
            NullInstance();   
        }
    }

    public static void SetDicProfile(Dictionary<string, object> parm)
    {
        if(m_Instance != null)
        {
            m_Instance.SetDicProfile(parm);
        }
        else
        {
            NullInstance();   
        }
    }

    public static void TrackInstallation()
    {
        if(m_Instance != null)
        {
            m_Instance.TrackInstallation();
        }
        else
        {
            NullInstance();   
        }
    }

    public static void Init(string channel, string codename, bool isdebug, string version)
    {
        #if UNITY_EDITOR

        #elif UNITY_ANDROID
        m_Instance = new SensorsDataAndroid();
        #elif UNITY_IOS
        m_Instance = new SensorsDataIOS();
        #endif

        if(m_Instance != null)
        {
            m_Instance.InitSensorsData(SERVER_URL, CONFIG_URL, isdebug);
            m_Instance.SetGameInfo(channel, codename);
            m_Instance.SetProfile("version", version);
        }
        else
        {
            NullInstance();   
        }
    }
}