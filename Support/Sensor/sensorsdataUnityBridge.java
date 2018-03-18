package com.sensorsdata.Unity;


import org.json.JSONException;
import org.json.JSONObject;

import android.app.Activity;

import com.sensorsdata.analytics.android.sdk.SensorsDataAPI;
import com.sensorsdata.analytics.android.sdk.exceptions.InvalidDataException;

/**
 * Created by 张昊阳 on 2016/12/23
 */

public class sensorsdataUnityBridge 
{
	private static Activity m_UnityActive;
	
    public static void initSensorsDataAPI(Activity Context , String serverurl, String configurl, int isDebug) 
    {
    	SensorsDataAPI.DebugMode debugmode = SensorsDataAPI.DebugMode.DEBUG_OFF;
		if(isDebug == 1)
		{
			debugmode = SensorsDataAPI.DebugMode.DEBUG_AND_TRACK;
		}
		m_UnityActive = Context;
		SensorsDataAPI.sharedInstance(Context,serverurl,configurl,debugmode);
		SensorsDataAPI.sharedInstance(m_UnityActive).enableAutoTrack();
    }
	    
    public static void TrackEvent(String EventName, String EventJson)
    {
	   try 
	   {
	       JSONObject Json = new JSONObject(EventJson);
	       SensorsDataAPI.sharedInstance(m_UnityActive).track(EventName, Json);
	   }
	   catch (InvalidDataException e) 
	   {
	       e.printStackTrace();
	   }
	   catch (JSONException e) 
	   {
		   e.printStackTrace();
	   }
    }
    
    public static void TrackEvent(String EventName)
    {
	   try 
	   {
	       JSONObject Json = new JSONObject();
	       SensorsDataAPI.sharedInstance(m_UnityActive).track(EventName, Json);
	   }
	   catch (InvalidDataException e) 
	   {
	       e.printStackTrace();
	   }
    }
    
    public static void SetUserId(String userid) 
    {
    	try
    	{
    		SensorsDataAPI.sharedInstance(m_UnityActive).login(userid);
    	}
    	catch(InvalidDataException e)
    	{
    		e.printStackTrace();
    	}
	}
    
    public static void SetGameInfo(String channel, String codename)
    {
	    try 
	    {
	    	JSONObject properties = new JSONObject();
	    	properties.put("Channel", channel);
	    	properties.put("codename", codename);
	    	SensorsDataAPI.sharedInstance(m_UnityActive).profileSetOnce(properties);	
	    }
	    catch (InvalidDataException e) 
	    {
	    	e.printStackTrace();
	    }
	    catch (JSONException e) 
	    {
	    	e.printStackTrace();
	    }
    }
    
    public static void SetProfile(String name, String value) 
    {
    	try 
 	    {
 	    	JSONObject properties = new JSONObject();
 	    	properties.put(name, value);
 	    	SensorsDataAPI.sharedInstance(m_UnityActive).profileSet(properties);	
 	    }
 	    catch (InvalidDataException e) 
 	    {
 	    	e.printStackTrace();
 	    }
 	    catch (JSONException e) 
 	    {
 	    	e.printStackTrace();
 	    }
	}
    
    public static void Logout()
    {
    	SensorsDataAPI.sharedInstance(m_UnityActive).logout();
    }
    
    public static void TrackInstallation()
    {
	   try 
	   {
	      JSONObject properties = new JSONObject();

	      // 追踪渠道效果
	      SensorsDataAPI.sharedInstance(m_UnityActive).trackInstallation("AppInstall", properties);
	    } catch (Exception e) {
	      e.printStackTrace();
	    }
    }
}
