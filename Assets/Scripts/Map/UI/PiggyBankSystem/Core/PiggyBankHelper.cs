using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PiggyBankHelper : MonoBehaviour {

    public static string GetProductID()
    {
        int timesHasPaidInPiggy = GetTimesHasPaidInPiggy();
        var nextPiggyInfoData = GetNextPiggyInfoData(timesHasPaidInPiggy);
        Debug.Log("--------CurrProductID is " + nextPiggyInfoData.CurItemId);
        return nextPiggyInfoData.CurItemId.ToString();
    }

    public static PiggyInfoData GetNextPiggyInfoData(int times)
    {
        // 因为价格是根据下一次的次数决定，所以 + 1
        return PiggyInfoConfig.Instance.FindPiggyInfoDataWithPayTimes(times + 1); 
    }

    public static int GetTimesHasPaidInPiggy()
    {
        return UserBasicData.Instance.TimesHasPaidInPiggy;
    }

    // Deprecated! 需求变更，不再需要每天重置小猪存钱罐购买次数
//    public static int GetTimesHasPaidInPiggy()
//    {
//        Debug.Log("--------when Get TimesHasPaid, the lastDayPaidPiggy is " + UserBasicData.Instance.LastDayPaidPiggy);
//        if (UserBasicData.Instance.TimesHasPaidInPiggy == 0)
//            return 0;
//
//        DateTime lastDayPayPiggy = UserBasicData.Instance.LastDayPaidPiggy;
//        DateTime nowDt = NetworkTimeHelper.Instance.GetNowTime();
//
//        if (lastDayPayPiggy.ToString("d") != nowDt.ToString("d"))
//        {
//            UserBasicData.Instance.SetTimesHasPaidInPiggy(0);
//        }
//        return UserBasicData.Instance.TimesHasPaidInPiggy;
//    }	
}
