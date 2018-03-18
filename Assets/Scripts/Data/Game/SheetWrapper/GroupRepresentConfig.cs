using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum GroupRepresent
{
	PayAmount,
	PayCount,
	HistoryMaxPay,
//	LastLogin,
	Spin,
	Session
}
	
public class GroupRepresentConfig : SimpleSingleton<GroupRepresentConfig>
{
	public static readonly string Name = "GroupRepresent";
	private GroupRepresentSheet _sheet;
	private int _validDays = 7;

	private Dictionary<GroupRepresent,string> _representDic = new Dictionary<GroupRepresent,string>()
	{
		{GroupRepresent.PayAmount,"PayAmout"},
		{GroupRepresent.PayCount,"PayCount"},
		{GroupRepresent.HistoryMaxPay,"HistoryMaxPay"},
//		{GroupRepresent.LastLogin,"LastLogin"},
		{GroupRepresent.Spin,"Spin"},
		{GroupRepresent.Session,"Session"}
	};

	public GroupRepresentConfig()
	{
		LoadData ();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<GroupRepresentSheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload GroupRepresent");
		GroupRepresentConfig.Instance.LoadData ();
	}

	public int GetHasPayInValidPeriodRepresent(DateTime date)
	{
		return Convert.ToInt32(TimeUtility.IsInPeriodDays (date, _validDays));
	}
		
	public int GetLen(GroupRepresent type)
	{
		string name	= _representDic [type];
		int result = 0;
		int index = ListUtility.Find (_sheet.DataArray, (GroupRepresentData data) => {
			return data.Model.Equals(name);
		});
		if (index != -1)
			result = _sheet.DataArray [index].PresentValue.Length;
		return result;
	}

	public int GetRepresent(int value,GroupRepresent type)
	{
		string name	= _representDic [type];
		int result = 0;
		int index = ListUtility.Find (_sheet.DataArray, (GroupRepresentData data) => {
			return data.Model.Equals(name);
		});
		if (index != -1)
			result = GetIndex (value, _sheet.DataArray [index].RealValue);
		if (result < _sheet.dataArray [index].PresentValue.Length)
			return _sheet.dataArray [index].PresentValue [result];
		else
		{
			Debug.Assert(false, "error group represent excel");
			return _sheet.dataArray [index].PresentValue [0];
		}
	}

	int GetIndex(int value,int[] arr)
	{
		int result = arr.Length;
		int index = ListUtility.Find (arr, (int data) => {
			return value < data ;
		});
		if (index != -1)
			result = index;
		return result;
	}
}


