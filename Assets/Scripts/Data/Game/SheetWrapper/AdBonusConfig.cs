using System.Collections.Generic;
using UnityEngine;

public class AdBonusConfig : SimpleSingleton<AdBonusConfig>
{
	public static readonly string Name = "AdBonus";
    public List<int> RequireSpinCountList;

    private AdBonusSheet _sheet;

    public AdBonusConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<AdBonusSheet>(Name);
    }

	public static void Reload()
	{
		Debug.Log("Reload AdBonus Config");
		Instance.LoadData();
	}

    public AdBonusData GetAdBonusDataByAdType(string adType)
    {
        return ListUtility.FindFirstOrDefault(_sheet.dataArray, x => x.BonusType == adType);
    }

    public bool AvailableForThisMachine(string machineName, int adTypeId)
    {
        bool result = false;

        AdBonusData data = ListUtility.FindFirstOrDefault(_sheet.dataArray, x => x.AdTypeId == adTypeId);
        if (data != null && !data.UnavailableMachineList.Contains(machineName))
        {
            result = true;
        }

        return result;
    }

    public AdBonusData GetBestFitAdBonusData(string machineName)
    {
        AdBonusData result = null;

        if (!UserBasicData.Instance.IsPayUser && UserMachineData.Instance.TotalSpinCount != 0)
        {
            ListUtility.ForEach(_sheet.dataArray, x =>
            {
                if (!TimeUtility.IsInPeriodDays(UserBasicData.Instance.RegistrationTime, x.MinRegisterDays) 
                     && SpinCountSatisfied(x)
                     && !x.UnavailableMachineList.Contains(machineName)
                     && !LimitByAdStrategy(x)
                     && x.Enable == 1)
                {
                    if (result == null)
                        result = x;
                    else
                        result = x.Priority >= result.Priority ? x : result;
                }
            });
        }

        return result;
    }

    bool SpinCountSatisfied(AdBonusData data)
    {
        bool result = false;

#if DEBUG
        int requireSpinCount = RequireSpinCountList != null && RequireSpinCountList.Count > data.AdTypeId ? RequireSpinCountList[data.AdTypeId] : data.RequireSpinCount;
        result = UserMachineData.Instance.TotalSpinCount % requireSpinCount == 0;
#else
        result = UserMachineData.Instance.TotalSpinCount % data.RequireSpinCount == 0;
#endif

        return result;
    }

    bool LimitByAdStrategy(AdBonusData data)
    {
        //this only works for doublewin reward ad so far
        return data.AdTypeId == 0 &&
               !AdStrategyConfig.Instance.IsDoubleWinRewardAdActive(GroupConfig.Instance.GetAdStrategyId());
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   