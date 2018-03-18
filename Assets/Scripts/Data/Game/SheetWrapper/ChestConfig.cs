using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestConfig : SimpleSingleton<ChestConfig>
{
	public static readonly string Name = "Chest";

	private ChestSheet _sheet;

	public ChestConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<ChestSheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload Chest");
		ChestConfig.Instance.LoadData();
	}
		
	public int GetCDTime(float Money)
	{
		return GetDataWithMoney (Money).CDTime;
	}

	public int GetLTLucky(float Money)
	{
		return GetDataWithMoney (Money).LTLucky;
	}

	public int[] GetReward(float Money)
	{
		return GetDataWithMoney (Money).Reward;
	}

	public int[] GetWeight(float Money)
	{
		return GetDataWithMoney (Money).Weights;
	}

	public ChestData GetDataWithMoney(float Money)
	{
		for (int i = 1; i < _sheet.dataArray.Length; i++) 
		{
			ChestData d = _sheet.dataArray [i];
			if (d.Money > Money)
				return _sheet.dataArray [i - 1];
		}
		return _sheet.dataArray [_sheet.dataArray.Length - 1];
	}
		
}
