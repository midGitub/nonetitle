using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MachineTestEditorWindow : EditorWindow
{
	private static readonly string _configFilePath = "Assets/Test/MachineTest/machineConfig.asset";

	private static MachineTestConfig _config;
	private static MachineTestEngine _engine;
	private static MachineTestEditorWindow _window;

	static Vector2 _scrollPosition = Vector2.zero;

	[MenuItem("Tools/Machine Test")]
	static void InitWindow()
	{
		InitConfig();
		InitEngine();
		ShowWindow();
	}

	static void InitConfig()
	{
		_config = AssetDatabase.LoadAssetAtPath<MachineTestConfig>(_configFilePath);
		if(_config == null)
		{
			_config = new MachineTestConfig();
			AssetDatabase.CreateAsset(_config, _configFilePath);
		}
		else
		{
			//handle the case that after adding new machine in CoreDefine.AllMachineNames
			//reading the old .asset file won't add the new machine
			if(_config._selectMachines.Length < CoreDefine.AllMachineNames.Length)
			{
				bool[] machines = new bool[CoreDefine.AllMachineNames.Length];
				_config._selectMachines.CopyTo(machines, 0);
				_config._selectMachines = machines;
				EditorUtility.SetDirty(_config);
				AssetDatabase.SaveAssets();
			}
            if(_config._allMachines.Length < CoreDefine.AllMachineNames.Length)
            {
                _config._allMachines = CoreDefine.AllMachineNames;
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }
		}
	}

	static void InitEngine()
	{
		_engine = new MachineTestEngine();
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 650);
		_window = (MachineTestEditorWindow)EditorWindow.GetWindowWithRect(typeof(MachineTestEditorWindow), rect, true);
		_window.Show();
	}

	void OnGUI()
	{
		_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, true);

		ShowSingleUserConfig(_config);

		GUILayout.Space(10);

		ShowAllUsersConfig(_config);

		GUILayout.Space(10);

		ShowMachineConfig(_config);

		GUILayout.Space(10);

		if(GUILayout.Button("Save config", GUILayout.Width(160.0f)))
			SaveConfigButtonDown();

		if(GUILayout.Button("Run test", GUILayout.Width(160.0f)))
			RunButtonDown();

		EditorGUILayout.EndScrollView();
	}

	public static void ShowSingleUserConfig(MachineTestConfig config)
	{
		EditorGUILayout.LabelField("----- single user config -----");
		config._initLucky = EditorGUILayout.IntField("initLucky", config._initLucky);
		config._initCredit = EditorGUILayout.LongField("initCredit", config._initCredit);
		config._spinCount = EditorGUILayout.IntField("spinCount", config._spinCount);
		config._isPayProtectionEnable = EditorGUILayout.Toggle ("pay protection", config._isPayProtectionEnable);

		config._betMode = (MachineTestBetMode)EditorGUILayout.EnumPopup("betMode", config._betMode);
		if(config._betMode == MachineTestBetMode.FixBetAmount)
		{
			config._betAmount = EditorGUILayout.IntField("  betAmount", config._betAmount);
		}
		else if(config._betMode == MachineTestBetMode.FixBetPercentage)
		{
			config._betPercentage = EditorGUILayout.FloatField("  betPercentage %", config._betPercentage);
			config._minBetAmountInPercentageMode = EditorGUILayout.FloatField("  minBetAmount", config._minBetAmountInPercentageMode);
		}

		config._stopCredit = EditorGUILayout.IntField("stopCredit", config._stopCredit);
	}

	public static void ShowAllUsersConfig(MachineTestConfig config)
	{
		EditorGUILayout.LabelField("----- all users config -----");
		config._userCount = EditorGUILayout.IntField("userCount", config._userCount);
		config._seedMode = (MachineTestSeedMode)EditorGUILayout.EnumPopup("seedMode", config._seedMode);
		if(config._seedMode == MachineTestSeedMode.Fixed)
		{
			config._startSeedForFixedMode = (uint)EditorGUILayout.IntField("  startSeedForFixedMode", (int)config._startSeedForFixedMode);

			//show seed range
			uint startSeed = config._startSeedForFixedMode;
			uint endSeed = config._startSeedForFixedMode + (uint)config._userCount - 1;
			string s = string.Format("  User seed range: [{0}, {1}]", startSeed, endSeed);
			EditorGUILayout.LabelField(s);
		}
	}

	static void ShowMachineConfig(MachineTestConfig config)
	{
		EditorGUILayout.LabelField("----- machine config -----");
		const int countPerRow = 2;
		int rowNum = (CoreDefine.AllMachineNames.Length + countPerRow - 1) / countPerRow;
		for(int i = 0; i < rowNum; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for(int k = 0; k < countPerRow; k++)
			{
				int index = i * countPerRow + k;
				if(index < CoreDefine.AllMachineNames.Length)
				{
					string m = CoreDefine.AllMachineNames[index];
					config._selectMachines[index] = EditorGUILayout.Toggle(m, config._selectMachines[index]);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		if(GUILayout.Button("Select all machines", GUILayout.Width(160.0f)))
			ListUtility.FillElements(config._selectMachines, true);
		if(GUILayout.Button("Deselect all machines", GUILayout.Width(160.0f)))
			ListUtility.FillElements(config._selectMachines, false);
	}

	void SaveConfigButtonDown()
	{
//		AssetDatabase.CreateAsset(_config, _configFilePath);
		EditorUtility.SetDirty(_config);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void RunButtonDown()
	{
		_engine.Init(_config);
		_engine.RunSelectedMachines();
		ShowNotification(new GUIContent("Done!"));
	}
}
