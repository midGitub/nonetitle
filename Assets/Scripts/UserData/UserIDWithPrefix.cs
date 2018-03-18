using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserIDWithPrefix
{
	public static string DeviceIdPrefix = "DeviceID";
	public static string FBIdPrefix = "FB";

	public static string GetDeviceIDWithPrefix()
	{
		return DeviceIdPrefix + "_" + DeviceUtility.GetDeviceId();
	}

	public static string GetFBIDWithPrefix()
	{
		if(FacebookHelper.IsLoggedIn)
			return GetFBIDWithPrefix(FacebookHelper.SocialId);
		return "";
	}

	public static string GetFBIDWithPrefix(string str)
	{
		return FBIdPrefix + "_" + str;
	}

	public static string GetFBIDNoPrefix()
	{
		var currSID = UserDeviceLocalData.Instance.GetCurrSocialAppID;
		if(currSID == "" || currSID == UserIDWithPrefix.GetDeviceIDWithPrefix())
		{
			return "";
		}

		var sa = currSID.Split('_');
		return sa[sa.Length - 1];
	}

	public static string GetFBIDNoPrefix(string oldID)
	{
		var currSID = oldID;
		if(currSID == "" || !currSID.Contains("FB_"))
		{
			return "";
		}

		var sa = currSID.Split('_');
		return sa[sa.Length - 1];
	}
}
