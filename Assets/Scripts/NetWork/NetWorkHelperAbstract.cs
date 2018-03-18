using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetWorkHelperAbstract<T> where T : NetWorkHelperAbstract<T>, new()
{
	static private T _instance = null;
	static public T Instance
	{
		get
		{
			if(_instance == null)
				_instance = new T();
			return _instance;
		}
	}

	protected Dictionary<string, string> ServerFunctionName;

	/// <summary>
	/// _baseURL
	/// </summary>
	public string _baseUrl = ServerConfig.GameServerUrl;

	protected float TimeOutTime = 10f;

	protected NetWorkHelperAbstract()
	{
		AddServerFunctionName();
	}

	public abstract void AddServerFunctionName();

	/// <summary>
	/// 得到的当前分数的上下3个数据
	/// </summary>
	/// <returns>The uid request.</returns>
	public IEnumerator NetCall(MonoBehaviour HelperGameObject, string functionName, Dictionary<string, object> SendDic, Action<JSONObject> resultAction, Action<JSONObject> errorAction,bool compress = false)
	{
		AddDefaultValueToDic(SendDic,functionName);
		yield return HelperGameObject.StartCoroutine(NetWorkHelper.UniversalNetCall(
			HelperGameObject, TimeOutTime, _baseUrl, ServerFunctionName[functionName], SendDic, resultAction, errorAction,compress
		));
	}

	protected virtual void AddDefaultValueToDic(Dictionary<string, object> dic , string functionName = null)
	{
		dic.Add("ProjectName", BuildUtility.GetProjectName());
		dic.Add("UDID", UserBasicData.Instance.UDID);
	}
}

//public class NetBFactory : SimpleSingleton<NetB>
//{
//}


