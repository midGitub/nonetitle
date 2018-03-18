using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

public enum ServerFieldName
{
	SocialId,
	Name,
	UserLevel,
	UserLevelPoint,
	UserLevelProgress,
	// 所有的true false 以string类型存储
	Status,
	VipPoint,
	Credits,
	LongLucky,
	ShortLucky,
	LastBonusDay,
	LastDayBounsDate,
	LastCloseScorePageData,
	DayBonusDaysType,

	LastChestDate,
	LastLoginDate,
	LoginDays,
	LoginTimes,
	LastSpinDate,
	SpinDays,
	HasBoughtSpecialOffer,
	FirstEnterGameTime,
	LastShowSpecialOffer,
	Email,
	MachineData,
	HistoryMaxPaid,
	// 其他 已string类型存储
	Misc,
	Md5,
	Friend,
	PayProtection,//付费玩家保护机制
	PayProtectionLastBankruptBuytime,//上次破产保护时的购买次数
	LastPurchaseItemID,// 上次内购商品ID
	CreditsWhenLastPurchase,// 上次内购时玩家credits
	LuckyWhenLastPurchase,// 上次内购时玩家lucky
	LuckyAddPeriodProgress,// 分阶段送lucky进度
}

public enum StateField
{
	FirstLoadingFaceBook,
	FistStartGameBonus,
	FirstSpin,
	FirstScore,
}

public enum MiscField
{
	PlayerPayState,
	PaidAmount,
	NoADS,
	RegistrationTime,
	PiggyBankCoins,
	BuyNumber,
	TimesHasPaidInPiggy,
	LastTimeLimitedStore,
	LimitedStoreItemID,
	HasBoughtTLItem,
	FacebookLikes,
	TotalPayAmount,
	LastPayTime,
    MaxWinDuringActivity,
    HasUserGetChristmasMaxWinReward,
    LastGetMaxWinActivityRewardDate,
    FreeNodeIndex,
    FreeQueue,
    FreeWinQueue,
    PayNodeIndex,
    PayQueue,
    PayWinQueue,
    IsAllVipMachineUnlock,
    LastReceiveBroadcastId,
}

public static class UserBasicDataJSON
{
	public static Dictionary<string, object> CreatDataDic()
	{
		Dictionary<string, object> baseDataDic = new Dictionary<string, object>();
		baseDataDic.Add(ServerFieldName.UserLevel.ToString(), (float)UserBasicData.Instance.UserLevel.Level);
		baseDataDic.Add(ServerFieldName.UserLevelPoint.ToString(), (float)UserBasicData.Instance.UserLevel.LevelPoint);
		baseDataDic.Add(ServerFieldName.UserLevelProgress.ToString(), UserBasicData.Instance.UserLevelProgress);

		// 状态state字典
		Dictionary<string, object> stateDic = new Dictionary<string, object>();
		stateDic.Add (StateField.FirstSpin.ToString(), UserBasicData.Instance.IsFirstSpin.ToString());
		stateDic.Add (StateField.FirstScore.ToString (), UserBasicData.Instance.IsFirstScore.ToString());
		stateDic.Add(StateField.FirstLoadingFaceBook.ToString(), UserBasicData.Instance.IsFirstLoadingFaceBook.ToString());
		stateDic.Add(StateField.FistStartGameBonus.ToString(), UserBasicData.Instance.IsFistStartGameBonus.ToString());
		var statjson = Json.Serialize(stateDic);
		//state结束

		baseDataDic.Add(ServerFieldName.Status.ToString(), statjson);
		baseDataDic.Add(ServerFieldName.VipPoint.ToString(), UserBasicData.Instance.VIPPoint);
		baseDataDic.Add(ServerFieldName.Credits.ToString(), UserBasicData.Instance.Credits);
		baseDataDic.Add(ServerFieldName.LongLucky.ToString(), UserBasicData.Instance.LongLucky);
		baseDataDic.Add(ServerFieldName.ShortLucky.ToString(), UserBasicData.Instance.ShortLucky);
		baseDataDic.Add(ServerFieldName.LastBonusDay.ToString(), UserBasicData.Instance.LastHourBonusDateTime.ToString());
		baseDataDic.Add(ServerFieldName.LastDayBounsDate.ToString(), UserBasicData.Instance.LastDayBonusDateTime.ToString());
		baseDataDic.Add(ServerFieldName.LastCloseScorePageData.ToString(), UserBasicData.Instance.LastCloseScorePageTime.ToString());
		baseDataDic.Add(ServerFieldName.DayBonusDaysType.ToString(), UserBasicData.Instance.BonusDaysType);
		baseDataDic.Add(ServerFieldName.HistoryMaxPaid.ToString(), UserBasicData.Instance.HistoryMaxPaid);

		Dictionary<string, object> MiscDic = new Dictionary<string, object>();

		MiscDic.Add(MiscField.PlayerPayState.ToString(), (int)UserBasicData.Instance.PlayerPayState);
		MiscDic.Add(MiscField.NoADS.ToString(), UserBasicData.Instance.NoAds);
		MiscDic.Add(MiscField.RegistrationTime.ToString(), UserBasicData.Instance.RegistrationTime.ToString());
		MiscDic.Add(MiscField.PiggyBankCoins.ToString(), UserBasicData.Instance.PiggyBankCoins);
        MiscDic.Add(MiscField.TimesHasPaidInPiggy.ToString(), UserBasicData.Instance.TimesHasPaidInPiggy);
        MiscDic.Add(MiscField.LastTimeLimitedStore.ToString(), UserBasicData.Instance.LastTimeLimitedStore);
        MiscDic.Add(MiscField.LimitedStoreItemID.ToString(), UserBasicData.Instance.LimitedStoreItemID);
        MiscDic.Add(MiscField.HasBoughtTLItem.ToString(), UserBasicData.Instance.HasBoughtTLItem);
		MiscDic.Add(MiscField.FacebookLikes.ToString(), UserBasicData.Instance.LikeOurAppInFacebook);

		MiscDic.Add(MiscField.PaidAmount.ToString(), 0); //PaidAmount is useless, so just set 0
		MiscDic.Add(MiscField.BuyNumber.ToString(), UserBasicData.Instance.BuyNumber);
		MiscDic.Add(MiscField.TotalPayAmount.ToString(), UserBasicData.Instance.TotalPayAmount);
		MiscDic.Add(MiscField.LastPayTime.ToString(), UserBasicData.Instance.LastPayTime.ToString());
        MiscDic.Add(MiscField.MaxWinDuringActivity.ToString(), UserBasicData.Instance.MaxWinDuringActivity);
        MiscDic.Add(MiscField.HasUserGetChristmasMaxWinReward.ToString(), UserBasicData.Instance.HasUserGetChristmasMaxWinReward);
        MiscDic.Add(MiscField.LastGetMaxWinActivityRewardDate.ToString(), UserBasicData.Instance.LastGetMaxWinActivityRewardDate.ToString());
        MiscDic.Add(MiscField.FreeNodeIndex.ToString(), UserBasicData.Instance.FreeNodeIndex);
        MiscDic.Add(MiscField.FreeQueue.ToString(), UserBasicData.Instance.FreeQueue);
        MiscDic.Add(MiscField.FreeWinQueue.ToString(), UserBasicData.Instance.FreeWinQueue);
        MiscDic.Add(MiscField.PayNodeIndex.ToString(), UserBasicData.Instance.PayNodeIndex);
        MiscDic.Add(MiscField.PayQueue.ToString(), UserBasicData.Instance.PayQueue);
        MiscDic.Add(MiscField.PayWinQueue.ToString(), UserBasicData.Instance.PayWinQueue);
        MiscDic.Add(MiscField.IsAllVipMachineUnlock.ToString(), UserBasicData.Instance.IsAllVipMachineUnlock);
        MiscDic.Add(MiscField.LastReceiveBroadcastId.ToString(), UserBasicData.Instance.LastReceivedBroadcastId);

        var miscjson = Json.Serialize(MiscDic);
		baseDataDic.Add(ServerFieldName.Misc.ToString(), miscjson);

		baseDataDic.Add(ServerFieldName.LastChestDate.ToString(), UserBasicData.Instance.LastGetChestDateTime.ToString());
		baseDataDic.Add (ServerFieldName.LastLoginDate.ToString (), UserBasicData.Instance.LastLoginDateTime.ToString ());
		baseDataDic.Add(ServerFieldName.LoginDays.ToString(), UserBasicData.Instance.LoginDays);
		baseDataDic.Add(ServerFieldName.LoginTimes.ToString(), UserBasicData.Instance.LoginTimes);
		baseDataDic.Add(ServerFieldName.LastSpinDate.ToString(), UserBasicData.Instance.LastSpinDate.ToString());
		baseDataDic.Add(ServerFieldName.SpinDays.ToString(), UserBasicData.Instance.SpinDays);
		baseDataDic.Add (ServerFieldName.HasBoughtSpecialOffer.ToString (), UserBasicData.Instance.HasBoughtSpecialOffer.ToString ());
		baseDataDic.Add (ServerFieldName.LastShowSpecialOffer.ToString (), UserBasicData.Instance.LastShowSpecialOffer.ToString ());
		baseDataDic.Add (ServerFieldName.FirstEnterGameTime.ToString (), UserBasicData.Instance.FirstEnterGameTime.ToString ());
		baseDataDic.Add (ServerFieldName.PayProtection.ToString (), UserBasicData.Instance.PayProtectionEnable.ToString ());
		baseDataDic.Add (ServerFieldName.PayProtectionLastBankruptBuytime.ToString (), UserBasicData.Instance.PayProtectionLastBankruptBuytimes.ToString ());
		baseDataDic.Add (ServerFieldName.Email.ToString (), UserBasicData.Instance.FacebookBindEmail);
		baseDataDic.Add(ServerFieldName.LastPurchaseItemID.ToString(), UserBasicData.Instance.LastPurchaseItemID);
		baseDataDic.Add(ServerFieldName.CreditsWhenLastPurchase.ToString(), UserBasicData.Instance.CreditsWhenLastPurchase);
		baseDataDic.Add(ServerFieldName.LuckyWhenLastPurchase.ToString(), UserBasicData.Instance.LuckyWhenLastPurchase);
		baseDataDic.Add(ServerFieldName.LuckyAddPeriodProgress.ToString(), UserBasicData.Instance.LuckyAddPeriodProgress);

		return baseDataDic;
	}

	// Important note:
	// This functions is called by both fetching UserData and fetching GM UserData.
	// When Fetching GM UserData, only the json fields that are changed by GM tools are available.
	// So we need to check if the field exists before setting any value to UserData.
	public static void UseJSONToData(JSONObject jsonObject)
	{
		LevelData levelData = UserBasicData.Instance.UserLevel;
		bool hasLevelData = false;
		if(jsonObject.HasField(ServerFieldName.UserLevel.ToString()))
		{
			levelData.Level = (float)jsonObject.GetField(ServerFieldName.UserLevel.ToString()).n;
			hasLevelData = true;
		}
		if(jsonObject.HasField(ServerFieldName.UserLevelPoint.ToString()))
		{
			levelData.LevelPoint = (float)jsonObject.GetField(ServerFieldName.UserLevelPoint.ToString()).n;
			hasLevelData = true;
		}

		if(hasLevelData){
			UserBasicData.Instance.SetUserLevelData(levelData, false);
			UserLevelSystem.Instance.UpdateUserLevelData();
		}

		if(jsonObject.HasField(ServerFieldName.Status.ToString()))
		{
			var statejs = new JSONObject(jsonObject.GetField(ServerFieldName.Status.ToString()).str);

			if (statejs.HasField(StateField.FirstLoadingFaceBook.ToString()))
				UserBasicData.Instance.IsFirstLoadingFaceBook = statejs.GetField(StateField.FirstLoadingFaceBook.ToString()).b;

			if (statejs.HasField(StateField.FistStartGameBonus.ToString()))
				UserBasicData.Instance.IsFistStartGameBonus = statejs.GetField(StateField.FistStartGameBonus.ToString()).b;

			if (statejs.HasField(StateField.FirstSpin.ToString()))
				UserBasicData.Instance.IsFirstSpin = statejs.GetField(StateField.FirstSpin.ToString()).b;
			
			if(statejs.HasField(StateField.FirstScore.ToString()))
				UserBasicData.Instance.IsFirstScore = statejs.GetField(StateField.FirstScore.ToString()).b;
		}

		if(jsonObject.HasField(ServerFieldName.VipPoint.ToString()))
			UserBasicData.Instance.SetVIPPoint((int)jsonObject.GetField(ServerFieldName.VipPoint.ToString()).n, false);

		if(jsonObject.HasField(ServerFieldName.Credits.ToString()))
			UserBasicData.Instance.SetCredits((ulong)jsonObject.GetField(ServerFieldName.Credits.ToString()).n, false);

		if(jsonObject.HasField(ServerFieldName.LongLucky.ToString()))
			UserBasicData.Instance.SetLongLucky((int)jsonObject.GetField(ServerFieldName.LongLucky.ToString()).n, false);

		if(jsonObject.HasField(ServerFieldName.ShortLucky.ToString()))
			UserBasicData.Instance.SetShortLucky((int)jsonObject.GetField(ServerFieldName.ShortLucky.ToString()).n, false);

		if(jsonObject.HasField(ServerFieldName.LastBonusDay.ToString()))
			UserBasicData.Instance.SetLastGetHourBonusDate(Convert.ToDateTime(jsonObject.GetField(ServerFieldName.LastBonusDay.ToString()).str));

		if(jsonObject.HasField(ServerFieldName.LastDayBounsDate.ToString()))
			UserBasicData.Instance.SetLastGetDayBonusDate(Convert.ToDateTime(jsonObject.GetField(ServerFieldName.LastDayBounsDate.ToString()).str));

		if(jsonObject.HasField(ServerFieldName.DayBonusDaysType.ToString()))
			UserBasicData.Instance.SetBonusDay((int)jsonObject.GetField(ServerFieldName.DayBonusDaysType.ToString()).n);

		if(jsonObject.HasField(ServerFieldName.Misc.ToString()))
		{
			var miscjs = new JSONObject(jsonObject.GetField(ServerFieldName.Misc.ToString()).str);

			if(miscjs.HasField(MiscField.PlayerPayState.ToString()))
			{
				int payState = (int)miscjs.GetField(MiscField.PlayerPayState.ToString()).n;
				UserBasicData.Instance.PlayerPayState = (UserPayState)payState;
			}

			if(miscjs.HasField(MiscField.NoADS.ToString()))
				UserBasicData.Instance.NoAds = miscjs.GetField(MiscField.NoADS.ToString()).b;

			//do nothing to UserBasicData.Instance.PaidAmount
//			if(miscjs.HasField(MiscField.PaidAmount.ToString()))
//				UserBasicData.Instance.PaidAmount = (int)miscjs.GetField(MiscField.PaidAmount.ToString()).n;

			if(miscjs.HasField(MiscField.BuyNumber.ToString()))
				UserBasicData.Instance.SetBuyNumber((int)miscjs.GetField(MiscField.BuyNumber.ToString()).n, false);

			if(miscjs.HasField(MiscField.TotalPayAmount.ToString()))
				UserBasicData.Instance.SetTotalPayAmount(miscjs.GetField(MiscField.TotalPayAmount.ToString()).f, false);

			try
			{
				if(miscjs.HasField(MiscField.LastPayTime.ToString()))
				{
					string payTimeStr = miscjs.GetField(MiscField.LastPayTime.ToString()).str;
					UserBasicData.Instance.SetLastPayTime(Convert.ToDateTime(payTimeStr), false);
				}
			}
			catch(Exception e)
			{
				Debug.LogError("UseJSONToData exception: " + e.ToString());
			}

			if(miscjs.HasField(MiscField.RegistrationTime.ToString()))
				UserBasicData.Instance.RegistrationTime = Convert.ToDateTime(miscjs.GetField(MiscField.RegistrationTime.ToString()).str);

			if(miscjs.HasField(MiscField.PiggyBankCoins.ToString()))
				UserBasicData.Instance.SetPiggyBankCoins((int)(miscjs.GetField(MiscField.PiggyBankCoins.ToString()).n), true);

			if (miscjs.HasField(MiscField.TimesHasPaidInPiggy.ToString()))
				UserBasicData.Instance.SetTimesHasPaidInPiggy((int)(miscjs.GetField(MiscField.TimesHasPaidInPiggy.ToString()).n));
			
			if (miscjs.HasField(MiscField.LastTimeLimitedStore.ToString()))
				UserBasicData.Instance.SetLastTimeLimitedStore(Convert.ToDateTime(miscjs.GetField(MiscField.LastTimeLimitedStore.ToString()).str));
			
			if (miscjs.HasField(MiscField.LimitedStoreItemID.ToString()))
				UserBasicData.Instance.SetLimitedStoreItemID(miscjs.GetField(MiscField.LimitedStoreItemID.ToString()).str);
			
			if (miscjs.HasField(MiscField.HasBoughtTLItem.ToString()))
				UserBasicData.Instance.SetHasBoughtTLItem(miscjs.GetField(MiscField.HasBoughtTLItem.ToString()).b);
			
			if (miscjs.HasField(MiscField.MaxWinDuringActivity.ToString()))
				UserBasicData.Instance.MaxWinDuringActivity = (ulong)miscjs.GetField(MiscField.MaxWinDuringActivity.ToString()).n;
			
			if (miscjs.HasField(MiscField.HasUserGetChristmasMaxWinReward.ToString()))
				UserBasicData.Instance.HasUserGetChristmasMaxWinReward = miscjs.GetField(MiscField.HasUserGetChristmasMaxWinReward.ToString()).b;
			
			if (miscjs.HasField(MiscField.LastGetMaxWinActivityRewardDate.ToString()))
				UserBasicData.Instance.LastGetMaxWinActivityRewardDate = Convert.ToDateTime(miscjs.GetField(MiscField.LastGetMaxWinActivityRewardDate.ToString()).str);

            if (miscjs.HasField(MiscField.FreeNodeIndex.ToString()))
                UserBasicData.Instance.FreeNodeIndex = (uint)miscjs.GetField(MiscField.FreeNodeIndex.ToString()).n;

            if (miscjs.HasField(MiscField.FreeQueue.ToString()))
                UserBasicData.Instance.FreeQueue = miscjs.GetField(MiscField.FreeQueue.ToString()).str;

            if (miscjs.HasField(MiscField.FreeWinQueue.ToString()))
                UserBasicData.Instance.FreeWinQueue = miscjs.GetField(MiscField.FreeWinQueue.ToString()).str;

            if (miscjs.HasField(MiscField.PayNodeIndex.ToString()))
				UserBasicData.Instance.PayNodeIndex = (uint)miscjs.GetField(MiscField.PayNodeIndex.ToString()).n;
			
			if (miscjs.HasField(MiscField.PayQueue.ToString()))
				UserBasicData.Instance.PayQueue = miscjs.GetField(MiscField.PayQueue.ToString()).str;
			
			if (miscjs.HasField(MiscField.PayWinQueue.ToString()))
				UserBasicData.Instance.PayWinQueue = miscjs.GetField(MiscField.PayWinQueue.ToString()).str;

			if (miscjs.HasField(MiscField.FacebookLikes.ToString()))
				UserBasicData.Instance.LikeOurAppInFacebook = miscjs.GetField(MiscField.FacebookLikes.ToString()).b;
            if (miscjs.HasField(MiscField.IsAllVipMachineUnlock.ToString()))
                UserBasicData.Instance.IsAllVipMachineUnlock = miscjs.GetField(MiscField.IsAllVipMachineUnlock.ToString()).b;
            if (miscjs.HasField(MiscField.LastReceiveBroadcastId.ToString()))
                UserBasicData.Instance.LastReceivedBroadcastId = (ulong)miscjs.GetField(MiscField.LastReceiveBroadcastId.ToString()).n;
        }

		if(jsonObject.HasField(ServerFieldName.LastCloseScorePageData.ToString()))
			UserBasicData.Instance.SetLastCloseScorePageDate(Convert.ToDateTime (jsonObject.GetField (ServerFieldName.LastCloseScorePageData.ToString()).str));

		if (jsonObject.HasField(ServerFieldName.LastChestDate.ToString()))
			UserBasicData.Instance.SetLastGetChestDateTime (Convert.ToDateTime (jsonObject.GetField (ServerFieldName.LastChestDate.ToString ()).str));

		if (jsonObject.HasField (ServerFieldName.LastLoginDate.ToString ()))
			UserBasicData.Instance.SetLastLoginDateTime (Convert.ToDateTime (jsonObject.GetField (ServerFieldName.LastLoginDate.ToString ()).str));

		if (jsonObject.HasField(ServerFieldName.LoginDays.ToString()))
			UserBasicData.Instance.LoginDays = (int)jsonObject.GetField(ServerFieldName.LoginDays.ToString()).n;

		if (jsonObject.HasField(ServerFieldName.LoginTimes.ToString()))
			UserBasicData.Instance.LoginTimes = (int)jsonObject.GetField(ServerFieldName.LoginTimes.ToString()).n;

		if (jsonObject.HasField(ServerFieldName.LastSpinDate.ToString()))
			UserBasicData.Instance.LastSpinDate = Convert.ToDateTime(jsonObject.GetField(ServerFieldName.LastSpinDate.ToString()).str);

		if (jsonObject.HasField(ServerFieldName.SpinDays.ToString()))
			UserBasicData.Instance.SpinDays = (int)jsonObject.GetField(ServerFieldName.SpinDays.ToString()).n;

		if (jsonObject.HasField (ServerFieldName.HasBoughtSpecialOffer.ToString ())) 
			UserBasicData.Instance.HasBoughtSpecialOffer = (Convert.ToBoolean (jsonObject.GetField (ServerFieldName.HasBoughtSpecialOffer.ToString ()).str));
		
		if (jsonObject.HasField (ServerFieldName.LastShowSpecialOffer.ToString ())) 
			UserBasicData.Instance.LastShowSpecialOffer = (Convert.ToDateTime (jsonObject.GetField (ServerFieldName.LastShowSpecialOffer.ToString ()).str));

		if (jsonObject.HasField (ServerFieldName.FirstEnterGameTime.ToString ())) 
		{
			UserBasicData.Instance.FirstEnterGameTime = (Convert.ToDateTime (jsonObject.GetField (ServerFieldName.FirstEnterGameTime.ToString ()).str));
			UserDeviceLocalData.Instance.FirstEnterGameTime = UserBasicData.Instance.FirstEnterGameTime;
		}

		if (jsonObject.HasField (ServerFieldName.PayProtection.ToString ()))
			UserBasicData.Instance.PayProtectionEnable = jsonObject.GetField (ServerFieldName.PayProtection.ToString ()).b;
	
		if (jsonObject.HasField (ServerFieldName.PayProtectionLastBankruptBuytime.ToString ()))
			UserBasicData.Instance.PayProtectionLastBankruptBuytimes = (int)jsonObject.GetField (ServerFieldName.PayProtectionLastBankruptBuytime.ToString ()).n;

		if (jsonObject.HasField (ServerFieldName.Email.ToString ())) 
			UserBasicData.Instance.FacebookBindEmail = jsonObject.GetField (ServerFieldName.Email.ToString ()).str;

		if (jsonObject.HasField(ServerFieldName.HistoryMaxPaid.ToString()))
			UserBasicData.Instance.SetHistoryMaxPaid(jsonObject.GetField(ServerFieldName.HistoryMaxPaid.ToString()).f,false);

		if (jsonObject.HasField(ServerFieldName.UserLevelProgress.ToString()))
			UserLevelUtility.UpdateUserLevelFromJson(jsonObject);

		if (jsonObject.HasField(ServerFieldName.LastPurchaseItemID.ToString())){
			UserBasicData.Instance.LastPurchaseItemID = (int)jsonObject.GetField(ServerFieldName.LastPurchaseItemID.ToString()).n;
		}

		if (jsonObject.HasField(ServerFieldName.CreditsWhenLastPurchase.ToString())){
			UserBasicData.Instance.CreditsWhenLastPurchase = (ulong)jsonObject.GetField(ServerFieldName.CreditsWhenLastPurchase.ToString()).n;
		}

		if (jsonObject.HasField(ServerFieldName.LuckyWhenLastPurchase.ToString())){
			UserBasicData.Instance.LuckyWhenLastPurchase = (ulong)jsonObject.GetField(ServerFieldName.LuckyWhenLastPurchase.ToString()).n;
		}

		if (jsonObject.HasField(ServerFieldName.LuckyAddPeriodProgress.ToString())){
			UserBasicData.Instance.LuckyAddPeriodProgress = (int)jsonObject.GetField(ServerFieldName.LuckyAddPeriodProgress.ToString()).n;
		}

		UserBasicData.Instance.Save();
	}
}
