using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;

public static class BuildAssetBundleHelper
{
	private static readonly string[] _filterFiles = new string[] { ".DS_Store", ".svn", ".meta" };
	private static readonly string _resourceDir = "Resources/";

	public static string TrimToRelativePath(string path)
	{
		string result = "";

		// the path should be relative not absolute one to make it work on any platform.
		int index = path.IndexOf("Assets");
		if(index >= 0)
			result = path.Substring(index);

		return result;
	}

	public static BuildTarget GetBuildTarget(PlatformType type)
	{
		BuildTarget target = BuildTarget.NoTarget;
		switch(type)
		{
			case PlatformType.iOS:
				target = BuildTarget.iOS;
				break;
			case PlatformType.Android:
				target = BuildTarget.Android;
				break;
			case PlatformType.Web:
				target = BuildTarget.WebGL;
				break;
			default:
				target = BuildTarget.Android;
				break;
		}
		return target;
	}

	public static ExcelDirType GetExcelDirType(BuildTarget target)
	{
		ExcelDirType result = ExcelDirType.Default;
		switch(target)
		{
			case BuildTarget.Android:
				result = ExcelDirType.Default;
				break;
			case BuildTarget.iOS:
				result = ExcelDirType.IOS;
				break;
			default:
				Debug.Assert(false);
				break;
		}
		return result;
	}

	public static void SaveConfigButtonDown(ScriptableObject config)
	{
		EditorUtility.SetDirty(config);
		AssetDatabase.SaveAssets();
	}

	public static string OpenFilePanel(string initPath)
	{
		string folder = Path.GetDirectoryName(initPath);
		string path = EditorUtility.OpenFilePanel("Open file", folder, "asset,png,prefab");
		return path;
	}

	public static void DeleteAllFilesInDir(string dir)
	{
		string wholeDir = Path.Combine(Application.dataPath, dir);
		if(Directory.Exists(wholeDir))
		{
			DirectoryInfo dirInfo = new DirectoryInfo(wholeDir);
			FileInfo[] fileInfos = dirInfo.GetFiles();
			foreach(FileInfo info in fileInfos)
			{
				File.Delete(info.FullName);
			}
		}
	}

	public static string GetAssetBundlePath(PlatformType type)
	{
		return "AssetBundles/" + type.ToString();
	}

	public static string GetDefaultResourcePath()
	{
		return Path.Combine(Application.dataPath, "Resources");
	}

	public static List<FileInfo> GetUsefulFileInfosFromDir(string path)
	{
		List<FileInfo> fileInfos = GetFileInfosFromDir(path);
		List<FileInfo> result = ListUtility.FilterList(fileInfos, (FileInfo info) => {
			return !ListUtility.IsContainElement(_filterFiles, info.Name);
		});
		return result;
	}

	static List<FileInfo> GetFileInfosFromDir(string path)
	{
		Debug.Assert(Directory.Exists(path));
		DirectoryInfo dirInfo = new DirectoryInfo(path);
		FileInfo[] fileInfos = dirInfo.GetFiles("*", SearchOption.AllDirectories);
		List<FileInfo> result = ListUtility.FilterList(fileInfos, (FileInfo info) => {
			return !info.FullName.Contains(".svn");
		});
		return result;
	}

	public static List<string> GetSingleExcelResourcePaths(ExcelDirType dirType, string excelName)
	{
		string resourcePath = _resourceDir + ExcelConfig.GetTopDir(dirType);
		string excelPath = Path.Combine(Application.dataPath, resourcePath);
		List<FileInfo> fileInfos = GetUsefulFileInfosFromDir(excelPath);
		List<string> paths = new List<string>();
		string prefix = excelName + "_";

		foreach(var info in fileInfos)
		{
			if(info.Name.Contains(prefix) && !info.Name.Contains(".meta"))
			{
				string p = TrimToRelativePath(info.FullName);
				paths.Add(p);
			}
		}

		if(paths.Count == 0)
			Debug.Log("Warning: GetSingleExcelResourcePaths is empty: " + excelName);

		return paths;
	}

	public static List<string> GetAllExcelResourcePaths(ExcelDirType dirType, List<string> excelNames)
	{
		List<string> result = new List<string>();
		foreach(var name in excelNames)
		{
			List<string> paths = GetSingleExcelResourcePaths(dirType, name);
			result.AddRange(paths);
		}
		return result;
	}
}

