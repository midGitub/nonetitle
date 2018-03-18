#define INCLUDE_VERSION_IN_URL
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using CitrusFramework;

public delegate void MachineAssetDownloadCompleteDelegate(string machineName);
public delegate void MachineAssetDownloadUpdateDelegate(string machineName, float progress);

public class MachineAssetDownloader : MonoBehaviour
{
	private static float _downloadVersionFileTimeout = 60.0f; //short
	private static float _downloadAssetBundleTimeout = 60.0f * 30; //long enough

	private string _machineName;
	private byte[] _tempVersionBytes;
	private MachineAssetDownloadUpdateDelegate _onUpdateCallback;
	private MachineAssetDownloadCompleteDelegate _onSuccessCallback;
	private MachineAssetDownloadCompleteDelegate _onFailCallback;

	#region Public

	public void StartDownloadMachineAsset(string machineName, MachineAssetDownloadUpdateDelegate onUpdate,
		MachineAssetDownloadCompleteDelegate onSuccess, MachineAssetDownloadCompleteDelegate onFail)
	{
		_machineName = machineName;
		_onUpdateCallback = onUpdate;
		_onSuccessCallback = onSuccess;
		_onFailCallback = onFail;

		DownloadMachineVersionFile();
	}

	#endregion

	#region Path convenient methods

	//ConfigUrl(".../slots-debug/machineassets/") + Platform + MachineName + Version + MachineName
	private string GetMachineAssetBundleDownloadUrl(string machineName)
	{
		#if DEBUG
		string result = MapSettingConfig.Instance.MachineAssetUrlDebug;
		#else
		string result = MapSettingConfig.Instance.MachineAssetUrlRelease;
		#endif

		string platform = PlatformManager.Instance.GetPlatformString();

		//for test
		#if UNITY_EDITOR
		platform = "Android";
		#endif

		result = Path.Combine(result, platform);
		result = Path.Combine(result, machineName);
		#if INCLUDE_VERSION_IN_URL
		int version = RemoteMachineVersionConfig.Instance.GetVersion(machineName);
		result = Path.Combine(result, version.ToString());
		#endif
		result = Path.Combine(result, machineName);
		return result;
	}

	//ConfigUrl(".../slots-debug/machineassets/") + Platform + MachineName + Version + MachineName + "_version"
	string GetMachineVersionDownloadUrl(string machineName)
	{
		#if DEBUG
		string result = MapSettingConfig.Instance.MachineAssetUrlDebug;
		#else
		string result = MapSettingConfig.Instance.MachineAssetUrlRelease;
		#endif

		string platform = PlatformManager.Instance.GetPlatformString();

		//for test
		#if UNITY_EDITOR
		platform = "Android";
		#endif

		result = Path.Combine(result, platform);
		result = Path.Combine(result, machineName);
		#if INCLUDE_VERSION_IN_URL
		int version = RemoteMachineVersionConfig.Instance.GetVersion(machineName);
		result = Path.Combine(result, version.ToString());
		#endif
		result = Path.Combine(result, machineName + LiveUpdateConfig._machineVersionFileSuffix);
		return result;
	}

	private void MakeMachineAssetDir()
	{
		MachineAssetManager.Instance.MakeSaveRootDir();

		string path = MachineAssetManager.Instance.GetMachineAssetDir(_machineName);
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
	}

	#endregion

	#region Download Version File

	void DownloadMachineVersionFile()
	{
		string versionUrl = GetMachineVersionDownloadUrl(_machineName);
		Debug.Log("Download machine version url:" + versionUrl);
		StartCoroutine(LiveUpdateHelper.StartNetworkCoroutine(versionUrl, null, _downloadVersionFileTimeout, 
			null, VersionDownloadSuccessCallback, VersionDownloadFailCallback));
	}

	void VersionDownloadSuccessCallback(WWW www, string url)
	{
		Debug.Log("Success to download machine version");

		//record bytes here but not write to file until the AssetBundle is downloaded
		_tempVersionBytes = www.bytes;

		DownloadMachineAssetBundle();
	}

	void VersionDownloadFailCallback(WWW www, string url)
	{
		Debug.Log("Fail to download machine version file");

		if(_onFailCallback != null)
			_onFailCallback(_machineName);
	}

	#endregion

	#region Download Machine AssetBundle

	void DownloadMachineAssetBundle()
	{
		string url = GetMachineAssetBundleDownloadUrl(_machineName);
		Debug.Log("Download machine AssetBundle url:" + url);
		StartCoroutine(LiveUpdateHelper.StartNetworkCoroutine(url, null, _downloadAssetBundleTimeout, 
			ResourceInfoDownloadUpdateCallback, ResourceInfoDownloadSuccessCallback, ResourceInfoDownloadFailCallback));
	}

	void ResourceInfoDownloadUpdateCallback(WWW www, string url)
	{
		if(_onUpdateCallback != null)
			_onUpdateCallback(_machineName, www.progress);
	}

	void ResourceInfoDownloadSuccessCallback(WWW www, string url)
	{
		Debug.Log("Success to download ResourceInfo");

		MakeMachineAssetDir();

		string versionFilePath = MachineAssetManager.Instance.GetMachineAssetVersionFilePath(_machineName);
		File.WriteAllBytes(versionFilePath, _tempVersionBytes);

		string assetFilePath = MachineAssetManager.Instance.GetMachineAssetBundleFilePath(_machineName);
		File.WriteAllBytes(assetFilePath, www.bytes);

		MachineAssetManager.Instance.SaveDownloadedMachine(_machineName);

		if(_onSuccessCallback != null)
			_onSuccessCallback(_machineName);
	}

	void ResourceInfoDownloadFailCallback(WWW www, string url)
	{
		Debug.Log("Fail to download machine AssetBundle");

		if(_onFailCallback != null)
			_onFailCallback(_machineName);
	}

	#endregion
}
