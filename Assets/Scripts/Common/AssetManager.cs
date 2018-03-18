using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using System;
using System.IO;

public class AssetManager : Singleton<AssetManager>
{
	Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();

	#region Private

//	private void LoadAllAssetBundles()
//	{
//		if(LiveUpdateManager.Instance.IsResourceAvailable())
//		{
//			List<string> bundleNames = LiveUpdateManager.Instance.BundleInfo.AssetBundleNames;
//			ListUtility.ForEach(bundleNames, (string name) => {
//				LoadSingleAssetBundle(name);
//			});
//		}
//	}

	private void UnloadAllAssetBundles()
	{
		if(_assetBundleDict.Count > 0)
		{
			foreach(var pair in _assetBundleDict)
			{
				pair.Value.Unload(false);
			}
		}
	}

	private AssetBundle LoadSingleAssetBundle(string dir, string bundleName, bool isEncrypted)
	{
		AssetBundle result = null;

		if(_assetBundleDict.ContainsKey(bundleName))
		{
			result = _assetBundleDict[bundleName];
		}
		else
		{
			string path = Path.Combine(dir, bundleName);
			if(isEncrypted)
			{
				byte[] encryptedBytes = FileHelper.ReadFromStreamingAsset(path);
				if(encryptedBytes != null && encryptedBytes.Length > 0)
				{
					byte[] decryptedBytes = EncryptUtility.AES_Decrypt(encryptedBytes);
					result = AssetBundle.LoadFromMemory(decryptedBytes);

					if(result == null)
					{
						Debug.LogError("Fail to load AssetBundle:" + bundleName);
						Debug.Assert(false);

						//try to load as non-encryption
						result = AssetBundle.LoadFromFile(path);
					}
				}
			}
			else
			{
				result = AssetBundle.LoadFromFile(path);
			}

			if(result != null)
				_assetBundleDict.Add(bundleName, result);
		}

		return result;
	}

	private string GetPathInAssetBundle(string assetName)
	{
		string result = assetName.ToLower();

		#if DEBUG
		Debug.Assert(!result.StartsWith("assets") && !result.StartsWith("resources"));
		#endif

		result = Path.Combine("assets/resources", result);
		return result;
	}

	private string TryGetAssetBundleNameFromAsset(string assetName)
	{
		string result = "";
		Dictionary<string, string> assetMapDict = LiveUpdateManager.Instance.AssetMapDict;
		if(assetMapDict.ContainsKey(assetName))
			result = assetMapDict[assetName];
		return result;
	}

	private T LoadAssetFromLiveUpdateAssetBundle<T>(string assetName) where T : UnityEngine.Object
	{
		T result = null;
		if(LiveUpdateManager.Instance.IsResourceAvailable())
		{
			assetName = GetPathInAssetBundle(assetName);
			string bundleName = TryGetAssetBundleNameFromAsset(assetName);
			if(!bundleName.IsNullOrEmpty())
			{
				string dir = LiveUpdateManager.Instance.GetResourceSaveDir();
				AssetBundle bundle = LoadSingleAssetBundle(dir, bundleName, false);
				if(bundle != null)
				{
					string name = Path.GetFileName(assetName);
					result = bundle.LoadAsset<T>(name);
				}
			}
		}
		return result;
	}

	string GetExcelAssetFilePath(ExcelDirType dirType, string subDir, string excelName, string sheetName)
	{
		string totalDir = ExcelConfig.GetTopDir(dirType) + subDir;
		string fileName = ExcelConfig.GetExportFileName(excelName, sheetName);
		string path = Path.Combine(totalDir, fileName);
		return path;
	}

	T LoadExcelAssetFromAssetBundle<T>(ExcelDirType dirType, string excelName, string sheetName, string path) where T : UnityEngine.Object
	{
		T asset = LoadAssetFromLiveUpdateAssetBundle<T>(path);
		if(asset == null)
		{
			string bundleName = ExcelConfig.GetBundleName(dirType, excelName.ToLower());
			AssetBundle bundle = LoadSingleAssetBundle(Application.streamingAssetsPath, bundleName, AssetConfig.IsExcelAssetBundleEncrypted);
			if(bundle != null)
			{
				string assetName = ExcelConfig.GetExportFileName(excelName, sheetName);
				asset = bundle.LoadAsset<T>(assetName);
			}
		}

		return asset;
	}

	#endregion

	#region Public

	public T LoadAsset<T>(string path) where T : UnityEngine.Object
	{
		T asset = LoadAssetFromLiveUpdateAssetBundle<T>(path);
		if(asset == null)
			asset = Resources.Load<T>(path);
		return asset;
	}

	public T LoadMachineAsset<T>(string path, string machineName) where T : UnityEngine.Object
	{
		T asset = LoadAssetFromLiveUpdateAssetBundle<T>(path);
		CoreDebugUtility.Log("LoadMachineAsset path = "+path + " machine = "+machineName);
		if(asset == null)
		{
			if(MachineAssetManager.Instance.IsRemoteAssetMachine(machineName))
			{
				asset = MachineAssetManager.Instance.LoadAssetFromMachineAssetBundle<T>(path, machineName);

				CoreDebugUtility.Log("IsRemoteAssetMachine path = "+path + " machine = "+machineName);
			}

			if(asset == null){
				asset = Resources.Load<T>(path);
				//CoreDebugUtility.Log("Resources.Load path = "+path + " machine = "+machineName);
			#if UNITY_EDITOR
				if (asset == null){
					asset = MachineAssetManager.Instance.LoadAssetFromMachineAssetBundle<T>(path, machineName);
				}
			#endif
			}

		}
		return asset;
	}

	public T LoadExcelAsset<T>(string subDir, string excelName, string sheetName) where T : UnityEngine.Object
	{
		T result = null;

		for(int i = 0; i < ExcelConfig.PriorityDirTypes.Count; i++)
		{
			ExcelDirType dirType = ExcelConfig.PriorityDirTypes[i];
			string path = GetExcelAssetFilePath(dirType, subDir, excelName, sheetName);

			if(AssetConfig.IsUseExcelAssetBundle)
			{
				#if UNITY_EDITOR
				result = LoadAsset<T>(path);
				//result = LoadExcelAssetFromAssetBundle<T>(dirType, excelName, sheetName, path);
				#else
				result = LoadExcelAssetFromAssetBundle<T>(dirType, excelName, sheetName, path);
				#endif
			}
			else
			{
				result = LoadAsset<T>(path);
			}

			if(result != null)
				break;
		}

		if(result == null)
		{
			for(int i = 0; i < ExcelConfig.PriorityDirTypes.Count; i++)
			{
				ExcelDirType dirType = ExcelConfig.PriorityDirTypes[i];
				string path = GetExcelAssetFilePath(dirType, subDir, excelName, sheetName);
				result = Resources.Load<T>(path);

				if(result != null)
					break;
			}

			if(result == null)
			{
				Debug.LogError("Fail to load excel from AssetBundle:" + excelName + " " + sheetName);
				Debug.Assert(false);
			}
		}

		return result;
	}

	#endregion
}
