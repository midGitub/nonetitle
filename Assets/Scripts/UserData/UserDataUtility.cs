using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UserSocialState
{
	Device,
	Facebook
}

public static class UserDataUtility
{
	public static string GetSocialIdWithPrefix(UserSocialState state)
	{
		string result = "";
		switch(state)
		{
			case UserSocialState.Device:
				result = UserIDWithPrefix.GetDeviceIDWithPrefix();
				break;
			case UserSocialState.Facebook:
				result = UserIDWithPrefix.GetFBIDWithPrefix();
				break;
			default:
				Debug.Assert(false);
				break;
		}
		return result;
	}
}
