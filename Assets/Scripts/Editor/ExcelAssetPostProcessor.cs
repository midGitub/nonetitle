using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityQuickSheet;
using System;
using System.Reflection;

public class ExcelAssetPostProcessConfig
{
	public string _importPath;
	public string _exportPath;
	public ExcelSheetInfo _sheetInfo;

	public ExcelAssetPostProcessConfig(string importPath, string exportPath, ExcelSheetInfo sheetInfo)
	{
		_importPath = importPath;
		_exportPath = exportPath;
		_sheetInfo = sheetInfo;
	}
}

public class ExcelAssetPostprocessor : AssetPostprocessor
{
	static readonly string _importExcelTopDir = "Assets/";
	static readonly string _exportAssetTopDir = "Assets/Resources/";

	static readonly List<ExcelAssetPostProcessConfig> _configs = new List<ExcelAssetPostProcessConfig>();

	#region Init

	static ExcelAssetPostprocessor()
	{
		_configs.Clear();

		foreach(string excelDir in ExcelConfig.TopDirs)
		{
			AddCoreConfigAssets(excelDir);
			AddMachineConfigAssets(excelDir);
			AddGameConfigAssets(excelDir);
			AddLocalizationConfigAssets(excelDir);
			AddLocalNotificationConfigAssets(excelDir);
		}
	}

	static void AddCoreConfigAssets(string excelDir)
	{
		string importPath = GetImportPath(excelDir, CoreConfigTable.SubDir);
		string exportPath = GetExportPath(excelDir, CoreConfigTable.SubDir);
		ListUtility.ForEach(CoreConfigTable.SheetInfos, (ExcelSheetInfo sheetInfo) => {
			ExcelAssetPostProcessConfig config = new ExcelAssetPostProcessConfig(importPath, exportPath, sheetInfo);
			_configs.Add(config);
		});
	}

	static void AddMachineConfigAssets(string excelDir)
	{
		string importPath = GetImportPath(excelDir, MachineConfigTable.SubDir);
		string exportPath = GetExportPath(excelDir, MachineConfigTable.SubDir);
		ListUtility.ForEach(MachineConfigTable.SheetInfos, (ExcelSheetInfo sheetInfo) => {
			ExcelAssetPostProcessConfig config = new ExcelAssetPostProcessConfig(importPath, exportPath, sheetInfo);
			_configs.Add(config);
		});
	}

	static void AddGameConfigAssets(string excelDir)
	{
		string importPath = GetImportPath(excelDir, GameConfigTable.SubDir);
		string exportPath = GetExportPath(excelDir, GameConfigTable.SubDir);
		ListUtility.ForEach(GameConfigTable.SheetInfos, (ExcelSheetInfo sheetInfo) => {
			ExcelAssetPostProcessConfig config = new ExcelAssetPostProcessConfig(importPath, exportPath, sheetInfo);
			_configs.Add(config);
		});
	}

	static void AddLocalizationConfigAssets(string excelDir)
	{
		string importPath = GetImportPath(excelDir, LocalizationConfigTable.SubDir);
		string exportPath = GetExportPath(excelDir, LocalizationConfigTable.SubDir);
		ListUtility.ForEach(LocalizationConfigTable.SheetInfos, (ExcelSheetInfo sheetInfo) => {
			ExcelAssetPostProcessConfig config = new ExcelAssetPostProcessConfig(importPath, exportPath, sheetInfo);
			_configs.Add(config);
		});
	}

	static void AddLocalNotificationConfigAssets(string excelDir)
	{
		string importPath = GetImportPath(excelDir, LocalNotificationConfigTable.SubDir);
		string exportPath = GetExportPath(excelDir, LocalNotificationConfigTable.SubDir);
		ListUtility.ForEach(LocalNotificationConfigTable.SheetInfos, (ExcelSheetInfo sheetInfo) => {
			ExcelAssetPostProcessConfig config = new ExcelAssetPostProcessConfig(importPath, exportPath, sheetInfo);
			_configs.Add(config);
		});
	}

	#endregion

	#region Utility

	static string GetImportPath(string excelDir, string subDir)
	{
		return _importExcelTopDir + excelDir + subDir;
	}

	static string GetExportPath(string excelDir, string subDir)
	{
		return _exportAssetTopDir + excelDir + subDir;
	}

	#endregion

	#region Post Process
    
    static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string importFilePath in importedAssets) 
        {
			if(importFilePath.EndsWith(".xls"))
			{
				for(int i = 0; i < _configs.Count; i++)
				{
					string importPath = _configs[i]._importPath;
					string exportPath = _configs[i]._exportPath;
					ExcelSheetInfo sheetInfo = _configs[i]._sheetInfo;

					if(importFilePath.StartsWith(importPath))
					{
						string sheetName = sheetInfo.SheetName;
						Type dataType = sheetInfo.ReflectDataType;
						Type dataArrayType = dataType.MakeArrayType();
						Type sheetType = sheetInfo.ReflectionSheetType;

						ExcelQuery query = new ExcelQuery(importFilePath, sheetName);
						if (query != null && query.IsValid())
						{
							IList queryDatas = query.Deserialize(dataType);
							Array dataArray = (Array)queryDatas.Send("ToArray", null);

							if(dataArray != null && dataArray.Length > 0)
							{
								string excelFileName = Path.GetFileNameWithoutExtension(importFilePath);
								string assetFileName = excelFileName + "_" + sheetName + ".asset";
								string assetFilePath = Path.Combine(exportPath, assetFileName);

								var sheet = AssetDatabase.LoadAssetAtPath(assetFilePath, sheetType);
								if (sheet == null)
								{
									sheet = ScriptableObject.CreateInstance(sheetType);

									sheet.SetFieldValue("SheetName", importFilePath);
									sheet.SetFieldValue("WorksheetName", sheetName);

									AssetDatabase.CreateAsset ((ScriptableObject)sheet, assetFilePath);
								}

								sheet.SetFieldValue("dataArray", dataArray);

								ScriptableObject obj = AssetDatabase.LoadAssetAtPath (assetFilePath, typeof(ScriptableObject)) as ScriptableObject;
								EditorUtility.SetDirty (obj);
								AssetDatabase.SaveAssets();
							}
						}
					}
				}
			}
        }
    }

	#endregion
}
