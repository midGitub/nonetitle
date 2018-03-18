using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeMaxBetRewardAdController : BaseRewardADController
{
    public MachineRewardAdManager AdManager;
    public override void GetBonus()
    {
        base.GetBonus();
        AdManager.OnAdOver(SpecialMode.FreeMaxBet);
    }

    public override void OnAdShow()
    {
        bool needShowUnfinishAd = !TimeUtility.IsDatePast(UserDeviceLocalData.Instance.LastMachineAdEndTime) && AdManager.IsShowingUnfinishedAd;
        AdBonusData data = AdBonusConfig.Instance.GetAdBonusDataByAdType(BindRewardAdButton.AdTypeName);
        AdDurationTime = data.Duration;
        if (needShowUnfinishAd)
        {
            int unfinishedAdLeftTime = (int)TimeUtility.CountdownOfDateFromNowOn(UserDeviceLocalData.Instance.LastMachineAdEndTime).TotalSeconds;
            AdDurationTime = unfinishedAdLeftTime > data.Duration ? data.Duration : unfinishedAdLeftTime;
        }

        UserDeviceLocalData.Instance.LastMachineAdId = data.AdTypeId;
        UserDeviceLocalData.Instance.LastMachineAdEndTime = NetworkTimeHelper.Instance.GetNowTime() +
                                                            new TimeSpan(0, 0, AdDurationTime);
        base.OnAdShow();
    }

    public override void OnAdClose()
    {
        base.OnAdClose();
        UserDeviceLocalData.Instance.LastMachineAdEndTime = DateTime.MinValue;
    }
}
