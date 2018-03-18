using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CitrusFramework;

public class MachineAssetDownloadCallback
{
	public MachineAssetDownloadUpdateDelegate _onUpdate;
	public MachineAssetDownloadCompleteDelegate _onSuccess;
	public MachineAssetDownloadCompleteDelegate _onFail;

	public MachineAssetDownloadCallback(MachineAssetDownloadUpdateDelegate onUpdate, 
		MachineAssetDownloadCompleteDelegate onSuccess, MachineAssetDownloadCompleteDelegate onFail)
	{
		_onUpdate = onUpdate;
		_onSuccess = onSuccess;
		_onFail = onFail;
	}
}

public class MachineAssetDownloaderManager : Singleton<MachineAssetDownloaderManager>
{
	Dictionary<string, MachineAssetDownloader> _downloaderDict = new Dictionary<string, MachineAssetDownloader>();
	Dictionary<string, MachineAssetDownloadCallback> _callbackDict = new Dictionary<string, MachineAssetDownloadCallback>();

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
	
	}

	public void Init()
	{
	}

	public void InitDownloader(string machineName)
	{
		if(!_downloaderDict.ContainsKey(machineName))
		{
			MachineAssetDownloader downloader = this.gameObject.AddComponent<MachineAssetDownloader>();
			_downloaderDict.Add(machineName, downloader);
		}
	}

//	public MachineAssetDownloader GetDownloader(string machineName)
//	{
//		MachineAssetDownloader result = null;
//		_downloaderDict.TryGetValue(machineName, out result);
//		return result;
//	}

	public void StartDownloadMachineAsset(string machineName, MachineAssetDownloadUpdateDelegate onUpdate,
		MachineAssetDownloadCompleteDelegate onSuccess, MachineAssetDownloadCompleteDelegate onFail)
	{
		InitDownloader(machineName);

		_callbackDict[machineName] = new MachineAssetDownloadCallback(onUpdate, onSuccess, onFail);

		_downloaderDict[machineName].StartDownloadMachineAsset(machineName,
			(string m, float progress) => {
				MachineAssetDownloadCallback callback = null;
				_callbackDict.TryGetValue(m, out callback);
				if(callback != null)
					callback._onUpdate(m, progress);
			},
			(string m) => {
				MachineAssetDownloadCallback callback = null;
				_callbackDict.TryGetValue(m, out callback);
				if(callback != null)
					callback._onSuccess(m);
			},
			(string m) => {
				MachineAssetDownloadCallback callback = null;
				_callbackDict.TryGetValue(m, out callback);
				if(callback != null)
					callback._onFail(m);
			}
		);
	}

	public void UnregisterDownloadCallbacks(string machineName)
	{
		if(_callbackDict != null && !string.IsNullOrEmpty(machineName))
			_callbackDict.Remove(machineName);
	}
}

