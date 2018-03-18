using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildLiveUpdateWindow : EditorWindow
{
	static BuildLiveUpdateAssetBundleConfig _liveUpdateConfig;
	static BuildLiveUpdateWindow _window;

	static string _lastResourcePath;
	static Vector2 _scrollPosition = Vector2.zero;

	[MenuItem("Build/Build Live Update", false, 52)]
	static void InitWindow()
	{
		InitConfig();
		ShowWindow();
	}

	static void InitConfig()
	{
		_liveUpdateConfig = AssetDatabase.LoadAssetAtPath<BuildLiveUpdateAssetBundleConfig>(BuildLiveUpdateAssetBundleConfig._configFilePath);
		if(_liveUpdateConfig == null)
		{
			_liveUpdateConfig = new BuildLiveUpdateAssetBundleConfig();
			AssetDatabase.CreateAsset(_liveUpdateConfig, BuildLiveUpdateAssetBundleConfig._configFilePath);
		}
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 500);
		_window = (BuildLiveUpdateWindow)EditorWindow.GetWindowWithRect(typeof(BuildLiveUpdateWindow), rect, true);
		_window.maxSize = new Vector2(700, 800);
		_window.Show();
	}

	void OnGUI()
	{
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

		GUILayout.Label("*********************************************************************");
		GUILayout.Label("*                       LiveUpdate Asset                            *");
		GUILayout.Label("*********************************************************************");

		_liveUpdateConfig._platformType = (PlatformType)EditorGUILayout.EnumPopup("PlatformType", _liveUpdateConfig._platformType);
		_liveUpdateConfig._bundleName = EditorGUILayout.TextField("BundleName", _liveUpdateConfig._bundleName);
		_liveUpdateConfig._version = EditorGUILayout.TextField("ResourceVersion", _liveUpdateConfig._version);

		GUILayout.Space(20);

		GUILayout.Label("Excel File Names:");
		for(int i = 0; i < _liveUpdateConfig._excelFileNames.Count; i++)
		{
			GUILayout.BeginHorizontal();

			_liveUpdateConfig._excelFileNames[i] = EditorGUILayout.TextField("Name:", _liveUpdateConfig._excelFileNames[i]);

			GUILayout.Space(10);

			if(GUILayout.Button("-", GUILayout.Width(30)))
			{
				_liveUpdateConfig._excelFileNames.RemoveAt(i);
				i--;
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(30)))
		{
			_liveUpdateConfig._excelFileNames.Insert(_liveUpdateConfig._excelFileNames.Count, "");
		}

		GUILayout.Space(20);

		GUILayout.Label("Resource Paths:");
		for(int i = 0; i < _liveUpdateConfig._resourcePaths.Count; i++)
		{
			GUILayout.BeginHorizontal();

			string path = _liveUpdateConfig._resourcePaths[i];
			path = EditorGUILayout.TextField(path, GUILayout.Width(300));
			if(string.IsNullOrEmpty(path))
			{
				if(!string.IsNullOrEmpty(_lastResourcePath))
					path = _lastResourcePath;
				else
					path = BuildAssetBundleHelper.GetDefaultResourcePath();
			}

			if(GUILayout.Button("...", GUILayout.Width(30)))
			{
				string selectPath = BuildAssetBundleHelper.OpenFilePanel(path);

				if (selectPath.Length != 0)
				{
					// the path should be relative not absolute one to make it work on any platform.
					string trimPath = BuildAssetBundleHelper.TrimToRelativePath(selectPath);
					if(!string.IsNullOrEmpty(trimPath))
					{
						// set relative path
						_liveUpdateConfig._resourcePaths[i] = trimPath;
						_lastResourcePath = trimPath;
					}
					else
					{
						EditorUtility.DisplayDialog("Error", "Folder should be under 'Assets' folder", "OK");
						return;
					}
				}
			}

			if(GUILayout.Button("-", GUILayout.Width(30)))
			{
				_liveUpdateConfig._resourcePaths.RemoveAt(i);
			}

			if(GUILayout.Button("+", GUILayout.Width(30)))
			{
				_liveUpdateConfig._resourcePaths.Insert(i + 1, "");
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(30)))
		{
			_liveUpdateConfig._resourcePaths.Insert(_liveUpdateConfig._resourcePaths.Count, "");
		}

		GUILayout.Space(20);

		if(GUILayout.Button("Build LiveUpdate AssetBundles", GUILayout.Width(180.0f)))
			BuildLiveUpdateButtonDown();

		GUILayout.Space(20);

		if(GUILayout.Button("Save config", GUILayout.Width(100)))
			BuildAssetBundleHelper.SaveConfigButtonDown(_liveUpdateConfig);

		GUILayout.EndScrollView();
	}

	void BuildLiveUpdateButtonDown()
	{
		bool result = BuildLiveUpdateAssets();
		if(result)
			ShowNotification(new GUIContent("Done!"));
		else
			ShowNotification(new GUIContent("Error"));
	}

	static bool BuildLiveUpdateAssets()
	{
		bool result = false;
		BuildTarget target = BuildAssetBundleHelper.GetBuildTarget(_liveUpdateConfig._platformType);
		if(target != BuildTarget.NoTarget)
		{
			//Note: Important fix. If don't switch platform, everytime before building AssetBundle,
			//it will take much time to switch to the particular platform.
			PerformBuild.SwitchBuildPlatform(target);

			string path = BuildAssetBundleHelper.GetAssetBundlePath(_liveUpdateConfig._platformType);
			BuildAssetBundleHelper.DeleteAllFilesInDir(path);

			ExcelDirType dirType = BuildAssetBundleHelper.GetExcelDirType(target);
			List<string> allResPaths = BuildAssetBundleHelper.GetAllExcelResourcePaths(dirType, _liveUpdateConfig._excelFileNames);
			allResPaths.AddRange(_liveUpdateConfig._resourcePaths);

			result = BuildAssetBundles.BuildBundlesFromMap(path, _liveUpdateConfig._version, _liveUpdateConfig._platformType, target, 
				true, _liveUpdateConfig._bundleName, allResPaths);
		}
		return result;
	}

	static void BuildLiveUpdateCommandLine()
	{
		InitConfig();
		bool result = BuildLiveUpdateAssets();
		if(result)
			Debug.Log("Succeed to build live update assets");
		else
			Debug.Log("Fail to build live update assets");
	}
}
