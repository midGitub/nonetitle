using System.Collections.Generic;
using UnityEngine;

public class GenPresetAssetConfig : ScriptableObject
{
	public static readonly string _configFilePath = "Assets/PresetAssets/Config/presetAssetConfig.asset";

	public string _machineName;
	public bool _isDownloadMachine;
	public bool _isTinyMachine;
	public bool _genMapMachine;
	public bool _genPaytable;
	public bool _genBackground;
	public bool _genEffects;
}
