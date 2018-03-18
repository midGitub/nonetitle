using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;

public static class BuildABTestHelper
{
	static public void CopyABToProject(string versionName, ExcelDirType dirType)
	{
		CopyBetweenProjectAndAB(versionName, dirType, true);
	}

	static public void CopyProjectToAB(string versionName, ExcelDirType dirType)
	{
		CopyBetweenProjectAndAB(versionName, dirType, false);
	}

	static void CopyBetweenProjectAndAB(string versionName, ExcelDirType dirType, bool isABToProject)
	{
		string excelTopDir = ExcelConfig.GetTopDir(dirType);

		string abDir = GetCurrentVersionPath(versionName);
		if(!Directory.Exists(abDir))
		{
			Debug.LogError("Error: directory doesn't exist:" + abDir);
			return;
		}

		List<FileInfo> abInfos = BuildAssetBundleHelper.GetUsefulFileInfosFromDir(abDir);

		string projectPath = Application.dataPath;
		List<FileInfo> allInfos = BuildAssetBundleHelper.GetUsefulFileInfosFromDir(projectPath);

		foreach(FileInfo abInfo in abInfos)
		{
			bool isExcel = abInfo.Name.EndsWith(".xls");

			FileInfo projectInfo = ListUtility.FindFirstOrDefault(allInfos, (FileInfo i) => {
				//Debug.Log("name:" + i.Name);
				bool result = abInfo.Name == i.Name;
				if(result && isExcel)
					result = IsContainDirectory(i.FullName, excelTopDir);
				return result;
			});

			if(projectInfo == null)
			{
				string errStr = "Can't find corresponding project file:" + abInfo.Name;
				Debug.LogError(errStr);
			}
			else
			{
				if(isABToProject)
				{
					Debug.Log("copy from " + abInfo.FullName);
					Debug.Log("copy to " + projectInfo.FullName);

					File.Copy(abInfo.FullName, projectInfo.FullName, true);

					string relativePath = BuildAssetBundleHelper.TrimToRelativePath(projectInfo.FullName);
					AssetDatabase.ImportAsset(relativePath);
				}
				else
				{
					File.Copy(projectInfo.FullName, abInfo.FullName, true);
				}
			}
		}
	}

	static string GetCurrentVersionPath(string versionName)
	{
		string path = Path.Combine(Application.dataPath, "../Assets_ABTest/");
		path = Path.Combine(path, versionName);
		return path;
	}

	//Note: be careful of the difference between Windows and Mac
	static bool IsContainDirectory(string path, string dir)
	{
		string[] dirs = dir.Split(new char[]{ '/' });
		dir = dirs[0];
		string[] paths = path.Split(Path.DirectorySeparatorChar);
		bool result = ListUtility.IsContainElement(paths, dir);
		return result;
	}
}

