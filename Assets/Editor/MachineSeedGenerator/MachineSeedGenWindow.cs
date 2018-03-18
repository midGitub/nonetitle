using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MachineSeedGenWindow : EditorWindow
{
	private static readonly string _configFilePath = "Assets/Editor/MachineSeedGenerator/Config/machineSeedConfig.asset";

	private static MachineSeedGenConfig _genConfig;
	private static MachineSeedGenWindow _window;
	private static MachineSeedGenEngine _engine;

	private static MachineTestConfig _testConfig; //convert to from _genConfig

	static Vector2 _scrollPosition = Vector2.zero;

	[MenuItem("Tools/Machine Seed Generator")]
	static void InitWindow()
	{
		InitEngine();
		InitConfig();
		ShowWindow();
	}

	static void InitEngine()
	{
		_engine = new MachineSeedGenEngine();
	}

	static void InitConfig()
	{
		_genConfig = AssetDatabase.LoadAssetAtPath<MachineSeedGenConfig>(_configFilePath);
		if(_genConfig == null)
		{
			_genConfig = new MachineSeedGenConfig();
			AssetDatabase.CreateAsset(_genConfig, _configFilePath);
		}

		_testConfig = new MachineTestConfig();

		_engine.UpdateGenConfigToTestConfig(_genConfig, _testConfig, true);
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 650);
		_window = (MachineSeedGenWindow)EditorWindow.GetWindowWithRect(typeof(MachineSeedGenWindow), rect, true);
		_window.Show();
	}

	void OnGUI()
	{
		_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, true);

		MachineTestEditorWindow.ShowSingleUserConfig(_testConfig);

		GUILayout.Space(10);

		MachineTestEditorWindow.ShowAllUsersConfig(_testConfig);

		_engine.UpdateTestConfigToGenConfig(_testConfig, _genConfig);

		GUILayout.Space(10);

		_genConfig._machineName = EditorGUILayout.TextField("Machine name", _genConfig._machineName);

		GUILayout.Space(10);

		ShowLimitationConfig(_genConfig);

		GUILayout.Space(10);

		ShowOtherConfig(_genConfig);

		GUILayout.Space(10);

		if(GUILayout.Button("Save config", GUILayout.Width(160.0f)))
			SaveConfigButtonDown();

		if(GUILayout.Button("Run test", GUILayout.Width(160.0f)))
			RunButtonDown();

		EditorGUILayout.EndScrollView();
	}

	void ShowLimitationConfig(MachineSeedGenConfig genConfig)
	{
		EditorGUILayout.LabelField("----- limitation config -----");

		for(int i = 0; i < genConfig._limitConfigs.Count; i++)
		{
			MachineSeedLimitationConfig limitConfig = genConfig._limitConfigs[i];

			GUILayout.BeginHorizontal();

			limitConfig._type = (MachineSeedLimitationType)EditorGUILayout.EnumPopup(limitConfig._type, GUILayout.ExpandWidth(false), GUILayout.MinWidth(70));

			if(limitConfig._type == MachineSeedLimitationType.Bankcrupt)
			{
				EditorGUILayout.LabelField("StartSpinCount", GUILayout.MaxWidth(100));
				limitConfig._startSpinCount = EditorGUILayout.IntField(limitConfig._startSpinCount);

				EditorGUILayout.LabelField("EndSpinCount", GUILayout.MaxWidth(100));
				limitConfig._endSpinCount = EditorGUILayout.IntField(limitConfig._endSpinCount);
			}
			else if(limitConfig._type == MachineSeedLimitationType.CreditRange)
			{
				EditorGUILayout.LabelField("SpinCount", GUILayout.MaxWidth(70));
				limitConfig._spinCount = EditorGUILayout.IntField(limitConfig._spinCount);

				EditorGUILayout.LabelField("MinCredit", GUILayout.MaxWidth(70));
				limitConfig._minCredit = EditorGUILayout.LongField(limitConfig._minCredit);

				EditorGUILayout.LabelField("MaxCredit", GUILayout.MaxWidth(70));
				limitConfig._maxCredit = EditorGUILayout.LongField(limitConfig._maxCredit);
			}
			else
			{
				Debug.Assert(false);
			}

			if(GUILayout.Button("-", GUILayout.Width(20)))
			{
				genConfig._limitConfigs.RemoveAt(i);
			}

			if(GUILayout.Button("+", GUILayout.Width(20)))
			{
				genConfig._limitConfigs.Insert(i + 1, new MachineSeedLimitationConfig());
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(20)))
		{
			genConfig._limitConfigs.Add(new MachineSeedLimitationConfig());
		}
	}

	void ShowOtherConfig(MachineSeedGenConfig genConfig)
	{
		genConfig._isOutputUserResult = EditorGUILayout.Toggle("IsOutputUserResult", genConfig._isOutputUserResult);
		genConfig._isOutputSeeds = EditorGUILayout.Toggle("IsOutputSeeds", genConfig._isOutputSeeds);
	}

	void SaveConfigButtonDown()
	{
		EditorUtility.SetDirty(_genConfig);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void RunButtonDown()
	{
		_engine.Run(_genConfig, _testConfig);

		ShowNotification(new GUIContent("Done!"));
	}
}
