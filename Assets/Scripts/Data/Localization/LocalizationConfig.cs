using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationConfig : SimpleSingleton<LocalizationConfig> 
{
	public readonly static string Name = "Content";
	public readonly static string ExcelName = "Localization";

	private ContentSheet _contentSheet;
	private Dictionary<string, string> _contentDict = new Dictionary<string, string>();

	public LocalizationConfig(){
		LoadRawData ();
		InitContentDict ();
	}

	private void LoadRawData(){
		_contentSheet = AssetManager.Instance.LoadExcelAsset<ContentSheet>
			(LocalizationConfigTable.SubDir, ExcelName, Name);
	}

	private void InitContentDict(){
		if (_contentSheet != null) {
			ContentData[] dataArray = _contentSheet.dataArray;
			int max = dataArray.Length;
			for (int i = 0; i < max; ++i) {
				if (!_contentDict.ContainsKey (dataArray [i].Key)) {
					_contentDict.Add(dataArray[i].Key, GetRegionValue(dataArray[i]));
				} else {
					LogUtility.Log ("content repeat key : " + dataArray[i].Key, Color.red);
				}
			}
		}
	}

	private string GetRegionValue(ContentData data){
		if (data == null)
			return "";

		// TODO: 根据语言来区分返回值，当前只对应英语
		return data.Eng;
	}

	public string GetValue(string key){
		if (_contentDict.ContainsKey (key)) {
			return _contentDict [key];
		} 

		LogUtility.Log ("Get localization value failed key = " + key, Color.red);
		return "";
	}
}
