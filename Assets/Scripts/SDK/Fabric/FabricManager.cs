using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using Fabric.Crashlytics;

public class FabricManager : Singleton<FabricManager>
{
	public void Init()
	{
		SetDeviceId();
		RefreshUDID();
	}

	void SetDeviceId()
	{
		string deviceId = DeviceUtility.GetDeviceId();
		Crashlytics.SetUserIdentifier(deviceId);
		Crashlytics.SetKeyValue("Device Id", deviceId); //key-value is convenient to check on Fabric webpage
	}

	void RefreshUDID()
	{
		string udid = UserBasicData.Instance.UDID;
		if(!string.IsNullOrEmpty(udid))
			SetUDID(udid);
	}

	public void SetUDID(string udid)
	{
		Crashlytics.SetKeyValue("UDID", udid);
	}

    public void SendCrashAndNonFatalMsg()
    {
        Crashlytics.Crash();
        Crashlytics.ThrowNonFatal();
    }
}
