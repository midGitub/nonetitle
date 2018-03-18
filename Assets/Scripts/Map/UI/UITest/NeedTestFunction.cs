using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;
using System.Text.RegularExpressions;

public class NeedTestFunction : MonoBehaviour
{

	public void AddServerTime(string hours)
	{
		float h = Convert.ToSingle(hours);
		NetworkTimeHelper.Instance.AddDebugHours(h);
	}

	#if DEBUG
	public void SetServerTime(string dateInfo)
	{
		NetworkTimeHelper.Instance.AddDebguHours = 0;

		DateTime targetDate = Convert.ToDateTime(dateInfo);
		DateTime curTime = NetworkTimeHelper.Instance.GetNowTime();
		TimeSpan span = targetDate - curTime;
		NetworkTimeHelper.Instance.AddDebguHours = (float)span.TotalHours;
		LogUtility.Log("DebugConsole : current sever time is : " + NetworkTimeHelper.Instance.GetNowTime(), Color.red);
	}
	#endif

	public void AddVIP(string point)
	{
		VIPSystem.Instance.AddVIPPoint(Convert.ToInt32(point));
	}

	public void AddLevelXP(string point)
	{
		UserLevelSystem.Instance.AddLevelXP(Convert.ToInt32(point));
	}

	public void ShowDailyBouns()
	{
		DayBonus.Instance.Test();
	}

	public void CleanVIP()
	{
		UserBasicData.Instance.SetVIPPoint(0, true);
	}

	public void CleanUserLevel()
	{
		UserLevelSystem.Instance.TestCleanLevel();
	}

	public void CleanCoins()
	{
		UserBasicData.Instance.CleanCredits();
	}

	public void AddCoins(string coins)
	{
		UInt64 num;
		bool result = UInt64.TryParse(coins, out num);
		if (result){
			UserBasicData.Instance.AddCredits(num, FreeCreditsSource.NotFree, true);
		}
	}

	public void AddLastDicePayDate(string days)
	{
		LogUtility.Log("previous pay for dice date " + UserDeviceLocalData.Instance.LastPayForDiceDate);
		UserDeviceLocalData.Instance.LastPayForDiceDate = UserDeviceLocalData.Instance.LastPayForDiceDate.AddDays(-Convert.ToInt32(days));
		LogUtility.Log("cur pay for dice date " + UserDeviceLocalData.Instance.LastPayForDiceDate);
	}

	public void AddLastCloseDicePageDate(string days)
	{
		LogUtility.Log("previous close dice page date " + UserDeviceLocalData.Instance.LastCloseDicePageDate);
		UserDeviceLocalData.Instance.LastCloseDicePageDate = UserDeviceLocalData.Instance.LastCloseDicePageDate.AddDays(-Convert.ToInt32(days));
		LogUtility.Log("cur close dice page date " + UserDeviceLocalData.Instance.LastCloseDicePageDate);
	}

    public void OpenDiceUiWithWinAmount(string arg)
    {
#if DEBUG
        ulong winAmount;
        float defaultWinRatio = 40;
        if (!ulong.TryParse(arg, out winAmount))
            Debug.LogError("try parse input str to ulong type failed, please check the input string : " + arg);
        DiceManager.Instance.LoadDiceGameForTestPurpose(winAmount, defaultWinRatio);
#endif
    }

    public void SetPayUser(string arg)
	{
		bool isPayuser = int.Parse(arg) > 0;
		UserBasicData.Instance.PlayerPayState = isPayuser ? UserPayState.PayUser : UserPayState.FreeUser;
		if (!isPayuser)
		{
			UserBasicData.Instance.SetTotalPayAmount(0, false);
			UserBasicData.Instance.SetHistoryMaxPaid(0, false);
			UserBasicData.Instance.SetBuyNumber(0, false);
			UserBasicData.Instance.SetLastPayTime(NetworkTimeHelper.Instance.GetNowTime().AddDays(-100),false);
		}
	}

	public void EnableHugeWinMode()
	{
		CoreDebugManager.Instance.IsHugeWinMode = true;
	}

	public void DisableHigeWinMode()
	{
		CoreDebugManager.Instance.IsHugeWinMode = false;
	}

	public void ShowGroup()
	{
		GameObject gameobject = null;
		if (GameObject.Find ("Test_Group(Clone)") == null)
			gameobject = UGUIUtility.InstantiateUI ("Map/UIBar/Test_Group");
		else 
		{
			gameobject = GameObject.Find ("Test_Group(Clone)");
			Canvas canvas = gameobject.GetComponentInChildren<Canvas> ();
			canvas.enabled = !canvas.enabled;
		}
	}

	public void SetWatchBonusAdArgs(string arg)
	{
		string[] temple = arg.Split(',');
		//it does't matter use parse instead of tryparse here
		List<int> result = ListUtility.MapList(temple, x => int.Parse(x));

		AdBonusConfig.Instance.RequireSpinCountList = result;
	}


	public void DailyBounsTimeMinusOneDay()
	{
		UserBasicData.Instance.SetLastGetDayBonusDate(DateTime.Now.AddDays(-1));
	}
		
	public void CleanSocialID()
	{
		// UserDeviceLocalData.Instance.GetCurrSocialAppID = UserIDWithPrefix.GetDeviceIDWhithPrefix();
		//清空但当前运行不切换存档
	}

	public void RegisterSocialID(string socialID)
	{
		CitrusEventManager.instance.Raise(new UserLoginOrRegisterEvent(socialID));
	}

	public void UpDataToServer()
	{
		CitrusEventManager.instance.Raise(new SaveUserDataToServerEvent());
	}

	public void ForcedUpDataToServer()
	{
		UserDataHelper.Instance.SaveUserDataToServer(true);
	}

	public void MakeConflictAndUpdateTOserver()
	{
		UserBasicData.Instance.SetLastGetHourBonusDate(NetworkTimeHelper.Instance.GetNowTime().AddDays(3));
	}

	public void UseTourist()
	{
		UserDataHelper.Instance.TryLogOutSocialApp();
	}

	public void ShowADSVideo()
	{
		ADSManager.Instance.ShowVideoADS();
	}

	public void ShowADSPicture()
	{
		ADSManager.Instance.ShowPictureADS();
	}

	public void LoadAPR()
	{
		AppLovin.PreloadInterstitial();
	}

	public void ShowAPR()
	{
		if(AppLovin.HasPreloadedInterstitial())
		{
			// An ad is currently available, so show the interstitial.
			AppLovin.ShowInterstitial();
		}
		else
		{
			Debug.Log("视频还没有加载");
			// No ad is available.  Perform failover logic...
		}
	}

	public void TouristsGetUDID()
	{
		CitrusEventManager.instance.Raise(new UserLoginOrRegisterEvent(UserIDWithPrefix.GetDeviceIDWithPrefix()));
	}

	public void CleanLuck()
	{
		UserBasicData.Instance.CleanLongLucky();
	}

	public void SetFPS(string s)
	{
		int fps = 0;
		int.TryParse(s, out fps);
		if(fps == 30)
			SystemUtility.Set30FPS();
		else if(fps == 60)
			SystemUtility.Set60FPS();
		else
			Debug.LogError("Set FPS fail, set it to 30 or 60");
	}

	public void ShareToFaceBook(string dir)
	{
		ShareManager.Instance.PublishEasyShare
					  (
						  ServerConfig.APPLINK
						  ,
						 "Huge Win Slots",
							dir
						 ,
			"http://us.cj.down.s3-website-us-east-1.amazonaws.com/slots-release/image/share.jpg"
		);
	}

	public void ShareToFaceBookWithScreeSort()
	{
		ScreenShotMakeTexture.TakeScreenshot(this, (obj) =>
		{
			FacebookUtility.UploadScreenShotAndShare(obj.EncodeToPNG(), null);
			Debug.Log("发布截图视频成功");
		});
	}

	public void AddCoinsInPiggyBank(string coins)
	{
		PiggyBankSystem.Instance.AddCoinsInPiggyBank(Convert.ToInt32(coins));
	}

	public void CleanCoinsInPiggyBank()
	{
		UserBasicData.Instance.SetPiggyBankCoins(PiggyBankConfig.Instance.GetFirstPiggyBankData().MinCredits, false);
		PiggyBankSystem.Instance.UpdateData();
	}

	public void StartADTest()
	{
		//ADTest.Instance.StartTest();
	}

	public void GetFaceBookIcoin(string id)
	{
		FindObjectOfType<MakeSpriteForFaceBook>().GetIcon(id);
	}

	public void TournamentGetInfor()
	{
		CitrusEventManager.instance.Raise(new CallTournamentServerEvent(TournamentServerFunction.GetInfor, 50));
	}

	public void TournamentGetReward()
	{
		CitrusEventManager.instance.Raise(new CallTournamentServerEvent(TournamentServerFunction.GetReward));
	}

	public void TournamentCheckReward()
	{
		TournamentHelper.Instance.CheckReward();
	}

	public void TournamentReport()
	{
		CitrusEventManager.instance.Raise(new CallTournamentServerEvent(TournamentServerFunction.Report, 50, 50, 10));
	}

	public void ChineseURLForTournament()
	{
		TournamentNetworkHelper.Instance._baseUrl = "http://inner.linux.citrusjoy.cn:49010/";
		Debug.Log("服务器修改成国内");
	}

	public void OutCountryURLForTournament()
	{
		TournamentNetworkHelper.Instance._baseUrl = ServerConfig.GameServerUrl;

	}

	public void GetMail()
	{
		MailHelper.Instance.GetAllMail();
	}
	public void AddMail()
	{
		MailHelper.Instance.TestAddMail();
	}

	public void AddTouranmentReport(string num)
	{
		FindObjectOfType<TournamentManager>().Report(Convert.ToUInt64(num));
	}
	public void TestDeepLink()
	{
		FacebookHelper.FetchDeepLink();
	}

    public void AddFirstEnterGameDays(string days)
    {
        LogUtility.Log("previous firstEnterGame date " + UserDeviceLocalData.Instance.FirstEnterGameTime);
        UserDeviceLocalData.Instance.FirstEnterGameTime = UserDeviceLocalData.Instance.FirstEnterGameTime.AddDays(-Convert.ToInt32(days));
        LogUtility.Log("cur firstEnterGame date " + UserDeviceLocalData.Instance.FirstEnterGameTime);
    }

    public void AddBackFlowDays(string days)
    {
        LogUtility.Log("previous lastlogin date " + UserBasicData.Instance.LastLoginDateTime);
        UserBasicData.Instance.SetLastLoginDateTime(UserBasicData.Instance.LastLoginDateTime.AddDays(-Convert.ToInt32(days)));
        LogUtility.Log("cur lastlogin date " + UserBasicData.Instance.LastLoginDateTime);
    }
		
	//付费保护措施开关
	public void SetPayProtection(string arg){
		if (arg.IsNullOrEmpty ())
			return;
		
		bool isPayProtectionEnable = arg.Equals ("on");
		UserBasicData.Instance.PayProtectionEnable = isPayProtectionEnable;
	}
		
    public void SendCrashMsgToFabric()
    {
        FabricManager.Instance.SendCrashAndNonFatalMsg();
    }
		
	public void SimulatePauseToReloadGame(string arg)
	{
		int intArg = 0;
		int.TryParse(arg, out intArg);
		bool r = intArg != 0;
		ReloadGameManager.Instance.TestApplicationPause(r);
	}

    public void CleanMaxWinActivityRecord()
    {
        UserBasicData.Instance.LastGetMaxWinActivityRewardDate = DateTime.MinValue;
        UserBasicData.Instance.MaxWinDuringActivity = 0;
    }

	public void SetMachineSeed(string arg)
	{
		string[] args = arg.Split(new char[]{ ',' });
		if(args.Length == 2)
		{
			string machineName = args[0];
			uint seed = 0;
			if(uint.TryParse(args[1], out seed))
			{
				UserMachineData.Instance.SaveMachineSeed(machineName, seed);

				// change the runtime roller
				if(GameScene.Instance != null)
				{
					LCG lcg = (LCG)GameScene.Instance.PuzzleMachine.CoreMachine.Roller;
					lcg.LastValue = seed;
				}
			}
			else
			{
				Debug.LogError("index can not be parsed");
			}
		}
		else
		{
			Debug.LogError("arg parameter wrong, should be something like: \'M1,3\'");
		}	
	}

	public void SetReelFreeSpinTIme(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleConfig.SetFreespinTimesGoldFinger(str);
		}
	}

	public void SetReelSpinTIme(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleConfig.SetSpinTimesGoldFinger(str);
		}
	}

	public void SetSpinSpeed(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetSpinSpeed(str);
			CitrusEventManager.instance.Raise(new PuzzleConfigUpdateEvent());
		}
	}

	public void SetStartSpinOffsetFactor(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetStartSpinOffsetFactor(str);
		}
	}
	public void SetStartSpinTime(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetStartSpinTime(str);
		}
	}
	public void SetSpinNeighorTime(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetSpinNeighorTime(str);
		}
	}
	public void SetSlideReelInterval(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetSlideReelInterval(str);
		}
	}
	public void SetEndSpinOffsetFactors(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetEndSpinOffsetFactors(str);
		}
	}
	public void SetEndSpinTimes(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetEndSpinTImes(str);
		}
	}
	public void SetFasterSpeedFactors(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetFasterSpeedFactors(str);
		}
	}
	public void SetSlowerSpeedFactors(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			GameScene.Instance._puzzleMachine.PuzzleReelSpinConfig.SetSlowerSpeedFactors(str);
		}
	}
	public void EnablePuzzleConfigGoldFinger(string str){
		if (GameScene.Instance != null && GameScene.Instance._puzzleMachine != null){
			int value = int.Parse(str);
			GameScene.Instance._puzzleMachine.PuzzleConfig.EnableGoldFinger(value);
		}
	}

#if DEBUG
    public static int DefaultAttributionId = -1;
    public static int AttributionId = DefaultAttributionId;
    public void AdjustAttributionTest(string arg)
    {
        int intArg = 0;
        int.TryParse(arg, out intArg);
        AttributionId = intArg;
        LogUtility.Log(string.Format("Set user source as {0}", UserSourceConfig.Instance.GetSourceStr(intArg)));
    }
#endif

}
