using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildABTestWindow : EditorWindow
{
	static BuildABTestAssetBundleConfig _abTestConfig;
	static BuildABTestWindow _window;

	static string _lastResourcePath;
	static Vector2 _scrollPosition = Vector2.zero;

	[MenuItem("Build/Build AB Test", false, 53)]
	static void InitWindow()
	{
		InitConfig();
		ShowWindow();
	}

	static void InitConfig()
	{
		_abTestConfig = AssetDatabase.LoadAssetAtPath<BuildABTestAssetBundleConfig>(BuildABTestAssetBundleConfig._configFilePath);
		if(_abTestConfig == null)
		{
			_abTestConfig = new BuildABTestAssetBundleConfig();
			AssetDatabase.CreateAsset(_abTestConfig, BuildABTestAssetBundleConfig._configFilePath);
		}
	}

	static void ShowWindow()
	{
		Rect rect = new Rect (0, 0, 500, 700);
		_window = (BuildABTestWindow)EditorWindow.GetWindowWithRect(typeof(BuildABTestWindow), rect, true);
		_window.maxSize = new Vector2(700, 1200);
		_window.Show();
	}

	void OnGUI()
	{
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

		GUILayout.Label("*********************************************************************");
		GUILayout.Label("*                           AB Test                                 *");
		GUILayout.Label("*********************************************************************");

		_abTestConfig._platformType = (PlatformType)EditorGUILayout.EnumPopup("PlatformType", _abTestConfig._platformType);
		_abTestConfig._bundleName = EditorGUILayout.TextField("BundleName", _abTestConfig._bundleName);
		_abTestConfig._version = EditorGUILayout.TextField("Version", _abTestConfig._version);

		GUILayout.Space(20);

		GUILayout.Label("AB Versions:");
		for(int i = 0; i < _abTestConfig._abVersions.Count; i++)
		{
			GUILayout.BeginHorizontal();

			_abTestConfig._abVersions[i] = EditorGUILayout.TextField("Version:", _abTestConfig._abVersions[i]);
			string versionName = _abTestConfig._abVersions[i];

			GUILayout.Space(10);

			if(GUILayout.Button("AB -> Project", GUILayout.Width(90)))
			{
				BuildTarget target = BuildAssetBundleHelper.GetBuildTarget(_abTestConfig._platformType);
				if(target != BuildTarget.NoTarget)
				{
					ExcelDirType dirType = BuildAssetBundleHelper.GetExcelDirType(target);
					BuildABTestHelper.CopyABToProject(versionName, dirType);
				}
				else
				{
					Debug.LogError("Can't copy since platformType is wrong");
				}
			}

			GUILayout.Space(10);

			if(GUILayout.Button("-", GUILayout.Width(30)))
			{
				_abTestConfig._abVersions.RemoveAt(i);
				i--;
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(30)))
		{
			_abTestConfig._abVersions.Insert(_abTestConfig._abVersions.Count, "");
		}

		GUILayout.Space(20);

		GUILayout.Label("Excel File Names:");
		for(int i = 0; i < _abTestConfig._excelFileNames.Count; i++)
		{
			GUILayout.BeginHorizontal();

			_abTestConfig._excelFileNames[i] = EditorGUILayout.TextField("Name:", _abTestConfig._excelFileNames[i]);

			GUILayout.Space(10);

			if(GUILayout.Button("-", GUILayout.Width(30)))
			{
				_abTestConfig._excelFileNames.RemoveAt(i);
				i--;
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(30)))
		{
			_abTestConfig._excelFileNames.Insert(_abTestConfig._excelFileNames.Count, "");
		}

		GUILayout.Space(20);

		GUILayout.Label("Resource Paths:");
		for(int i = 0; i < _abTestConfig._resourcePaths.Count; i++)
		{
			GUILayout.BeginHorizontal();

			string path = _abTestConfig._resourcePaths[i];
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
						_abTestConfig._resourcePaths[i] = trimPath;
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
				_abTestConfig._resourcePaths.RemoveAt(i);
			}

			if(GUILayout.Button("+", GUILayout.Width(30)))
			{
				_abTestConfig._resourcePaths.Insert(i + 1, "");
			}

			GUILayout.EndHorizontal();
		}

		if(GUILayout.Button("+", GUILayout.Width(30)))
		{
			_abTestConfig._resourcePaths.Insert(_abTestConfig._resourcePaths.Count, "");
		}

		GUILayout.Space(20);

		if(GUILayout.Button("Build ABTest AssetBundles", GUILayout.Width(180.0f)))
			BuildABTestButtonDown();

		GUILayout.Space(20);

		if(GUILayout.Button("Save config", GUILayout.Width(100)))
			BuildAssetBundleHelper.SaveConfigButtonDown(_abTestConfig);

		GUILayout.EndScrollView();
	}

	void BuildABTestButtonDown()
	{
		bool result = BuildABTestAssets();
		if(result)
			ShowNotification(new GUIContent("Done!"));
		else
			ShowNotification(new GUIContent("Error"));
	}

	static bool BuildABTestAssets()
	{
		bool result = false;
		BuildTarget target = BuildAssetBundleHelper.GetBuildTarget(_abTestConfig._platformType);
		if(target != BuildTarget.NoTarget)
		{
			//Note: Important fix. If don't switch platform, everytime before building AssetBundle,
			//it will take much time to switch to the particular platform.
			PerformBuild.SwitchBuildPlatform(target);

			ExcelDirType dirType = BuildAssetBundleHelper.GetExcelDirType(target);
			result = true;
			foreach(string abVersion in _abTestConfig._abVersions)
			{
				string path = BuildAssetBundleHelper.GetAssetBundlePath(_abTestConfig._platformType);
				BuildAssetBundleHelper.DeleteAllFilesInDir(path);

				BuildABTestHelper.CopyABToProject(abVersion, dirType);

				string version = _abTestConfig._version + "." + abVersion;

				List<string> allResPaths = BuildAssetBundleHelper.GetAllExcelResourcePaths(dirType, _abTestConfig._excelFileNames);
				allResPaths.AddRange(_abTestConfig._resourcePaths);

				bool r = BuildAssetBundles.BuildBundlesFromMap(path, version, _abTestConfig._platformType, target, 
					true, _abTestConfig._bundleName, allResPaths);

				if(!r)
					result = false;
			}
		}
		return result;
	}

	static void BuildABTestCommandLine()
	{
		InitConfig();
		bool result = BuildABTestAssets();
		if(result)
			Debug.Log("Succeed to build AB test assets");
		else
			Debug.Log("Fail to build AB test assets");
	}
}