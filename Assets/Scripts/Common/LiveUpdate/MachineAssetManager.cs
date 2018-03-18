using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using CitrusFramework;

public class MachineAssetManager : Singleton<MachineAssetManager>
{
	private static readonly string _assetSaveDir = "MachineAsset";
	private static readonly string _downloadedMachinesFileName = "DownloadedMachines";

	private List<string> _downloadedMachines = new List<string>();
	private Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();
	private Dictionary<string, bool> _assetMatchedDict = new Dictionary<string, bool>();

	#region Init

	public void Init()
	{
		MakeSaveRootDir();
		LoadDownloadedMachines();
		InitAssetMatchDict();
	}

	#endregion

	#region AssetMatchDict

	void InitAssetMatchDict()
	{
		foreach(var m in CoreDefine.AllMachineNames)
		{
			if(IsRemoteAssetMachine(m))
				RefreshMachineAssetMatchDict(m);
		}
	}

	void RefreshMachineAssetMatchDict(string machineName)
	{
		_assetMatchedDict[machineName] = IsMachineDownloadedAndMatchedRuntime(machineName);
	}

	#endregion

	#region Path

	//return: persistentDataPath + "MachineAsset"
	public string GetSaveRootDir()
	{
		string result = Path.Combine(Application.persistentDataPath, _assetSaveDir);
		return result;
	}

	//return: persistentDataPath + "MachineAsset" + machineName
	public string GetMachineAssetDir(string machineName)
	{
		string result = MachineAssetManager.Instance.GetSaveRootDir();
		result = Path.Combine(result, machineName);
		return result;
	}

	//return: persistentDataPath + "MachineAsset" + machineName + machineName
	public string GetMachineAssetBundleFilePath(string machineName)
	{
		string result = GetMachineAssetDir(machineName);
		result = Path.Combine(result, machineName);
		return result;
	}

	//return: persistentDataPath + "MachineAsset" + machineName + machineName + "_version"
	public string GetMachineAssetVersionFilePath(string machineName)
	{
		string result = GetMachineAssetDir(machineName);
		result = Path.Combine(result, machineName + LiveUpdateConfig._machineVersionFileSuffix);
		return result;
	}

	public void MakeSaveRootDir()
	{
		string path = GetSaveRootDir();
		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);
	}

	#endregion

	#region Downloaded Machines

	string GetRecordMachineFilePath()
	{
		string path = GetSaveRootDir();
		path = Path.Combine(path, _downloadedMachinesFileName);
		return path;
	}

	void LoadDownloadedMachines()
	{
		string filePath = GetRecordMachineFilePath();
		if(File.Exists(filePath))
		{
			string text = File.ReadAllText(filePath);
			if(!string.IsNullOrEmpty(text))
			{
				string[] machines = StringUtility.ParseToArray<string>(text);
				_downloadedMachines = new List<string>(machines);
			}
		}
	}

	public void SaveDownloadedMachine(string machineName)
	{
		if(!ListUtility.IsContainElement(_downloadedMachines, machineName))
			_downloadedMachines.Add(machineName);
		string filePath = GetRecordMachineFilePath();
		string text = string.Join(",", _downloadedMachines.ToArray());
		File.WriteAllText(filePath, text);

		RefreshMachineAssetMatchDict(machineName);
	}

	bool IsMachineDownloadedAndMatchedRuntime(string machineName)
	{
		bool result = ListUtility.IsContainElement(_downloadedMachines, machineName);
		if(result)
		{
			//more strict check conditions
			string filePath = GetMachineAssetBundleFilePath(machineName);
			bool isExist = File.Exists(filePath);
			if(isExist)
			{
				result = IsMachineVersionMatched(machineName);
				//Debug.Log("MachineAssetBundleFilePath = " + filePath + ", is Exist: " + isExist + ", version matched:" + result.ToString());
			}
		}
		return result;
	}

	bool IsMachineDownloadedAndMatchedRuntimeFromDict(string machineName)
	{
		bool result = false;
		if(_assetMatchedDict.ContainsKey(machineName))
			result = _assetMatchedDict[machineName];
		return result;
	}

	int GetCurrentMachineVersion(string machineName)
	{
		int result = 0;
		string filePath = GetMachineAssetVersionFilePath(machineName);
		if(File.Exists(filePath))
		{
			string[] lines = File.ReadAllLines(filePath);
			Debug.Assert(lines.Length >= 1);
			int.TryParse(lines[0], out result);
		}
		return result;
	}

	bool IsMachineVersionMatched(string machineName)
	{
		int currentVersion = GetCurrentMachineVersion(machineName);
		int configVersion = RemoteMachineVersionConfig.Instance.GetVersion(machineName);
		bool result = (currentVersion == configVersion);
		return result;
	}

	public bool IsMachineDownloadedAndMatched(string machineName)
	{
		#if UNITY_EDITOR
		if(GetLoadButtonFlag())
			return IsMachineDownloadedAndMatchedRuntimeFromDict(machineName);
		else
			return true;
		#else
		return IsMachineDownloadedAndMatchedRuntimeFromDict(machineName);
		#endif
	}

	#endregion

	#region Button In Editor

	#if UNITY_EDITOR

	const string _loadButtonName = "Tools/Load Downloaded MachineAssets";

	static bool GetLoadButtonFlag()
	{
		return EditorPrefs.GetBool(_loadButtonName, false);
	}

	static void SetLoadButtonFlag(bool flag)
	{
		EditorPrefs.SetBool(_loadButtonName, flag);
	}

	[InitializeOnLoadMethod]
	static void InitLoadAssetMode()
	{
		EditorApplication.delayCall += () => {
			//Debug.Log("Refresh LoadButton in delayCall  Editor");
			RefreshLoadAssetMode();
		};
	}

	static void RefreshLoadAssetMode()
	{
		bool flag = GetLoadButtonFlag();
		Menu.SetChecked(_loadButtonName, flag);
	}

	[MenuItem(_loadButtonName)]
	static void SetLoadAssetMode()
	{
		bool flag = GetLoadButtonFlag();
		flag = !flag;
		SetLoadButtonFlag(flag);
		Menu.SetChecked(_loadButtonName, flag);
	}

	[MenuItem(_loadButtonName, true)]
	static bool SetLoadAssetModeValidate()
	{
		RefreshLoadAssetMode();
		return true;
	}

	#endif

	#endregion

	#region Load asset

//	private string GetPathInAssetBundle(string assetName)
//	{
//		string result = assetName.ToLower();
//
//		#if DEBUG
//		Debug.Assert(!result.StartsWith("assets") && !result.StartsWith("resources"));
//		#endif
//
//		result = Path.Combine("assets/machineasset", result);
//		return result;
//	}

	public void UnloadMachineAssetBundle(string machineName)
	{
		if(_assetBundleDict.ContainsKey(machineName))
		{
			_assetBundleDict[machineName].Unload(false);
			_assetBundleDict.Remove(machineName);
		}
	}

	#if UNITY_EDITOR
	private void ReloadShader(object[] materials){
		foreach (Material m in materials)
		{
			var shaderName = m.shader.name;
			var newShader = Shader.Find(shaderName);
			if (newShader != null)
			{
				LogUtility.Log("Assign new Shader = " + newShader.ToString(), Color.red);
				m.shader = newShader;
			}
			else
			{
				Debug.LogWarning("unable to refresh shader: " + shaderName + " in material " + m.name);
			}
		}
	}
	#endif

	AssetBundle LoadMachineAssetBundle(string machineName)
	{
		AssetBundle result = null;
		if(_assetBundleDict.ContainsKey(machineName))
		{
			result = _assetBundleDict[machineName];
		}
		else
		{
			string path = GetMachineAssetBundleFilePath(machineName);
			result = AssetBundle.LoadFromFile(path);
			if(result != null){
				_assetBundleDict.Add(machineName, result);

				#if UNITY_EDITOR
				// reload materals in ab
				object[] materials = result.LoadAllAssets(typeof(Material));
				ReloadShader(materials);

				// reload materials in objs in ab
				var objects = result.LoadAllAssets(typeof(GameObject));
				foreach(GameObject obj in objects){
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
					materials = ListUtility.MapList(renderers, (Renderer r)=>{
						return r.sharedMaterial;
					}).ToArray();
					ReloadShader(materials);
				}
				#endif
			}
			else
				Debug.Assert(false);
		}
		return result;
	}

	#if UNITY_EDITOR

	T LoadAssetFromEditor<T>(string assetName, string machineName) where T : UnityEngine.Object
	{
		T result = null;
		string bundleName = machineName.ToLower();
		assetName = Path.GetFileName(assetName);
		string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
		if(paths.Length == 0)
		{
			//do nothing
			//Debug.LogError("LoadAssetFromEditor error: no asset found: " + machineName + " ," + assetName);
		}
		else if(paths.Length == 1)
		{
			result = AssetDatabase.LoadAssetAtPath<T>(paths[0]);
		}
		else
		{
			Debug.LogError("LoadAssetFromEditor found more than one asset, assetBundleName: " + machineName + " ,assetName: " + assetName);
			foreach(string p in paths)
			{
				Debug.Log(p);
			}
		}
		return result;
	}

	#endif

	T LoadAssetInRuntime<T>(string assetName, string machineName) where T : UnityEngine.Object
	{
		T result = null;
		if(IsMachineDownloadedAndMatched(machineName))
		{
			AssetBundle bundle = LoadMachineAssetBundle(machineName);
			if(bundle != null)
			{
				var splited = assetName.Split('/');
				var name = splited[splited.Length - 1];
				result = bundle.LoadAsset<T>(name);
			}
		}
		return result;
	}

	public T LoadAssetFromMachineAssetBundle<T>(string assetName, string machineName) where T : UnityEngine.Object
	{
		#if UNITY_EDITOR
		if(GetLoadButtonFlag())
			return LoadAssetInRuntime<T>(assetName, machineName);
		else
			return LoadAssetFromEditor<T>(assetName, machineName);
		#else
		return LoadAssetInRuntime<T>(assetName, machineName);
		#endif
	}

	#endregion

	#region Public

	public bool IsLocalAssetMachine(string machineName)
	{
		bool result = ListUtility.IsContainElement(MachineUnlockSettingConfig.Instance.LocalAssetMachines, machineName);
		return result;
	}

	public bool IsRemoteAssetMachine(string machineName)
	{
		return !IsLocalAssetMachine(machineName);
	}

	#endregion
}
