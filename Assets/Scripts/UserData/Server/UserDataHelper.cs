using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CitrusFramework;
using MiniJSON;
using UnityEngine;

public class UserDataHelper : Singleton<UserDataHelper>
{
	/// 从服务器获得的上次的MD5与本地存储的当前MD5不同时暂存的数据
	public JSONObject MD5DifferentJsonObject = null;

	public Dictionary<string, object> CreateUserDataDic()
	{
		Dictionary<string, object> baseDataDic = UserBasicDataJSON.CreatDataDic();
		baseDataDic.Add(ServerFieldName.MachineData.ToString(), UserMachineDataJSON.CreatDataDicJSON());
		return baseDataDic;
	}

	/// <summary>
	/// 字典序列化成Json并且返回MD5值
	/// 第一个参数返回MD5,第二个返回JsonStr
	/// </summary>
	/// <param name="dic">Dic.</param>
	/// <param name="getreturn"></param>
	public string DictionaryJSONGetMD5(Dictionary<string, object> dic)
	{
		string jsonStr = Json.Serialize(dic);

		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(jsonStr);
		byte[] hash = md5.ComputeHash(inputBytes);

		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < hash.Length; i++)
		{
			sb.Append(hash[i].ToString("X2"));
		}
		return sb.ToString();
	}

	/// <summary>
	/// 根据字段名得到JSONOBJECT
	/// </summary>
	/// <returns>The field data for name.</returns>
	/// <param name="jsobject">Jsobject.</param>
	/// <param name="sfn">Sfn.</param>
	public JSONObject GetFieldDataForName(JSONObject jsobject, ServerFieldName sfn)
	{
		return jsobject.GetField(sfn.ToString());
	}

	/// <summary>
	/// 覆盖用户数据
	/// </summary>
	/// <param name="jsonObject">Json object.</param>
	private void OverlayUserData(JSONObject jsonObject)
	{
		UserBasicDataJSON.UseJSONToData(jsonObject);
		var usermachine = new JSONObject(jsonObject.GetField(ServerFieldName.MachineData.ToString()).str);
		UserMachineDataJSON.UseJSONToData(usermachine);
	}

	public void UserSocialLogin(UserSocialState state)
	{
		Debug.Log("UserSocialLogin called: " + state.ToString());
		string socialId = UserDataUtility.GetSocialIdWithPrefix(state);
		CitrusEventManager.instance.Raise(new UserLoginOrRegisterEvent(socialId));

		if(UserDeviceLocalData.Instance.GetCurrSocialAppID == socialId && !string.IsNullOrEmpty(UserBasicData.Instance.UDID)
			&& UserLoginStateHelper.Instance.IsLoginWorkflowStateOK)
		{
			Debug.LogError("UserSocialLogin: ignore the call, should not be here?");
			Debug.Assert(false);
			HandleUserLoginSuccess();
		}
		else
		{
			StartCoroutine(UserSocialLoginCoroutine(socialId));
		}
	}

	private IEnumerator UserSocialLoginCoroutine(string socialID)
	{
		Debug.Log("UserSocialLoginCoroutine called");
		Debug.Log("From:" + UserDeviceLocalData.Instance.GetCurrSocialAppID + "to:" + socialID);

		bool isSocialIdChanged = (UserDeviceLocalData.Instance.GetCurrSocialAppID != socialID);

		HanleUserLoginStart();
		CitrusEventManager.instance.Raise(new UserDataServerEvent(UserDataServerEvent.ShowUI.Loading));

		//By nichos:
		//I guess the order that assigning socialId before generating files is necessary
		UserDeviceLocalData.Instance.GetCurrSocialAppID = socialID;
		if(isSocialIdChanged)
		{
			UserDataFileController.CreateSocialIDUserDataFile();
			UserDataFileController.LoadAllFile();
		}

		Debug.Log("Start fetch UDID, current is:" + UserBasicData.Instance.UDID);

		bool isFirstRegister = false;
		bool isError = false;
		yield return StartCoroutine(UserDataNetworkHelper.Instance.RegisterUDIDRequest(socialID,
			(bool first, string udid) => {
				isFirstRegister = first;
				UserBasicData.Instance.UDID = udid;
				LogUtility.Log("Fetch UDID succeed:" + udid, Color.green);
			},
			() => {
				isError = true;
			})
		);

		if(isError)
		{
			Debug.LogError("Register UDID error");
			HandleUserLoginFail();
			yield break;
		}
		else
		{
			if(isFirstRegister)
				yield return StartCoroutine(HandleNewUserLogin());
			else
				yield return StartCoroutine(HandleOldUserLogin());
		}
	}

	IEnumerator HandleNewUserLogin()
	{
		Debug.Log("HandleNewUserLogin called");

		UserBasicData.Instance.RegistrationTime = NetworkTimeHelper.Instance.GetNowTime();

		//Actually, it means first login game, but designer don't want to change the event name
		AnalysisManager.Instance.FirstOpenGame();

		yield return StartCoroutine(SaveUserDataToServerCoroutine(true));

		HandleUserLoginSuccess();
		yield break;
	}

	IEnumerator HandleOldUserLogin()
	{
		Debug.Log("HandleOldUserLogin called");

		JSONObject serverObj = null;
		bool isError = false;

	    DateTime today = NetworkTimeHelper.Instance.GetNowTime().Date;
        bool isSecondDayLeft = today == UserBasicData.Instance.RegistrationTime.AddDays(1).Date;
	    bool isFirstLoginToday = UserBasicData.Instance.LastLoginDateTime.Date != today;

        if (isSecondDayLeft && isFirstLoginToday)
	    {
	        AdjustManager.Instance.SecondDayLeftEvent();
	    }

        AnalysisManager.Instance.SendFetchUserDataFromServer();

		yield return StartCoroutine(UserDataNetworkHelper.Instance.FetchUserData(this,
			(JSONObject obj) => {
				if(obj != null)
				{
					serverObj = obj;
					AnalysisManager.Instance.SendFetchUserDataFromServerCallback("success");
				}
				else
				{
					isError = true;
					AnalysisManager.Instance.SendFetchUserDataFromServerCallback("null json");
				}
			},
			() => {
				isError = true;
				AnalysisManager.Instance.SendFetchUserDataFromServerCallback("error");
			}));

		if(isError || serverObj == null)
		{
			LogUtility.Log("HandleOldUserLogin: fetch UserData fail", Color.red);
			HandleUserLoginFail();
			yield break;
		}

		if(IsServerAndLocalDataTheSame(serverObj))
		{
			LogUtility.Log("HandleOldUserLogin: server and local UserData is the same", Color.cyan);
			HandleUserLoginSuccess();
			yield break;
		}
		else
		{
			MD5DifferentJsonObject = serverObj;

			LogUtility.Log("HandleOldUserLogin: server and local UserData is NOT the same", Color.cyan);

			if(UserDeviceLocalData.Instance.IsNewGame || UserDeviceLocalData.Instance.ShouldHandleUserDataLoss)
			{
				LogUtility.Log("HandleOldUserLogin: is fresh install, so overwrite server UserData to local", Color.green);

				GetServerDataToOverlayUserData(serverObj);
				//by nichos. todo, uncomment it in next version and test
				//ForceBindDevice();
				HandleUserLoginSuccess();

				AnalysisManager.Instance.TouristLoginOrOverallInstall();
				yield break;
			}
			else
			{
				LogUtility.Log("HandleOldUserLogin: ask user to choose UserData", Color.cyan);

				CitrusEventManager.instance.Raise(new UserDataServerEvent(UserDataServerEvent.ShowUI.Ask));
				yield break;
			}
		}
	}

	bool IsServerAndLocalDataTheSame(JSONObject serverObj)
	{
		string serverHash = "";
		if (serverObj != null && serverObj.HasField (ServerFieldName.Md5.ToString ()))
			serverHash = serverObj.GetField(ServerFieldName.Md5.ToString()).str;
		string localHash = DictionaryJSONGetMD5(CreateUserDataDic());

		return (!serverHash.IsNullOrEmpty() && serverHash == localHash);
	}

	void HanleUserLoginStart()
	{
		UserLoginStateHelper.Instance.SetLoginWorkflowState(UserLoginWorkflowState.Pending);
	}

	void HandleUserLoginFail()
	{
		CitrusEventManager.instance.Raise(new UserDataServerEvent(UserDataServerEvent.ShowUI.Error));
		TryLogOutSocialApp();
		UserLoginStateHelper.Instance.SetLoginWorkflowState(UserLoginWorkflowState.OK);
	}

	void HandleUserLoginSuccess()
	{
		CitrusEventManager.instance.Raise(new UserDataServerEvent(UserDataServerEvent.ShowUI.Success));
		UserLoginStateHelper.Instance.SetLoginWorkflowState(UserLoginWorkflowState.OK);

		UserDeviceLocalData.Instance.ShouldHandleUserDataLoss = false;
	}

	public void HandleUserChooseAskEnd()
	{
		UserLoginStateHelper.Instance.SetLoginWorkflowState(UserLoginWorkflowState.OK);

		UserDeviceLocalData.Instance.ShouldHandleUserDataLoss = false;
	}

	public void TryLogOutSocialApp()
	{
		if(FacebookHelper.IsLoggedIn)
		{
			FacebookHelper.LogoutFB();
			Debug.Log("LogOutSocialApp: logout ok");
		}
		else
		{
			Debug.Log("LogOutSocialApp: no logout since no login");
		}
	}

	public void UserSocialLogoutCallback()
	{
		string socialAppID = UserDeviceLocalData.Instance.GetCurrSocialAppID;
		if(!UserLoginStateHelper.Instance.IsFacebookLoginState)
		{
			LogUtility.Log("非FB登录状态，无需处理", Color.cyan);
			return;
		}

		UserDeviceLocalData.Instance.GetCurrSocialAppID = UserIDWithPrefix.GetDeviceIDWithPrefix();

		UserDataFileController.LoadAllFile();
	}

	public void TryHandlePendingLoginWorkflow()
	{
		if(UserLoginStateHelper.Instance.IsLoginWorkflowStatePending)
		{
			LogUtility.Log("LoginWorkflowState is Pending, so handle it", Color.cyan);
			TryLogOutSocialApp();
		}
	}

	public void SaveUserDataToServer(bool isForceUpload)
	{
		StartCoroutine(SaveUserDataToServerCoroutine(isForceUpload));
	}

	private IEnumerator SaveUserDataToServerCoroutine(bool isForceUpload)
	{
		// 没登录不上传
		if(UserDeviceLocalData.Instance.GetCurrSocialAppID == "")
		{
			LogUtility.Log("没登录不上传", Color.cyan);
			yield break;
		}
		// 强制覆盖 LAST为空
		if(isForceUpload)
		{
			UserBasicData.Instance.LastArchiveMD5 = "";
			//强制覆盖清空UI
			CitrusEventManager.instance.Raise(new UserDataServerEvent(UserDataServerEvent.ShowUI.NoOne));
		}

		var currMD5 = DictionaryJSONGetMD5(CreateUserDataDic());
		var lastMD5 = UserBasicData.Instance.LastArchiveMD5;
		if(currMD5 == lastMD5)
		{
			LogUtility.Log("上传存档与上次上传存档相同不上传", Color.cyan);
			yield break;
		}
		// 是否有冲突
		bool haveUserDataConflict = false;
		yield return StartCoroutine(UserDataNetworkHelper.Instance.SaveUserDataRequest(this,isForceUpload, (jo) => {
			MD5DifferentJsonObject = jo;
			haveUserDataConflict = true;
		}));

		// 有冲突
		if(haveUserDataConflict)
		{
			CitrusEventManager.instance.Raise(new UserDataServerEvent(UserDataServerEvent.ShowUI.Ask));
			yield break;
		}

		yield return null;
	}

	public void ForceBindDevice()
	{
		StartCoroutine(UserDataNetworkHelper.Instance.ForceBindDevice());
	}

	public bool UseDFMD5ServerDataOverlayUserData()
	{
		return GetServerDataToOverlayUserData(MD5DifferentJsonObject);
	}

	private bool GetServerDataToOverlayUserData(JSONObject jsob)
	{
		bool result = false;
		if(jsob != null)
		{
			result = true;
			OverlayUserData(jsob);
			UserBasicData.Instance.LastArchiveMD5 = jsob.GetField(ServerFieldName.Md5.ToString()).str;
			UserDataFileController.LoadAllFile();

			LogUtility.Log("GetServerDataToOverlayUserData: overwrite UserData ok", Color.cyan);
		}
		else
		{
			result = false;
			Debug.LogError("GetServerDataToOverlayUserData: overwrite UserData is null");
			Debug.Assert(false);

			//send event for later track
			AnalysisManager.Instance.SendOverwriteUserDataError("null json");
		}
		return result;
	}

	public void HandleDeviceIdChange()
	{
		StartCoroutine(HandleDeviceIdChangeCoroutine());
	}

	IEnumerator HandleDeviceIdChangeCoroutine()
	{
		if(!FacebookHelper.IsLoggedIn && !string.IsNullOrEmpty(UserBasicData.Instance.UDID))
		{
			while(true)
			{
				if(DeviceUtility.IsConnectInternet() && !FacebookHelper.IsLoggedIn)
				{
					yield return UserDataNetworkHelper.Instance.BindDeviceIdPatch();
					yield return UserDataHelper.Instance.SaveUserDataToServerCoroutine(true);

					//By nichos:
					//This line can be written in the success callback of UserDataNetworkHelper.Instance.BindDeviceIdPatch
					//But writing here is also ok for me
					UserDeviceLocalData.Instance.ShouldHandleDeviceIdChange = false;

					yield break;
				}
				else
				{
					yield return new WaitForSeconds(1.0f);
				}
			}
		}
	}

	public void FetchGMUserData(Callback endCallback)
	{
		AnalysisManager.Instance.SendFetchGMUserDataFromServer();

		StartCoroutine(UserDataNetworkHelper.Instance.FetchGMUserData(this,
			(JSONObject obj) => {
				if(obj != null)
				{
					AnalysisManager.Instance.SendFetchGMUserDataFromServerCallback("has data");
					UserBasicDataJSON.UseJSONToData(obj);
					CitrusEventManager.instance.Raise(new UserDataLoadEvent());
				}
				else
				{
					AnalysisManager.Instance.SendFetchGMUserDataFromServerCallback("no data");
				}

				endCallback();
			},
			() => {
				AnalysisManager.Instance.SendFetchGMUserDataFromServerCallback("error");

				endCallback();
			}));
	}
}
