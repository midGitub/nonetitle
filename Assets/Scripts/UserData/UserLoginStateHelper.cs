using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;

// Pending:
// it's a middle and unstable state, we should handle the case if it's Pending state when launching the game
public enum UserLoginWorkflowState
{
	OK,
	Pending
}

public class UserLoginStateHelper : Singleton<UserLoginStateHelper>
{
	public bool IsDeviceLoginState
	{
		get {
			return GetCurrentSocialLoginState() == UserSocialState.Device;
		}
	}

	public bool IsFacebookLoginState
	{
		get {
			return GetCurrentSocialLoginState() == UserSocialState.Facebook;
		}
	}

	public bool IsLoginWorkflowStatePending
	{
		get {
			return GetLoginWorkflowState() == UserLoginWorkflowState.Pending;
		}
	}

	public bool IsLoginWorkflowStateOK
	{
		get {
			return GetLoginWorkflowState() == UserLoginWorkflowState.OK;
		}
	}

	public UserSocialState GetCurrentSocialLoginState()
	{
		UserSocialState result = UserSocialState.Device;

		string socialId = UserDeviceLocalData.Instance.GetCurrSocialAppID;
		if(string.IsNullOrEmpty(socialId) || socialId.Contains(UserIDWithPrefix.DeviceIdPrefix))
			result = UserSocialState.Device;
		else if(socialId.Contains(UserIDWithPrefix.FBIdPrefix))
			result = UserSocialState.Facebook;
		else
			Debug.Assert(false);

		return result;
	}

	public void SetLoginWorkflowState(UserLoginWorkflowState s)
	{
		UserDeviceLocalData.Instance.LoginWorkflowState = s;
	}

	public UserLoginWorkflowState GetLoginWorkflowState()
	{
		return UserDeviceLocalData.Instance.LoginWorkflowState;
	}
}
