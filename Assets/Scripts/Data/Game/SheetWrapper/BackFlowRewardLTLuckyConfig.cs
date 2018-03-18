using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackFlowRewardLTLuckyConfig : SimpleSingleton<BackFlowRewardLTLuckyConfig>
{
	public static readonly string Name = "BackFlowRewardLTLucky";

	private BackFlowRewardLTLuckySheet _sheet;

	public BackFlowRewardLTLuckyConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<BackFlowRewardLTLuckySheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload BackFlowRewardLTLucky");
		BackFlowRewardLTLuckyConfig.Instance.LoadData();
	}

	public int GetLinerX(int days)
	{
		return GetDataWithDays (days).LinearX;
	}

	public int GetLinerY(int days)
	{
		return GetDataWithDays (days).LinearY;
	}

	public BackFlowRewardLTLuckyData GetDataWithDays(int days)
	{
		for (int i = 1; i < _sheet.dataArray.Length; i++) 
		{
			BackFlowRewardLTLuckyData d = _sheet.dataArray [i];
			if (d.Days>days)
				return _sheet.dataArray [i - 1];
		}
		return _sheet.dataArray [_sheet.dataArray.Length - 1];
	}

}
