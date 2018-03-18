using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiggyBankConfig : SimpleSingleton<PiggyBankConfig>
{
	public static readonly string Name = "PiggyBank";

	private PiggyBankSheet _piggyBankSheet;

	public PiggyBankConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_piggyBankSheet = GameConfig.Instance.LoadExcelAsset<PiggyBankSheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload PiggyBankConfig");
		PiggyBankConfig.Instance.LoadData();
	}

	public PiggyBankData GetFirstPiggyBankData()
	{
		return ListUtility.First(_piggyBankSheet.dataArray);
	}

	public PiggyBankData GetLastPiggyBankData()
	{
		return ListUtility.Last(_piggyBankSheet.dataArray);
	}

	public PiggyBankData FindPiggyBankDataWithCoins(int coins)
	{
		for(int i = 0; i < _piggyBankSheet.dataArray.Length; i++)
		{
			var data = _piggyBankSheet.dataArray[i];

			// 到达最后一行 0为无限大
			if(data.MaxCredits == 0)
			{
				return data;
			}
			if(data.MinCredits <= coins && coins < data.MaxCredits)
			{
				return data;
			}
		}
		Debug.LogError("钱不在范围出现错误");
		return null;
	}
}
