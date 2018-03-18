using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using CitrusFramework;

public enum LiveUpdateState
{
	None,
	Updating,
	Success,
	Fail,
	Count
}

public class BundleInfo
{
	public string _resourceVersion;
	public Dictionary<string, string> _hashDict;
	public List<string> AssetBundleNames
	{
		get { return new List<string>(_hashDict.Keys); }
	}

	public BundleInfo(string resourceVersion, Dictionary<string, string> hashDict)
	{
		_resourceVersion = resourceVersion;
		_hashDict = hashDict;
	}
}

public class LiveUpdateManager : Singleton<LiveUpdateManager>
{
	private static string _versionUrl = ServerConfig.GameServerUrl;
	private static string _fetchVersionPath = "get_version";
	private static string _resSaveDir = "LiveUpdate";
	private static string _liveUpdateVersionFileName = "LiveUpdateVersion";
	private static string _abTestVersionFileName = "ABTestVersion";

	private string _assetMapText;
	private Dictionary<string, string> _assetMapDict = new Dictionary<string, string>();
	private string _bundleInfoText;
	private BundleInfo _bundleInfo;
	private Dictionary<string, byte[]> _assetBundleBytesDict;

	private LiveUpdateState _state = LiveUpdateState.None;
	private bool _hasDownloadedAssetBundle; //if there is assetBundle in local or just downloaded now 
	private string _downloadUrl = "";
	private string _fetchedVersion;
	private string _fetchedABVersion;

	private string _liveUpdateResourceVersion;
	private string _abTestResourceVersion;

	public event Callback LiveUpdateSuccessEvent = delegate {};
	public event Callback LiveUpdateFailEvent = delegate {};

	public Dictionary<string, string> AssetMapDict { get { return _assetMapDict; } }
	public BundleInfo BundleInfo { get { return _bundleInfo; } }
	public LiveUpdateState State { get { return _state; } }

	#region Init

	public void Init()
	{
		LoadAllFiles();
	}

	#endregion

	#region Fetch version

	private void FetchVersion()
	{
		string url = GetVersionUrl();
		Dictionary<string, string> dict = new Dictionary<string, string>();
		dict.Add("ProjectName", BuildUtility.GetProjectName());
		dict.Add("MainVersion", BuildUtility.GetBundleMajorMinorVersion());
		dict.Add("DeviceId", DeviceUtility.GetDeviceId());

		#if UNITY_EDITOR
		string platform = "android";
		#else
		string platform = PlatformManager.Instance.GetPlatformString().ToLower();
		#endif

		//The requirement on server is special, so differentiate it here
		#if USE_iOS_IW
		dict.Add("Platform", PlatformManager.Instance.GetChannelString().ToLower());
		#else
		dict.Add("Platform", platform);
		#endif

		string postData = MiniJSON.Json.Serialize(dict);
		byte[] bytes = Encoding.UTF8.GetBytes(postData);

		StartCoroutine(LiveUpdateHelper.StartNetworkCoroutine(url, bytes, 10.0f, null,
			VersionFetchSuccessCallback, VersionFetchFailCallback));
	}

	private void VersionFetchSuccessCallback(WWW www, string url)
	{
		Debug.Log("Version fetch success");

		//first, make sure directory exists
		string saveRootPath = GetLiveUpdateRootDir();
		if(!Directory.Exists(saveRootPath))
			Directory.CreateDirectory(saveRootPath);

		Dictionary<string, object> dict = MiniJSON.Json.Deserialize(www.text) as Dictionary<string, object>;
		if(dict != null && dict.ContainsKey("error"))
		{
			int errorCode = Convert.ToInt32(dict["error"]);
			if(errorCode == 0)
			{
				if(dict.ContainsKey("MainVersion") && dict.ContainsKey("SubVersion") && dict.ContainsKey("BaseUrl"))
				{
					_fetchedVersion = string.Format("{0}.{1}", dict["MainVersion"] as string, dict["SubVersion"] as string);

					if(dict.ContainsKey("AbVersion"))
					{
						_fetchedABVersion = dict["AbVersion"] as string;
					}
					else
					{
						//when no AB test, set ABTestResourceVersion to empty, to disable AB test
						SetABTestResourceVersion("");
					}
					
					_downloadUrl = dict["BaseUrl"] as string;

					Debug.Log("fetchedVersion: " + _fetchedVersion + ", fetchedABVersion: " + _fetchedABVersion);

					if(ShouldDownloadResource(_fetchedVersion))
					{
						DownloadResourceInfo();
					}
					else
					{
						Debug.Log("No need to live update");
						LiveUpdateSuccessCallback();
					}
				}
				else
				{
					LiveUpdateSuccessCallback();
				}
			}
			else
			{
				LiveUpdateFailCallback();
			}
		}
		else
		{
			Debug.Assert(false);
			LiveUpdateFailCallback();
		}
	}

	private bool ShouldDownloadResource(string remoteResourceVersion)
	{
		string bundleVersion = BuildUtility.GetBundleVersion();
		string curResVersion = GetResourceVersion();
		bool result = VersionUtility.CanVersionUpdate(bundleVersion, remoteResourceVersion)
			&& VersionUtility.IsVersionHigher(curResVersion, remoteResourceVersion);
		return result;
	}

	private void VersionFetchFailCallback(WWW www, string url)
	{
		Debug.Log("Version fetch fail");

		LiveUpdateFailCallback();
	}

	private string GetVersionUrl()
	{
		string result = Path.Combine(_versionUrl, _fetchVersionPath);
		return result;
	}

	private bool ShouldUseResourceVersion(string resVersion)
	{
		string bundleVersion = BuildUtility.GetBundleVersion();
		bool result = VersionUtility.CanVersionUpdate(bundleVersion, resVersion);
		return result;
	}

	#endregion

	#region Download coroutine

	private string GetResourceDownloadUrl(string path)
	{
		string majorMinorVersion = BuildUtility.GetBundleMajorMinorVersion();

		string toServerVersion;
		if(!string.IsNullOrEmpty(_fetchedABVersion))
			toServerVersion = majorMinorVersion + "." + _fetchedABVersion;
		else
			toServerVersion = majorMinorVersion + ".0";
		
		string platform = PlatformManager.Instance.GetPlatformString();

		//for test
		#if UNITY_EDITOR
		platform = "Android";
		#endif

		string result = Path.Combine(Path.Combine(_downloadUrl, toServerVersion), platform);
		result = Path.Combine(result, path);
		return result;
	}

	#endregion

	#region Parse and Load

	private Dictionary<string, string> ParseAssetMapDict(string text)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		using(var reader = new StringReader(text))
		{
			string line = "";
			while(true)
			{
				line = reader.ReadLine();
				if(!string.IsNullOrEmpty(line))
				{
					var contents = line.Split(',');
					if(contents.Length <= 1)
						break;
					else
						result.Add(contents[0], contents[1]);
				}
				else
					break;
			}

			reader.Close();
		}
		return result;
	}

	private BundleInfo ParseBundleInfo(string text)
	{
		string resourceVersion = BuildUtility.GetBundleVersion();
		Dictionary<string, string> hashDict = new Dictionary<string, string>();
		using(var reader = new StringReader(text))
		{
			string line = reader.ReadLine();
			if(!string.IsNullOrEmpty(line))
			{
				resourceVersion = line;
				while(true)
				{
					line = reader.ReadLine();
					if(!string.IsNullOrEmpty(line))
					{
						var contents = line.Split(',');
						hashDict.Add(contents[0], contents[1]);
					}
					else
						break;
				}
			}
			else
			{
				Debug.Assert(false);
			}

			reader.Close();
		}

		Debug.Assert(hashDict.Count > 0);
		BundleInfo result = new BundleInfo(resourceVersion, hashDict);

		return result;
	}

	private void LoadAllFiles()
	{
		string filePath;
		string saveRootPath = GetLiveUpdateRootDir();
		bool isAssetMapExist = false, isBundleInfoExist = false;

		//0.abTestVersion
		filePath = Path.Combine(saveRootPath, _abTestVersionFileName);
		if(File.Exists(filePath))
		{
			_abTestResourceVersion = File.ReadAllText(filePath);
		}

		//0.liveUpdateVersion
		filePath = Path.Combine(saveRootPath, _liveUpdateVersionFileName);
		if(File.Exists(filePath))
		{
			_liveUpdateResourceVersion = File.ReadAllText(filePath);
		}

		//1 assetMap
		string resourceSavePath = GetResourceSaveDir();
		filePath = Path.Combine(resourceSavePath, LiveUpdateConfig._assetMapFileName);
		if(File.Exists(filePath))
		{
			_assetMapText = File.ReadAllText(filePath);
			_assetMapDict = ParseAssetMapDict(_assetMapText);
			isAssetMapExist = true;
		}

		//2 bundleInfo
		filePath = Path.Combine(resourceSavePath, LiveUpdateConfig._bundleInfoFileName);
		if(File.Exists(filePath))
		{
			_bundleInfoText = File.ReadAllText(filePath);
			_bundleInfo = ParseBundleInfo(_bundleInfoText);
			isBundleInfoExist = true;
		}

		Debug.Assert((isAssetMapExist && isBundleInfoExist) || (!isAssetMapExist && !isBundleInfoExist));
		_hasDownloadedAssetBundle = (isAssetMapExist && isBundleInfoExist);
	}

	#endregion

	#region Download

	private void DownloadResourceInfo()
	{
		Debug.Log("Start download resourceInfo");

		string url = GetResourceDownloadUrl(LiveUpdateConfig._resourceInfoFileName);
		StartCoroutine(LiveUpdateHelper.StartNetworkCoroutine(url, null, 10.0f, null,
			ResourceInfoDownloadSuccessCallback, ResourceInfoDownloadFailCallback));
	}

	private void ResourceInfoDownloadSuccessCallback(WWW www, string url)
	{
		Debug.Log("Success to download ResourceInfo");

		//safe checks to resolve crash on Fabric
		if(www != null)
		{
			if(www.assetBundle != null)
			{
				AssetBundle bundle = www.assetBundle;

				_assetMapText = bundle.LoadAsset<TextAsset>(LiveUpdateConfig._assetMapFileName).text;
				_assetMapDict = ParseAssetMapDict(_assetMapText);

				_bundleInfoText = bundle.LoadAsset<TextAsset>(LiveUpdateConfig._bundleInfoFileName).text;
				_bundleInfo = ParseBundleInfo(_bundleInfoText);

				DownloadAssetBundles();
			}
			else
			{
				Debug.LogError("www.assetBundle is null");
				Debug.Assert(false);

				LiveUpdateFailCallback();
			}
		}
		else
		{
			Debug.LogError("www is null");
			Debug.Assert(false);

			LiveUpdateFailCallback();
		}
	}

	private void ResourceInfoDownloadFailCallback(WWW www, string url)
	{
		Debug.Log("Fail to download ResourceInfo");

		LiveUpdateFailCallback();
	}

	private void DownloadAssetBundles()
	{
		//todo: check if there is already assetBundles in local device. If yes, don't download again
		_assetBundleBytesDict = new Dictionary<string, byte[]>();
		int downloadCount = _bundleInfo.AssetBundleNames.Count;
		List<string> names = _bundleInfo.AssetBundleNames;

		for(int i = 0; i < names.Count; i++)
		{
			string name = names[i];
			DownloadCompleteDelegate onSuccessHandler = (WWW www, string path) => {
				Debug.Log("Success to download one assetBundle: " + name);
				_assetBundleBytesDict.Add(name, www.bytes);
				--downloadCount;
				if(downloadCount == 0)
					AllAssetBundlesDownloadSuccessCallback();
			};
			DownloadCompleteDelegate onFailHandler = (WWW www, string path) => {
				Debug.Log("Fail to download one assetBundle: " + name);
				AllAssetBundlesDownloadFailCallback();
			};

			string url = GetResourceDownloadUrl(name);
			StartCoroutine(LiveUpdateHelper.StartNetworkCoroutineRepeated(this, url, null, 10.0f, 
				null, onSuccessHandler, onFailHandler, 3));
		}
	}

	private void AllAssetBundlesDownloadSuccessCallback()
	{
		Debug.Log("All assetBundles are downloaded");

		SaveAllFiles();

		LogUtility.Log("Use abTestVersion: " + _abTestResourceVersion + ", liveUpdateVersion: " + _liveUpdateResourceVersion, Color.red);
	}

	private void AllAssetBundlesDownloadFailCallback()
	{
		_assetBundleBytesDict.Clear();

		LiveUpdateFailCallback();
	}

	private void SaveAllFiles()
	{
		string filePath;
		string saveRootPath = GetLiveUpdateRootDir();
		if(!Directory.Exists(saveRootPath))
			Directory.CreateDirectory(saveRootPath);

		//0 versionFile
		if(IsABTestResource(_bundleInfo))
		{
			SetABTestResourceVersion(_bundleInfo._resourceVersion);
		}
		else
		{
			SetLiveUpdateResourceVersion(_bundleInfo._resourceVersion);
		}

		//1 assetMap
		string resourceSavePath = GetResourceSaveDir();
		if(!Directory.Exists(resourceSavePath))
			Directory.CreateDirectory(resourceSavePath);
		
		filePath = Path.Combine(resourceSavePath, LiveUpdateConfig._assetMapFileName);
		File.WriteAllText(filePath, _assetMapText);

		//2 bundleInfo
		filePath = Path.Combine(resourceSavePath, LiveUpdateConfig._bundleInfoFileName);
		File.WriteAllText(filePath, _bundleInfoText);

		//3 assetBundles
		foreach(var pair in _assetBundleBytesDict)
		{
			string path = Path.Combine(resourceSavePath, pair.Key);
			File.WriteAllBytes(path, pair.Value);
		}
		//clear it since it's useless
		_assetBundleBytesDict.Clear();

		_hasDownloadedAssetBundle = true;

		ReloadExcelAssets();

		LiveUpdateSuccessCallback();
	}

	private void LiveUpdateSuccessCallback()
	{
		_state = LiveUpdateState.Success;

		LiveUpdateSuccessEvent();
	}

	private void LiveUpdateFailCallback()
	{
		_state = LiveUpdateState.Fail;

		LiveUpdateFailEvent();
	}

	private string GetLiveUpdateRootDir()
	{
		string result = Path.Combine(Application.persistentDataPath, _resSaveDir);
		return result;
	}

	private void SetLiveUpdateResourceVersion(string version)
	{
		string saveRootPath = GetLiveUpdateRootDir();
		_liveUpdateResourceVersion = version;
		string filePath = Path.Combine(saveRootPath, _liveUpdateVersionFileName);
		File.WriteAllText(filePath, _liveUpdateResourceVersion);
	}

	void ReloadExcelAssets()
	{
		//1 CoreSetting
		if(CoreConfig.HasInstance())
		{
			bool hasCoreSetting = false;
			foreach(var pair in	_assetMapDict)
			{
				string assetName = Path.GetFileName(pair.Key);
				if(CoreConfig.Instance.IsCoreSettingAsset(assetName))
				{
					hasCoreSetting = true;
					break;
				}
			}

			if(hasCoreSetting)
				CoreConfig.Instance.Reload();
		}

		//2 GameSetting
		foreach(var pair in _assetMapDict)
		{
			string assetName = Path.GetFileName(pair.Key);
			ReloadInfo info = GameConfigTable.GetReloadInfo(assetName);
			if(info != null)
			{
				if(info._shouldReloadHandler())
					info._reloadHandler();
			}
		}
	}

	#endregion

	#region AB Test

	private bool IsABTestResource(BundleInfo info)
	{
		string[] array = info._resourceVersion.Split('.');
		bool result = (array.Length == 4);
		return result;
	}

	private void SetABTestResourceVersion(string version)
	{
		string saveRootPath = GetLiveUpdateRootDir();
		_abTestResourceVersion = version;
		string filePath = Path.Combine(saveRootPath, _abTestVersionFileName);
		File.WriteAllText(filePath, _abTestResourceVersion);
	}

	#endregion

	#region Public

	public string GetResourceSaveDir()
	{
		string path = Path.Combine(Application.persistentDataPath, _resSaveDir);
		string resourceVersion = GetResourceVersion();
		path = Path.Combine(path, resourceVersion);
		return path;
	}

	public string GetResourceVersion()
	{
		string result = BuildUtility.GetBundleVersion();
		if(!string.IsNullOrEmpty(_abTestResourceVersion))
			result = _abTestResourceVersion;
		else if(!string.IsNullOrEmpty(_liveUpdateResourceVersion))
			result = _liveUpdateResourceVersion;
		return result;
	}

	public void StartLiveUpdate()
	{
		if(DeviceUtility.IsConnectInternet())
		{
			_state = LiveUpdateState.Updating;
			Debug.Log("Start live update");
			FetchVersion();
		}
		else
		{
			Debug.Log("No network, skip live update");
			LiveUpdateFailCallback();
		}
	}

	public bool IsResourceAvailable()
	{
		bool result = false;
		if(_hasDownloadedAssetBundle)
		{
			string resVersion = GetResourceVersion();
			result = ShouldUseResourceVersion(resVersion);
		}
		return result;
	}

	public string GetUsingResourceVersion()
	{
		string result = BuildUtility.GetBundleVersion();
		if(IsResourceAvailable())
			result = GetResourceVersion();
		return result;
	}

	#endregion

	#region check func

	public bool HasResourceVersionFolder(string preVersion){
		string rootDir = GetLiveUpdateRootDir ();
		if (!Directory.Exists (rootDir))
			return false;

		DirectoryInfo dir = new DirectoryInfo (rootDir);
		DirectoryInfo[] dirInfos = dir.GetDirectories (preVersion);
		return dirInfos.Length > 0;
	}

	#endregion
}
