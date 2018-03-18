using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class StartLoading : MonoBehaviour
{
#if UNITY_EDITOR
    public static bool StartLoadingSceneHasLoaded;
#endif
    static readonly float _loadingTime = 3;
	static readonly float _rotateInterval = 0.1f;
	static readonly float _handleUserDataLossWaitTime = 60.0f;
		
	public GameObject _RotateGameObject;
	public float RotateSpeed;
	private bool _isTouristsLoading = false;

	private Coroutine _handleUserDataLossTimeoutCoroutine;

	void OnEnable()
	{
		CitrusEventManager.instance.AddListener<UserDataServerEvent>(UserDataServerMessageProcess);
		CitrusEventManager.instance.AddListener<UserChouseUserDataEvent>(UserChooseUserDataCallback);

		LiveUpdateManager.Instance.LiveUpdateSuccessEvent += LiveUpdateSuccessCallback;
		LiveUpdateManager.Instance.LiveUpdateFailEvent += LiveUpdateFailCallback;
	}

	void OnDisable()
	{
		CitrusEventManager.instance.RemoveListener<UserDataServerEvent>(UserDataServerMessageProcess);
		CitrusEventManager.instance.RemoveListener<UserChouseUserDataEvent>(UserChooseUserDataCallback);

		CitrusEventManager.instance.RemoveListener<UserLoginFBSuccessEvent>(LoginFBSuccessCallback);
		CitrusEventManager.instance.RemoveListener<UserLoginFBFailEvent>(LoginFBFailCallback);

		LiveUpdateManager.Instance.LiveUpdateSuccessEvent -= LiveUpdateSuccessCallback;
		LiveUpdateManager.Instance.LiveUpdateFailEvent -= LiveUpdateFailCallback;
	}

	void Start()
	{
		Debug.Log("StartLoading start");

		//Note: Live update should be ahead of all other operations
		//When live update is completed, continue other initializations
		LiveUpdateManager.Instance.Init();
		LiveUpdateManager.Instance.StartLiveUpdate();
	}

	void LiveUpdateSuccessCallback()
	{
		Debug.Log("StartLoading: live update success");

		StartCoroutine(StartLoadingCoroutine());
	}

	void LiveUpdateFailCallback()
	{
		Debug.Log("StartLoading: live update fail");

		StartCoroutine(StartLoadingCoroutine());
	}

	IEnumerator StartLoadingCoroutine()
	{
		yield return StartCoroutine(GameManager.Instance.InitCoroutine());

		Debug.Log("StartLoadingCoroutine continue");

		TrackFirstEnterGame();

		UserDeviceLocalData.Instance.ResetDailyFriendGiftNum();
		UserDeviceLocalData.Instance.RefreshIsFirstLoginToday(); //call this before reset LastLoginTime
		UserDeviceLocalData.Instance.LastLoginTime = NetworkTimeHelper.Instance.GetNowTime();

		StartCoroutine(Rotate());

		UserDataHelper.Instance.TryHandlePendingLoginWorkflow();

		TryHandleDeviceIdChange();

		var socialId = UserDeviceLocalData.Instance.GetCurrSocialAppID;
		if(UserLoginStateHelper.Instance.IsDeviceLoginState || UserBasicData.Instance.UDID == "")
		{
			//LogInWithTourists();

			//magic code
			UnityTimer.Instance.StartTimer(1, LogInWithTourists);
		}
		else
		{
			LoginStageEnd();
		}

		Debug.Log("StartLoading started: " + Time.time);
		UpdateLoginInfo();
		AnalysisManager.Instance.StartLoading();
	}

	void UpdateLoginInfo()
	{
		UserBasicData.Instance.LoginTimes++;
		if (!TimeUtility.IsSameDay(NetworkTimeHelper.Instance.GetNowTime(), UserBasicData.Instance.LastLoginDateTime))
		{
			UserBasicData.Instance.LoginDays++;
		}
	}

	void TrackFirstEnterGame()
	{
		//Note: add IsNewGame check here
		//Because IsFirstEnterGame is added in v1.9.0 and old users don't have this flag,
		//in order that old users don't send AppInstall event, add IsNewGame check
		if(UserDeviceLocalData.Instance.IsFirstEnterGame && UserDeviceLocalData.Instance.IsNewGame)
		{
			AnalysisManager.Instance.TrackInstall();
			UserDeviceLocalData.Instance.FirstEnterGameTime = NetworkTimeHelper.Instance.GetNowTime();
			UserDeviceLocalData.Instance.IsFirstEnterGame = false;
		}
	}

	IEnumerator Rotate()
	{
		while(true)
		{
			_RotateGameObject.transform.Rotate(-RotateSpeed * Vector3.forward);
			yield return new WaitForSeconds(_rotateInterval);
		}
	}

	void LogInWithTourists()
	{
		LogUtility.Log("CurrSocialAppID:" + UserDeviceLocalData.Instance.GetCurrSocialAppID + ", UDID:" + UserBasicData.Instance.UDID, Color.cyan);

		//By nichos: If not login, set the flag to false
		if(UserDeviceLocalData.Instance.ShouldHandleUserDataLoss && !FacebookHelper.IsLoggedIn)
			UserDeviceLocalData.Instance.ShouldHandleUserDataLoss = false;

		if(UserDeviceLocalData.Instance.ShouldHandleUserDataLoss && FacebookHelper.IsLoggedIn)
		{
			HandleUserDataLoss();
		}
		else if(UserDeviceLocalData.Instance.GetCurrSocialAppID == "" || UserBasicData.Instance.UDID == ""
			|| UserLoginStateHelper.Instance.IsLoginWorkflowStatePending)
		{
			UserDataHelper.Instance.UserSocialLogin(UserSocialState.Device);
			Debug.Log("LogInWithTourists Register");
			_isTouristsLoading = true;
		}
		else
		{
			LoginStageEnd();
		}
	}

	void UserDataServerMessageProcess(UserDataServerEvent uds)
	{
        LogUtility.Log("UserDataServerEvent showUI: " + uds.ShowUi.ToString(), Color.magenta);
		switch(uds.ShowUi)
		{
			case UserDataServerEvent.ShowUI.Error:
				if(_isTouristsLoading)
				{
					LoginStageEnd();
				}
				else
				{
					StopAllCoroutines();
					DefaultState();
				}
				break;

			case UserDataServerEvent.ShowUI.Loading:
				break;

			case UserDataServerEvent.ShowUI.Success:
				LoginStageEnd();
				break;

			case UserDataServerEvent.ShowUI.Ask:
				//StartCoroutine(StartRotate(_loadingTime));
				break;

			case UserDataServerEvent.ShowUI.NoOne:
				//do nothing
				break;

			default:
				break;
		}
	}

	void UserChooseUserDataCallback(UserChouseUserDataEvent e)
	{
		LoginStageEnd();
	}

	void LoginStageEnd()
	{
		Debug.Log("StartLoading: enter LoginStageEnd");

		StopUserDataLossTimeoutCoroutine();

		if(DeviceUtility.IsConnectInternet())
			UserDataHelper.Instance.FetchGMUserData(StartRotate);
		else
			StartRotate();
	}

	void StartRotate()
	{
		StartCoroutine(StartRotateCoroutine());
	}

	bool CanEndLoading()
	{
		bool result = (NetworkTimeHelper.Instance.IsServerTimeGetted ||
			Application.internetReachability == NetworkReachability.NotReachable)
			&& LiveUpdateManager.Instance.State != LiveUpdateState.Updating;
		return result;
	}

	IEnumerator StartRotateCoroutine()
	{
		Debug.Log("StartLoading: StartRotateCoroutine called");
		
		float startTime = Time.unscaledTime;
		while(Time.unscaledTime - startTime < _loadingTime)
		{
			if(CanEndLoading())
				break;
			yield return new WaitForEndOfFrame();
		}

		LoadingFinishedCallback();

		StartCoroutine(ScenesController.Instance.StartLoadingEnterMainMapScene());
		yield break;
	}

	void DefaultState()
	{
		_RotateGameObject.SetActive(false);
	}

	void LoadingFinishedCallback()
	{
		Debug.Log("StartLoading finished: " + Time.time);
		AnalysisManager.Instance.EndLoading();
#if UNITY_EDITOR
        StartLoadingSceneHasLoaded = true;
#endif
    }

	#region Handle UserData loss bug

	void HandleUserDataLoss()
	{
		Debug.Log("Handle UserData loss bug...");
		FacebookHelper.LogoutFB();

		_handleUserDataLossTimeoutCoroutine = UnityTimer.Start(this, _handleUserDataLossWaitTime, WaitLoginFBTimeout);

		CitrusEventManager.instance.AddListener<UserLoginFBSuccessEvent>(LoginFBSuccessCallback);
		CitrusEventManager.instance.AddListener<UserLoginFBFailEvent>(LoginFBFailCallback);

		FacebookHelper.LoginWithFB();
	}

	void WaitLoginFBTimeout()
	{
		_handleUserDataLossTimeoutCoroutine = null;

		LoginStageEnd();
	}

	void LoginFBSuccessCallback(UserLoginFBSuccessEvent e)
	{
		StopUserDataLossTimeoutCoroutine();
	}

	void LoginFBFailCallback(UserLoginFBFailEvent e)
	{
		LoginStageEnd();
	}

	void StopUserDataLossTimeoutCoroutine()
	{
		if(_handleUserDataLossTimeoutCoroutine != null)
		{
			StopCoroutine(_handleUserDataLossTimeoutCoroutine);
			_handleUserDataLossTimeoutCoroutine = null;
		}
	}

	#endregion

	#region Handle device id change

	// Note: device id changes on iOS from 1.15 to 1.16, but it's completely handled in 1.17
	// But this function is designed for general use when device id changes
	void TryHandleDeviceIdChange()
	{
		CheckShouldHandleDeviceIdChange();
		if(UserDeviceLocalData.Instance.ShouldHandleDeviceIdChange)
		{
			Debug.Log("Handle device id change");
			UserDataHelper.Instance.HandleDeviceIdChange();
		}
	}

	void CheckShouldHandleDeviceIdChange()
	{
		if(!string.IsNullOrEmpty(UserDeviceLocalData.Instance.GetCurrSocialAppID)
			&& !string.IsNullOrEmpty(UserBasicData.Instance.UDID)
			&& UserLoginStateHelper.Instance.IsDeviceLoginState)
		{
			if(UserDeviceLocalData.Instance.GetCurrSocialAppID != UserIDWithPrefix.GetDeviceIDWithPrefix())
			{
				LogUtility.Log("Detect device id changes", Color.red);
				LogUtility.Log("Old device id: " + UserDeviceLocalData.Instance.GetCurrSocialAppID, Color.red);
				LogUtility.Log("New device id: " + UserIDWithPrefix.GetDeviceIDWithPrefix(), Color.red);

				UserDeviceLocalData.Instance.GetCurrSocialAppID = UserIDWithPrefix.GetDeviceIDWithPrefix();

				UserDeviceLocalData.Instance.ShouldHandleDeviceIdChange = true;
			}
		}
	}

	#endregion

//	#if DEBUG
//    void OnGUI()
//    {
//        if (GUI.Button(new Rect(10, 160, 100, 50), "Success"))
//        {
//            UserDataServerEvent e = new UserDataServerEvent(UserDataServerEvent.ShowUI.Success);
//            UserDataServerMessageProcess(e);
//        }
//    }
//	#endif
}
