#define NEW_NOTIFY// zhousen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;
using System.Text;

public enum OpenPos
{
	Auto, // Auto means bankruptcy!!! Can not believe it but have to learn to accept it
    EnterLobby,
	GameUp,
	Lobby,
	GameBelow,
	CollectHourlyBonus,
	HugeWin,
	EpicWin,
    LobbyNotice,
}

public class AnalysisManager : Singleton<AnalysisManager>
{
	private static readonly string _delimiter = ",";

	private string _advertisingId = "";

	#region Init

	public void Init()
	{
		InitSensor();
		InitAdvertisingId();
	}

	void InitSensor()
	{
		string channel = PlatformManager.Instance.GetChannelString();
		bool isDebug = false;
#if DEBUG
		isDebug = true;
		channel += "_Debug";
#else
		channel += "_Release";
#endif

		SensorsData.Init(channel, "Trojan", isDebug, BuildUtility.GetBundleVersion());
		string userId = GetTrackUserId();
		SensorsData.SetUserId(userId);
	}

	void InitAdvertisingId()
	{
		Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) => {
			if(string.IsNullOrEmpty(error))
			{
				Debug.Log("Request advertisingId ok: " + advertisingId);
				_advertisingId = advertisingId;

				//force send ad id
				SendProfile();
			}
			else
			{
				Debug.Log("Request advertisingId error: " + error);
			}
		});
	}

	#endregion

	#region Private

	void TrackEvent(string name, Dictionary<string, object> data)
	{
		//Debug.Log("TrackEvent: " + name.ToString());

		//add a prefix to better differentiate with other project events
		name = "Trojan_" + name;

		//add a postfix for DEBUG version
#if DEBUG
		name += "_Debug";
        CheckDataType(name, data);
#endif
		SensorsData.TrackEvent(name, data);
	}

	#endregion

	#region Public

	public void TrackInstall()
	{
		SensorsData.TrackInstallation();
	}

	public void TouristLoginOrOverallInstall()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		//md5 is not useful
//		d ["str0"] = lastMD5;
//		d ["str1"] = currMD5;
		d ["str2"] = UserBasicData.Instance.UDID;
		d ["str3"] = UserDeviceLocalData.Instance.GetCurrSocialAppID;
		//critical fix: it must assign a string. Otherwise, execption throws
		d ["str4"] = UserDeviceLocalData.Instance.IsNewGame.ToString();

		AddDefaultValueToEvent (d);
		TrackEvent ("TouristLoginOrOverallInstall", d);
	}

	public void FirstOpenGame()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		TrackEvent("FirstOpenGame", d);
	}

	public void StartLoading()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		AddHourBonusData(d);
		TrackEvent("StartLoading", d);
	}

	public void EndLoading()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		AddHourBonusData(d);
		TrackEvent("EndLoading", d);
	}

	public void EnterLobby()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		AddHourBonusData(d);
		TrackEvent("EnterLobby", d);
	}

	public void EnterMachine(string name)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str0"] = name;
		AddDefaultValueToEvent(d);
		AddHourBonusData(d);
		TrackEvent("EnterMachine", d);
	}

	public void SendSpin(string machineName, CoreSpinResult spinResult, SpinMode spinMode, SmallGameState smallGameState, SpecialMode specialMode)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();

		d["str0"] = machineName;

		List<string> finalSymbolNames = ListUtility.MapList (spinResult.FinalSymbolList, (CoreSymbol s) => {
			return s.SymbolData.Name;
		});
        List<string> subNodeList = PropertyTrackManager.Instance.HandleSpinResul(spinResult);

        d["str1"] = string.Join(_delimiter, finalSymbolNames.ToArray());
		d["str2"] = spinResult.LuckyMode.ToString();
		d["str3"] = spinResult.Type.ToString();
		d["str4"] = spinMode.ToString();

		d["str5"] = TrackEventUtility.ConstructTriggerString(spinResult.CoreMachine);
		d["str6"] = TrackEventUtility.ConstructSmallGameType (spinResult.CoreMachine);
	    d["str8"] = TrackEventUtility.GetDoubleLevelUpStr();
	    d["str9"] = subNodeList[0];
        d["str10"] = subNodeList[1];

        d["integer0"] = spinResult.ConsumedBetAmount;
		d["integer1"] = CoreUtility.GetSpinResultRowId(spinResult);
		d["integer2"] = UserMachineData.Instance.TotalSpinCount;
		d["integer3"] = spinResult.WinAmount;
		d["integer4"] = spinResult.LongLuckyChange;
		d["integer5"] = UserMachineData.Instance.GetInitMachineSeed(machineName);
		d["integer6"] = UserMachineData.Instance.GetSpinCountOfMachine(machineName, smallGameState);
		d["integer7"] = spinResult.BetAmount;

		d["double0"] = spinResult.WinRatio;

		AddDefaultValueToEvent(d);

	    string spin = GetSpecialSpin(specialMode);
        TrackEvent(spin, d);
	}

	public void SendSpinLevelUp(string machineName, CoreSpinResult spinResult, SpinMode spinMode, SmallGameState smallGameState, SpecialMode specialMode){
		Dictionary<string, object> d = new Dictionary<string, object>();

		d["str0"] = machineName;

		List<string> finalSymbolNames = ListUtility.MapList (spinResult.SymbolList, (CoreSymbol s) => {
			return s.SymbolData.Name;
		});

		d["str1"] = string.Join(_delimiter, finalSymbolNames.ToArray());
		d["str2"] = spinResult.LuckyMode.ToString();
		d["str3"] = spinResult.Type.ToString();
		d["str4"] = spinMode.ToString();

		d["str5"] = TrackEventUtility.ConstructTriggerString(spinResult.CoreMachine);
		d["str8"] = TrackEventUtility.GetDoubleLevelUpStr();

		d["integer0"] = spinResult.ConsumedBetAmount;
		d["integer1"] = CoreUtility.GetSpinResultRowId(spinResult);
		d["integer2"] = UserMachineData.Instance.TotalSpinCount;
		d["integer3"] = spinResult.WinAmount;
		d["integer4"] = spinResult.LongLuckyChange;
		d["integer5"] = UserMachineData.Instance.GetInitMachineSeed(machineName);
		d["integer6"] = UserMachineData.Instance.GetSpinCountOfMachine(machineName, smallGameState);
		d["integer7"] = spinResult.BetAmount;

		d["double0"] = spinResult.WinRatio;

		AddDefaultValueToEvent(d);

        TrackEvent("Spin_Levelup", d);
	}

	public void SendWheelSpin(ulong betAmount, PuzzleMachine machine, float totalRatio, SmallGameState smallGameState, SpecialMode specialMode)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();

		CoreSpinResult spinResult = machine.CoreMachine.SpinResult;

		int winAmount = (int)(totalRatio * spinResult.BetAmount);

        List<string> subNodeList = PropertyTrackManager.Instance.HandleSpinResul(spinResult);

        // 转盘信息
        d["str0"] = machine.MachineName;
		d["str6"] = "Wheel";
		d["integer0"] = betAmount;
		d["integer3"] = winAmount;
		d["double0"] = totalRatio;

		/// 其他spin信息
		d["str2"] = spinResult.LuckyMode.ToString();
		d["str3"] = spinResult.Type.ToString();
		d["str4"] = machine._spinMode.ToString();
		d["str5"] = TrackEventUtility.ConstructTriggerString(spinResult.CoreMachine);
        d["str8"] = TrackEventUtility.GetDoubleLevelUpStr();
        d["str9"] = subNodeList[0];
        d["str10"] = subNodeList[1];

        d["integer1"] = CoreUtility.GetSpinResultRowId(spinResult);
		d["integer2"] = UserMachineData.Instance.TotalSpinCount;
		d["integer4"] = spinResult.LongLuckyChange;
		d["integer5"] = UserMachineData.Instance.GetInitMachineSeed(machine.CoreMachine.Name);
		d["integer6"] = UserMachineData.Instance.GetSpinCountOfMachine(machine.CoreMachine.Name, smallGameState);
		d["integer7"] = spinResult.BetAmount;

		AddDefaultValueToEvent(d);

        string spin = GetSpecialSpin(specialMode);
        TrackEvent(spin, d);
    }

	public void SendTapBox(PuzzleMachine machine, ulong winAmount, string customData, SpecialMode specialMode)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		CoreSpinResult spinResult = machine.CoreMachine.SpinResult;
        List<string> subNodeList = PropertyTrackManager.Instance.HandleSpinResul(spinResult);

        d["str0"] = machine.MachineName;
		d["str1"] = customData;
		d["str6"] = "TapBox";
		d["integer0"] = spinResult.BetAmount;
		d["integer3"] = winAmount;
		d["double0"] = (float)winAmount / (float)spinResult.BetAmount;

		d["str2"] = spinResult.LuckyMode.ToString();
		d["str3"] = spinResult.Type.ToString();
		d["str4"] = machine._spinMode.ToString();
		d["str5"] = TrackEventUtility.ConstructTriggerString(spinResult.CoreMachine);
        d["str8"] = TrackEventUtility.GetDoubleLevelUpStr();
        d["str9"] = subNodeList[0];
        d["str10"] = subNodeList[1];
        d["integer1"] = CoreUtility.GetSpinResultRowId(spinResult);
		d["integer2"] = UserMachineData.Instance.TotalSpinCount;
		d["integer4"] = spinResult.LongLuckyChange;
		d["integer5"] = UserMachineData.Instance.GetInitMachineSeed(machine.CoreMachine.Name);
		d["integer6"] = UserMachineData.Instance.GetSpinCountOfMachine(machine.CoreMachine.Name, SmallGameState.TapBox);

		AddDefaultValueToEvent(d);

        string spin = GetSpecialSpin(specialMode);
        TrackEvent(spin, d);
    }

	public void SendBetChange(string machineName, int prevBet, int curBet)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["integer0"] = prevBet;
		d["integer1"] = curBet;

		AddDefaultValueToEvent(d);

		TrackEvent("BetChange", d);
	}

	public void SendMaxBet(string machineName, int prevBet, int curBet)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["integer0"] = prevBet;
		d["integer1"] = curBet;

		AddDefaultValueToEvent(d);

		TrackEvent("MaxBet", d);
	}

	public void SendBankruptcy(string machineName, SpinMode spinMode, bool isLessThanMinBet, bool isLessThanCurBet, ulong currentBet, ulong totalCredits)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();

		d["str0"] = machineName;
		d["str1"] = spinMode.ToString();
		d["integer0"] = isLessThanMinBet ? 1 : 0;
		d["integer1"] = isLessThanCurBet ? 1 : 0;
		d["integer2"] = currentBet;
		d["integer3"] = totalCredits;

		AddDefaultValueToEvent(d);

		TrackEvent("Bankruptcy", d);
	}

	public void SendBankruptcyCredits(string machineName, int credits){
		Dictionary<string, object> d = new Dictionary<string, object>();

		d["str6"] = machineName;
		d["integer0"] = UserBasicData.Instance.BuyNumber;
		d["integer1"] = credits;
		d["integer3"] = UserBasicData.Instance.TotalPayAmount;

		AddDefaultValueToEvent(d);

		TrackEvent("BankruptcyCredits", d);
	}

	public void OpenShop()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str6"] = GetCurrentMachineName ();
		AddDefaultValueToEvent(d);
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		TrackEvent("OpenShop", d);
	}

	public void CloseShop()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		TrackEvent("CloseShop", d);
	}

    public void MachineComment(string machineName, int rate)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["str0"] = machineName;
        d["integer0"] = rate;
        AddDefaultValueToEvent(d);
        TrackEvent("RateMachine", d); 
    }

	public void StartPurchase()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		TrackEvent("StartPurchase", d);
	}

	public void ChannelFailPurchase(string error = "")
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str4"] = error;
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		TrackEvent("ChannelFailPurchase", d);
	}

	public void ChannelSuccessPurchase()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		TrackEvent("ChannelSuccessPurchase", d);
	}

	public void StartVerify()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		TrackEvent("StartVerify", d);
	}

	public void FailVerify(string error = "")
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str4"] = error;
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		TrackEvent("FailVerify", d);
	}

	public void SuccessVerify(string success = "")
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str4"] = success;
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		d["integer1"] = UserBasicData.Instance.BuyNumber;
        PropertyTrackManager.Instance.AddPurcahseCreateNodeInfo(d);
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);
		TrackEvent("SuccessVerify", d);
	}
		
	public void SkipVerifyAndSuccessPurchase()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str6"] = GetCurrentMachineName ();
		StoreController.Instance.CurrStoreAnalysisData.FillData(d);
		AddDefaultValueToEvent(d);
		d["integer1"] = UserBasicData.Instance.BuyNumber;
		PropertyTrackManager.Instance.AddPurcahseCreateNodeInfo(d);
		PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);
		TrackEvent("SkipVerifyAndSuccessPurchase", d);
	}

	public void SuccessReceiveIAP(string productId, long credits, int vipPoint)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str1"] = productId;
		d["integer0"] = credits;
		d["integer1"] = vipPoint;
		AddDefaultValueToEvent(d);
		TrackEvent("SuccessReceiveIAP", d);
	}

	public void OpenDailyBonus()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		TrackEvent("OpenDailyBonus", d);
	}

	public void SpinDailyBonus()
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		TrackEvent("SpinDailyBonus", d);
	}

	public void GetDailyBonus(int credit, int longLuck, int dayType)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["integer0"] = credit;
		d["integer1"] = longLuck;
		d["integer2"] = dayType;
		AddDefaultValueToEvent(d);
		TrackEvent("GetDailyBonus", d);
	}

    public void OnWheelOfLuckEnd(long luckyCredits, int signInDays, long totalCredits)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["integer0"] = luckyCredits;
        d["integer1"] = signInDays;
        d["integer2"] = totalCredits;
        AddDefaultValueToEvent(d);
        TrackEvent("WheelOfLuck", d);
    }

    public void OnDiceGameOver(int diceRatio,long winCredits, float price)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        d["integer0"] = diceRatio;
        d["integer1"] = winCredits;
        d["str6"] = GetCurrentMachineName();
        d["double0"] = price;
        AddDefaultValueToEvent(d);
        TrackEvent("CrazyDice", d);
    }

    public void GetTimeBonus(int credit, int lucky)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["integer0"] = credit;
		d["integer1"] = lucky;
		AddDefaultValueToEvent(d);
		TrackEvent("GetTimeBonus", d);
	}

	public void GetFacebookBonus(string id, int credit, int longluck)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["str0"] = id;
		d["integer0"] = credit;
		d["integer1"] = longluck;
		AddDefaultValueToEvent(d);
		TrackEvent("GetFacebookBonus", d);
	}

	public void WatchBonusAd(int watchFinished, string machineName, RewardAdType type)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
        d["str0"] = machineName;
        d["str1"] = type.ToString();
        d["integer3"] = watchFinished;
		TrackEvent("WatchBonusAd", d);
	}

	public void WatchAd(int watchFinished,ADSPos pos = ADSPos.Entermachine)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		d["integer3"] = watchFinished;
		d["str1"] = pos.ToString();
		TrackEvent("WatchAd", d);
	}

	public void LevelUp(int currLevel, int credits, long longluck)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		d["integer0"] = currLevel;
		d["integer1"] = UserMachineData.Instance.TotalSpinCount;
		d["integer2"] = (NetworkTimeHelper.Instance.GetNowTime() - UserBasicData.Instance.RegistrationTime).TotalDays;
        d["integer3"] = credits;
        d["integer4"] = longluck;
        TrackEvent("LevelUp", d);
	}

	public void VIPLevelUp(int currLevel){
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d ["integer0"] = currLevel;
		d ["integer1"] = UserMachineData.Instance.TotalSpinCount;
		d ["integer2"] = (NetworkTimeHelper.Instance.GetNowTime () - UserBasicData.Instance.RegistrationTime).TotalDays;
		TrackEvent ("VIPLevelUp", d);
	}

	public void RemotePushReceived (string type,string title)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = type;
		d ["str1"] = title;
		TrackEvent ("Trojan_StartAppByNotification", d);
	}

	public void GetBackFlowReward (int credit,int LTLucky,int leftDay)
	{
		Dictionary<string,object> d = new Dictionary<string,object> ();
		AddDefaultValueToEvent (d);
		d ["integer0"] = credit;
		d ["integer1"] = LTLucky;
		d ["integer2"] = leftDay;
		TrackEvent ("GetReturnBonus", d);
	}
	public void Get5MinuteBonus (ulong credit,int LTLucky)
	{
		Dictionary<string,object> d = new Dictionary<string,object> ();
		AddDefaultValueToEvent (d);
		d ["integer0"] = (int)credit;
		d ["integer1"] = LTLucky;
		TrackEvent ("Get5MinuteBonus", d);
	}

	#if Trojan_FB

	private void SetFacebookUserProfileCallback(Facebook.Unity.IGraphResult result)
	{
		Debug.Log("SetFacebookUserProfileCallback called");
		Debug.Log("RawResult = " + result.RawResult + " Error: " + result.Error);
		if (FacebookUtility.ResultValid (result)) {
			Debug.Log("result is valid!");
		}
	}

	public void SendStartLoginFacebook()
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		TrackEvent("StartLoginFacebook", d);
	}

	public void SendLoginFacebookSuccessCallback()
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d["str0"] = FacebookHelper.SocialId;
		d ["str1"] = FacebookHelper.UserEmail;
		TrackEvent("LoginFacebookSuccess", d);
	}

	public void SendLoginFacebookFailCallback()
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		TrackEvent("LoginFacebookFail", d);
	}

	public void ReadMail(MailInfor info, MailInforExtension extension){
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = info.Type;// 邮件类型(具体参考MailType）
		d ["str1"] = extension != null ? extension.SystemType.ToString() : "";// 系统邮件才有的类型

		if (info.Bonus > 0) {
			d ["integer0"] = info.Bonus;
		} else if (extension != null) {
			if (extension.Priority == 99)// 是礼包
			{
				d ["str0"] = "Gift";
			}
			d ["integer0"] = extension.Credits;
			d ["integer1"] = extension.LongLucky;
		}

		TrackEvent("ReadMailBox", d);
	}

	#endif

	public void SendProfile()
	{
		string deviceId = DeviceUtility.GetDeviceId();
		Dictionary<string, object> dict = new Dictionary<string, object>();
		dict["udID"] = UserBasicData.Instance.UDID;
		dict["deviceID"] = deviceId;
		dict["userID"] = deviceId;

		dict["isFacebookLoggedIn"] = FacebookHelper.IsLoggedIn;
		if(!string.IsNullOrEmpty(FacebookHelper.SocialId))
			dict["facebookID"] = FacebookHelper.SocialId;
	    
		dict["registerTime"] = TimeUtility.ConvertDateTime2String(UserDeviceLocalData.Instance.FirstEnterGameTime);
		dict["channel"] = PlatformManager.Instance.GetChannelString();
		dict["faceBookBindTime"] = TimeUtility.ConvertDateTime2String(UserDeviceLocalData.Instance.FirstBindFacebookTime);
		dict["lastLoginTime"] = TimeUtility.ConvertDateTime2String(UserDeviceLocalData.Instance.LastLoginTime);
		dict["lastPayTime"] = TimeUtility.ConvertDateTime2String(UserBasicData.Instance.LastPayTime);

		dict["lv"] = (int)UserLevelSystem.Instance.CurrUserLevelData.Level;
		dict["exp"] = (int)UserLevelSystem.Instance.CurrUserLevelData.LevelPoint;
		dict["vipLevel"] = (int)VIPSystem.Instance.GetCurrVIPLevelData.Level;
		dict["vipExp"] = (int)VIPSystem.Instance.GetCurrVIPLevelData.LevelPoint;
		dict["credit"] = UserBasicData.Instance.Credits;
		dict["lucky"] = UserBasicData.Instance.LongLucky;
		dict["payTimes"] = UserBasicData.Instance.BuyNumber;
		dict["payAmount"] = UserBasicData.Instance.TotalPayAmount;
		dict["currentMachine"] = UserMachineData.Instance.CurrentMachine;
		dict["hasRemoveAd"] = UserBasicData.Instance.NoAds;
		dict["version"] = BuildUtility.GetBundleVersion();
		dict["resourceVersion"] = LiveUpdateManager.Instance.GetUsingResourceVersion();
		dict["piggyBankCredits"] = UserBasicData.Instance.PiggyBankCoins;
		dict["totalSpinCount"] = UserMachineData.Instance.TotalSpinCount;
	
		// 这个字段前后2次传的参数类型不同，会导致神策爆掉，于是用个新KEY
		//dict ["recency"] = GroupRepresentConfig.Instance.GetHasPayInValidPeriodRepresent(UserBasicData.Instance.LastPayTime);
/*		
		dict ["recency_paid"] = GroupRepresentConfig.Instance.GetHasPayInValidPeriodRepresent(UserBasicData.Instance.LastPayTime);
		dict["frequency"] = GroupRepresentConfig.Instance.GetPayCountRepresent();
		dict["monetary"] = GroupRepresentConfig.Instance.GetPayAmountRepresent();
		dict ["group"] = UserGroup.GetUserGroupID ();
*/		
		GroupMember groupMember = new GroupMember ();
		GroupMemberRepresent memberRepresent = new GroupMemberRepresent(groupMember);
		dict ["recency_paid"] = memberRepresent.HasPayInValidPeriod;
		dict ["frequency"] = memberRepresent.PayCount;
		dict ["monetary"] = memberRepresent.PayAmount;
		dict ["H"] = memberRepresent.HistoryMaxPay.ToString();
		dict ["historyMax"] = UserBasicData.Instance.HistoryMaxPaid;
		dict ["group"] = UserGroup.GetUserGroupID ();
		dict ["SPIN"] = memberRepresent.SpinAverage.ToString();
		dict ["Session"] = memberRepresent.SessionAverage.ToString();
		dict ["group2"] = GroupConfig.Instance.GetActiveID().ToString();
		dict ["spin"] = groupMember.SpinAverage;
		dict ["session"] = groupMember.SessionAverage;
	    dict["payNodeCount"] = PropertyTrackManager.Instance.PayQueueLength();
	    dict["payQueueTotalAmount"] = PropertyTrackManager.Instance.TotalPayAmount();
	    dict["payWinNodeCount"] = PropertyTrackManager.Instance.PayWinQueueLength();
	    dict["payWinQueueTotalAmount"] = PropertyTrackManager.Instance.TotalPayWinAmount();
        dict["AdjustId"] = AdjustManager.Instance.AdjustId;

        if (!string.IsNullOrEmpty(_advertisingId))
			dict["advertiserId"] = _advertisingId;
		
		if (!string.IsNullOrEmpty (FacebookHelper.UserEmail))
			dict ["Email"] = FacebookHelper.UserEmail;

	    dict["Source"] = AdjustManager.Instance.UserSource;
        dict["AdStrategy"] = GroupConfig.Instance.GetAdStrategyId();

        SensorsData.SetDicProfile(dict);
	    PackageConfigManager.Instance.CurPackageConfig.FireBaseTrackEvent(dict);

		#if Trojan_FB
        FacebookUtility.SetUserProfile(deviceId, dict, _advertisingId, SetFacebookUserProfileCallback);
		#endif
	}

	public string GetTrackUserId()
	{
		return DeviceUtility.GetDeviceId();
	}

	public void OpenLoadServerData(int serverLevel, int serverVIPPoint, ulong serverCredits)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();
		AddDefaultValueToEvent(dict);
		AddServerLocalData(dict, serverLevel, serverVIPPoint, serverCredits);
		TrackEvent("OpenLoadServerData", dict);
		Debug.Log("OpenLoadServerDataMessage");
	}

	public void SelectLoadServerData(int serverLevel, int serverVIPPoint, ulong serverCredits)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();
		AddDefaultValueToEvent(dict);
		AddServerLocalData(dict, serverLevel, serverVIPPoint, serverCredits);
		TrackEvent("SelectLoadServerData", dict);
		Debug.Log("SelectLoadServerDataMessage");
	}

	public void SelectUseLocalData(int serverLevel, int serverVIPPoint, ulong serverCredits)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();
		AddDefaultValueToEvent(dict);
		AddServerLocalData(dict, serverLevel, serverVIPPoint, serverCredits);
		TrackEvent("SelectUseLocalData", dict);
		Debug.Log("SelectUseLocalDataMessage");
	}

	public void TournamentWinToRank(string machineName,string bet,int lastTime)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		d["str0"] = machineName;
		d["str1"] = bet;
		d["integer0"] = lastTime;
		TrackEvent("TournamentWinToRank", d);
	}

	public void TournamentFinalRank(string machineName, string bet,int rank,int score,int reward,int poolcoins)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		AddDefaultValueToEvent(d);
		d["str0"] = machineName;
		d["str1"] = bet;
		d["integer0"] = rank;
		d["integer1"] = score;
		d["integer2"] = reward;
		d["integer3"] = poolcoins;
		d["integer4"] = reward * CoreConfig.Instance.LuckyConfig.RewardFactor;
		TrackEvent("TournamentFinalRank", d);
	}

	public void SendGift(int credits, int creditsDailySum){
		Dictionary<string, object> d = new Dictionary<string , object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = FacebookHelper.SocialId;
		d ["integer0"] = credits;
		d ["integer1"] = creditsDailySum;
		TrackEvent ("FB_SendGift", d);
	}

	public void CollectGift(int credits, int creditsDailySum){
		Dictionary<string, object> d = new Dictionary<string , object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = FacebookHelper.SocialId;
		d ["integer0"] = credits;
		d ["integer1"] = creditsDailySum;
		TrackEvent ("FB_ReceiveGift", d);
	}

	public void InviteFriend(){
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = FacebookHelper.SocialId;
		TrackEvent ("FB_InviteFriend", d);
	}

	public void PushNotificationEnter(){
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		TrackEvent ("PushNotificationEnter", d);
	}

	public void SendSaveUserDataToServer()
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		TrackEvent ("SaveUserDataToServer", d);
	}

	public void SendSaveUserDataToServerCallback(string result)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d["str0"] = result;
		TrackEvent ("SaveUserDataToServerComplete", d);
	}

	public void SendFetchUserDataFromServer()
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		TrackEvent ("FetchUserDataFromServer", d);
	}

	public void SendFetchUserDataFromServerCallback(string result)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d["str0"] = result;
		TrackEvent ("FetchUserDataFromServerComplete", d);
	}

	public void SendFetchGMUserDataFromServer()
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		TrackEvent ("FetchGMUserDataFromServer", d);
	}

	public void SendFetchGMUserDataFromServerCallback(string result)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d["str0"] = result;
		TrackEvent ("FetchGMUserDataFromServerComplete", d);
	}

	public void SendOverwriteUserDataError(string errorInfo)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d["str0"] = errorInfo;
		d["str1"] = UserDeviceLocalData.Instance.GetCurrSocialAppID;
		d["str2"] = UserBasicData.Instance.UDID;
		TrackEvent ("OverwriteUserDataError", d);
	}

	public void LocalPush(string info)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = "Local";
		d ["str1"] = info;
		TrackEvent("StartAppByNotification",d);
	}

	private void ProcessNotify(int id){
		if (NotifyIDFactory.IsLocalNotification(id)){
			id = NotifyIDFactory.ParseNotifyID(id);
			LogUtility.Log("NotifyIDFactory.IsLocalNotification(id) = " + id, Color.red);
			LocalNotificationData data = LocalNotificationConfig.Instance.GetData(id);
			if (data != null){
				LogUtility.Log("key = " + data.Key, Color.red);
				LocalPush(data.Key);
			}
		}else if (NotifyIDFactory.IsFestivalNotification(id)){
			id = NotifyIDFactory.ParseNotifyID(id);
			LogUtility.Log("NotifyIDFactory.IsFestivalNotification(id) = " + id, Color.red);
			LocalNotificationfestivalData data = LocalNotificationfestivalConfig.Instance.GetData(id);
			if (data != null){
				LogUtility.Log("key = " + data.Key, Color.red);
				LocalPush(data.Key);
			}
		}else {
			LogUtility.Log("Enum.IsDefined(typeof(NotificationUtility.NotifyType),id) = " + id, Color.red);
			if(Enum.IsDefined(typeof(NotificationUtility.NotifyType),id)){
				NotificationUtility.NotifyType notify = (NotificationUtility.NotifyType)id;
				LogUtility.Log("notify = " + notify.ToString(), Color.red);
				LocalPush(notify.ToString());
			}
		}
	}

	public void LocalIOSPush(string ID){
		int id = 0;
		bool result = int.TryParse(ID,out id);

		#if NEW_NOTIFY// zhousen
		if(result)
		{
			ProcessNotify(id);
		}
		else{
			LocalPush(ID);
		}
		#else
		LocalPush(ID);
		#endif
	}
		
	public void LocalAndroidPush(string ID)
	{
		int id = 0;
		int.TryParse(ID,out id);

		#if NEW_NOTIFY// zhousen
		if(id != 0)
		{
			ProcessNotify(id);
		}
		#else
		if(id != 0)
		{
			if(Enum.IsDefined(typeof(NotificationUtility.NotifyType),id))
			{
				NotificationUtility.NotifyType notify = (NotificationUtility.NotifyType)id;
				LocalPush(notify.ToString());
			}
		}
		#endif
	}

	public void SendDeviceToken(string token)
	{
		Dictionary<string, object> d = new Dictionary<string, object> ();
		AddDefaultValueToEvent (d);
		d ["str0"] = token;
		TrackEvent("DeviceTokenReceived",d);
	}

    public void FreeNodeCreate(PropertyTrackFreeNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str2"] = node.CreditsSource.ToString();
        d["str3"] = node.BornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        d["integer2"] = UserBasicData.Instance.LongLucky;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, false);

        TrackEvent("FN_Create", d);
    }

    public void FreeWinNodeCreate(PropertyTrackFreeWinNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str2"] = node.CreditsSource.ToString();
        d["str3"] = node.BornTime.ToString();
        d["str4"] = node.LinkNodeBornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str6"] = node.LinkNodeBornTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("WFN_Create", d);
    }

    public void FreeNodeDestroy(PropertyTrackFreeNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str2"] = node.CreditsSource.ToString();
        d["str3"] = node.BornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        d["integer2"] = node.DeQueueTotalSpinCount - node.EnQueueTotalSpinCount;
        d["double0"] = (NetworkTimeHelper.Instance.GetNowTime() - node.BornTime).TotalHours;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, false);

        TrackEvent("FN_Out", d);
    }

    public void FreeWinNodeDestroy(PropertyTrackFreeWinNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str2"] = node.CreditsSource.ToString();
        d["str3"] = node.BornTime.ToString();
        d["str4"] = node.LinkNodeBornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str6"] = node.LinkNodeBornTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        d["integer2"] = node.DeQueueTotalSpinCount - node.EnQueueTotalSpinCount;
        d["integer3"] = node.DeQueueTotalSpinCount - node.LinkNodeBornTotalSpinCount;
        d["double0"] = (NetworkTimeHelper.Instance.GetNowTime() - node.BornTime).TotalHours;
        d["double1"] = (NetworkTimeHelper.Instance.GetNowTime() - node.LinkNodeBornTime).TotalHours;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, false);

        TrackEvent("WFN_Out", d);
    }

    public void PayNodeCreate(PropertyTrackPayNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str2"] = node.PaidOrderId;
        d["str3"] = node.BornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("PN_Create", d);
    }

    public void PayWinNodeCreate(PropertyTrackPayWinNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str3"] = node.BornTime.ToString();
        d["str4"] = node.LinkNodeBornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str6"] = node.LinkNodeBornTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("WN_Create", d);
    }

    public void PayNodeDestroy(PropertyTrackPayNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str2"] = node.PaidOrderId;
        d["str3"] = node.BornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        d["integer2"] = node.DeQueueTotalSpinCount - node.EnQueueTotalSpinCount;
        d["double0"] = (NetworkTimeHelper.Instance.GetNowTime() - node.BornTime).TotalHours;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("PN_Out", d);
    }

    public void PayWinNodeDestroy(PropertyTrackPayWinNode node)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = node.NodeType.ToString();
        d["str1"] = node.Id.ToString();
        d["str3"] = node.BornTime.ToString();
        d["str4"] = node.LinkNodeBornTime.ToString();
        d["str5"] = node.EnQueueTotalSpinCount.ToString();
        d["str6"] = node.LinkNodeBornTotalSpinCount.ToString();
        d["str7"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str8"] = UserMachineData.Instance.TotalSpinCount.ToString();
        d["integer0"] = (int)node.OriginalAmount;
        d["integer1"] = (int)node.RemainAmount;
        d["integer2"] = node.DeQueueTotalSpinCount - node.EnQueueTotalSpinCount;
        d["integer3"] = node.DeQueueTotalSpinCount - node.LinkNodeBornTotalSpinCount;
        d["double0"] = (NetworkTimeHelper.Instance.GetNowTime() - node.BornTime).TotalHours;
        d["double1"] = (NetworkTimeHelper.Instance.GetNowTime() - node.LinkNodeBornTime).TotalHours;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("WN_Out", d);
    }

    public void PropertyNodeChange(PropertyTrackBaseNode preNode, PropertyTrackBaseNode curNode)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = curNode.NodeType.ToString();
        d["str1"] = curNode.Id.ToString();
        d["str3"] = NetworkTimeHelper.Instance.GetNowTime().ToString();
        d["str4"] = curNode.LastDecreaseTime.ToString();
        d["str5"] = preNode.Id.ToString();
        d["integer0"] = (int)curNode.OriginalAmount;
        d["integer1"] = (int)curNode.RemainAmount;
        d["double0"] = (NetworkTimeHelper.Instance.GetNowTime() - curNode.LastDecreaseTime).TotalHours;
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("Node_Change", d);
    }

    public void PropertyInfoAfterWholeSubtract()
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        PropertyTrackManager.Instance.AddDefaultValueToEvent(d, true);

        TrackEvent("PW_AfterNodeChange_AfterSpin", d);
    }

    public void PayUserDailySpin(int payUserGroupId, bool isSpinToday)
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["integer0"] = payUserGroupId;
        d["integer1"] = isSpinToday ? 1 : 0;

        TrackEvent("PayUserMonitor", d);
    }

	public void ReceiveMail(string mailStr, string sysStr)
	{
        Dictionary<string, object> d = new Dictionary<string, object>();
        AddDefaultValueToEvent(d);
        d["str0"] = mailStr;
        d["str1"] = sysStr;

		LogUtility.Log("ReceiveMail mail = "+mailStr+" sys = "+sysStr, Color.red);

        TrackEvent("ReceiveMail", d);
	}

    #endregion

    private void AddDefaultValueToEvent(Dictionary<string, object> dic)
	{
		dic["Credit"] = UserBasicData.Instance.Credits;
		dic["LTLucky"] = UserBasicData.Instance.LongLucky;
		dic["channel"] = PlatformManager.Instance.GetChannelString();

		dic["lv"] = (int)UserLevelSystem.Instance.CurrUserLevelData.Level;
		dic["exp"] = (int)UserLevelSystem.Instance.CurrUserLevelData.LevelPoint;
		dic["vipLevel"] = (int)VIPSystem.Instance.GetCurrVIPLevelData.Level;
		dic["vipExp"] = (int)VIPSystem.Instance.GetCurrVIPLevelData.LevelPoint;
		dic["version"] = BuildUtility.GetBundleVersion();
		dic["resourceVersion"] = LiveUpdateManager.Instance.GetUsingResourceVersion();
	}

	private void AddHourBonusData(Dictionary<string, object> dic)
	{
		dic["integer0"] = BonusHelper.GetHoursBonusLastSecond() <= 0 ? 1 : 0;
		dic["integer1"] = BonusHelper.GetHoursBonusLastSecond();

	}

	private void AddServerLocalData(Dictionary<string, object> dict, int serverLevel, int serverVIPPoint, ulong serverCredits)
	{
		dict["integer0"] = serverLevel;
		dict["integer1"] = serverVIPPoint;
		dict["integer2"] = serverCredits;
		dict["integer3"] = (int)UserBasicData.Instance.UserLevel.Level;
		dict["integer4"] = UserBasicData.Instance.VIPPoint;
		dict["integer5"] = UserBasicData.Instance.Credits;
	}

	public void StartSendProfilerRuntime(){
		StartCoroutine (RuntimeSendProfilerCoroutine());
	}

	private IEnumerator RuntimeSendProfilerCoroutine(){
		while (true) {
			SendProfileHelper.SendProfile();
			yield return new WaitForEndOfFrame ();
		}
	}

	public void DeepLink(string url)
	{
		GameDebug.Log ("Great! You installed the app due to the ad. URL is " + url);
		if(!string.IsNullOrEmpty(url))
		{
			string mainPart = url.Substring (20);
			string[] seperatePart = mainPart.Split ('/');

			if (seperatePart.Length >= 2) {
				GameDebug.Log ("Campaign is " + seperatePart [0]);
				GameDebug.Log ("AdSet is " + seperatePart [1]);
				UpdateAdScheme (seperatePart [0], seperatePart [1]);
			}
		}
	}

	public void UpdateAdScheme(string campaign, string adset)
	{
		Dictionary<string, object> dic = new Dictionary<string, object> (); 
		dic.Add ("AdCampaign", campaign);
		dic.Add ("AdSet", adset);
		SensorsData.SetDicProfile(dic);
	}

	private string GetCurrentMachineName(){
		if (ScenesController.Instance == null)
			return "None";

		string currentSceneName = ScenesController.Instance.GetCurrSceneName ();
		GameScene gameScene = GameScene.Instance;

		if (gameScene != null && currentSceneName.Equals ("Game")) {
			return gameScene.PuzzleMachine.CoreMachine.Name;
		}
		return "None";
	}

    void CheckDataType(string eventName, Dictionary<string, object> d)
    {
        foreach (var kv in d)
        {
            string k = kv.Key;
            object v = kv.Value;

            if ((k.StartsWith("str") && v.GetType() != typeof(string))
                || (k.StartsWith("interger") && v.GetType() != typeof(int))
                || (k.StartsWith("double") && v.GetType() != typeof(double) && v.GetType() != typeof(float)))
            {
                Debug.LogError(String.Format("AnalysisManager : typeError, eventName : {0}, keyName : {1}, value : {2}", eventName, k, v));
            }

        }
    }

    string GetSpecialSpin(SpecialMode specialMode)
    {
        string result = "";
        switch (specialMode)
        {
            case SpecialMode.Normal:
                result = "Spin";
                break;
            case SpecialMode.DoubleWin:
                result = "Spin_doubleWin";
                break;
            case SpecialMode.FreeMaxBet:
                result = "Spin_freeMaxBet";
                break;
        }

        return result;
    }
}
