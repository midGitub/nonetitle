using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.Events;
using GoogleMobileAds.Api;

public enum RewardAdType
{
    None,
    Lobby,
    LobbyBuy,
    GameUpBuy,
    GameBelowBuy,
    DoubleWinning,
    MaxBet,
	LobbyButton,
}

public enum ADSPos
{
	Entermachine,
	HomeIn
}

public class ADSManager : Singleton<ADSManager>
{

	public UnityAction ADFinishedGetBonus;
	public UnityAction InterstitialFinished;
    public UnityAction InterstitialOpened;
    public UnityAction RewardBasedVideoOpen;
    public UnityAction RewardBasedVideoClosed;

    private SimpleMessageQueue _messageQueue = new SimpleMessageQueue();
    /// <summary>
    ///  当前的奖励视频
    /// </summary>
    private RewardBasedVideoAd rewardBasedVideo;

	/// <summary>
	/// 当前插屏广告
	/// </summary>
	private InterstitialAd interstitial;

	private enum ADState {
		HasntWatched = 0,
		AlreadyWatched = 1,
	};
	private ADState _adState = ADState.HasntWatched;
    private RewardAdType _rewardAdType;

    // 从开始
	private bool _startPrepareWatch = false;
	public bool StartPrepareWatch{
		get { return _startPrepareWatch; }
        set { _startPrepareWatch = value; }
	}

    //cache广告播放时背景音乐是否开启,仅iOS版本用到
    private bool _isMusicOnWhenShowAD;

    private void RequestRewardBasedVideo()
	{
#if UNITY_EDITOR
		string adUnitId = "unused";
#elif UNITY_ANDROID
		string adUnitId = "ca-app-pub-1221767908077728/1531791493";
#elif UNITY_IPHONE || UNITY_IOS
        string adUnitId = PackageConfigManager.Instance.CurPackageConfig.AdmobRewardId;
#else
		string adUnitId = "unexpected_platform";
#endif
        AdRequest request = new AdRequest.Builder().Build();
		rewardBasedVideo.LoadAd(request, adUnitId);
	}

	private void RequestInterstitial()
	{
#if UNITY_EDITOR
		string adUnitId = "unused";
#elif UNITY_ANDROID
		string adUnitId = "ca-app-pub-1221767908077728/4066455492";
#elif UNITY_IPHONE || UNITY_IOS
        string adUnitId = PackageConfigManager.Instance.CurPackageConfig.AdmobInterstitialId;
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(adUnitId);


		interstitial.OnAdFailedToLoad += HandleInterstitialVideoFailedToLoad;
		interstitial.OnAdOpening += HandleInterstitialVideoOpened;
		interstitial.OnAdClosed += HandleInterstitialVideoClosed;
		interstitial.OnAdLeavingApplication += HandleInterstitialVideoLeftApplication;
		interstitial.OnAdLoaded += HandleInterstitialVideoLoaded;

		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		//#endif
//		Debug.Log("请求插屏广告");
		// Load the interstitial with the request.
		interstitial.LoadAd(request);
	}

	public bool HaveVideoAD()
	{
#if UNITY_EDITOR
		return false;
#endif
		if(rewardBasedVideo == null)
		{
			rewardBasedVideo = RewardBasedVideoAd.Instance;
			return false;
		}
		bool bLoad = rewardBasedVideo.IsLoaded();
		//if(bLoad == false)
		//{
		//	RequestRewardBasedVideo();
		//}
		return bLoad;
	}

	public bool HavePictureAD()
	{
		#if UNITY_EDITOR
		return false;
		#else
		//if(interstitial == null)
		//{
		//	RequestInterstitial();
		//}
		if(interstitial == null)
		{
			return false;
		}
		bool bLoad = interstitial.IsLoaded();
		return bLoad;
		#endif
	}

	private void Start()
	{
#if !UNITY_EDITOR
		AppLovin.InitializeSdk();
		//AppLovin.SetUnityAdListener(this.gameObject.name);
#endif
		Vungle.init("5940fa9ccdd8346b18001fe4", PackageConfigManager.Instance.CurPackageConfig.VungleiOSAppId);
		rewardBasedVideo = RewardBasedVideoAd.Instance;
		RequestRewardBasedVideo();
		RequestInterstitial();
		rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
		rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
		rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
		rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
		rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
		rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
		rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;
		//StartCoroutine(ADSRequesterRVideo());
	}

    public void Update()
    {
        _messageQueue.Run();
    }

    public void Init()
	{
	}

	void HandleRewardBasedVideoLoaded(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Video successfully loaded");
	}

	void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		GameDebug.Log("Admob: Video fails to load" + args.Message);
		UnityTimer.Start(NetworkTimeHelper.Instance, 20f, () => {
			RequestRewardBasedVideo();
		});
	}

	void HandleRewardBasedVideoOpened(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Video successfully opened");
		_adState = ADState.HasntWatched;
        MuteBgMusic();

        if (RewardBasedVideoOpen != null)
            RewardBasedVideoOpen();
	}

	void HandleRewardBasedVideoStarted(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Video successfully started");
	}

	void HandleRewardBasedVideoRewarded(object sender, Reward args)
	{
		GameDebug.Log("Admob: Video successfully rewarded");
		_adState = ADState.AlreadyWatched;
	}

	void HandleRewardBasedVideoClosed(object sender, System.EventArgs args)
	{
        //why doing this is because this methed is called by googleAd thread, u can't use unity api by non_unity thread
        _messageQueue.Add(new SimpleMessage(() => HandleRewardVideoClosed(sender, args)));
	}

	void HandleRewardBasedVideoLeftApplication(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Video successfully left");
	}

	void HandleInterstitialVideoLoaded(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Interstitial successfully loaded");
	}

	void HandleInterstitialVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		GameDebug.Log("Admob: Interstitial fails to load" + args.Message);
		UnityTimer.Start(NetworkTimeHelper.Instance, 20f, () =>
			{
				RequestInterstitial();
			});
	}

	void HandleInterstitialVideoOpened(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Interstitial successfully opened");
		_adState = ADState.HasntWatched;
        if (InterstitialOpened != null)
        {
            InterstitialOpened();
        }
	}

	void HandleInterstitialVideoClosed(object sender, System.EventArgs args)
	{
		_adState = ADState.AlreadyWatched;
		//it is likely the user receive close event first and then reward event, in this case, there will be two events sent
		GameDebug.Log("Admob: Interstitial successfully closed");
		RequestInterstitial();
		AnalysisManager.Instance.WatchAd((int)_adState);
		if(InterstitialFinished != null)
		{
			InterstitialFinished();
		}
        _startPrepareWatch = false;
	}

	void HandleInterstitialVideoLeftApplication(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Interstitial successfully left");
	}

    void HandleRewardVideoClosed(object sender, System.EventArgs args)
    {
        //it is likely the user receive close event first and then reward event, in this case, there will be two events sent
        GameDebug.Log("Admob: Video successfully closed");
        AnalysisManager.Instance.WatchBonusAd((int)_adState, MapScene.CurrentMachineName, _rewardAdType);
        RestoreBgMusic();
        if (_adState.Equals(ADState.AlreadyWatched))
        {
            UnityTimer.Start(this, 0.2f, () =>
            {
                if (ADFinishedGetBonus != null)
                {
                    ADFinishedGetBonus();
                }
            });
        }
        _startPrepareWatch = false;
        RequestRewardBasedVideo();

        if (RewardBasedVideoClosed != null)
            RewardBasedVideoClosed();
    }

    // 播放广告时对应的开启和关闭游戏机台背景音乐接口

    private void StopMachineBGM(){
		if (ScenesController.Instance.GetCurrSceneName () == ScenesController.GameSceneName) {
			AudioType[] audios = GetMachineBGMArray ();
			AudioManager.Instance.StopSoundBGM (audios);
		}
	}

	private void ResumeMachineBGM(){
		if (ScenesController.Instance.GetCurrSceneName () == ScenesController.GameSceneName) {
			AudioType[] audios = GetMachineBGMArray ();
			AudioManager.Instance.PlaySoundBGM (audios);
		}
	}

	private AudioType[] GetMachineBGMArray(){
		GameScene gameScene = GameScene.Instance;
		if (gameScene != null && gameScene.PuzzleMachine != null) {
			return gameScene.PuzzleMachine.MachineConfig.BasicConfig.MachineBGM;
		}

		return new AudioType[0];
	}

    private void MuteBgMusic()
    {
#if UNITY_IPHONE || UNITY_IOS
        _isMusicOnWhenShowAD = AudioManager.Instance.IsMusicOn;
        AudioManager.Instance.IsMusicOn = false;
#endif
    }

    private void RestoreBgMusic()
    {
#if UNITY_IPHONE || UNITY_IOS
        AudioManager.Instance.IsMusicOn = _isMusicOnWhenShowAD;
#endif
    }

    //void onAppLovinEventReceived(string ev)
    //{
    //	if(ev.Contains("DISPLAYEDINTER"))
    //	{
    //		// An ad was shown.  Pause the game.
    //		GameDebug.Log("Interstitial ad is displayed");
    //	}
    //	else if(ev.Contains("HIDDENINTER"))
    //	{
    //		// Ad ad was closed.  Resume the game.
    //		// If you're using PreloadInterstitial/HasPreloadedInterstitial, make a preload call here.
    //		GameDebug.Log("Interstitial ad is hidden");
    //		AppLovin.PreloadInterstitial();
    //	}
    //	else if(ev.Contains("LOADEDINTER"))
    //	{
    //		// An interstitial ad was successfully loaded.
    //		GameDebug.Log("Interstitial ad is loaded");
    //	}
    //	else if(string.Equals(ev, "LOADINTERFAILED"))
    //	{
    //		// An interstitial ad failed to load.
    //		GameDebug.Log("Interstitial ad fails to load");
    //	}
    //	else if(string.Equals(ev, "CLICKED"))
    //	{
    //		// An interstitial ad failed to load.
    //		GameDebug.Log("Ad is clicked");
    //	}
    //	else if(ev.Contains("REWARDAPPROVEDINFO"))
    //	{
    //		;

    //	}
    //	else if(ev.Contains("LOADEDREWARDED"))
    //	{
    //		// A rewarded video was successfully loaded.
    //		GameDebug.Log("Rewarded video is loaded");
    //	}
    //	else if(ev.Contains("LOADREWARDEDFAILED"))
    //	{
    //		// A rewarded video failed to load.
    //		GameDebug.Log("Rewared video fails to load");
    //	}
    //	else if(ev.Contains("HIDDENREWARDED"))
    //	{
    //		// A rewarded video has been closed.  Preload the next rewarded video.
    //		GameDebug.Log("Rewarded video is hidden");
    //		AppLovin.LoadRewardedInterstitial();
    //	}
    //}

    public void SetAnalysisData(RewardAdType type)
    {
        _rewardAdType = type;
    }

    public void ShowVideoADS()
	{
		if(rewardBasedVideo.IsLoaded())
		{
			Debug.Log("展示rewardBasedVideo广告");
			rewardBasedVideo.Show();
			_startPrepareWatch = true;
		}
		else
		{
			Debug.Log("There is no rewarded video available");
			RequestRewardBasedVideo();
		}
	}

	public void ShowPictureADS()
	{
		if(interstitial.IsLoaded())
		{
			Debug.Log("展示interstitial广告");
			interstitial.Show();
            _startPrepareWatch = true;
		}
		else
		{
			Debug.Log("There is no interstitial video available");
			RequestInterstitial();
		}
	}
}
