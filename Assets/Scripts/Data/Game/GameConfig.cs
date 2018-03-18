using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : SimpleSingleton<GameConfig>
{
	public static readonly string ExcelName = "GameSetting";

	public T LoadExcelAsset<T>(string sheetName) where T : UnityEngine.Object
	{
		return AssetManager.Instance.LoadExcelAsset<T>(GameConfigTable.SubDir, ExcelName, sheetName);
	}
}
