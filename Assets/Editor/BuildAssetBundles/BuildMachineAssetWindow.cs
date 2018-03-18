using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildMachineAssetWindow : EditorWindow
{
	static BuildMachineAssetBundleConfig _machineConfig;
	static BuildMachineAssetWindow _window;

	static string _lastResourcePath;
	static Vector2 _scrollPosition = Vector2.zero;

	[MenuItem("Build/Build Machine Assets %g", false, 51)]
	static void InitWindow()
	{
		InitConfig();
		ShowWindow();
	}

	static void InitConfig()
	{
		_machineConfig = AssetDatabase.LoadAssetAtPath<BuildMachineAssetBundleConfig>(BuildMachineAssetBundleConfig._configFilePath);
		if(_machineConfig == null)
		{
			_machineConfig = new BuildMachineAssetBundleConfig();
			AssetDatabase.CreateAsset(_machineConfig, BuildMachineAssetBundleConfig._configFilePath);
		}
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 700);
		_window = (BuildMachineAssetWindow)EditorWindow.GetWindowWithRect(typeof(BuildMachineAssetWindow), rect, true);
		_window.maxSize = new Vector2(700, 900);
		_window.Show();
	}

	void OnGUI()
	{
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

		GUILayout.Label("*********************************************************************");
		GUILayout.Label("*                          Machine Asset                            *");
		GUILayout.Label("*********************************************************************");

		_machineConfig._platformType = (PlatformType)EditorGUILayout.EnumPopup("PlatformType", _machineConfig._platformType);

		GUILayout.Space(20);

		GUILayout.Label("Machine Names:");
		for(int i = 0; i < _machineConfig._machineConfigs.Count; i++)
		{
			GUILayoutOption[] options = new GUILayoutOption[]{ };

			GUILayout.BeginHorizontal();

			_machineConfig._machineConfigs[i]._selected = EditorGUILayout.Toggle(_machineConfig._machineConfigs[i]._selected, options);

			_machineConfig._machineConfigs[i]._name = EditorGUILayout.TextField("Name:", _machineConfig._machineConfigs[i]._name, options);
			string v = EditorGUILayout.TextField("Version:", _machineConfig._machineConfigs[i]._version, options);
			int parseVersion = 0;
			int.TryParse(v, out parseVersion);
			if(parseVersion > 0)
				_machineConfig._machineConfigs[i]._version = v;
			else
				Debug.LogError("Version should be integer and >0");

			GUILayout.Space(10);

			if(GUILayout.Button("-", GUILayout.Width(30)))
			{
				_machineConfig._machineConfigs.RemoveAt(i);
				i--;
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(30)))
		{
			_machineConfig._machineConfigs.Insert(_machineConfig._machineConfigs.Count, new SingleMachineAssetConfig());
		}

		GUILayout.Space(20);

		if(GUILayout.Button("Select All", GUILayout.Width(100.0f)))
			SelectAllButtonDown();

		if(GUILayout.Button("Deselect All", GUILayout.Width(100.0f)))
			DeselectAllButtonDown();

		GUILayout.Space(20);

		if(GUILayout.Button("Build Machine AssetBundles", GUILayout.Width(180.0f)))
			BuildMachineButtonDown();

		GUILayout.Space(20);

		GUILayout.Label("Upload Setting:");
		_machineConfig._isUploadDebugServer = EditorGUILayout.Toggle("Upload debug server", _machineConfig._isUploadDebugServer);
		_machineConfig._isUploadReleaseServer = EditorGUILayout.Toggle("Upload release server", _machineConfig._isUploadReleaseServer);
		_machineConfig._isUseVPN = EditorGUILayout.Toggle("Use VPN", _machineConfig._isUseVPN);

		if(GUILayout.Button("Upload To Server", GUILayout.Width(120)))
			UploadMachineButtonDown();

		GUILayout.Space(20);

		if(GUILayout.Button("Save config", GUILayout.Width(100)))
			BuildAssetBundleHelper.SaveConfigButtonDown(_machineConfig);

		GUILayout.EndScrollView();
	}

	void BuildMachineButtonDown()
	{
		bool result = BuildMachineAssets(_machineConfig._platformType);
		if(result)
			ShowNotification(new GUIContent("Done!"));
		else
			ShowNotification(new GUIContent("Error!"));
	}

	void UploadMachineButtonDown()
	{
		if(OutputUploaderToolScript(_machineConfig._platformType))
			RunUploaderToolScript();
	}

	static bool BuildMachineAssets(PlatformType platformType)
	{
		bool result = false;
		BuildTarget target = BuildAssetBundleHelper.GetBuildTarget(platformType);
		if(target != BuildTarget.NoTarget)
		{
			//Note: Important fix. If don't switch platform, everytime before building AssetBundle,
			//it will take much time to switch to the particular platform.
			PerformBuild.SwitchBuildPlatform(target);

			string path = BuildAssetBundleHelper.GetAssetBundlePath(platformType);
			BuildAssetBundleHelper.DeleteAllFilesInDir(path);

			foreach(SingleMachineAssetConfig config in _machineConfig._machineConfigs)
			{
				if(config._selected)
				{
					string bundleName = config._name.ToLower();
					string[] assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
					bool buildResult = BuildAssetBundles.BuildBundlesFromMap(path, "", platformType, target,
						false, bundleName, assetNames);
					Debug.Log("Build MachineAsset result:" + buildResult.ToString());
					Debug.Assert(buildResult);

					WriteMachineVersionToFile(path, bundleName, config._version);
				}
			}

			result = true;

		}
		return result;
	}

	static void WriteMachineVersionToFile(string path, string machineName, string version)
	{
		string filePath = Path.Combine(Application.dataPath, path);
		string fileName = machineName + "_version";
		filePath = Path.Combine(filePath, fileName);
		File.WriteAllText(filePath, version);
	}

	static string ConstructUploaderToolNodeScriptLine(PlatformType platformType)
	{
		string format = "node index.js -platform {0} -build {1} -machines {2} -versions {3} -vpn {4}";

		string platform = platformType.ToString();
		List<string> buildList = new List<string>();
		if(_machineConfig._isUploadDebugServer)
			buildList.Add("Debug");
		if(_machineConfig._isUploadReleaseServer)
			buildList.Add("Release");

		if(buildList.Count == 0)
		{
			Debug.LogError("No Debug or Release to upload");
			return string.Empty;
		}

		string builds = string.Join(",", buildList.ToArray());

		List<string> machineNameList = ListUtility.FoldList(_machineConfig._machineConfigs, new List<string>(),
			(List<string> list, SingleMachineAssetConfig c) => {
				if(c._selected)
					list.Add(c._name);
				return list;
			});
		List<string> machineVersionList = ListUtility.FoldList(_machineConfig._machineConfigs, new List<string>(),
			(List<string> list, SingleMachineAssetConfig c) => {
				if(c._selected)
					list.Add(c._version);
				return list;
			});
		string machineNames = string.Join(",", machineNameList.ToArray());
		string machineVersions = string.Join(",", machineVersionList.ToArray());

		string isUseVPN = _machineConfig._isUseVPN ? "1" : "0";

		string result = string.Format(format, platform, builds, machineNames, machineVersions, isUseVPN);
		return result;
	}

	static string[] ConstructUploaderToolScriptLines(PlatformType platformType)
	{
		string[] result = null;
		if(Application.platform == RuntimePlatform.OSXEditor)
		{
			string line0 = "#!/bin/sh";
			string line1 = "cd " + Path.Combine(Application.dataPath, "../Support/UploaderTool/");
			string line2 = "npm install";
			string line3 = ConstructUploaderToolNodeScriptLine(platformType);
			if(string.IsNullOrEmpty(line3))
				result = new string[0];
			else
				result = new string[]{line0, line1, line2, line3};
		}
		else if(Application.platform == RuntimePlatform.WindowsEditor)
		{
			string line1 = "cd " + Path.Combine(Application.dataPath, "../Support/UploaderTool/");
			//add "call" to prevent npm abort the .bat script
			string line2 = "call npm install";
			string line3 = ConstructUploaderToolNodeScriptLine(platformType);
			if(string.IsNullOrEmpty(line3))
				result = new string[0];
			else
				result = new string[]{line1, line2, line3};
		}
		else
		{
			UnityEngine.Debug.Assert(false, "Platform error");
		}

		return result;
	}

	static bool OutputUploaderToolScript(PlatformType platformType)
	{
		bool result = false;
		string[] lines = ConstructUploaderToolScriptLines(platformType);
		if(lines != null && lines.Length > 0)
		{
			string path = "";
			if(Application.platform == RuntimePlatform.OSXEditor)
				path = Path.Combine(Application.dataPath, "../Support/UploaderTool/run.sh");
			else if(Application.platform == RuntimePlatform.WindowsEditor)
				path = Path.Combine(Application.dataPath, "../Support/UploaderTool/run.bat");
			else
				UnityEngine.Debug.Assert(false, "Platform error");
			File.WriteAllLines(path, lines);
			result = true;
		}
		return result;
	}

	static void RunUploaderToolScript()
	{
		string arguments = Path.Combine(Application.dataPath, "../Support/UploaderTool/run.sh");
		string workingDir = Path.Combine(Application.dataPath, "../Support/UploaderTool/");
		RunProcessHelper.Run(arguments, workingDir);
	}

	static void BuildMachineAssetCommandLine()
	{
		InitConfig();

		//Windows Jenkins only builds Android assets
		//Mac Jenkins only builds iOS assets
		PlatformType type = PlatformType.None;
		switch(Application.platform)
		{
			case RuntimePlatform.OSXEditor:
				type = PlatformType.iOS;
				break;
			case RuntimePlatform.WindowsEditor:
				type = PlatformType.Android;
				break;
			default:
				Debug.LogError("Not support Application.platform");
				throw new Exception("Not support Application.platform");
				break;
		}

		if(type != PlatformType.None)
		{
			bool result = BuildMachineAssets(type);
			if(result)
				Debug.Log("Succeed to build machine assets");
			else
				Debug.Log("Fail to build machine assets");
			OutputUploaderToolScript(type);
		}
	}

	static void SelectAllButtonDown()
	{
		ListUtility.ForEach(_machineConfig._machineConfigs, (SingleMachineAssetConfig config) => {
			config._selected = true;
		});
	}

	static void DeselectAllButtonDown()
	{
		ListUtility.ForEach(_machineConfig._machineConfigs, (SingleMachineAssetConfig config) => {
			config._selected = false;
		});
	}
}
