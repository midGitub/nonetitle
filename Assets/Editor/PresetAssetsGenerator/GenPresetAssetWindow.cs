using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityQuickSheet;

public class GenPresetAssetWindow : EditorWindow
{
	static string _presetAssetPath = "Assets/PresetAssets/Assets/";

	static string _mapMachinePath = "Assets/Resources/Map/Machine/Prefab";
	static string _backgroundPathOfRemoteMachine = "Assets/Resources/Game";
	static string _backgroundPathOfLocalMachine = "Assets/Resources/Game";
	static string _payTablePathOfRemoteMachine = "Assets/MachineAsset/Game/PayTable";
	static string _payTablePathOfLocalMachine = "Assets/Resources/Game/PayTable";
	static string _effectsPathOfRemoteMachine = "Assets/MachineAsset/Effect/Prefab";
	static string _effectsPathOfLocalMachine = "Assets/Resources/Effect/Prefab";

	static GenPresetAssetConfig _config;
	static GenPresetAssetWindow _window;

	[MenuItem("Tools/Gen Preset Assets")]
	static void InitWindow()
	{
		InitConfig();
		ShowWindow();
	}

	static void InitConfig()
	{
		_config = AssetDatabase.LoadAssetAtPath<GenPresetAssetConfig>(GenPresetAssetConfig._configFilePath);
		if(_config == null)
		{
			_config = new GenPresetAssetConfig();
			AssetDatabase.CreateAsset(_config, GenPresetAssetConfig._configFilePath);
		}
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 500);
		_window = (GenPresetAssetWindow)EditorWindow.GetWindowWithRect(typeof(GenPresetAssetWindow), rect, true);
		_window.maxSize = new Vector2(700, 900);
		_window.Show();
	}

	void OnGUI()
	{
		GUILayout.Label("*********************************************************************");
		GUILayout.Label("*                          Preset Assets                            *");
		GUILayout.Label("*********************************************************************");

		_config._machineName = EditorGUILayout.TextField("Machine name:", _config._machineName);

		GUILayout.Space(20);

		_config._isDownloadMachine = EditorGUILayout.Toggle("Is download machine", _config._isDownloadMachine);
		_config._isTinyMachine = EditorGUILayout.Toggle("Is tiny machine", _config._isTinyMachine);
		_config._genMapMachine = EditorGUILayout.Toggle("Gen MapMachine", _config._genMapMachine);
		_config._genPaytable = EditorGUILayout.Toggle("Gen PayTable", _config._genPaytable);
		_config._genBackground = EditorGUILayout.Toggle("Gen Background", _config._genBackground);
		_config._genEffects = EditorGUILayout.Toggle("Gen Effects", _config._genEffects);

		GUILayout.Space(20);

		if(GUILayout.Button("Generate", GUILayout.Width(100.0f)))
			GenButtonDown();

		GUILayout.Space(10);

		if(GUILayout.Button("Save config", GUILayout.Width(100)))
			SaveConfigButtonDown();
	}

	void GenButtonDown()
	{
		if(_config._genMapMachine)
			GenMapMachine();

		if(_config._genBackground)
			GenBackground();

		if(_config._genPaytable)
			GenPayTable();

		if(_config._genEffects)
			GenEffects();

		ShowNotification(new GUIContent("Done!"));
	}

	void GenMapMachine()
	{
		string srcName = _config._isTinyMachine ? "MapMachine_Tiny.prefab" : "MapMachine_Big.prefab";
		string srcPath = Path.Combine(_presetAssetPath, srcName);

		string destName = string.Format("MapMachine_{0}.prefab", _config._machineName);
		string destPath = Path.Combine(_mapMachinePath, destName);

		TryCopyAsset(srcPath, destPath);
	}

	void GenBackground()
	{
		string srcPath = Path.Combine(_presetAssetPath, "Background_Preset.prefab");

		string destName = string.Format("Background{0}.prefab", _config._machineName);
		string destPath = _config._isDownloadMachine ? _backgroundPathOfRemoteMachine : _backgroundPathOfLocalMachine;
		destPath = Path.Combine(destPath, destName);

		TryCopyAsset(srcPath, destPath);
	}

	void GenPayTable()
	{
		string srcPath = Path.Combine(_presetAssetPath, "BasePayTabel_Preset.prefab");

		string destName = string.Format("BasePayTabel_{0}.prefab", _config._machineName);
		string destPath = _config._isDownloadMachine ? _payTablePathOfRemoteMachine : _payTablePathOfLocalMachine;
		destPath = Path.Combine(destPath, destName);

		TryCopyAsset(srcPath, destPath);
		SetAssetBundleName(destPath);
	}

	void GenEffects()
	{
		SymbolSheet symbolSheet = LoadMachineExcelAsset<SymbolSheet, SymbolData>(_config._machineName, "Symbol");
		BasicSheet basicSheet = LoadMachineExcelAsset<BasicSheet, BasicData>(_config._machineName, "Basic");

		string srcPath = "";
		string destName = "";
		string destPath = "";

		//symbol effects
		ListUtility.ForEach(symbolSheet.DataArray, (SymbolData data) => {
			srcPath = Path.Combine(_presetAssetPath, "FX_SymbolEffect_Preset.prefab");

			if(!string.IsNullOrEmpty(data.WinEffect))
				destName = data.WinEffect;
			else if(!string.IsNullOrEmpty(data.WinEffect3D))
				destName = data.WinEffect3D;
			else
				destName = "";

			if(!string.IsNullOrEmpty(destName))
			{
				destPath = _config._isDownloadMachine ? _effectsPathOfRemoteMachine : _effectsPathOfLocalMachine;
				destPath = Path.Combine(destPath, destName);
				destPath += ".prefab";

				string dir = Path.GetDirectoryName(destPath);
				if(!Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				TryCopyAsset(srcPath, destPath);
				SetAssetBundleName(destPath);
			}
		});

		//super symbol
//		string superSymbolName = BasicValueFromKey(basicSheet.DataArray, "SuperSymbolLightEffect");
//		if(!string.IsNullOrEmpty(superSymbolName))
//		{
//			srcPath = Path.Combine(_presetAssetPath, "FX_Spin_SuperSymbol_ReelLight.prefab");
//
//			destPath = _config._isDownloadMachine ? _effectsPathOfRemoteMachine : _effectsPathOfLocalMachine;
//			destPath = Path.Combine(destPath, superSymbolName);
//
//			TryCopyAsset(srcPath, destPath);
//		}
	}

	void TryCopyAsset(string src, string dest)
	{
		if(!File.Exists(dest))
		{
			AssetDatabase.CopyAsset(src, dest);
			Debug.Log("Generate: " + dest);
		}
	}

	void SetAssetBundleName(string dest)
	{
		AssetImporter importer = AssetImporter.GetAtPath(dest);
		importer.assetBundleName = _config._machineName.ToLower();
	}

	void SaveConfigButtonDown()
	{
		EditorUtility.SetDirty(_config);
		AssetDatabase.SaveAssets();
	}

	#region Read Excel

	SheetType LoadMachineExcelAsset<SheetType, DataType>(string excelName, string sheetName) 
		where SheetType : new()
	{
		string totalDir = Path.Combine(Application.dataPath, "Excels/Machine/");
		string path = totalDir + excelName + ".xls";
		Debug.Assert(File.Exists(path));

		ExcelQuery query = new ExcelQuery(path, sheetName);

		SheetType result = new SheetType();
		Type type = result.GetType();
		System.Reflection.PropertyInfo info = type.GetProperty("DataArray");
		DataType[] array = query.Deserialize<DataType>().ToArray();
		info.SetValue(result, array, null);
		return result;
	}

	string BasicValueFromKey(BasicData[] dataArray, string key)
	{
		string result = "";
		int index = ListUtility.Find(dataArray, (BasicData data) => {
			return data.Key == key;
		});
		if(index >= 0)
			result = dataArray[index].Val;
		return result;
	}

	#endregion
}
