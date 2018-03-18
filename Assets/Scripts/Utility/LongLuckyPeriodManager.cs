#define ENABLE_LONGLUCKY_ADD
// #define ENABLE_LONGLUCKY_LOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class LongLuckyPeriodManager : Singleton<LongLuckyPeriodManager> {
	public static readonly int PeriodPhase1 = 1 << 0;
	public static readonly int PeriodPhase2 = 1 << 1;
	public static readonly int PeriodPhase3 = 1 << 2;
	public static readonly	int allPeriodGet = PeriodPhase1| PeriodPhase2 | PeriodPhase3;// 三阶段flag

	public void Init(){
		#if ENABLE_LONGLUCKY_ADD
		CitrusEventManager.instance.AddListener<LongLuckyPeriodEvent>(CheckLongLuckyPeriod);
		#endif
	}

	void OnDestroy()
	{
		base.OnDestroy();
		#if ENABLE_LONGLUCKY_ADD
		CitrusEventManager.instance.RemoveListener<LongLuckyPeriodEvent>(CheckLongLuckyPeriod);
		#endif
	}

	public void CheckLongLuckyPeriod(LongLuckyPeriodEvent message){
		#if ENABLE_LONGLUCKY_ADD
		ulong curCredit = UserBasicData.Instance.Credits;
		ulong curLucky = (ulong)UserBasicData.Instance.LongLucky;
		ulong lastCredit = UserBasicData.Instance.CreditsWhenLastPurchase;
		ulong lastLucky = UserBasicData.Instance.LuckyWhenLastPurchase;
		int itemID = UserBasicData.Instance.LastPurchaseItemID;
		#if ENABLE_LONGLUCKY_LOG
		LogUtility.Log("CheckLongLuckyPeriod credit = "+curCredit+" curLucky = "+curLucky+" lastCredit = "+lastCredit+" lastLucky = "+lastLucky+" lastItemID = "+itemID, Color.red);
		LogUtility.Log("UserBasicData.Instance.LuckyAddPeriodProgress = "+UserBasicData.Instance.LuckyAddPeriodProgress, Color.red);
		#endif

		if (itemID != 0 
			&& UserBasicData.Instance.LuckyAddPeriodProgress != allPeriodGet){
			
			IAPCatalogData item = IAPCatalogConfig.Instance.FindIAPItemByID(itemID.ToString());
			CoreDebugUtility.Assert(item != null);
			int result = CoreConfig.Instance.LuckyConfig.IsInLongLuckyPeriod(curCredit, curLucky, lastCredit, lastLucky, (ulong)item.CREDITS);
			if (result != 0 && UserBasicData.Instance.CanAddLuckyInPeriod(result)){
				// 获得阶段lucky
				int lucky = 0;
				if (result == 1){
					lucky = item.CreditsAddLongLucky2 * (int)item.CREDITS;
				}else if (result == 2){
					lucky = item.CreditsAddLongLucky3 * (int)item.CREDITS;
					// 重置lastcredits, lastlucky, itemid
					UserBasicData.Instance.SetInfosWhenPurchase(0, 0, 0);
				}
				// 设置progress
				UserBasicData.Instance.SetLuckyAddPeriodProgress(result);
				UserBasicData.Instance.AddLongLucky(lucky, true);
				#if ENABLE_LONGLUCKY_LOG
				LogUtility.Log("Get period lucky phase = " + result + " lucky = " + lucky, Color.red);
				#endif
			}
		}
		#endif
	}

}
