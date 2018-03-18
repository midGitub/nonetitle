using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using Facebook.Unity;

public class GameManager : Singleton<GameManager>
{
	public static string SecurityToken = "eda35bf1d58c3a6202eab39f7af9c3b5";
	private static bool _isInit = false;

	[RuntimeInitializeOnLoadMethod]
	void PreInit()
	{
		#if DEBUG
		Debug.logger.logEnabled = true;
		#else
		Debug.logger.logEnabled = false;
		#endif
	}

	public void Init()
	{
		StartCoroutine(InitCoroutine());
	}

	public IEnumerator InitCoroutine()
	{
		if(!_isInit)
		{
			_isInit = true;
			yield return StartCoroutine(DoInitCoroutine());
		}
	}

	private IEnumerator DoInitCoroutine()
	{
		Debug.Log("GameManager: Start init");

		//Init Facebook first and wait until it's initialized
		FacebookHelper.InitSDK();

		LoadSingle.Load();

		PopWindowConfig.Instance.Init();

		MachineAssetManager.Instance.Init();

		AnalysisManager.Instance.Init();
		CmdLineManager.Instance.Init();
		NotificationUtility.Instance.Init();
		var ads = ShowADSController.Instance;
		HomeInADS.Instance.Init();
		PuzzleJackpotManager.Instance.Init();
		UIManager.Instance.Init();

		NetworkTimeHelper.Instance.GetNowTime();
		ScoreAppSystem.Instance.Init();
		GetCredits.Instance.Init();
		FacebookLikes.Instance.Init();
	    PayRotaryTableSystem.Instance.Init();

		//todo: 30 or 60?
		#if UNITY_IOS
		SystemUtility.Set60FPS();
		#else
		SystemUtility.Set30FPS();
        #endif	 

        PackageConfigManager.Instance.CurPackageConfig.InitFireBase();
        FabricManager.Instance.Init();
		BonusManager.Instance.Init();
		MailHelper.Instance.Init();
	    MapMachinePosManager.Instance.Init();
        RegisterMaxWinActivity.Instance.Init();
		ReloadGameManager.Instance.Init();
        PropertyTrackManager.Instance.Init();
        PayUserMonitor.Instance.Init();
        AnnunciationServerManager.Instance.Init();
        AnnunciationVipLvUpManager.Instance.Init();
		LongLuckyPeriodManager.Instance.Init();
        AdjustManager.Instance.Init();

        AnalysisManager.Instance.StartSendProfilerRuntime();

		InitDontDestroyManager (_loadingPath);

		while(!FacebookHelper.IsInitialized)
			yield return new WaitForEndOfFrame();

		Debug.Log("GameManager: Facebook initialized complete");

		VersionProcessing();
		VersionHelper.WriteVersion ();
	}

	private static void VersionProcessing()
	{
		VersionInfo versionInfo = VersionUtility.GetVersionInfo(BuildUtility.GetBundleVersion());
		string nowV = versionInfo._major + "." + versionInfo._minor;
		if(nowV == UserDeviceLocalData.Instance.VersionName)
		{
			LogUtility.Log("版本相同", Color.green);
		}
		else
		{
			if(UserDeviceLocalData.Instance.VersionName != "")
			{
				// 如果原来的版本不是空的不是重新装的,而是升级的就创建一个升级邮件
				CreatMailSelf.Instance.CreatUpdateMail();
			}

			LogUtility.Log("版本不相同", Color.red);

			CheckProcessUserDataLossBug();

			UserDeviceLocalData.Instance.VersionName = nowV;
		}

		if(UserBasicData.Instance.UDID == "")
			LogUtility.Log("UDID is empty", Color.red);
	}

	//There is a critical bug of user data loss in 1.12.0
	static void CheckProcessUserDataLossBug()
	{
		//1 check
		bool result = false;
		string lastVersion = UserDeviceLocalData.Instance.VersionName;
		if(!string.IsNullOrEmpty(lastVersion))
		{
			string[] array = lastVersion.Split('.');
			if(array.Length >= 2)
			{
				int major = 0, minor = 0;
				int.TryParse(array[0], out major);
				int.TryParse(array[1], out minor);

				result = ((major == 1) && (minor == 12));
			}
		}

		//2 perform
		if(result)
			UserDeviceLocalData.Instance.ShouldHandleUserDataLoss = true;
	}

	#region init dontdestroy ui manager

	private static readonly string _loadingPath = "Map/Loading/Loading";

	private void InitDontDestroyManager(string path){
		GameObject prefab = AssetManager.Instance.LoadAsset<GameObject> (path);
		if (prefab != null) {
			GameObject obj = UGUIUtility.CreateObj (prefab, null);
			if (obj != null) {
				DontDestroyOnLoad (obj);
			}
		}
	}

	#endregion
}
