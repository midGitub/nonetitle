using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BuildLiveUpdateAssetBundleConfig : ScriptableObject
{
	public static readonly string _configFilePath = "Assets/AssetBundles/Config/liveUpdateAssetBundleConfig.asset";

	public PlatformType _platformType;
	public string _bundleName;
	public string _version;
	public List<string> _excelFileNames = new List<string>();
	public List<string> _resourcePaths = new List<string>();
}
