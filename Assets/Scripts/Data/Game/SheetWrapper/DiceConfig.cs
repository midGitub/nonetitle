using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceConfig : SimpleSingleton<DiceConfig>
{
	public static readonly string Name = "Dice";

	private DiceSheet _sheet;
	public List<DiceData> ListSheet { private set; get; }

	public DiceConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<DiceSheet>(Name);
		ListSheet = _sheet.dataArray.ToList();
	}

	public static void Reload()
	{
		Debug.Log("Reload DiceConfig");
		DiceConfig.Instance.LoadData();
	}

    public DiceData GetDiceDataById(int id)
    {
        DiceData result = ListUtility.FindFirstOrDefault(ListSheet, (x) => {
			return x.DiceType == id;
		});
        return result;
    }

    public DiceData GetDiceDataByOptions(ulong credits, float radio)
    {
        DiceData result = ListUtility.FindFirstOrDefault(ListSheet, (x) => {
			return radio >= x.MinRatio && credits >= (ulong)x.Minreward && credits <= (ulong)x.Maxreward;
		});
		return result;
	}

    public int GetIAPIdByDifferentUser(DiceData data)
    {
        //if user is a payuser, he will get a IAPId differs from normal user
		bool isPayUser = UserBasicData.Instance.IsPayUser;
        int result = isPayUser ? data.PayUserIAPId : data.IAPId;

        return result;
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   