using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocalNotificationfestivalConfig :SimpleSingleton<LocalNotificationfestivalConfig> {

	public static readonly string Name = "LocalNotificationfestival";
	public static readonly string ExcelName = "LocalNotification";

	private LocalNotificationfestivalSheet _sheet;

	public LocalNotificationfestivalConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = AssetManager.Instance.LoadExcelAsset<LocalNotificationfestivalSheet>
			(LocalNotificationConfigTable.SubDir, ExcelName, Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload LocalNotificationfestival");
		LocalNotificationfestivalConfig.Instance.LoadData();
	}

	public LocalNotificationfestivalSheet Sheet
	{
		get
		{
			return _sheet;
		}
	}

	public LocalNotificationfestivalData GetData(int id){
		LocalNotificationfestivalData result = null;
		int i = ListUtility.Find<LocalNotificationfestivalData>(_sheet.DataArray, (LocalNotificationfestivalData data)=>{
			return data.ID == id;
		});
		if (i >= 0){
			result = _sheet.DataArray[i];
		}
		return result;
	}
}
