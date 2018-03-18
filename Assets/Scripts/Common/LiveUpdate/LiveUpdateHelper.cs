using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public delegate void DownloadCompleteDelegate(WWW www, string path);
public delegate void DownloadUpdateDelegate(WWW www, string path);

public static class LiveUpdateHelper
{
	public static IEnumerator StartNetworkCoroutine(string url, byte[] postData, float timeout,
		DownloadUpdateDelegate onUpdate, DownloadCompleteDelegate onSuccess, DownloadCompleteDelegate onFail)
	{
		float timer = 0;
		bool isFail = false;
		WWW www = null;
		if(postData != null)
			www = new WWW(url, postData);
		else
			www = new WWW(url);

		while(!www.isDone)
		{
			if(timer > timeout)
			{
				isFail = true;
				break;
			}
			timer += Time.deltaTime;

			if(onUpdate != null)
				onUpdate(www, url);
			
			yield return null;
		}

		if(isFail)
		{
			www.Dispose();
		}
		else
		{
			if(!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError("Error to connect: " + url + ", " + www.error);
				isFail = true;
			}
			else if(string.IsNullOrEmpty(www.text))
			{
				Debug.LogError("www.text is null");
				isFail = true;
			}
		}

		if(isFail)
		{
			if(onFail != null)
				onFail(www, url);
		}
		else
		{
			Debug.Log("Success to connect " + url);
			if(onSuccess != null)
				onSuccess(www, url);
		}
	}

	public static IEnumerator StartNetworkCoroutineRepeated(MonoBehaviour obj, string url, byte[] postData, float timeout, 
		DownloadUpdateDelegate onUpdate, DownloadCompleteDelegate onSuccess, DownloadCompleteDelegate onFail, int tryCount)
	{
		yield return obj.StartCoroutine(StartNetworkCoroutine(url, postData, timeout, onUpdate, onSuccess, (WWW www, string p) => {
			if(--tryCount > 0)
				StartNetworkCoroutineRepeated(obj, url, postData, timeout, onUpdate, onSuccess, onFail, tryCount);
			else
				onFail(www, p);
		}));
	}
}
