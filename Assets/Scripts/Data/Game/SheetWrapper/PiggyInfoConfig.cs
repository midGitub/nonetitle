using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiggyInfoConfig : SimpleSingleton<PiggyInfoConfig>
{
	public static readonly string Name = "PiggyInfo";

    private PiggyInfoSheet _piggyInfoSheet;

    public PiggyInfoConfig()
    {
        LoadData();
    }

    private void LoadData()
    {
		_piggyInfoSheet = GameConfig.Instance.LoadExcelAsset<PiggyInfoSheet>(Name);
    }

	public static void Reload()
	{
		Debug.Log("Reload PiggyInfoConfig");
		PiggyInfoConfig.Instance.LoadData();
	}

    public PiggyInfoData FindPiggyInfoDataWithPayTimes(int times)
    {
        for(int i = 0; i < _piggyInfoSheet.dataArray.Length; i++)
        {
            var data = _piggyInfoSheet.dataArray[i];

            // 到达最后一行 0为无限大
            if(data.MaxPayTimes == 0)
            {
                return data;
            }
            if(data.MinPayTimes <= times && times <= data.MaxPayTimes)
            {
                return data;
            }
        }
        Debug.LogError("购买错误不在范围内");
        return null;
    }
}
