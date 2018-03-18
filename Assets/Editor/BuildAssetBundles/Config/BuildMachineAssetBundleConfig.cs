using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SingleMachineAssetConfig
{
	public string _name;
	public bool _selected;
	public string _version;
}

public class BuildMachineAssetBundleConfig : ScriptableObject
{
	public static readonly string _configFilePath = "Assets/AssetBundles/Config/machineAssetBundleConfig.asset";

	public PlatformType _platformType;
	public List<SingleMachineAssetConfig> _machineConfigs = new List<SingleMachineAssetConfig>();
	public bool _isUploadDebugServer;
	public bool _isUploadReleaseServer;
	public bool _isUseVPN;
}
