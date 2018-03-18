using UnityEngine;
using System;
using CitrusFramework;

public class PlayerDiceData
{
	public PlayerDiceData(DiceData diceData, ulong winCredits, float ratio)
	{
		DiceData = diceData;
		WinCredits = winCredits;
		Ratio = ratio;
	}
	
	public DiceData DiceData;
	public ulong WinCredits;
	public float Ratio;
}

public class DiceManager : Singleton<DiceManager>
{
    private readonly int _payForMoreDicesId = 2;
	//when dice game start, after delay time, change all dices's rotation to what we want
	private readonly float _changeRotDelayTime = 1f;

	public int ResultRatio{ get; set;}

	public PlayerDiceData PlayerDiceData { get; set;}
	public bool HavePaidForPlaying;
    private IAPData _iapData;

	void Start()
	{
		CitrusEventManager.instance.AddListener<SpinEndEvent>(NotifyForSpinEnd);
        CitrusEventManager.instance.AddListener<DiceGameEndEvent>(OnGameOver);
    }

	void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<SpinEndEvent>(NotifyForSpinEnd);
        CitrusEventManager.instance.RemoveListener<DiceGameEndEvent>(OnGameOver);
    }

    public void LoadDiceUi()
    {
        UIManager.Instance.LoadPopupAtPath(UIManager.DiceUiPath);
    }

    void NotifyForSpinEnd(SpinEndEvent e)
	{
		if (CanPlay(e.WinAmount, e.WinRatio))
		{
			LoadDiceGame();	
		}
	}
		
	public bool CanPlay(ulong winCredits, float ratio)
	{
		bool result = false;

		DiceData diceData = DiceConfig.Instance.GetDiceDataByOptions(winCredits, ratio);
        bool correctInput = diceData != null;
        bool inCooldownTime = DiceHelper.Instance.UserInNoDisturbState();
        bool inGroupList = GroupConfig.Instance.IsProductExist(StoreType.CrazyDice);

        if (correctInput && !inCooldownTime && inGroupList)
        {
            PlayerDiceData = new PlayerDiceData(diceData, winCredits, ratio);
            result = true;
        }

        LogUtility.Log(string.Format("diceModule: is correctInput : {0}    wincredits: {1}    ratio: {2}    inGroupList: {3}", correctInput, winCredits, ratio, inGroupList), Color.yellow);
		return result;
	}

#if DEBUG
    public void LoadDiceGameForTestPurpose(ulong winCredits, float ratio)
    {
        DiceData diceData = DiceConfig.Instance.GetDiceDataByOptions(winCredits, ratio);
        if (diceData != null)
            PlayerDiceData = new PlayerDiceData(diceData, winCredits, ratio);
        LoadDiceGame();
    }
#endif

    void LoadDiceGame()
	{
		UIManager.Instance.OpenDiceUi();
        //AudioManager.Instance.PlaySoundBGM(new AudioType[]{AudioType.M10_WheelBGM});
        AudioManager.Instance.PlaySound(AudioType.M10_WheelBGM, true);
		SetAnalysisData();
	}

	void SetAnalysisData()
	{
		StoreController.Instance.CurrStoreAnalysisData = new StoreAnalysisData();
		StoreController.Instance.CurrStoreAnalysisData.OpenPosition = OpenPos.EpicWin.ToString();
		StoreController.Instance.CurrStoreAnalysisData.StoreEntrance = StoreType.CrazyDice.ToString();

        AnalysisManager.Instance.OpenShop();
    }

    void StartGame()
	{
        HavePaidForPlaying = false;

        if (PlayerDiceData != null)
	    {
            ResultRatio = DiceHelper.Instance.PlayDiceResult(PlayerDiceData.DiceData);
            CitrusEventManager.instance.Raise(new DiceGameStartEvent(ResultRatio, PlayerDiceData.DiceData.DiceNum, _changeRotDelayTime));
        }
        else
        {
            //Notice: playerDiceData will be null if this is a revalid purchase, so we need to read dice data from userLocalDeviceData
            GiveReward(UserDeviceLocalData.Instance.DiceRatio * (int)UserDeviceLocalData.Instance.DiceInitCredits, UserDeviceLocalData.Instance.DiceIapId);
        }
   }

    void OnGameOver(DiceGameEndEvent e)
    {
        GiveReward((int)PlayerDiceData.WinCredits * ResultRatio, PlayerDiceData.DiceData.IAPId);
        SendAnalysisData();
    }

    void GiveReward(long rewardCredits, int iapId)
    {
        StoreManager.Instance.AddCreditsAndLuckyByItemId(iapId.ToString(), rewardCredits);
        PropertyTrackManager.Instance.OnPayGameRewardUser(_iapData.TransactionId, (ulong)rewardCredits);
    }

    void SendAnalysisData()
    {
        float price = IAPCatalogConfig.Instance.FindIAPItemByID(PlayerDiceData.DiceData.IAPId.ToString()).Price;
        AnalysisManager.Instance.OnDiceGameOver(ResultRatio, (long)PlayerDiceData.WinCredits, price);
    }

    #region Public

    public DiceData GetOrigDiceData()
    {
        return DiceConfig.Instance.GetDiceDataByOptions(PlayerDiceData.WinCredits, PlayerDiceData.Ratio);
    }

    public DiceData GetPayForMoreDicesData()
    {
        return DiceConfig.Instance.GetDiceDataById(_payForMoreDicesId);
    }

    //in only one dice condition, user may want more dices var pay more money
    public void PlayerWantMoreDices()
    {
        DiceData newData = GetPayForMoreDicesData();
        Debug.Assert(newData != null, "DiceModule: Error, Can't find DiceTypeID : " + _payForMoreDicesId + " In DiceData!");

        if (newData != null)
        {
            PlayerDiceData.DiceData = newData;
            CitrusEventManager.instance.Raise(new PayForMoreDicesEvent());
        }
    }

    public void ResetDiceCount()
    {
        DiceData diceData = DiceConfig.Instance.GetDiceDataByOptions(PlayerDiceData.WinCredits, PlayerDiceData.Ratio);
        Debug.Assert(diceData != null, "diceModule: diceData is null,  wincredits :" + PlayerDiceData.WinCredits + "    PlayerDiceData :" + PlayerDiceData.Ratio);
        if (diceData != null)
        {
            PlayerDiceData.DiceData = diceData;
            CitrusEventManager.instance.Raise(new ResetDiceCountEvent());
        }
    }

    public void SaveDiceDataToDevice()
    {
        UserDeviceLocalData.Instance.DiceInitCredits = PlayerDiceData.WinCredits;
        UserDeviceLocalData.Instance.DiceIapId = PlayerDiceData.DiceData.IAPId;
        UserDeviceLocalData.Instance.DiceRatio = DiceHelper.Instance.PlayDiceResult(PlayerDiceData.DiceData);
        UserDeviceLocalData.Instance.Save();
    }

    public void PayForPlayingGame()
	{
		string productId = DiceConfig.Instance.GetIAPIdByDifferentUser(PlayerDiceData.DiceData).ToString();
		StoreManager.Instance.InitiatePurchase(productId);
		AudioManager.Instance.PlaySound(AudioType.Click);
	}

	public void OnPaymentValid(IAPData data)
	{
	    _iapData = data;
		UserDeviceLocalData.Instance.LastPayForDiceDate = DateTime.Now;
		
		StartGame();
	}

	#endregion
}
