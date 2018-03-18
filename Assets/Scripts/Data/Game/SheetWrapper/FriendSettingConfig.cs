using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendSettingConfig  : SimpleSingleton<FriendSettingConfig>
{
	public static readonly string Name = "FriendSetting";

	private FriendSettingSheet _sheet;

	public int CollectNumMax{ get; set; }
	public int SendNumMax { get; set; }
	public float CreditsToLuckyFactor { get; set; }
	public int GiftCoins { get; set; }

	public FriendSettingConfig()
	{
		LoadData();
	}

	void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<FriendSettingSheet>(Name);

		CollectNumMax = GetIntValueFromKey("CollectNumMax");
		SendNumMax = GetIntValueFromKey ("SendNumMax");
		CreditsToLuckyFactor = GetFloatValueFromKey ("CreditsToLuckyFactor");
		GiftCoins = GetIntValueFromKey("GiftCoins");
		CoreDebugUtility.Log("Gift coins = "+GiftCoins);
	}

	public static void Reload()
	{
		Debug.Log("Reload FriendSettingConfig");
		FriendSettingConfig.Instance.LoadData();
	}

	private int GetIntValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		CoreDebugUtility.Assert(!string.IsNullOrEmpty(str));
		return int.Parse(str);
	}

	private float GetFloatValueFromKey(string key)
	{
		string str = ValueFromKey(key);
		CoreDebugUtility.Assert(!string.IsNullOrEmpty(str));
		return float.Parse(str);
	}

	private string ValueFromKey(string key)
	{
		string result = "";
		int index = ListUtility.Find(_sheet.dataArray, (FriendSettingData data) => {
			return data.Key == key;
		});
		if(index >= 0)
			result = _sheet.dataArray[index].Val;
		return result;
	}
}
