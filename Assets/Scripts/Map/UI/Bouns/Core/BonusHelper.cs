using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public static class BonusHelper
{
	/// <summary>
	/// 时间比较是否超过多少小时
	/// </summary>
	/// <returns><c>true</c>, if contrast was timed, <c>false</c> otherwise.</returns>
	/// <param name="nowTime">Now.</param>
	/// <param name="oldTime">Old time.</param>
	public static bool TimeContrast(DateTime nowTime, DateTime oldTime, float passhour)
	{
		TimeSpan ts = nowTime - oldTime;
		return ts.TotalHours >= passhour;
	}

	public static bool CanGetDayBonus()
	{
		string lastTime = UserBasicData.Instance.LastDayBonusDateTime.ToString("d");
		string nowTime = NetworkTimeHelper.Instance.GetNowTime().ToString("d");
		//LogUtility.Log("每日奖励上次领取的时间" + lastTime + "现在的时间" + nowTime, Color.green);
		// 日期不同日历时间已过0点
		return lastTime != nowTime;

	}

	/// <summary>
	/// 得到还有多少秒可以领每小时奖励
	/// </summary>
	/// <returns>The hours bonus last second.</returns>
	public static int GetHoursBonusLastSecond()
	{
		return 3600 - LeftTime(NetworkTimeHelper.Instance.GetNowTime(), UserBasicData.Instance.LastHourBonusDateTime);
	}

	/// <summary>
	/// 同一天内返回总共超过多少秒
	/// </summary>
	/// <returns>The time.</returns>
	/// <param name="nowTime">Now time.</param>
	/// <param name="oldTime">Old time.</param>
	public static int LeftTime(DateTime nowTime, DateTime oldTime)
	{
		TimeSpan span = nowTime - oldTime;
		return (int)span.TotalSeconds;
	}

    /// <summary>
    /// Processes the day bonus data. 
    /// basecoins,vipacoins,allcoins
    /// </summary>
    /// <param name="dbInfor">Db infor.</param>
    /// <param name="dayBonusCoins">dayBonusCoins.</param>
    /// <param name="result">Result.</param>
    public static void ProcessDayBonusData(DayBonusTypeInfor dbInfor, int dayBonusCoins, int payRotateTableCoins, Action<int[]> result = null)
	{
		UserBasicData.Instance.SetLastGetDayBonusDate(NetworkTimeHelper.Instance.GetNowTime(null));
		UserBasicData.Instance.AddBonusDay();
		Debug.Log("basecoins" + dayBonusCoins);
		int basecoins = dbInfor.MagnificationNum * dayBonusCoins;
	    int vipCoins = (int)(VIPSystem.Instance.GetCurrVIPInforData.DailyBonusAddition * basecoins);
	    int dailyCoins = basecoins + vipCoins;
        int coins = dailyCoins + payRotateTableCoins;
		LogUtility.Log("原始daybounCOins " + dbInfor.MagnificationNum * dayBonusCoins + "  VIP加成" + VIPSystem.Instance.GetCurrVIPInforData.DailyBonusAddition
                   + "   付费转盘coins: " + payRotateTableCoins
				   + "   计算后" + coins, Color.blue);

	    IAPCatalogData iapData = IAPCatalogConfig.Instance.FindIAPItemByID(PayRotaryTableSystem.Instance.PRTPurchaseItemId);
        int payRotaryTableLucky = payRotateTableCoins * iapData.CreditsAddLongLucky;
//	    int dailyLucky = (int)(CoreConfig.Instance.LuckyConfig.DailyCreditsAddLongLucky * (1 + VIPSystem.Instance.GetCurrVIPInforData.DailyBonusAddition));
		int dailyLucky = (int)CoreConfig.Instance.LuckyConfig.DailyCreditsAddLongLucky;
	    int totalLucky = payRotaryTableLucky + dailyLucky;

        LogUtility.Log("原始daybounsLuck" + CoreConfig.Instance.LuckyConfig.DailyCreditsAddLongLucky 
                       + "VIP加成" + VIPSystem.Instance.GetCurrVIPInforData.DailyBonusAddition
                       + "  付费转盘lucky: " + payRotaryTableLucky  
                       + "  计算后" + totalLucky
                       + "  玩家现有longLucky : " +　UserBasicData.Instance.LongLucky, Color.blue);

        //Note: only save once for performance
        UserBasicData.Instance.AddLongLucky(dailyLucky, false);
	    UserBasicData.Instance.AddCredits((ulong)dailyCoins, FreeCreditsSource.DailyBonus, false);
        StoreManager.Instance.AddCreditsAndLuckyByItemId(PayRotaryTableSystem.Instance.PRTPurchaseItemId, payRotateTableCoins);

		AnalysisManager.Instance.GetDailyBonus(coins, totalLucky, dbInfor.MagnificationNum);
	    if (payRotateTableCoins > 0 )
	    {
            AnalysisManager.Instance.OnWheelOfLuckEnd(payRotateTableCoins, dbInfor.MagnificationNum, coins);
        }
		if(result != null)
		{
		    int[] coinsData = {dayBonusCoins, dbInfor.MagnificationNum, vipCoins, payRotateTableCoins, coins};
			result(coinsData);
		}
	}

	public static void ProcessHourBonusData(DateTime lastGetHourBDate, ulong coins)
	{
		UserBasicData.Instance.SetLastGetHourBonusDate(lastGetHourBDate);
		ulong newCoins = coins;
		newCoins = newCoins + (ulong)(newCoins * VIPSystem.Instance.GetCurrVIPInforData.HourBonusAddition);

		LogUtility.Log("原始每小时金币" + coins + "VIP加成" + VIPSystem.Instance.GetCurrVIPInforData.HourBonusAddition

			   + "计算后" + newCoins, Color.blue
			  );

		int longluck = CoreConfig.Instance.LuckyConfig.HourlyCreditsAddLongLucky;
//		longluck = longluck + (int)(longluck * VIPSystem.Instance.GetCurrVIPInforData.HourBonusAddition);

		LogUtility.Log("原始每小时luck" + CoreConfig.Instance.LuckyConfig.HourlyCreditsAddLongLucky + "VIP加成" + VIPSystem.Instance.GetCurrVIPInforData.HourBonusAddition

			   + "计算后" + longluck, Color.blue
			  );



		//Note: only save once for performance
		UserBasicData.Instance.AddCredits(newCoins, FreeCreditsSource.HourlyBonus, false);

		UserBasicData.Instance.AddLongLucky(longluck, false);
		UserBasicData.Instance.Save();

		AnalysisManager.Instance.GetTimeBonus((int)newCoins, longluck);
	}

	public static void GetFirstBonus()
	{
		UserBasicData.Instance.IsFistStartGameBonus = false;
		//  写入金币
		UserBasicData.Instance.AddCredits(1000000, FreeCreditsSource.NotFree, true);

		// UserBasicData.Instance.AddLongLucky(GameConfig.Instance.LuckyConfig.FistStartGameBonusLongLucky, true);
		Debug.Log("金币写入成功");
	}
}
