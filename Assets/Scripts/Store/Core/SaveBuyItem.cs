#define ENABLE_LONGLUCKY_ADD
using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public static class SaveBuyItem
{
	public static void SaveBuyItemToData(string storeID, float credits, float creditsWithoutPromotion, int vippoint)
	{
		var iapT = IAPCatalogConfig.Instance.FindIAPItemByID(storeID);

		LogUtility.Log("原始购入" + iapT.CREDITS + "VIP加成" + VIPSystem.Instance.GetCurrVIPInforData.StoreAddition
					   + "计算后" + credits, Color.blue);

		// calculate lucky with no promotion
		int lucky = iapT.CreditsAddLongLucky * (int)creditsWithoutPromotion;

		LogUtility.Log("原始Luck" + iapT.CreditsAddLongLucky + "VIP加成" + VIPSystem.Instance.GetCurrVIPInforData.StoreAddition
					   + "计算后" + lucky, Color.blue);


		#if ENABLE_LONGLUCKY_ADD
		ulong curCredits = UserBasicData.Instance.Credits;
		ulong curLucky = (ulong)UserBasicData.Instance.LongLucky;
		UserBasicData.Instance.AddCredits(Convert.ToUInt64(credits), FreeCreditsSource.NotFree, false);

		// 是否需要把上一次没给完的lucky给完
		AddLastLongLucky(iapT, (int)creditsWithoutPromotion);

		// 记录本次的各种状态
		int itemID = int.Parse(storeID);
		UserBasicData.Instance.SetInfosWhenPurchase(itemID, curCredits, curLucky);
		UserBasicData.Instance.LuckyAddPeriodProgress = LongLuckyPeriodManager.PeriodPhase1;

		// 加lucky
		lucky = iapT.CreditsAddLongLucky1 * (int)creditsWithoutPromotion;
		UserBasicData.Instance.AddLongLucky(lucky, true);
		#else
		UserBasicData.Instance.AddCredits(Convert.ToUInt64(credits), FreeCreditsSource.NotFree, false);
		UserBasicData.Instance.AddLongLucky(lucky, true);
		#endif

		LogUtility.Log("购买商品ID成功:" + storeID + "存入" + iapT.CREDITS, Color.cyan);
		LogUtility.Log("消费金额" + iapT.Price + "得到的VIP点数" + vippoint);

		VIPSystem.Instance.AddVIPPoint(vippoint);

		UserBasicData.Instance.PlayerPayState = UserPayState.PayUser;
		UserBasicData.Instance.NoAds = true;
        if(iapT.Title.Contains("PiggyBank"))
		{
			CitrusEventManager.instance.Raise(new BuyPiggyBankSuccessEvent());
			LogUtility.Log("添加小猪金币" + UserBasicData.Instance.PiggyBankCoins, Color.cyan);
			var longluck = UserBasicData.Instance.PiggyBankCoins * iapT.CreditsAddLongLucky;

            Debug.Log("-----------Original Luck val is " + longluck + ", Luck TopLimit is " + iapT.LuckyTopLimit);
            longluck = longluck > iapT.LuckyTopLimit ? iapT.LuckyTopLimit : longluck;
			// 添加luck
			UserBasicData.Instance.AddLongLucky(longluck,false);
			// 添加金币
			UserBasicData.Instance.AddCredits((ulong)UserBasicData.Instance.PiggyBankCoins, FreeCreditsSource.NotFree, false);
            // 先把次数累计，再根据下次
            UserBasicData.Instance.AddTimesHasPaidInPiggy();
            // 把金币设置为初始值
            UserBasicData.Instance.SetPiggyBankCoins(PiggyBankHelper.GetNextPiggyInfoData(UserBasicData.Instance.TimesHasPaidInPiggy).InitCredit, true);
		}

		UserBasicData.Instance.AddBuyNumber(true);
		Debug.Log("当前购买的次数" + UserBasicData.Instance.BuyNumber);

		// 开启付费玩家保护机制
		UserBasicData.Instance.EnablePayProtection ();
		CitrusEventManager.instance.Raise(new SaveUserDataToServerEvent());
	}

	#if ENABLE_LONGLUCKY_ADD
	private static void AddLastLongLucky(IAPCatalogData iapT, int credits){
		int progress = UserBasicData.Instance.LuckyAddPeriodProgress;
		bool shouldAddLastLongLucky = progress != LongLuckyPeriodManager.allPeriodGet;
		if (shouldAddLastLongLucky){
			int lucky = 0;
			if ( (progress & LongLuckyPeriodManager.PeriodPhase1) == 0){
				lucky += iapT.CreditsAddLongLucky1 * credits;
			}
			if ( (progress & LongLuckyPeriodManager.PeriodPhase2) == 0){
				lucky += iapT.CreditsAddLongLucky2 * credits;
			}
			if ( (progress & LongLuckyPeriodManager.PeriodPhase3) == 0){
				lucky += iapT.CreditsAddLongLucky3 * credits;
			}
			LogUtility.Log("AddLastLongLucky progress = " + progress + " lucky = " + lucky, Color.red);
			UserBasicData.Instance.AddLongLucky(lucky, false);
		}
	}
	#endif
}

