using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class SendMessageNetWork
{
	public static IEnumerator crFeedback(string Title, string l1,string l2,string l3,string Email)
	{
		Dictionary<string, object> dic = new Dictionary<string, object>();
		string udid = UserBasicData.Instance.UDID;
		string title = "Slots_" + udid + "_" + NetworkTimeHelper.Instance.GetNowTime() + "_" + Title;
		dic.Add("title", title);
		dic.Add("udid", udid);
		Dictionary<string, string> cDic = new Dictionary<string, string>();
		cDic.Add("l1", l1);
		cDic.Add("l2", l2);
		cDic.Add("l3", l3);
		dic.Add("content",MiniJSON.Json.Serialize(cDic));
		dic.Add("channel", PlatformManager.Instance.GetServerChannelString());

        Dictionary<string, string> miscDic = new Dictionary<string, string>();
		// miscDic.Add("mlv", UserStatusManager.Instance.GetClearedLevel().ToString());
		miscDic.Add("mlv", "0");
		miscDic.Add("BundleVersion", BuildUtility.GetBundleVersion());
		miscDic.Add("ResourceVersion",LiveUpdateManager.Instance.GetResourceVersion());
		miscDic.Add("DeviceID", DeviceUtility.GetDeviceId());
		miscDic.Add("Email", Email);
		miscDic.Add("Payamount", UserBasicData.Instance.TotalPayAmount.ToString());

		#if DEBUG
		miscDic.Add("isDebug", "true");
		#endif

        dic.Add("misc", MiniJSON.Json.Serialize(miscDic));

		string jsonStr = MiniJSON.Json.Serialize(dic);
		Debug.Log(jsonStr);
		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers.Add("Content-Type", "application/json");
		WWW www = new WWW(ServerConfig.FeedbackServerUrl, Encoding.UTF8.GetBytes(jsonStr), headers);
		float timeOut = 5.0f;
		float timer = 0;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > timeOut) { yield break; }
			timer += Time.deltaTime;
			yield return null;
		}

		if(string.IsNullOrEmpty(www.error))
		{
			Debug.Log("Send message successfully ");
		}
		else {
			Debug.Log(www.error);
		}

	}
}
