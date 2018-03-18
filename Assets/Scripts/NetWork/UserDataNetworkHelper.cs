using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using MiniJSON;
using System.Text;
using System;

public class UserDataNetworkHelper : SimpleSingleton<UserDataNetworkHelper>
{
	private static float _timeOutTime = 180f; //can't be too short

	private string _baseUrl = ServerConfig.GameServerUrl;
	private string _registeruid = "register_uid";
	private string _saveuserdata = "user_data/save";
	private string _fetchuserdata = "user_data/fetch";
	private string _bindDeviceId = "user_data/bind_device_id";
	private string _fetchGMUserData = "user_data/fetchgmt";

	/// 注册请求得到UDID如果ID已存在就会返回BindUserData":flase,游戏刚运行的时候运行
	/// 得到UDID和绑定信息通过社交账号,如果社交账号相同得到的UDID相同,可以重复注册重复注册就会冲掉之前的社交账号
	/// 得到的UDID也不同
	public IEnumerator RegisterUDIDRequest(string SocialId, Action<bool, string> resultAction = null, Action errorAction = null)
	{
		Debug.Log("RegisterUDIDRequest called");

		Dictionary<string, object> dic = new Dictionary<string, object>();
		dic.Add("ProjectName", BuildUtility.GetProjectName());
		dic.Add("SocialId", SocialId);
		string jsonstr = Json.Serialize(dic);

		WWW www = new WWW(_baseUrl + _registeruid, Encoding.UTF8.GetBytes(jsonstr));

		//yield return www;
		float timer = 0;
		bool timeoutfailed = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > _timeOutTime)
			{
				timeoutfailed = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(timeoutfailed || !string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("RegisterUDIDRequest: network error:" + www.error);
			if(errorAction != null)
				errorAction();
			yield break;
		}

		var result = new JSONObject(www.text);
		ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
		if(errorCode == ServerErrorCode.ok)
		{
			string udid = result.GetField("UDID").str;
			bool hadRegistered = result.GetField("BindUserData").b;
			LogUtility.Log("RegisterUDIDRequest: succeed to registered UDID:" + udid + ", isFirstRegister:" + hadRegistered, Color.green);

			if(resultAction != null)
				resultAction.Invoke(hadRegistered, udid);
		}
		else
		{
			Debug.LogError("RegisterUDIDRequest: response errorCode:" + errorCode.ToString());
			if(errorAction != null)
				errorAction();
		}

		yield return null;
	}

	public IEnumerator SaveUserDataRequest(MonoBehaviour helper,bool isForceUpload, Action<JSONObject> userDataConflict = null, Action errorAction = null)
	{
		Debug.Log("SaveUserDataRequest called");

		var userdatadic = UserDataHelper.Instance.CreateUserDataDic();
		string currmd5 = UserDataHelper.Instance.DictionaryJSONGetMD5(userdatadic);
		string lastmd5 = UserBasicData.Instance.LastArchiveMD5;
		userdatadic.Add("ProjectName", BuildUtility.GetProjectName());
		userdatadic.Add("UDID", UserBasicData.Instance.UDID);
		userdatadic.Add("DeviceId", DeviceUtility.GetDeviceId());
		userdatadic.Add("DebugPlatform", PlatformManager.Instance.GetPlatformString());
		userdatadic.Add("IsForce", isForceUpload ? 1 : 0);

		//Important note: when isForceUpload is true, don't add LastMd5 to make server force save
		if(lastmd5 != "" && !isForceUpload)
			userdatadic.Add("LastMd5", lastmd5);
		userdatadic.Add("Md5", currmd5);

		Debug.Log("LastMD5: " + lastmd5);
		Debug.Log("DeviceId: " + DeviceUtility.GetDeviceId());
		Debug.Log("UDID: " + UserBasicData.Instance.UDID);

		AnalysisManager.Instance.SendSaveUserDataToServer();

		string jsonstr = Json.Serialize(userdatadic);
		Debug.Log("SaveUserDataRequest json:" + jsonstr);

		yield return helper.StartCoroutine(NetWorkHelper.EasyNetCall(_timeOutTime, _baseUrl + _saveuserdata, (WWW obj) =>
		{
//			var result = new JSONObject (obj.text);
			var result = new JSONObject (NetWorkHelper.CheckGzip(obj));
			LogUtility.Log("SaveUserDataRequest result:" + result.ToString(), Color.cyan);

			ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
			if (errorCode == ServerErrorCode.ok)
			{
				LogUtility.Log("SaveUserDataRequest: succeed to save UserData to server", Color.green);

				// todo 测试
				UserBasicData.Instance.LastArchiveMD5 = currmd5;

				AnalysisManager.Instance.SendSaveUserDataToServerCallback("success");
			}
			else if (errorCode == ServerErrorCode.userDataConflict)
			{
				var data = result.GetField("data");
				LogUtility.Log("SaveUserDataRequest: conflict with server UserData:" + data, Color.cyan);

				if (userDataConflict != null)
					userDataConflict(data);

				AnalysisManager.Instance.SendSaveUserDataToServerCallback("conflict");
			}
			else
			{
				Debug.LogError("SaveUserDataRequest: save error, errorCode:" + errorCode.ToString());
				if (errorAction != null)
					errorAction();

				AnalysisManager.Instance.SendSaveUserDataToServerCallback("error_code:" + errorCode.ToString());
			}
		}, (WWW obj) =>
		{
			if(errorAction != null)
				errorAction();
			AnalysisManager.Instance.SendSaveUserDataToServerCallback("http error");
		}, Encoding.UTF8.GetBytes(jsonstr),true));
		/*
		WWW www = new WWW(_baseUrl + _saveuserdata, Encoding.UTF8.GetBytes(jsonstr));
		float timer = 0;
		bool isTimeout = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > _timeOutTime)
			{
				isTimeout = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(isTimeout || !string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("SaveUserDataRequest error: isTimeout:" + isTimeout + " www.error:" + www.error);

			if(errorAction != null)
				errorAction();

			AnalysisManager.Instance.SendSaveUserDataToServerCallback("http error");
			yield break;
		}

		var result = new JSONObject(www.text);
		LogUtility.Log("SaveUserDataRequest result:" + result.ToString(), Color.cyan);

		ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
		if(errorCode == ServerErrorCode.ok)
		{
			LogUtility.Log("SaveUserDataRequest: succeed to save UserData to server", Color.green);

			// todo 测试
			UserBasicData.Instance.LastArchiveMD5 = currmd5;

			AnalysisManager.Instance.SendSaveUserDataToServerCallback("success");
		}
		else if(errorCode == ServerErrorCode.userDataConflict)
		{
			var data = result.GetField("data");
			LogUtility.Log("SaveUserDataRequest: conflict with server UserData:" + data, Color.cyan);

			if(userDataConflict != null)
				userDataConflict(data);

			AnalysisManager.Instance.SendSaveUserDataToServerCallback("conflict");
		}
		else
		{
			Debug.LogError("SaveUserDataRequest: save error, errorCode:" + errorCode.ToString());
			if(errorAction != null)
				errorAction();

			AnalysisManager.Instance.SendSaveUserDataToServerCallback("error_code:" + errorCode.ToString());
		}
		*/
		yield return null;
	}

	public IEnumerator ForceBindDevice(Action<JSONObject> userDataConflict = null, Action errorAction = null)
	{
		var userdatadic = new Dictionary<string, object>();
		userdatadic.Add("ProjectName", BuildUtility.GetProjectName());
		userdatadic.Add("UDID", UserBasicData.Instance.UDID);
		userdatadic.Add("DeviceId", DeviceUtility.GetDeviceId());
		userdatadic.Add("DebugPlatform", PlatformManager.Instance.GetPlatformString());
		userdatadic.Add("IsForce", 1);

		var tempDic = UserDataHelper.Instance.CreateUserDataDic();
		string curMd5 = UserDataHelper.Instance.DictionaryJSONGetMD5(tempDic);
		userdatadic.Add("Md5", curMd5);

		string jsonstr = Json.Serialize(userdatadic);
		Debug.Log(jsonstr);
		WWW www = new WWW(_baseUrl + _saveuserdata, Encoding.UTF8.GetBytes(jsonstr));
		float timer = 0;
		bool isTimeout = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > _timeOutTime)
			{
				isTimeout = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(isTimeout || !string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("ForceBindDevice error: isTimeout:" + isTimeout + " www.error:" + www.error);

			if(errorAction != null)
				errorAction();
			yield break;
		}

		var result = new JSONObject(www.text);
		LogUtility.Log("ForceBindDevice result:" + result.ToString(), Color.cyan);

		ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
		if(errorCode != ServerErrorCode.ok)
			Debug.Log("ForceBindDevice: error code:" + errorCode.ToString());

		yield return null;
	}

	public IEnumerator FetchUserData(MonoBehaviour helper,Action<JSONObject> resultAction = null, Action errorAction = null)
	{
		Debug.Log("FetchUserData called");

		Dictionary<string, object> dic = new Dictionary<string, object>();
		dic.Add("ProjectName", BuildUtility.GetProjectName());
		//In server logic, the UserData is stored in a table which the key is UDID, not socialId
		dic.Add("UDID", UserBasicData.Instance.UDID);
		string jsonstr = Json.Serialize(dic);
		LogUtility.Log(jsonstr, Color.cyan);

		yield return helper.StartCoroutine(NetWorkHelper.EasyNetCall(_timeOutTime, _baseUrl + _fetchuserdata, (WWW obj) =>
		{
//			var result = new JSONObject (obj.text);
			var result = new JSONObject(NetWorkHelper.CheckGzip(obj));
			ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
			if(errorCode == ServerErrorCode.ok)
			{
				var data = result.GetField("data");
				if(resultAction != null)
				{
					LogUtility.Log("FetchUserData: succeed to fetch UserData: " + data, Color.green);
					resultAction.Invoke(data);
				}
			}
			else
			{
				Debug.LogError("FetchUserData: response errorCode:" + errorCode.ToString());
				if(errorAction != null)
					errorAction();
			}
		}, (WWW obj) =>
		{
			Debug.LogError("FetchUserData: download error: " + obj.error);
			if(errorAction != null)
				errorAction();
		},Encoding.UTF8.GetBytes(jsonstr), true));

/*		WWW www = new WWW(_baseUrl + _fetchuserdata, Encoding.UTF8.GetBytes(jsonstr));

		float timer = 0;
		bool isTimeout = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > _timeOutTime)
			{
				isTimeout = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(isTimeout || !string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("FetchUserData: download error: " + www.error);
			if(errorAction != null)
				errorAction();
			yield break;
		}

		var result = new JSONObject(www.text);
		ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
		if(errorCode == ServerErrorCode.ok)
		{
			var data = result.GetField("data");
			if(resultAction != null)
			{
				LogUtility.Log("FetchUserData: succeed to fetch UserData: " + data, Color.green);
				resultAction.Invoke(data);
			}
		}
		else
		{
			Debug.LogError("FetchUserData: response errorCode:" + errorCode.ToString());
			if(errorAction != null)
				errorAction();
		}
*/
		yield return null;
	}

	public IEnumerator FetchGMUserData(MonoBehaviour helper, Action<JSONObject> resultAction = null, Action errorAction = null)
	{
		Debug.Log("FetchGMUserData called");

		Dictionary<string, object> dic = new Dictionary<string, object>();
		dic.Add("ProjectName", BuildUtility.GetProjectName());
		//In server logic, the UserData is stored in a table which the key is UDID, not socialId
		dic.Add("UDID", UserBasicData.Instance.UDID);
		string jsonstr = Json.Serialize(dic);
		LogUtility.Log(jsonstr, Color.cyan);

		helper.StartCoroutine(NetWorkHelper.EasyNetCall(_timeOutTime, _baseUrl + _fetchGMUserData,
			(WWW obj) => {
				var result = new JSONObject(obj.text);
				ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
				if(errorCode == ServerErrorCode.ok)
				{
					var data = result.GetField("data");
					if(resultAction != null)
					{
						LogUtility.Log("FetchGMUserData: succeed to fetch UserData: " + data, Color.green);
						resultAction.Invoke(data);
					}
				}
				else
				{
					Debug.LogError("FetchGMUserData: response errorCode:" + errorCode.ToString());
					if(errorAction != null)
						errorAction();
				}
			},
			(WWW obj) => {
				Debug.LogError("FetchGMUserData: download error: " + obj.error);
				if(errorAction != null)
					errorAction();
			},
			Encoding.UTF8.GetBytes(jsonstr), false));
				
		yield return null;
	}

	public IEnumerator BindDeviceIdPatch(Action<JSONObject> resultAction = null, Action errorAction = null)
	{
		Debug.Log("BindDeviceIdPatch called");

		Dictionary<string, object> dic = new Dictionary<string, object>();
		dic.Add("ProjectName", BuildUtility.GetProjectName());
		dic.Add("DeviceId", DeviceUtility.GetDeviceId());
		dic.Add("UDID", UserBasicData.Instance.UDID);
		string jsonstr = Json.Serialize(dic);
		LogUtility.Log(jsonstr, Color.cyan);

		WWW www = new WWW(_baseUrl + _bindDeviceId, Encoding.UTF8.GetBytes(jsonstr));

		float timer = 0;
		bool isTimeout = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > _timeOutTime)
			{
				isTimeout = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(isTimeout || !string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("BindDeviceIdPatch error:" + www.error);
			if(errorAction != null)
				errorAction();
			yield break;
		}

		var result = new JSONObject(www.text);
		ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
		if(errorCode == ServerErrorCode.ok)
		{
			var data = result.GetField("data");
			if(resultAction != null)
			{
				Debug.Log("BindDeviceIdPatch success:" + data);
				resultAction.Invoke(data);
			}
		}
		else
		{
			Debug.LogError("BindDeviceIdPatch errorCode:" + errorCode.ToString());
			if(errorAction != null)
				errorAction();
		}

		yield return null;
	}
}
