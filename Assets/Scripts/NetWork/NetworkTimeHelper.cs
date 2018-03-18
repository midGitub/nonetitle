//#define DEBUG_SERVER_TIME
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CitrusFramework;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.Purchasing;

public class NetworkTimeHelper : Singleton<NetworkTimeHelper>
{
    public event Action ServerTimeHasGottenEvent;

	private string _serverTime = "";
	public bool IsGettingServerTime { private set; get; }
	public bool IsServerTimeGetted { get { return _serverTime != ""; } }

	private double ntpTimeOffset = 0;

#if DEBUG || DEBUG_SERVER_TIME
    private float _addDebugHours = 0f;

    public float AddDebguHours
    {
        set { _addDebugHours = value; }
        get { return _addDebugHours;}
    }
#endif

	void Awake()
	{
		IsGettingServerTime = false;
	}

	void Start()
	{
		//string sTime = "Thu Mar 30 2017 06:12:39 GMT+0000 (UTC)";
		//DateTime dt = DateTime.ParseExact(sTime, "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (UTC)'", new CultureInfo("en-GB"));
		//Debug.Log(dt);
	}

	public void CheckServerSystemTime(Action<bool> updateSuccess = null){
		if(_serverTime == "" && !IsGettingServerTime &&
			Application.internetReachability != NetworkReachability.NotReachable)
		{
			//Debug.Log("重新获得服务器时间");
			StartCoroutine(GetServerSystemTime(updateSuccess));
		}
	}

	private void Update()
	{
		CheckServerSystemTime ();
	}

	/// <summary>
	/// 更新现在的时间
	/// </summary>
	/// <param name="updateSuccess">Update success.</param>
	public void UpdateServerTime(Action<bool> updateSuccess)
	{
		StartCoroutine(GetServerSystemTime(updateSuccess));
	}

	IEnumerator GetServerSystemTime(Action<bool> gettingsuccess = null)
	{
		if(IsGettingServerTime)
		{
			yield break;
		}

		IsGettingServerTime = true;
		//#if UNITY_EDITOR
		//		_serverTime = DateTime.Now.ToString();
		//		ntpTimeOffset = 0;
		//		IsGettingServerTime = false;
		//		if(gettingsuccess != null)
		//		{
		//			gettingsuccess(true);
		//		}
		//		//LogUtility.Log("现在时间" + DateTime.Now.AddSeconds(-1.0f * ntpTimeOffset), Color.cyan);
		//		yield break;
		//#endif

		// string timeURL = "http://slots-game.citrusjoy.com:8063/time";

		string timeURL = "https://slots-game.citrusjoy.com/time";
		//#if UNITY_EDITOR
		//		timeURL = "http://192.168.3.231:8063/time";
		//#endif

		WWW www = new WWW(timeURL);

		float timer = 0;
		float timeOut = 6f;
		bool failed = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > timeOut) { failed = true; break; }
			timer += Math.Min(.1f,Time.unscaledDeltaTime);
			yield return null;
		}

		if(failed || !string.IsNullOrEmpty(www.error))
		{
			_serverTime = "";
			Debug.Log("GetServerSystemTime: error:" + www.error);
			IsGettingServerTime = false;
			if(gettingsuccess != null)
			{
				gettingsuccess(false);
			}
		}
		else
		{
			var result = new JSONObject(www.text);
			var doublesTime = result.GetField("Timestamp").n;
			Debug.Log(result);
			//DateTime dt = DateTime.ParseExact(sTime, "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (UTC)'", new CultureInfo("en-GB"));
			DateTime dt = ConvertIntDateTime(doublesTime);
			_serverTime = dt.ToString();
			ntpTimeOffset = (DateTime.UtcNow - dt).TotalSeconds;
			//Debug.Log("Local UTC time:" + DateTime.UtcNow);
			//Debug.Log("Server UTC time:" + _serverTime);
			IsGettingServerTime = false;
			if(gettingsuccess != null)
				gettingsuccess(true);

			Debug.Log("Current network time:" + DateTime.Now.AddSeconds(-1.0f * ntpTimeOffset));
            if (ServerTimeHasGottenEvent != null)
                ServerTimeHasGottenEvent();
		}
	}

	public System.DateTime ConvertIntDateTime(double d)
	{
		System.DateTime time = new System.DateTime(1970, 1, 1);
		time = time.AddMilliseconds(d);
		return time;
	}

	/// <summary>
	/// 得到时间 true为当前时间为服务器精确时间,false为本地时间
	/// </summary>
	/// <param name="canGetServerTimeFunction">Can get server time function.</param>
	public DateTime GetNowTime(Action<bool, DateTime> canGetServerTimeFunction = null)
	{
		DateTime localNow = DateTime.Now;
		localNow = HandleAddDebugHours(localNow);

		if(_serverTime == "")
		{
			if(canGetServerTimeFunction != null)
			{
				canGetServerTimeFunction(false, localNow);
			}
			return localNow;
		}
		else
		{
			try
			{
				DateTime now = localNow.AddSeconds(-1.0f * ntpTimeOffset);
				if(canGetServerTimeFunction != null)
				{
					canGetServerTimeFunction(true, now);
				}
				return now;
			}
			catch(Exception ex)
			{
				Debug.LogError(ex);
				if(canGetServerTimeFunction != null)
				{
					canGetServerTimeFunction(false, localNow);
				}
				return localNow;
			}
		}
	}

	DateTime HandleAddDebugHours(DateTime time)
	{
        #if DEBUG || DEBUG_SERVER_TIME
        return time.AddHours(_addDebugHours);
		#else
		return time;
		#endif
	}

	public void AddDebugHours(float hours)
	{
        #if DEBUG || DEBUG_SERVER_TIME
        _addDebugHours += hours;
        LogUtility.Log("Current Server Time : " + GetNowTime(), Color.yellow);
		#endif
	}
}
