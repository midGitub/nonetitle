using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

#if CORE_DLL
using UnityQuickSheet;
#endif

public class CoreAssetManager : SimpleSingleton<CoreAssetManager>
{
	public SheetType LoadExcelAsset<SheetType, DataType>(string subDir, string excelName, string sheetName) 
	#if CORE_DLL
		where SheetType : IExcelSheet<DataType>, new()
	#else
		where SheetType : UnityEngine.Object
	#endif
	{
		#if CORE_DLL
		return LoadExcelAssetInDllMode<SheetType, DataType>(subDir, excelName, sheetName);
		#else
		return LoadExcelAssetInRuntimeMode<SheetType, DataType>(subDir, excelName, sheetName);
		#endif
	}

	#if CORE_DLL

	SheetType LoadExcelAssetInDllMode<SheetType, DataType>(string subDir, string excelName, string sheetName) 
		where SheetType : IExcelSheet<DataType>, new()
	{
		string totalDir = Path.Combine(Environment.CurrentDirectory, "../../Excels/");
		totalDir = Path.Combine(totalDir, subDir);
		string path = totalDir + excelName + ".xls";
		ExcelQuery query = new ExcelQuery(path, sheetName);

		SheetType result = new SheetType();
		result.DataArray = query.Deserialize<DataType>().ToArray();
		return result;
	}

	#else

	SheetType LoadExcelAssetInRuntimeMode<SheetType, DataType>(string subDir, string excelName, string sheetName) 
		where SheetType : UnityEngine.Object
	{
		SheetType result = AssetManager.Instance.LoadExcelAsset<SheetType>(subDir, excelName, sheetName);
		return result;
	}

	#endif
}

