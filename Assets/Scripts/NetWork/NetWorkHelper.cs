using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using UnityEngine;
using CitrusFramework;
using ICSharpCode.SharpZipLib.GZip;

//ServerErrorCode should be consistent with server project -> slots_game_server -> util.js:
public enum ServerErrorCode
{
	ok = 0,
	notInCache = 1,
	invalidParam = 2,
	redisError = 3,
	invalidUser = 4,
	dbError = 5,
	userDataConflict = 6,
	unknown = 7,
	rpcDown = 8,
	rpcProto = 9,
	userDataUnchanged = 10,
	waitSysInit = 11,
	invalidState = 12,
	locked = 13,
	timeout = 14,
	giftNotExist = 16,
	giftRunOut = 17,
	giftAlreadyGot = 18,
	giftChannel = 19,
};

public static class NetWorkHelper
{
	private readonly static string UDIDkey = "UDID";

	/// <summary>
	/// 通用的NetworkCall
	/// </summary>
	/// <returns>The net call.</returns>
	/// <param name="TimeOutTime">Time out time.</param>
	/// <param name="baseUrl">Base URL.</param>
	/// <param name="functionName">Function name.</param>
	/// <param name="SendDic">Send dic.</param>
	/// <param name="resultAction">Result action.</param>
	/// <param name="errorAction">Error action.</param>
	public static IEnumerator UniversalNetCall(MonoBehaviour helper, float timeOutTime, string baseUrl, string functionName, Dictionary<string, object> SendDic, Action<JSONObject> resultAction, Action<JSONObject> errorAction,bool compress = false)
	{
		if(SendDic.ContainsKey(UDIDkey))
		{
			if(string.IsNullOrEmpty(SendDic[UDIDkey].ToString()))
			{
				Debug.LogError("UDID is null so don't send the request and return error");
				if(errorAction != null)
					errorAction(null);
				
				CitrusEventManager.instance.Raise(new CallServerUDIDISNullEvent());
				yield break;
			}
		}

		string jsonstr = Json.Serialize(SendDic);
		WWW www = null;
		bool serverError = false;

		yield return helper.StartCoroutine(EasyNetCall(timeOutTime, baseUrl + functionName,
			(WWW obj) => {
				www = obj;
			},
			(obj) => {
				serverError = true;
			},
			Encoding.UTF8.GetBytes(jsonstr),compress));

		if(serverError)
		{
			Debug.LogError("UniversalNetCall: server error");
			if(errorAction != null)
				errorAction(null);
			yield break;
		}

		JSONObject result = new JSONObject(CheckGzip(www));

		ServerErrorCode errorCode = (ServerErrorCode)result.GetField("error").n;
		if(errorCode == ServerErrorCode.ok)
		{
			if(resultAction != null)
				resultAction.Invoke(result);
		}
		else
		{
			if(errorAction != null)
				errorAction(result);
		}

		yield return null;
	}

	public static IEnumerator EasyNetCall(float timeOutTime, string url, Action<WWW> resultAction, Action<WWW> errorAction = null, byte[] bt = null,bool compress = false)
	{
		WWW www;
		if (bt == null)
		{
			www = new WWW (url);
		}
		else
		{
			if (!compress)
				www = new WWW (url, bt);
			else
			{
				www = new WWW (url, Compress.CompressString(bt), new Dictionary<string,string> () {
					{
						"Accept-Encoding",
						"application/gzip"
					},
					 {
						"Content-Encoding",
						"application/gzip"
					},
					 {
						"content-type",
						"application/json"
					}
				});
			}
		}

		float timer = 0;
		bool timeoutfailed = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > timeOutTime)
			{
				timeoutfailed = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(timeoutfailed || !string.IsNullOrEmpty(www.error))
		{
			Debug.Log("EasyNetCall: error: " + www.error + timeoutfailed);
			if(errorAction != null)
				errorAction(www);
		}
		else
		{
			if(resultAction != null)
				resultAction(www);
		}
		yield break;
	}

	public static IEnumerator GetDownloadingPicture(MonoBehaviour helper, string url, Action<Sprite> reasult, Action<WWW> error = null, float timeout = 10)
	{
		if(url == "")
			yield break;
		
		bool getSuccess = false;
		WWW profilePic = null; //+ "?access_token=" + FB.AccessToken);
		yield return helper.StartCoroutine(NetWorkHelper.EasyNetCall(timeout, url, 
			(WWW obj) =>
			{
				profilePic = obj;
				getSuccess = true; 
			},
			(WWW obj) => {
				if(error != null)
					error(obj);
			}));
		
		if(getSuccess)
		{
			Texture2D texture = profilePic.texture;
			Sprite m_sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
			reasult(m_sprite);
		}
		//Do whatever here
	}

	public  static string CheckGzip(WWW www)
	{
		string result = "";
		bool flag = false;
		if (www.responseHeaders.ContainsKey("Content-Encoding"))
		{
			if (www.responseHeaders ["Content-Encoding"].Contains("gzip"))
			{
				if (www.bytes.Length >=2 && www.bytes [0] == 0x1F && www.bytes [1] == 0x8B)//To check the bytes is gzip or not
				{
					try
					{
						result = Encoding.UTF8.GetString(Compress.Decompress(www.bytes));
						flag = true;
					}
					catch(GZipException e)
					{
						flag = false;
					}
				}
			}

		}
		if(!flag)
			result = www.text;
		return result;
	}
}
