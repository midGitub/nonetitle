using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocalNotificationConfig : SimpleSingleton<LocalNotificationConfig> {

	public static readonly string Name = "LocalNotification";
	public static readonly string ExcelName = "LocalNotification";

	private LocalNotificationSheet _sheet;

	public LocalNotificationConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = AssetManager.Instance.LoadExcelAsset<LocalNotificationSheet>
			(LocalNotificationConfigTable.SubDir, ExcelName, Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload LocalNotification");
		LocalNotificationConfig.Instance.LoadData();
	}

	public LocalNotificationData GetData(string key)
	{
		LocalNotificationData result = null;
		foreach (LocalNotificationData data in _sheet.DataArray)
		{
			if (data.Key == key)
			{
				result = data;
			}
		}
		return result;
	}

	public List<LocalNotificationData> GetDataList(string key)
	{
		List<LocalNotificationData> result = new List<LocalNotificationData>();
		foreach (LocalNotificationData data in _sheet.DataArray)
		{
			if (data.Key.Contains(key))
			{
				result.Add(data);
			}
		}
		return result;
	}

	public LocalNotificationData GetData(int id){
		LocalNotificationData result = null;
		int i = ListUtility.Find<LocalNotificationData>(_sheet.DataArray, (LocalNotificationData data)=>{
			return data.ID == id;
		});
		if (i >= 0){
			result = _sheet.DataArray[i];
		}
		return result;
	}
}
