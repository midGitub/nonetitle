using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public enum ExcelDirType
{
	Default = 0,
	IOS = 1,
	Count
}

public class ExcelSheetInfo
{
	public string SheetName;
	public Type ReflectDataType;
	public Type ReflectionSheetType;

	public ExcelSheetInfo(string name, Type dataType, Type sheetType)
	{
		SheetName = name;
		ReflectDataType = dataType;
		ReflectionSheetType = sheetType;
	}
}

public static class ExcelConfig
{
	private static string DefaultTopDir = "Excels/";
	private static string IOSTopDir = "ExcelsIOS/";

	public static string[] TopDirs = new string[(int)ExcelDirType.Count] {
		DefaultTopDir,
		IOSTopDir
	};

	public static string[] ExcelAssetBundlePostFixName = new string[(int)ExcelDirType.Count] {
		"excel",
		"excelios"
	};

	// Caution: Only use this field in runtime, not in editor
	private static List<ExcelDirType> _priorityDirTypes = new List<ExcelDirType>();
	public static List<ExcelDirType> PriorityDirTypes
	{
		get { return _priorityDirTypes; }
	}

	static ExcelConfig()
	{
		InitPriorityDirTypes();
	}

	public static string GetTopDir(ExcelDirType dirType)
	{
		return TopDirs[(int)dirType];
	}

	public static string GetExportFileName(string excelName, string sheetName)
	{
		return excelName + "_" + sheetName;
	}

	public static string GetBundleName(ExcelDirType dirType, string excelName)
	{
		return excelName + "_" + ExcelAssetBundlePostFixName[(int)dirType];
	}

	static void InitPriorityDirTypes()
	{
		_priorityDirTypes.Clear();

		#if UNITY_IPHONE
		_priorityDirTypes.Add(ExcelDirType.IOS);
		_priorityDirTypes.Add(ExcelDirType.Default);
		#else
		_priorityDirTypes.Add(ExcelDirType.Default);
		_priorityDirTypes.Add(ExcelDirType.IOS);
		#endif
	}
}
