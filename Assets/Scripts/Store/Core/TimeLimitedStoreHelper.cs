using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TimeLimitedStoreHelper{

    private static readonly int howManyMinsLast = 30;
    private static readonly TimeSpan timeSpanLast = new TimeSpan(0, howManyMinsLast, 0);

    public static bool ShouldPopupTLStore()
    {
		DateTime lastTime = UserBasicData.Instance.LastTimeLimitedStore;
		DateTime nowTime = NetworkTimeHelper.Instance.GetNowTime();

		bool cond1 = lastTime.ToString("d") != nowTime.ToString("d");

		TimeSpan timeSpan = nowTime - lastTime;
		bool cond2 = timeSpan.TotalHours >= 1;

		bool cond3 = UserMachineData.Instance.TotalSpinCount >= MapSettingConfig.Instance.MinSpinCountOfPopTimeLimitStore;

		#if DEBUG
		cond3 = true; //QA command
		#endif

		bool result = cond1 && cond2 && cond3;

		if (!GroupConfig.Instance.IsProductExist (StoreType.Deal_TL))
			result = false;

        return result;
    }

    public static bool IsInTLStorePeriod(Action<bool, TimeSpan> callback = null)
    {
        bool result;
        TimeSpan countDowntimeSpan = default(TimeSpan);

        if (UserBasicData.Instance.HasBoughtTLItem)
            result = false;
        else
        {
//            bool isNetOK = NetworkTimeHelper.Instance.IsServerTimeGetted;
            DateTime nowTime = NetworkTimeHelper.Instance.GetNowTime();

        
//            if (!isNetOK)
//            {
//                result = false;
//            }
//            else
//            {
                TimeSpan timeSpan = nowTime - UserBasicData.Instance.LastTimeLimitedStore;
                if (timeSpan.TotalMinutes <= howManyMinsLast && timeSpan.TotalSeconds > 0)
                {
                    result = true;
                    countDowntimeSpan = timeSpanLast - timeSpan;
                }
                else
                {
                    result = false;
                }
//            }
        }



        if (callback != null)
        {
            callback(result, countDowntimeSpan);
        }
			
		if (!GroupConfig.Instance.IsProductExist (StoreType.Deal_TL))
			result = false;
		
        return result;
    }

    public static bool InPeriodButNotShouldPop()
    {
        return IsInTLStorePeriod() && !ShouldPopupTLStore();
    }

    public static string GetProductID(VIPData vipData)
    {
		string result = "";
/*        string[] strIAPIDs = vipData.LimitedTimeIAPID.Split(',');
        string[] strProbabilities = vipData.LTProbabilities.Split(',');
        Debug.Assert(strIAPIDs.Length == strProbabilities.Length, "VIP表单中限时商城商品id与其概率对不上");
        Debug.Assert(strIAPIDs.Length > 0, "VIP表单中限时商城商品id个数不大于0");

        

        float randomP = UnityEngine.Random.Range(0, 1f);
        int i = 0;
        float minP = 0;
        float maxP = float.Parse(strProbabilities[i]);

        while (true)
        {
            if (randomP >= minP && randomP <= maxP)
            {
                result = strIAPIDs[i];
                break;            
            }
            else
            {
                i++;
                if (i >= strProbabilities.Length)
                {
                    Debug.LogError("找不到任何合适的限时商品ID"); 
                    break;
                }
                else
                {
                    minP = maxP;
                    maxP += float.Parse(strProbabilities[i]);

                }
            } 
        }
        */
		result = GroupConfig.Instance.GetProductID (StoreType.Deal_TL).ToString ();
        return result;
    }

    public static bool IsFirstBigWinToday()
    {
        string lastTime = UserDeviceLocalData.Instance.LastBigWinDay.ToString("d");
        string nowTime = NetworkTimeHelper.Instance.GetNowTime().ToString("d");

        if (lastTime != nowTime)
        {
            UserDeviceLocalData.Instance.LastBigWinDay = NetworkTimeHelper.Instance.GetNowTime();
            return true;
        }
        else
            return false;
    }
}
