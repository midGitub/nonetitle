using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UserDeviceLocalData
{
	static private string _currSocialAPPIDTag = "CurrSocialAPPID";
	static private string _loginWorkflowStateTag = "LoginWorkflowState";
	static private string _shouldHandleUserDataLossTag = "ShouldHandleUserDataLoss";
	static private string _shouldHandleDeviceIdChangeTag = "ShouldHandleDeviceIdChange";

	static private string _mVerityIAPsName = "VerityIAPDic";
	static private string _totalPayAmountTag = "TotalPayAmount";
	static private string _firstEnterGameTimeTag = "firstEnterGameTime";
	static private string _firstBindFacebookTimeTag = "firstBindFacebookTime";
	static private string _lastLoginTimeTag = "lastLoginTime";
	static private string _doubleLevelUpTimeTag = "doubleLevelUpTime";

	static private string _soundOnTag = "SoundOn";
	static private string _musicOnTag = "MusicOn";
	static private string _selectMPositionTag = "SelectMPosition";
	static private string _isNewGameTag = "ISNewGame";
	static private string _isFirstEnterGameTag = "IsFirstEnterGame";

	static private string _rewardADSVedioPlayTimeTag = "RewardADSVedioPlayTime";
	static private string _lastGetGetRewardADSVedioTimeTag = "LastGetGetRewardADSVedioTime";
	static private string _lastShowPlagueTimeTag = "LastShowPlagueTimeTag";
	static private string _lastShowNoADSStoreTimeTag = "LastShowNoADSStoreTime";
	static private string _lastShowThreeStoreTimeTag = "LastShowThreeStoreTime";
	static private string _firstplayMacthineDicTag = "PlayMacthineDic";
	static private string _tournamentScoreInforTag = "TournamentScoreInfor";
	static private string _friendTag = "Friends";
	static private string _friendCollectGiftNumTag = "FriendCollectGiftNum";
	static private string _friendSendGiftNumTag = "FriendSendGiftNum";
	static private string _friendCollectCreditsDailySumTag = "FriendCollectCreditsDailySum";
	static private string _friendSendCreditsDailySumTag = "FriendSendCreditsDailySum";
    static private string _lastRefuseMachineCommentTimeTag = "LastRefuseMachineCommentTime";
    static private string _lastMachineCommentTimeTag = "LastMachineCommentTime";
    static private string _machineCommentTimesTodayTag = "MachineCommentTimesToday";
    static private string _lastPopFaceBookLoginTag = "LastPopFaceBookLogin";
    static private string _lastBigWinDayTag = "LastBigWinDay";
	static private string _lastPayForDiceDateTag = "LastPayForDiceDateTag";
	static private string _lastCloseDicePageDateTag = "LastCloseDicePageDateTag";
	static private string _closeDicePageCountTag = "CloseDicePageCountTag";
    static private string _isInTinyMachineRoomTag = "InTinyMachineRoomTag";
    static private string _lastOpenChristMasMaxWinUiDateTag = "LastOpenChristMasMaxWinUiDate";
    static private string _lastOpenDoubleHourBonusUiDateTag = "LastOpenDoubleHourBonusUiDate";
    static private string _diceRatioTag = "DiceRatio";
    static private string _diceInitCreditsTag = "DiceInitCredits";
    static private string _diceIapIdTag = "DiceIapId";
    static private string _lastMachineAdEndDateTag = "LastMachineAdEndDateTag";
    static private string _lastMachineAdIdTag = "LastMachineAdIdTag";

    static private string _versionTag = "VersionName";
	static private string _notifyIDTag = "NotifyID";
    static private string _lastStayMapRoomTag = "LastStayMapRoomTag";
    static private string _mapRoomLastStayPosTag = "MapRoomPosInfoTag";

	private string _currSocialAppID = "";
	private UserLoginWorkflowState _loginWorkflowState = UserLoginWorkflowState.OK;
	private bool _shouldHandleUserDataLoss = false;
	private bool _shouldHandleDeviceIdChange = false;

	public Dictionary<string, IAPData> VerityIAPDic = new Dictionary<string, IAPData>();
	public Dictionary<string, string> FirstPlayMacthineDic = new Dictionary<string, string>();
    public Dictionary<MapMachineRoom, float> MapRoomLatStayPosDic = new Dictionary<MapMachineRoom, float>();
	private DateTime _firstEnterGameTime = System.DateTime.MinValue;
	private DateTime _firstBindFacebookTime = System.DateTime.MinValue;
	private DateTime _lastLoginTime = System.DateTime.MinValue;
	private DateTime _doubleLevelUpTime = System.DateTime.Now;
	private DateTime _lastGetGetRewardADSVedioTime = System.DateTime.Now.AddDays(-3);
	private DateTime _lastShowPlagueTime = DateTime.Now.AddDays(-3);
	private DateTime _lastShowNoADSStoreTime = DateTime.Now.AddDays(-4);
	private DateTime _lastShowThreeStoreTime = DateTime.Now.AddDays(-3);
	private int _rewardADSVedioPlayTime;

	private bool _isSoundOn;
	private bool _isMusicOn;

	private bool _isNewGame;
	private bool _isFirstEnterGame;
	private string _versionName;
    private bool _isInTinyMachineRoom;
    private DateTime _lastOpenChristMasMaxWinUiDate;
    private DateTime _lastOpenDoubleHourBonusUiDate;
    private int _diceRatio;
    private ulong _diceInitCredits;
    private int _diceIapId;
    private DateTime _lastMachineAdEndDate;
    private int _lastMachineAdId;
	private List<int> _notifyIDs = new List<int>();
    private MapMachineRoom _lastStayMapRoom;

    //Important note:
    //_totalPayAmount is useless. Instead, use UserBasicData._totalPayAmount
    //But for transfering _totalPayAmount to UserBasicData, this field should be still kept
    private float _totalPayAmount;
	public float TotalPayAmount_Deprecated { get { return _totalPayAmount; } }

	public int RewardADSVedioPlayTime { get { return _rewardADSVedioPlayTime; } set { _rewardADSVedioPlayTime = value; Save(); } }
	public DateTime LastGetGetRewardADSVedioTime { get { return _lastGetGetRewardADSVedioTime; } set { _lastGetGetRewardADSVedioTime = value; Save(); } }
	public DateTime LastShowPlagueTime { get { return _lastShowPlagueTime; } set { _lastShowPlagueTime = value; } }
	public DateTime LastShowNoADSStoreTime { get { return _lastShowNoADSStoreTime; } set { _lastShowNoADSStoreTime = value; Save(); } }
	public DateTime LastShowThreeStoreTime { get { return _lastShowThreeStoreTime; } set { _lastShowThreeStoreTime = value; Save(); } }
	public string VersionName { get { return _versionName; } set { _versionName = value; Save(); } }

	//Note: take care of the difference between _isNewGame and _isFirstEnterGame
	//This flag is true when entering the game for the first time,
	//and is set to false when entering MapScene
	public bool IsNewGame { get { return _isNewGame; } set { _isNewGame = value; } }
	//This flag is true when entering the game for the first time,
	//and is set to false immediately in StartLoading scene
	public bool IsFirstEnterGame { get { return _isFirstEnterGame; } set { _isFirstEnterGame = value; Save(); } }

	public Dictionary<TournamentMatchSection, TournamentScoreInfor> TournamentScoreInforDic = new Dictionary<TournamentMatchSection, TournamentScoreInfor>();
	private Dictionary<string, FriendData> _friendsDict = new Dictionary<string, FriendData>();
	private int _friendCollectGiftNum;
	private int _friendSendGiftNum;
	private int _friendCollectCreditsDailySum;
	private int _friendSendCreditsDailySum;

    private DateTime _lastRefuseMachineCommentTime;
    private DateTime _lastMachineCommentTime;
    private int _machineCommentTimesToday;
    private DateTime _lastPopFaceBookLogin;
    private DateTime _lastBigWinDay;
	private DateTime _lastPayForDiceDate;
	private DateTime _lastCloseDicePageDate;
	private int _closeDicePageCount;

	private bool _isFirstLoginToday; //don't need store to disk
	public bool IsFirstLoginToday { get { return _isFirstLoginToday; } }

	public string GetCurrSocialAppID
	{
		get { return _currSocialAppID; }
		set {
			if(value != _currSocialAppID)
			{
				_currSocialAppID = value;
				Save();
			}
		}
	}

	public UserLoginWorkflowState LoginWorkflowState
	{
		get { return _loginWorkflowState; }
		set {
			if(value != _loginWorkflowState)
			{
				_loginWorkflowState = value;
				Save();
			}
		}
	}

	public bool ShouldHandleUserDataLoss
	{
		get { return _shouldHandleUserDataLoss; }
		set {
			if(value != _shouldHandleUserDataLoss)
			{
				_shouldHandleUserDataLoss = value;
				Save();
			}
		}
	}

	public bool ShouldHandleDeviceIdChange
	{
		get { return _shouldHandleDeviceIdChange; }
		set {
			if(value != _shouldHandleDeviceIdChange)
			{
				_shouldHandleDeviceIdChange = value;
				Save();
			}
		}
	}

	public Dictionary<string, FriendData> FriendsDict{
		get { return _friendsDict; }
		set { 
			_friendsDict = value;
			Save ();
		}
	}

	public int FriendCollectGiftNum{
		get { return _friendCollectGiftNum; }
		set {
			_friendCollectGiftNum = value;
			Save ();
		}
	}

	public int FriendSendGiftNum{
		get { return _friendSendGiftNum; }
		set { 
			_friendSendGiftNum = value;
			Save ();
		}
	}

	public int FriendCollectCreditsDailySum{
		get { return _friendCollectCreditsDailySum; }
		set { 
			_friendCollectCreditsDailySum = value;
			Save ();
		}
	}

	public int FriendSendCreditsDailySum{
		get { return _friendSendCreditsDailySum; }
		set { 
			_friendSendCreditsDailySum = value;
			Save ();
		}
	}

    public DateTime LastRefuseMachineCommentTime{
        get { return _lastRefuseMachineCommentTime; }
        set { 
            _lastRefuseMachineCommentTime = value;
            Save ();
        }
    }

    public DateTime LastMachineCommentTime{
        get { return _lastMachineCommentTime; }
        set { 
            _lastMachineCommentTime = value;
            Save ();
        }
    }

    public int MachineCommentTimesToday{
        get { return _machineCommentTimesToday; }
        set { 
            _machineCommentTimesToday = value;
            Save ();
        }
    }

    public DateTime LastPopFaceBookLogin {
        get { return _lastPopFaceBookLogin; }
        set { 
            _lastPopFaceBookLogin = value;
            Save ();
        }
    }

    public DateTime LastBigWinDay
    {
        get { return _lastBigWinDay; }
        set
        {
            _lastBigWinDay = value;
            Save();
        }
    }

	public DateTime LastPayForDiceDate
	{
		get { return _lastPayForDiceDate; }
		set
		{
			_lastPayForDiceDate = value;
			Save();
		}
	}

	public DateTime LastCloseDicePageDate
	{
		get { return _lastCloseDicePageDate; }
		set
		{
			_lastCloseDicePageDate = value;
			Save();
		}
	}

	public int CloseDicePageCount
	{
		get { return _closeDicePageCount; }
		set
		{
			_closeDicePageCount = value;
			Save();
		}
	}

	public List<int> NotifyIDs{
		get { return _notifyIDs; }
	}

    public MapMachineRoom LastStayMapRoom
    {
        get { return _lastStayMapRoom;}
        set { _lastStayMapRoom = value;}
    }

    public override void Read(ES2Reader reader)
	{
		ReadCore(reader);

		if(IsTagExist(_mVerityIAPsName))
		{
			//Debug.Log("IAP读到字典");
			VerityIAPDic = reader.ReadDictionary<string, IAPData>(_mVerityIAPsName);
		}
		if(IsTagExist(_firstplayMacthineDicTag))
		{
			FirstPlayMacthineDic = reader.ReadDictionary<string, string>(_firstplayMacthineDicTag);
		}
	    MapRoomLatStayPosDic = IsTagExist(_mapRoomLastStayPosTag) ? reader.ReadDictionary<MapMachineRoom, float>(_mapRoomLastStayPosTag) : new Dictionary<MapMachineRoom, float>();
		if(IsTagExist(_tournamentScoreInforTag))
		{
			TournamentScoreInforDic = reader.ReadDictionary<TournamentMatchSection, TournamentScoreInfor>(_tournamentScoreInforTag);
		}

		if(IsTagExist(_totalPayAmountTag))
			_totalPayAmount = reader.Read<float>(_totalPayAmountTag);
		else
			_totalPayAmount = 0.0f;

		_firstEnterGameTime = IsTagExist(_firstEnterGameTimeTag) ?
			reader.Read<System.DateTime>(_firstEnterGameTimeTag) : System.DateTime.MinValue;
		_firstBindFacebookTime = IsTagExist(_firstBindFacebookTimeTag) ?
			reader.Read<System.DateTime>(_firstBindFacebookTimeTag) : System.DateTime.MinValue;
		_lastLoginTime = IsTagExist(_lastLoginTimeTag) ?
			reader.Read<System.DateTime>(_lastLoginTimeTag) : System.DateTime.MinValue;

		_doubleLevelUpTime = IsTagExist(_doubleLevelUpTimeTag) ?
			reader.Read<System.DateTime>(_doubleLevelUpTimeTag) : System.DateTime.Now;

		_isSoundOn = IsTagExist(_soundOnTag) ? reader.Read<bool>(_soundOnTag) : true;
		_isMusicOn = IsTagExist(_musicOnTag) ? reader.Read<bool>(_musicOnTag) : true;
		_currSocialAppID = IsTagExist(_currSocialAPPIDTag) ? reader.Read<string>(_currSocialAPPIDTag) : "";
		_rewardADSVedioPlayTime = IsTagExist(_rewardADSVedioPlayTimeTag) ? reader.Read<int>(_rewardADSVedioPlayTimeTag) : 0;

		_lastGetGetRewardADSVedioTime = IsTagExist(_lastGetGetRewardADSVedioTimeTag) ? reader.Read<DateTime>(_lastGetGetRewardADSVedioTimeTag) : DateTime.Now.AddDays(-3);
		_lastShowPlagueTime = IsTagExist(_lastShowPlagueTimeTag) ? reader.Read<DateTime>(_lastShowPlagueTimeTag) : DateTime.Now.AddDays(-3);
		_lastShowNoADSStoreTime = IsTagExist(_lastShowNoADSStoreTimeTag) ? reader.Read<DateTime>(_lastShowNoADSStoreTimeTag) : DateTime.Now.AddDays(-4);
		_lastShowThreeStoreTime = IsTagExist(_lastShowThreeStoreTimeTag) ? reader.Read<DateTime>(_lastShowThreeStoreTimeTag) : DateTime.Now.AddDays(-3);
		_versionName = IsTagExist(_versionTag) ? reader.Read<string>(_versionTag) : "";// 默认
		_isNewGame = IsTagExist(_isNewGameTag) ? reader.Read<bool>(_isNewGameTag) : true;
		_isFirstEnterGame = IsTagExist(_isFirstEnterGameTag) ? reader.Read<bool>(_isFirstEnterGameTag) : true;

		ReadFriendDictionary (reader);
		_friendCollectGiftNum = IsTagExist (_friendCollectGiftNumTag) ? reader.Read<int> (_friendCollectGiftNumTag) : 0;
		_friendSendGiftNum = IsTagExist (_friendSendGiftNumTag) ? reader.Read<int> (_friendSendGiftNumTag) : 0;
		_friendCollectCreditsDailySum = IsTagExist (_friendCollectCreditsDailySumTag) ? reader.Read<int> (_friendCollectCreditsDailySumTag) : 0;
		_friendSendCreditsDailySum = IsTagExist (_friendSendCreditsDailySumTag) ? reader.Read<int> (_friendSendCreditsDailySumTag) : 0;
        _lastRefuseMachineCommentTime = IsTagExist(_lastRefuseMachineCommentTimeTag) ? reader.Read<DateTime>(_lastRefuseMachineCommentTimeTag) : DateTime.Now.AddDays(-3);
        _lastMachineCommentTime = IsTagExist(_lastMachineCommentTimeTag) ? reader.Read<DateTime>(_lastMachineCommentTimeTag) : DateTime.Now.AddDays(-3);
        _machineCommentTimesToday = IsTagExist (_machineCommentTimesTodayTag) ? reader.Read<int> (_machineCommentTimesTodayTag) : 0;
        _lastPopFaceBookLogin = IsTagExist(_lastPopFaceBookLoginTag) ? reader.Read<DateTime>(_lastPopFaceBookLoginTag) : DateTime.Now.AddDays(-3);
        _lastBigWinDay = IsTagExist(_lastBigWinDayTag) ? reader.Read<DateTime>(_lastBigWinDayTag) : DateTime.Now.AddDays(-3);
		_lastPayForDiceDate = IsTagExist(_lastPayForDiceDateTag) ? reader.Read<DateTime>(_lastPayForDiceDateTag) : DateTime.Now.AddDays(-2);
		_lastCloseDicePageDate = IsTagExist(_lastCloseDicePageDateTag) ? reader.Read<DateTime>(_lastCloseDicePageDateTag) : DateTime.Now.AddDays(-3);
		_closeDicePageCount = IsTagExist(_closeDicePageCountTag) ? reader.Read<int>(_closeDicePageCountTag) : 0;
		_loginWorkflowState = IsTagExist(_loginWorkflowStateTag) ? reader.Read<UserLoginWorkflowState>(_loginWorkflowStateTag) : UserLoginWorkflowState.OK;
		_shouldHandleUserDataLoss = IsTagExist(_shouldHandleUserDataLossTag) ? reader.Read<bool>(_shouldHandleUserDataLossTag) : false;
		_shouldHandleDeviceIdChange = IsTagExist(_shouldHandleDeviceIdChangeTag) ? reader.Read<bool>(_shouldHandleDeviceIdChangeTag) : false;
        _isInTinyMachineRoom = IsTagExist(_isInTinyMachineRoomTag) ? reader.Read<bool>(_isInTinyMachineRoomTag) : false;
        _lastOpenChristMasMaxWinUiDate = IsTagExist(_lastOpenChristMasMaxWinUiDateTag) ? reader.Read<DateTime>(_lastOpenChristMasMaxWinUiDateTag) : DateTime.Now;
        _lastOpenDoubleHourBonusUiDate = IsTagExist(_lastOpenDoubleHourBonusUiDateTag) ? reader.Read<DateTime>(_lastOpenDoubleHourBonusUiDateTag) : DateTime.Now;
        _diceIapId = IsTagExist(_diceIapIdTag) ? reader.Read<int>(_diceIapIdTag) : 0;
        _diceRatio = IsTagExist(_diceRatioTag) ? reader.Read<int>(_diceRatioTag) : 0;
        _diceInitCredits = IsTagExist(_diceInitCreditsTag) ? reader.Read<ulong>(_diceInitCreditsTag) : 0;
        _lastMachineAdEndDate = IsTagExist(_lastMachineAdEndDateTag) ? reader.Read<DateTime>(_lastMachineAdEndDateTag) : DateTime.Now;
        _lastMachineAdId = IsTagExist(_lastMachineAdIdTag) ? reader.Read<int>(_lastMachineAdIdTag) : 0;
		_notifyIDs = IsTagExist(_notifyIDTag) ? reader.ReadList<int>(_notifyIDTag) : new List<int>();
        _lastStayMapRoom = IsTagExist(_lastStayMapRoomTag) ? reader.Read<MapMachineRoom>(_lastStayMapRoomTag) : MapMachineRoom.CUSTOM;
    }

	public override void Write(ES2Writer writer)
	{
		WriteCore(writer);

		writer.Write(VerityIAPDic, _mVerityIAPsName);
		writer.Write(_totalPayAmount, _totalPayAmountTag);
		writer.Write(_firstEnterGameTime, _firstEnterGameTimeTag);
		writer.Write(_firstBindFacebookTime, _firstBindFacebookTimeTag);
		writer.Write(_lastLoginTime, _lastLoginTimeTag);
		writer.Write(_doubleLevelUpTime, _doubleLevelUpTimeTag);
		writer.Write(_isSoundOn, _soundOnTag);
		writer.Write(_isMusicOn, _musicOnTag);
		writer.Write(_currSocialAppID, _currSocialAPPIDTag);
		writer.Write(_rewardADSVedioPlayTime, _rewardADSVedioPlayTimeTag);
		writer.Write(_lastGetGetRewardADSVedioTime, _lastGetGetRewardADSVedioTimeTag);
		writer.Write(_lastShowPlagueTime, _lastShowPlagueTimeTag);
		writer.Write(_lastShowNoADSStoreTime, _lastShowNoADSStoreTimeTag);
		writer.Write(_lastShowThreeStoreTime, _lastShowThreeStoreTimeTag);
		writer.Write(FirstPlayMacthineDic, _firstplayMacthineDicTag);
		writer.Write(_versionName, _versionTag);
		writer.Write(_isNewGame, _isNewGameTag);
		writer.Write(_isFirstEnterGame, _isFirstEnterGameTag);
		writer.Write(TournamentScoreInforDic,_tournamentScoreInforTag);
		WriteFriendDictionary (writer);
		writer.Write (_friendCollectGiftNum, _friendCollectGiftNumTag);
		writer.Write (_friendSendGiftNum, _friendSendGiftNumTag);
		writer.Write (_friendCollectCreditsDailySum, _friendCollectCreditsDailySumTag);
		writer.Write(_friendSendCreditsDailySum, _friendSendCreditsDailySumTag);
        writer.Write(_lastRefuseMachineCommentTime, _lastRefuseMachineCommentTimeTag);
        writer.Write(_lastMachineCommentTime, _lastMachineCommentTimeTag);
        writer.Write(_machineCommentTimesToday, _machineCommentTimesTodayTag);
        writer.Write(_lastPopFaceBookLogin, _lastPopFaceBookLoginTag);
        writer.Write(_lastBigWinDay, _lastBigWinDayTag);
		writer.Write(_lastPayForDiceDate, _lastPayForDiceDateTag);
		writer.Write(_lastCloseDicePageDate, _lastCloseDicePageDateTag);
		writer.Write(_closeDicePageCount, _closeDicePageCountTag);
		writer.Write(_loginWorkflowState, _loginWorkflowStateTag);
		writer.Write(_shouldHandleUserDataLoss, _shouldHandleUserDataLossTag);
		writer.Write(_shouldHandleDeviceIdChange, _shouldHandleDeviceIdChangeTag);
	    writer.Write(_isInTinyMachineRoom, _isInTinyMachineRoomTag);
        writer.Write(_lastOpenChristMasMaxWinUiDate, _lastOpenChristMasMaxWinUiDateTag);
        writer.Write(_lastOpenDoubleHourBonusUiDate, _lastOpenDoubleHourBonusUiDateTag);
        writer.Write(_diceIapId, _diceIapIdTag);
        writer.Write(_diceRatio, _diceRatioTag);
        writer.Write(_diceInitCredits, _diceInitCreditsTag);
        writer.Write(_lastMachineAdId, _lastMachineAdIdTag);
        writer.Write(_lastMachineAdEndDate, _lastMachineAdEndDateTag);
		writer.Write(_notifyIDs, _notifyIDTag);
        writer.Write(_lastStayMapRoom, _lastStayMapRoomTag);
        writer.Write(MapRoomLatStayPosDic, _mapRoomLastStayPosTag);
    }

	public DateTime FirstEnterGameTime
	{
		get
		{
			return _firstEnterGameTime;
		}
		set
		{
			_firstEnterGameTime = value;
			Save();
		}
	}

	public DateTime FirstBindFacebookTime
	{
		get
		{
			return _firstBindFacebookTime;
		}
		set
		{
			_firstBindFacebookTime = value;
			Save();
		}
	}

	public DateTime LastLoginTime
	{
		get { return _lastLoginTime; }
		set { _lastLoginTime = value; }
	}

	public DateTime DoubleLevelUpTime
	{
		get
		{
			return _doubleLevelUpTime;
		}
		set
		{
			_doubleLevelUpTime = value;
			Save();
		}
	}

    public bool IsInTinyMachineRoom
    {
        get { return _isInTinyMachineRoom; }
        set { _isInTinyMachineRoom = value; Save(); }
    }

    public DateTime LastOpenChristmasMaxWInUiDate
    {
        get { return _lastOpenChristMasMaxWinUiDate; }
        set { _lastOpenChristMasMaxWinUiDate = value; Save(); }
    }

    public DateTime LastOpenDoubleHourBonusUiDate
    {
        get { return _lastOpenDoubleHourBonusUiDate; }
        set { _lastOpenDoubleHourBonusUiDate = value; Save(); }
    }

    public int DiceRatio
    {
        set { _diceRatio = value;}
        get { return _diceRatio; }
    }

    public ulong DiceInitCredits
    {
        set { _diceInitCredits = value;}
        get { return _diceInitCredits; }
    }

    public int DiceIapId
    {
        set { _diceIapId = value;}
        get { return _diceIapId; }
    }

    public DateTime LastMachineAdEndTime
    {
        set{ _lastMachineAdEndDate = value; Save(); }
        get { return _lastMachineAdEndDate; }
    }

    public int LastMachineAdId
    {
        set { _lastMachineAdId = value; }
        get { return _lastMachineAdId; }
    }

    public bool IsSoundOn
	{
		get { return _isSoundOn; }
		set
		{
			if(_isSoundOn != value)
			{
				_isSoundOn = value;
				Save();
			}
		}
	}

	public bool IsMusicOn
	{
		get { return _isMusicOn; }
		set
		{
			if(_isMusicOn != value)
			{
				_isMusicOn = value;
				Save();
			}
		}
	}

	public void SetIAPData(IAPData data)
	{
		VerityIAPDic[data.Receipt] = data;
		Save();
	}

	public IAPData GetIAPData(string receipt)
	{
		IAPData data = null;
		if(VerityIAPDic.ContainsKey(receipt))
			data = VerityIAPDic[receipt];
		return data;
	}

	public void RemoveIAPData(string receipt)
	{
		if(VerityIAPDic.ContainsKey(receipt))
			VerityIAPDic.Remove(receipt);
		else
			Debug.LogError("VerityIAPDic doesn't contain key: " + receipt);

		Save();
	}

	public void SetIAPState(string receipt, IAPData.IAPState s)
	{
		if(VerityIAPDic.ContainsKey(receipt))
		{
			var data = VerityIAPDic[receipt];
			data.State = s;
			Save();
		}
	}

	public IAPData.IAPState GetIAPState(string receipt)
	{
		if(VerityIAPDic.ContainsKey(receipt))
		{
			return VerityIAPDic[receipt].State;
		}
		return IAPData.IAPState.End;
	}

	public bool GetReceiptDoneState(string receipt)
	{
		var data = VerityIAPDic[receipt];
		if(data.State == IAPData.IAPState.Verified
			|| data.State == IAPData.IAPState.Failed
			|| data.State == IAPData.IAPState.CancelPurchase
			|| data.State == IAPData.IAPState.End)
		{
			return true;
		}
		return false;
	}

	public bool GetReceiptWaitingState(string receipt)
	{
		if(!VerityIAPDic.ContainsKey(receipt))
		{
			Debug.LogError("No receipt in dict for: " + receipt);
			return false;
		}

		var data = VerityIAPDic[receipt];
		Debug.Log("Receipt state: " + data.State.ToString());
		bool result = (data.State == IAPData.IAPState.BeginPurchase
		              || data.State == IAPData.IAPState.FinishPurchase
		              || data.State == IAPData.IAPState.BeginVerify
		              || data.State == IAPData.IAPState.DelayPurchase);
        
		return result;
	}

	public void ClearTotalPayAmount()
	{
		_totalPayAmount = 0.0f;
		Save();
	}

	public void SetFriendDataDirectly(string udid, FriendData data){
		if (_friendsDict != null) {
			_friendsDict [udid] = data;
			Save ();
		}
	}

	public void SetFriendDataList(Dictionary<string, FriendData> dict){
		foreach (var pair in dict) {
			if (!_friendsDict.ContainsKey (pair.Key)) {
				_friendsDict [pair.Key] = pair.Value;
			} else {
				_friendsDict [pair.Key].Level = pair.Value.Level;
				if (pair.Value.LastSendTime != DateTime.MinValue) {
					_friendsDict [pair.Key].LastSendTime = pair.Value.LastSendTime;
				}
			}
		}

		Save ();
	}

	public FriendData GetFriendData(string udid){
		if (_friendsDict != null && _friendsDict.ContainsKey (udid)) {
			return _friendsDict [udid];
		}

		return null;
	}

	public void ReadFriendDictionary(ES2Reader reader){
		Dictionary<string, string> dict = new Dictionary<string, string> ();
		dict = IsTagExist(_friendTag) ? reader.ReadDictionary<string, string> (_friendTag) : new Dictionary<string, string>();

		foreach (var pair in dict) {
			FriendData data = FriendData.Deserialize (pair.Value);
			if (data != null) {
				_friendsDict [pair.Key] = data;
			} 
		}
	}

	public void WriteFriendDictionary(ES2Writer writer){
		Dictionary<string , string> dict = new Dictionary<string, string> ();
		foreach (var pair in _friendsDict) {
			dict.Add (pair.Key, FriendData.Serialize (pair.Value));
		}

		writer.Write (dict, _friendTag);
	}

	public void ResetDailyFriendGiftNum(){
		//每天0点清空
		DateTime currentTime = NetworkTimeHelper.Instance.GetNowTime();
		DateTime todayMinDateTime = new DateTime (currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);

		TimeSpan span = _lastLoginTime - todayMinDateTime;
		// 上一次登录为昨天
		if (span.TotalSeconds < 0) {
			_friendCollectGiftNum = 0;
			_friendSendGiftNum = 0;
			_friendCollectCreditsDailySum = 0;
			_friendSendCreditsDailySum = 0;
		}
	}

	public void RefreshIsFirstLoginToday()
	{
		DateTime now = NetworkTimeHelper.Instance.GetNowTime();
		DateTime nowDate = now.Date;
		DateTime lastDate = _lastLoginTime.Date;
		int compare = lastDate.CompareTo(nowDate);
		_isFirstLoginToday = compare < 0;
	}

	// 增加推送ID到cache
	public void AddNotifyID(int id, bool dirty){
		if (!_notifyIDs.Contains(id)){
			_notifyIDs.Add(id);
		}
		if (dirty)
			Save();
	}

	// 清理本地推送id cache
	public void ClearNotifyID(){
		_notifyIDs.Clear();
		Save();
	}

    public float GetMapRoomLastStayPos(MapMachineRoom room)
    {
        float result = 0;
        if (MapRoomLatStayPosDic.ContainsKey(room))
        {
            result = MapRoomLatStayPosDic[room];
        }

        return result;
    }

    public void SetMapRoomLastStayPos(MapMachineRoom room, float pos)
    {
        MapRoomLatStayPosDic[room] = pos;
        Save();
    }
}
