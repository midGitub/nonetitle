using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class DeviceUtility
{
	public static float DesignWidth = 1920;//开发时分辨率宽
	public static float DesignHeight = 1080;//开发时分辨率高
	public static float IphoneXWidth = 2436.0f;
	public static float IPhoneXHeight = 1125.0f;
	public static float DesignRatio = DesignWidth / DesignHeight;
	public static float IphoneXAspect = IphoneXWidth / IPhoneXHeight;
	private static float ScreenAspect = DesignRatio;

	private static string _deviceID = "";

	static DeviceUtility(){
		ScreenAspect = GetScreenWidthHeightRatio();
	}

	public static float GetDesignWidthHeightRatio()
	{
		return DesignRatio;
	}

	public static float GetScreenWidthHeightRatio()
	{
		return (float)Screen.width / (float)Screen.height;
	}

	public static bool IsIPadResolution()
	{
		return ScreenAspect <= (DesignRatio - 0.3f);
	}

	public static bool IsIphoneXResolution(){
		return ScreenAspect >= IphoneXAspect;
	}

	public static bool IsConnectInternet(){
		return !( Application.internetReachability == NetworkReachability.NotReachable );
	}

	public static bool IsConnectLocalNet(){
		return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
	}

	#if UNITY_IOS
	[DllImport ("__Internal")] 
	private static extern string SUDID();    
	#endif

	public static string GetDeviceId()
	{
        #if UNITY_EDITOR
        return SystemInfo.deviceUniqueIdentifier; 
		#elif UNITY_IOS
		if (_deviceID == "")
			_deviceID = SUDID();
		return _deviceID;
		#else
		return SystemInfo.deviceUniqueIdentifier;
		#endif
	}
}
