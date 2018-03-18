using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class BuildExcelAssetBundles
{
	static readonly List<string> _allExcelFileNames = new List<string>();
	static readonly List<string> _allExcelSubDirs = new List<string>();
	static string _buildOutputPath = "AssetBundles/ExcelBundles/";
	static string _streamingPath = "StreamingAssets/";
	static string _tempDir = "../Assets_Temp/";

	static BuildExcelAssetBundles()
	{
		// Caution by nichos:
		// Only add part of important excels for now, you could add more in the future
		_allExcelFileNames.Add(CoreConfig.ExcelName);
		_allExcelFileNames.AddRange(CoreDefine.AllMachineNames);
		_allExcelFileNames.Add(GameConfig.ExcelName);

		_allExcelSubDirs.Add(CoreConfigTable.SubDir);
		_allExcelSubDirs.Add(MachineConfigTable.SubDir);
		_allExcelSubDirs.Add(GameConfigTable.SubDir);
	}

	[MenuItem("Build/Build iOS Excel Bundles", false, 101)]
	public static void BuildiOSExcels()
	{
	    RemoveLastBuildExcelAssets();
        if (AssetConfig.IsUseExcelAssetBundle)
			BuildExcels(PlatformType.iOS, BuildTarget.iOS);
	}

	[MenuItem("Build/Build Android Excel Bundles", false, 102)]
	public static void BuildAndroidExcels()
	{
	    RemoveLastBuildExcelAssets();
        if (AssetConfig.IsUseExcelAssetBundle)
			BuildExcels(PlatformType.Android, BuildTarget.Android);
	}

	//[MenuItem("Build/Test Pre", false, 103)]
	public static void PreProcessBuildPlayer()
	{
		if(AssetConfig.IsUseExcelAssetBundle)
			MoveExcelResourcesOut();
	}

	//[MenuItem("Build/Test Post", false, 104)]
	public static void PostProcessBuildPlayer()
	{
		if(AssetConfig.IsUseExcelAssetBundle)
			MoveExcelResourcesIn();
	}

    static void RemoveLastBuildExcelAssets()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.dataPath, _streamingPath));
        FileInfo[] infos = directoryInfo.GetFiles("*_excel*");
        foreach (FileInfo info in infos)
        {
            info.Delete();
        }
    }

    static void MoveExcelResourcesOut()
	{
		//In case the last build fails
//		string topTempDir = GetMoveTempDir("");
//		if(Directory.Exists(topTempDir))
//			Directory.Delete(topTempDir, true);

		foreach(string topDir in ExcelConfig.TopDirs)
		{
			string tempDir = GetMoveTempDir(topDir);
			Directory.CreateDirectory(tempDir);

			foreach(string subDir in _allExcelSubDirs)
			{
				string p = topDir + subDir;
				string from = GetMoveResourcesDir(p);
				string to = GetMoveTempDir(p);
				if(Directory.Exists(from) && !Directory.Exists(to))
				{
					Directory.Move(from, to);
				}
			}
		}

		AssetDatabase.Refresh();
	}

	static void MoveExcelResourcesIn()
	{
		foreach(string topDir in ExcelConfig.TopDirs)
		{
			foreach(string subDir in _allExcelSubDirs)
			{
				string p = topDir + subDir;
				string from = GetMoveTempDir(p);
				string to = GetMoveResourcesDir(p);
				if(Directory.Exists(from))
				{
					Debug.Assert(!Directory.Exists(to));
					Directory.Move(from, to);
				}
			}
		}

		Directory.Delete(GetMoveTempDir(""), true);
		AssetDatabase.Refresh();
	}

	static void MoveExcelFolder(string from, string to)
	{
		DirectoryInfo fromInfo = new DirectoryInfo(from);
		FileInfo[] infos = fromInfo.GetFiles("*.asset");
		foreach(FileInfo info in infos)
		{
			AssetDatabase.MoveAsset(from + info.Name, to + info.Name);
		}
	}

	static string GetMoveResourcesDir(string p)
	{
		return Path.Combine(Application.dataPath, Path.Combine("Resources/", p));
		//return Path.Combine("Assets/Resources/", p);
	}

	static string GetMoveTempDir(string p)
	{
		return Path.Combine(Application.dataPath, Path.Combine(_tempDir, p));
		//return Path.Combine("Assets/Temp/", p);
	}

	static bool BuildSingleExcel(PlatformType platform, BuildTarget target, string excelName, string outputPath, ExcelDirType dirType)
	{
		List<string> allResPaths = BuildAssetBundleHelper.GetSingleExcelResourcePaths(dirType, excelName);
		string bundleName = ExcelConfig.GetBundleName(dirType, excelName);
		bool result = BuildAssetBundles.BuildBundlesFromMap(outputPath, "", platform, target, false, bundleName, allResPaths);
		if(!result)
			Debug.Log("Warn: Fail to build excel: " + excelName);

		return result;
	}

	static void EncryptAndCopyFile(string excelName, ExcelDirType dirType)
	{
		string name = excelName.ToLower();
		string bundleName = ExcelConfig.GetBundleName(dirType, name);
		string src = Path.Combine(Path.Combine(Application.dataPath, _buildOutputPath), bundleName);
		string dest = Path.Combine(Path.Combine(Application.dataPath, _streamingPath), bundleName);

		if(File.Exists(src))
		{
			byte[] originalBytes = File.ReadAllBytes(src);
			byte[] encryptedBytes = null;
			if(AssetConfig.IsExcelAssetBundleEncrypted)
				encryptedBytes = EncryptUtility.AES_Encrypt(originalBytes);
			else
				encryptedBytes = originalBytes;
			File.WriteAllBytes(dest, encryptedBytes);
		}
		else
		{
			//Debug.LogError("EncryptAndCopyFile fail: file doesn't exist: " + src);
		}
	}

	static void BuildExcels(PlatformType platform, BuildTarget target)
	{
		//0 clean directory
		BuildAssetBundleHelper.DeleteAllFilesInDir(_buildOutputPath);

		//1 build
		foreach(string name in _allExcelFileNames)
		{
			for(int i = 0; i < (int)ExcelDirType.Count; i++)
			{
				ExcelDirType dirType = (ExcelDirType)i;
				BuildSingleExcel(platform, target, name, _buildOutputPath, dirType);
			}
		}

		//2 encrypt and copy
		foreach(string name in _allExcelFileNames)
		{
			for(int i = 0; i < (int)ExcelDirType.Count; i++)
			{
				ExcelDirType dirType = (ExcelDirType)i;
				EncryptAndCopyFile(name, dirType);
			}
		}
	}
}
