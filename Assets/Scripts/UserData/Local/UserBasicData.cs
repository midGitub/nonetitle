using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using System;

public partial class UserBasicData
{
	static private string _lastGetHourBonusDateTag = "LastBonusDay";
	static private string _lastGetDayBonusDateTag = "LastDayBounsDate";
	static private string _lastShowSpecialDateTag = "LastShowSpecialDate";
	static private string _lastGetCloseScorePageDateTag = "LastCloseScorePageDate";
	static private string _fistStartGameBonusTag = "StartGameBonus";
	static private string _dayBonusDaysTypeTag = "DayBonusDaysType";
	static private string _isFristLoadingFaceBookTag = "FirstLoadingFaceBook";
	static private string _facebookBindEmailTag = "Email";
	static private string _vipLevelTag = "VipPointTag";
	static private string _piggyBankCoinsTag = "PiggyBankCoinsTag";

	static private string _creditsTag = "Credits";
	static private string _userIdTag = "UserId";

	static private string _userLevelTag = "UserLevelTag";
	static private string _userLevelProgressTag = "UserLevelProgress";
	static private string _lastArchiveMD5Tag = "LastArchive";

	static private string _playerPayStateTag = "PlayerPayState";
	static private string _noADSTag = "NOADS";
	static private string _registrationTimeTag = "RegistrationTime";
	static private string _firstEnterGameTimeTag = "firstEnterGameTime";
	static private string _mailInforDicTag = "MailInforDic";
	static private string _isFirstSpinTag = "IsFirstSpin";
	static private string _isFirstScoreTag = "IsFirstScore";

    static private string _timesHasPaidInPiggyTag = "TimesHasPaidInPiggy";
    static private string _lastTimeLimitedStoreTag = "LastTimeLimitedStore";
    static private string _limitedStoreItemIDTag = "LimitedStoreItemID";
    static private string _hasBoughtTLItemTag = "HasBoughtTLItem";
	static private string _facebookLikesTag = "FacebookLikesTag";

	static private string _lastGetChestDateTag = "LastChestDate";
	static private string _lastLoginDateTag = "LastLoginDate";
	static private string _loginDaysTag = "LoginDays";
	static private string _loginTimesTag = "LoginTimes";
	private static readonly string _lastSpinDateTag = "lastSpinDateTag";
	private static readonly string _spinDaysTag = "spinDaysTag";
	static private string _hasBoughtSpecialOfferTag = "HasBoughtSpecialOffer";

	static private string _lastNoticeIDTag = "LastNoticeID";

	//pay
	static private string _paidAmountTag = "PaidAmount";
	static private string _buyNumberTag = "BuyNumberTag";
	static private string _totalPayAmountTag = "TotalPayAmount";
	static private string _lastPayTimeTag = "LastPayTime";
	static private string _historyMaxPayTag ="HistoryMaxPay";

	//pay protection
	static private string _payProtectionLastBankruptBuytimesTag = "PayProtectionLastBankruptBuytimes";

    static private string _maxWinDuringActivityTag = "MaxWinDuringActivity";
    static private string _hasUserGetChristmasMaxWinRewardTag = "HasUserGetChristmasMaxWinRewardTag";
    static private string _lastGetMaxWinActivityRewardDateTag= "LastGetMaxWinActivityRewardDate";
    static private string _freeNodeIndexTag = "FreeNodeIndex";
    static private string _freeQueueTag = "FreeQueueTag";
    static private string _freeWinQueueTag = "FreeWinQueueTag";
    static private string _payNodeIndexTag = "PayNodeIndex";
    static private string _payQueueTag = "PayQueueTag";
    static private string _payWinQueueTag = "PayWinQueueTag";
    static private string _isAllVipMachineUnlockTag = "IsAllVipMachineUnlockTag";
    static private string _lastReceivedBroadcastIdTag = "LastReceivedBroadcastIdTag";

	// 分阶段给lucky
	static private string  _lastPurchaseItemIDTag = "LastPurchaseItemID";// 上一次内购的物品id
	static private string _creditsWhenLastPurchaseTag = "CreditsWhenLastPurchase";// 上一次内购时玩家的credits
	static private string _luckyWhenLastPurchaseTag = "LuckyWhenLastPurchase";// 上一次内购时玩家的lucky
	static private string _luckyAddPeriodProgressTag = "LuckyAddPeriodProgress";// 分阶段lucky的进度

    private ObscuredULong _credits;
	private string _userId;
	private string _facebookBindEmail = "";
	private DateTime _lastHourBonusDateTime = System.DateTime.Now;
	private DateTime _lastDayBonusDateTime = System.DateTime.Now;
	private DateTime _lastShowSpecialDateTime = System.DateTime.Now;
	private DateTime _lastCloseScorePageTime = System.DateTime.Now;
	private DateTime _registrationTime = System.DateTime.Now;
	private DateTime _firstEnterGameTime = System.DateTime.MinValue;
	private ObscuredInt _bonusDaysType;
	private bool _isFirstSpin;
	private bool _isFirstScore;
	private bool _isFirstLoadingFaceBook;
	private bool _isFistStartGameBonus;
	private ObscuredInt _vipPoint;
	private LevelData _userLevel;
	private float _userLevelProgress;
	private string _lastArchiveMD5;
	private UserPayState _playerPayState;
	private bool _noADS;
	private ObscuredInt _piggyBankCoins;
    private ObscuredInt _timesHasPaidInPiggy;
    private DateTime _lastTimeLimitedStore = System.DateTime.Now;
    private string _limitedStoreItemID;
    private bool _hasBoughtTLItem;
	private bool _facebookLikes;
	private bool _hasBoughtSpecialOffer = false;
	//上一次破产保护时的购买次数
	private int  _payProtectionLastBankruptBuytimes = 0;

	// 分阶段lucky
	private int _lastPurchaseItemID;
	private ulong _creditsWhenLastPurchase;
	private ulong _luckyWhenLastPurchase;
	private int _luckyAddPeriodProgress;

	private DateTime _lastGetChestDateTime = System.DateTime.Now;
	private DateTime _lastLoginDateTime= System.DateTime.Now;
	private int _loginDays = 0;
	private int _loginTimes = 0;
	private int _spinDays = 0;
	private DateTime _lastSpinDate = System.DateTime.MinValue;

	//Important note by nichos:
	//_paidAmount field is wrong and useless, so don't use it. And don't delete it, 
	//since deleting it will cause the old version bundle crash when parsing the new old format user data.
	//So just leave it here and don't use it.
	private int _paidAmount;
	private int _buyNumber;
	private float _totalPayAmount;
	private float _historyMaxPaid;
	private DateTime _lastPayTime = DateTime.MinValue;
    private ulong _maxWinDuringActivity;
	private string _lastNoticeID = "0";
    private bool _hasUserGetChristmasMaxWinReward;
    private DateTime _lastGetMaxWinActivityRewardDate = DateTime.Now;
    private uint _freeNodeIndex;
    private string _freeQueue;
    private string _freeWinQueue;
    private uint _payNodeIndex;
    private string _payQueue;
    private string _payWinQueue;
    private bool _isAllVipMachineUnlock;
    private ulong _lastReceivedBroadcastId;

    public ulong Credits { get { return _credits; } }
	public DateTime LastHourBonusDateTime { get { return _lastHourBonusDateTime; } }
	public DateTime LastDayBonusDateTime { get { return _lastDayBonusDateTime; } }
	public DateTime LastCloseScorePageTime{ get { return _lastCloseScorePageTime;}}
	public DateTime RegistrationTime { get { return _registrationTime; } set { _registrationTime = value; Save(); } }
	public DateTime FirstEnterGameTime{get{ return _firstEnterGameTime;} set { _firstEnterGameTime = value;Save ();}}
	public int BonusDaysType { get { return _bonusDaysType; } }
	public int VIPPoint { get { return _vipPoint; } }
	public LevelData UserLevel { get { return _userLevel; } }
	public float UserLevelProgress { get { return _userLevelProgress; } }
	public UserPayState PlayerPayState { get { return _playerPayState; } set { _playerPayState = value; Save(); } }
	public int PiggyBankCoins { get { return _piggyBankCoins; } }
	public Dictionary<string, MailInfor> MailInforDic = new Dictionary<string, MailInfor>();

    public int TimesHasPaidInPiggy { get { return _timesHasPaidInPiggy; } }
    public DateTime LastTimeLimitedStore { get { return _lastTimeLimitedStore; } }
    public string LimitedStoreItemID { get { return _limitedStoreItemID; } }
    public bool HasBoughtTLItem { get { return _hasBoughtTLItem; } }

	public DateTime LastGetChestDateTime {get{return _lastGetChestDateTime; } }
	public DateTime LastLoginDateTime{get { return _lastLoginDateTime;}}

	public int LastPurchaseItemID{
		get { return _lastPurchaseItemID; }
		set { _lastPurchaseItemID = value; }
	}

	public ulong CreditsWhenLastPurchase{
		get { return _creditsWhenLastPurchase; }
		set { _creditsWhenLastPurchase = value; }
	}

	public ulong LuckyWhenLastPurchase {
		get { return _luckyWhenLastPurchase; }
		set { _luckyWhenLastPurchase = value; }
	}

	public int LuckyAddPeriodProgress {
		get { return _luckyAddPeriodProgress; }
		set { _luckyAddPeriodProgress = value; }
	}


	public int PayProtectionLastBankruptBuytimes {
		get {
			return _payProtectionLastBankruptBuytimes;
		}
		set {
			_payProtectionLastBankruptBuytimes = value;
			Save ();
		}
	}

	public bool HasBoughtSpecialOffer
	{
		get
		{ 
			return _hasBoughtSpecialOffer;
		}
		set
		{
			_hasBoughtSpecialOffer = value;
			Save ();
		}
	}

	public DateTime LastShowSpecialOffer
	{
		get
		{
			return _lastShowSpecialDateTime;
		}
		set
		{
			_lastShowSpecialDateTime = value;
			Save ();
		}
	}

	public string FacebookBindEmail
	{
		get 
		{
			return _facebookBindEmail;
		}
		set 
		{
			_facebookBindEmail = value;
			Save ();
		}
	}

	public bool IsPayUser
	{
		get { return _playerPayState == UserPayState.PayUser; }
	}

	public bool IsFreeUser
	{
		get { return _playerPayState == UserPayState.FreeUser; }
	}

	public string LastNoticeID
	{
		get { return _lastNoticeID; }
		set
		{
			_lastNoticeID = value;
		}
	}

	public int LoginDays
	{
		get{ return _loginDays;}
		set
		{
			_loginDays = value;
		}
	}

	public int LoginTimes
	{
		get { return _loginTimes;}
		set
		{
			_loginTimes = value;
		}
	}

	public int SpinDays
	{
		get{ return _spinDays;}
		set
		{
			_spinDays = value;
		}
	}

	public DateTime LastSpinDate
	{
		get { return _lastSpinDate;}
		set
		{
			_lastSpinDate = value;
		}
	}
    public uint FreeNodeIndex
    {
        set
        {
            _freeNodeIndex = value;
            Save();
        }
        get { return _freeNodeIndex; }
    }

    public string FreeQueue
    {
        set
        {
            _freeQueue = value;
        }

        get { return _freeQueue; }
    }

    public string FreeWinQueue
    {
        set
        {
            _freeWinQueue = value;
        }

        get { return _freeWinQueue; }
    }

    public uint PayNodeIndex
    {
        set
        {
            _payNodeIndex = value;
            Save();
        }
        get { return _payNodeIndex; }
    }

    public string PayQueue
    {
        set
        {
            _payQueue = value;
        }

        get { return _payQueue;}
    }

    public string PayWinQueue
    {
        set
        {
            _payWinQueue = value;
        }

        get { return _payWinQueue; }
    }

    public bool IsAllVipMachineUnlock
    {
        set
        {
            _isAllVipMachineUnlock = value;
            Save();
        }
        get { return _isAllVipMachineUnlock; }
    }

    //Important note by nichos:
	//_paidAmount field is wrong and useless, so don't use it. And don't delete it, 
	//since deleting it will cause the old version bundle crash when parsing the new old format user data.
	//So just leave it here and set it to private.
	private int PaidAmount { get { return _paidAmount; } }
	public int BuyNumber { get { return _buyNumber; } }
	public float TotalPayAmount { get { return _totalPayAmount; } }
	public float HistoryMaxPaid { get { return _historyMaxPaid; } }
	public DateTime LastPayTime { get { return _lastPayTime; } }

	public string UDID
	{
		get { return _userId; }
		set
		{
			_userId = value;

			if(string.IsNullOrEmpty(_userId))
				FabricManager.Instance.SetUDID(_userId);

			Save();
		}
	}

	public bool IsFirstLoadingFaceBook
	{
		get { return _isFirstLoadingFaceBook; }
		set
		{
			if(_isFirstLoadingFaceBook != value)
			{
				_isFirstLoadingFaceBook = value;
				Save();
			}
		}
	}

	public bool IsFistStartGameBonus
	{
		get { return _isFistStartGameBonus; }
		set
		{
			if(_isFistStartGameBonus != value)
			{
				_isFistStartGameBonus = value;
				Save();
			}
		}

	}

	public bool IsFirstSpin
	{
		get{ return _isFirstSpin;}
		set
		{
			if(_isFirstSpin != value)
			{
				_isFirstSpin = value;
				Save();
			}
		}
	}

	public bool IsFirstScore
	{
		get{ return _isFirstScore;}
		set
		{
			if(_isFirstScore != value)
			{
				_isFirstScore = value;
				Save();
			}
		}
	}

	public string LastArchiveMD5
	{
		get { return _lastArchiveMD5; }
		set
		{
			if(value != _lastArchiveMD5)
			{
				_lastArchiveMD5 = value;
				Save();
			}
		}
	}

	public bool NoAds
	{
		get { return _noADS; }
		set
		{
			// 已经去广告了 就不能再修改了
			if(_noADS)
				return;
			_noADS = value;
		}
	}

	public bool LikeOurAppInFacebook
	{
		get{ return _facebookLikes;}
		set
		{
			if(_facebookLikes != value)
				_facebookLikes = value;
		}
	}

	public void Reset()
	{
		_credits = GetInitCredits();
	}

	private ulong GetInitCredits()
	{
		return (ulong)CoreConfig.Instance.MiscConfig.InitCredits;
	}

    public ulong MaxWinDuringActivity
    {
        get { return _maxWinDuringActivity;}
        set
        {
            if (_maxWinDuringActivity != value)
            {
                _maxWinDuringActivity = value;
                Save();
            }
        }
    }

    public bool HasUserGetChristmasMaxWinReward
    {
        get { return _hasUserGetChristmasMaxWinReward; }
        set
        {
            if (_hasUserGetChristmasMaxWinReward != value)
            {
                _hasUserGetChristmasMaxWinReward = value;
                Save();
            }
        }
    }

    public DateTime LastGetMaxWinActivityRewardDate
    {
        get { return _lastGetMaxWinActivityRewardDate; }
        set
        {
            _lastGetMaxWinActivityRewardDate = value;
            Save();
        }
    }

    public ulong LastReceivedBroadcastId
    {
        get { return _lastReceivedBroadcastId; }
        set
        {
            _lastReceivedBroadcastId = value;
            Save();
        }
    }

    public override void Read(ES2Reader reader)
	{
		ReadCore(reader);

		if(IsTagExist(_creditsTag))
			_credits = reader.Read<ulong>(_creditsTag);
		else
			_credits = GetInitCredits();

		if(IsTagExist(_userIdTag))
			_userId = reader.Read<string>(_userIdTag);
		else
			_userId = "";

		_isFistStartGameBonus = IsTagExist(_fistStartGameBonusTag) ? reader.Read<bool>(_fistStartGameBonusTag) : true;

		try
		{
			_lastHourBonusDateTime = IsTagExist(_lastGetHourBonusDateTag) ?
				Convert.ToDateTime(reader.Read<string>(_lastGetHourBonusDateTag))
				: NetworkTimeHelper.Instance.GetNowTime().AddHours(-12);
		}
		catch(Exception e)
		{
			_lastHourBonusDateTime = DateTime.MinValue;
		}

		try
		{
			_lastDayBonusDateTime = IsTagExist(_lastGetDayBonusDateTag) ?
				Convert.ToDateTime(reader.Read<string>(_lastGetDayBonusDateTag))
				: NetworkTimeHelper.Instance.GetNowTime().AddDays(-3);
		}
		catch(Exception e)
		{
			_lastDayBonusDateTime = DateTime.MinValue;
		}

		try
		{
			_lastShowSpecialDateTime = IsTagExist(_lastShowSpecialDateTag)?
				Convert.ToDateTime(reader.Read<string>(_lastShowSpecialDateTag))
				:NetworkTimeHelper.Instance.GetNowTime().AddDays(-3);

		}
		catch(Exception e) 
		{
			_lastShowSpecialDateTime = DateTime.MinValue;
		}

		try
		{
			_lastCloseScorePageTime = IsTagExist(_lastGetCloseScorePageDateTag) ?
				Convert.ToDateTime(reader.Read<string>(_lastGetCloseScorePageDateTag))
				: NetworkTimeHelper.Instance.GetNowTime().AddDays(-2);
		}
		catch(Exception e)
		{
			_lastCloseScorePageTime = DateTime.MinValue;
		}

		try
		{
			_registrationTime = IsTagExist(_registrationTimeTag) ?
				Convert.ToDateTime(reader.Read<string>(_registrationTimeTag))
				: NetworkTimeHelper.Instance.GetNowTime();
		}
		catch(Exception e)
		{
			_registrationTime = NetworkTimeHelper.Instance.GetNowTime();
		}

		try
		{
			_firstEnterGameTime = IsTagExist(_firstEnterGameTimeTag)?
				Convert.ToDateTime(reader.Read<string>(_firstEnterGameTimeTag))
				:NetworkTimeHelper.Instance.GetNowTime();
		}
		catch(Exception e) 
		{
			_firstEnterGameTime = NetworkTimeHelper.Instance.GetNowTime ();
		}

		try
		{
			_lastGetChestDateTime=IsTagExist(_lastGetChestDateTag)?
				Convert.ToDateTime(reader.Read<string>(_lastGetChestDateTag))
				: NetworkTimeHelper.Instance.GetNowTime().AddHours(-12);
		}
		catch(Exception e)
		{
			_lastGetChestDateTime = DateTime.MinValue;
		}

		try
		{
			_lastLoginDateTime=IsTagExist(_lastLoginDateTag)?
				Convert.ToDateTime(reader.Read<string>(_lastLoginDateTag))
				:NetworkTimeHelper.Instance.GetNowTime().AddHours(-12);
		}
		catch(Exception e)
		{
			_lastLoginDateTime = DateTime.MinValue;
		}

		try
		{
			_hasBoughtSpecialOffer=IsTagExist(_hasBoughtSpecialOfferTag)?
				Convert.ToBoolean(reader.Read<string>(_hasBoughtSpecialOfferTag))
				:false;
		}
		catch(Exception e) 
		{
			_hasBoughtSpecialOffer = false;
		}
			
		_bonusDaysType = IsTagExist(_dayBonusDaysTypeTag) ? reader.Read<int>(_dayBonusDaysTypeTag) : 0;

		_isFirstSpin = IsTagExist(_isFirstSpinTag) ? reader.Read<bool> (_isFirstSpinTag) : true;
		_isFirstScore = IsTagExist(_isFirstScoreTag) ? reader.Read<bool> (_isFirstScoreTag) : true;
		_isFirstLoadingFaceBook = IsTagExist(_isFristLoadingFaceBookTag) ? reader.Read<bool>(_isFristLoadingFaceBookTag) : true;
		_facebookBindEmail = IsTagExist (_facebookBindEmailTag) ? reader.Read<string> (_facebookBindEmailTag) : "";
		_vipPoint = IsTagExist(_vipLevelTag) ? reader.Read<int>(_vipLevelTag) : 0;
		_userLevel = IsTagExist(_userLevelTag) ? reader.Read<LevelData>(_userLevelTag) : new LevelData { Level = 1, LevelPoint = 0 };
		if (IsTagExist(_userLevelProgressTag)){
			_userLevelProgress = reader.Read<float>(_userLevelProgressTag);
			_userLevel = UserLevelUtility.FetchUserLevel(_userLevel, _userLevelProgress);
		}else{
			_userLevelProgress = UserLevelUtility.GetLevelProgressOld2NewVersion(_userLevel);// 旧经验转换新经验
			_userLevel = UserLevelUtility.FetchUserLevel(_userLevel, _userLevelProgress);
		}
		_lastArchiveMD5 = IsTagExist(_lastArchiveMD5Tag) ? reader.Read<string>(_lastArchiveMD5Tag) : "";

		int payState = IsTagExist(_playerPayStateTag) ? reader.Read<int>(_playerPayStateTag) : 0;
		_playerPayState = (UserPayState)payState;
		_noADS = IsTagExist(_noADSTag) ? reader.Read<bool>(_noADSTag) : false;
		_facebookLikes = IsTagExist(_facebookLikesTag) ? reader.Read<bool>(_facebookLikesTag) : false;

		for(int i = 0; i < (int)JackpotPoolType.Max; ++i)
		{
			_jackpotDataArray[i] = IsTagExist(_jackpotTagArray[i]) ? reader.Read<JackpotData>(_jackpotTagArray[i])
				: new JackpotData();
		}

		if (IsTagExist (_jackpotTag)) {
			_jackpotDictSerialize = reader.ReadDictionary<string, string> (_jackpotTag);
			_jackpotDict = ConvertToJackpotDict (_jackpotDictSerialize);
		} else {
			CopyFromOldJackpotData ();
		}

        _piggyBankCoins = IsTagExist(_piggyBankCoinsTag) ? reader.Read<int>(_piggyBankCoinsTag) : PiggyBankHelper.GetNextPiggyInfoData(0).InitCredit;
		MailInforDic = IsTagExist(_mailInforDicTag) ? reader.ReadDictionary<string, MailInfor>(_mailInforDicTag) : new Dictionary<string, MailInfor>();

        _timesHasPaidInPiggy = IsTagExist(_timesHasPaidInPiggyTag) ? reader.Read<int>(_timesHasPaidInPiggyTag) : 0;

        try
        {
            _lastTimeLimitedStore = IsTagExist(_lastTimeLimitedStoreTag) ?
                Convert.ToDateTime(reader.Read<string>(_lastTimeLimitedStoreTag))
                : NetworkTimeHelper.Instance.GetNowTime().AddDays(-3);
        }
        catch(Exception e)
        {
            _lastTimeLimitedStore = DateTime.MinValue;
        }

		try
		{
			_lastSpinDate = IsTagExist(_lastSpinDateTag)?
				Convert.ToDateTime(reader.Read<string>(_lastSpinDateTag))
				:System.DateTime.MinValue;
		}
		catch(Exception e)
		{
			_lastSpinDate = System.DateTime.MinValue;
		}

        _limitedStoreItemID = IsTagExist(_limitedStoreItemIDTag) ? reader.Read<string>(_limitedStoreItemIDTag) : "";
        _hasBoughtTLItem = IsTagExist(_hasBoughtTLItemTag) ? reader.Read<bool>(_hasBoughtTLItemTag) : true;

		_paidAmount = 0; //useless field, so just assign 0
		_buyNumber = ReadTag<int>(reader, _buyNumberTag, 0);
		_totalPayAmount = ReadTag<float>(reader, _totalPayAmountTag, 0.0f);
		_historyMaxPaid = ReadTag<float>(reader, _historyMaxPayTag, 0.0f);
		//transfer from UserDeviceLocalData to here
		TransferTotalPayAmount();
		_lastPayTime = ReadTag<DateTime>(reader, _lastPayTimeTag, DateTime.MinValue);

		_payProtectionLastBankruptBuytimes = ReadTag<int> (reader, _payProtectionLastBankruptBuytimesTag, 0);
	    _maxWinDuringActivity = ReadTag<ulong>(reader, _maxWinDuringActivityTag, 0);
		_lastNoticeID = ReadTag<string>(reader, _lastNoticeIDTag, "0");
        _hasUserGetChristmasMaxWinReward = ReadTag<bool>(reader, _hasUserGetChristmasMaxWinRewardTag, false);

		_lastGetMaxWinActivityRewardDate = ReadTag<DateTime>(reader, _lastGetMaxWinActivityRewardDateTag, DateTime.Now);
		_loginDays = ReadTag<int>(reader, _loginDaysTag, 0);
		_loginTimes = ReadTag<int>(reader, _loginTimesTag, 0);
		_spinDays = ReadTag<int>(reader, _spinDaysTag, 0);
        _freeNodeIndex = ReadTag<uint>(reader, _freeNodeIndexTag, 0);
        _freeQueue = ReadTag<string>(reader, _freeQueueTag, "");
        _freeWinQueue = ReadTag<string>(reader, _freeWinQueueTag, "");
        _payNodeIndex = ReadTag<uint>(reader, _payNodeIndexTag, 0);
	    _payQueue = ReadTag<string>(reader, _payQueueTag, "");
	    _payWinQueue = ReadTag<string>(reader, _payWinQueueTag, "");
	    _isAllVipMachineUnlock = ReadTag<bool>(reader, _isAllVipMachineUnlockTag, false);
	    _lastReceivedBroadcastId = ReadTag<ulong>(reader, _lastReceivedBroadcastIdTag, 0);
		_lastPurchaseItemID = ReadTag<int>(reader, _lastPurchaseItemIDTag, 0);
		_creditsWhenLastPurchase = ReadTag<ulong>(reader, _creditsWhenLastPurchaseTag, 0);
		_luckyWhenLastPurchase = ReadTag<ulong>(reader, _luckyWhenLastPurchaseTag, 0);
		_luckyAddPeriodProgress = ReadTag<int>(reader, _luckyAddPeriodProgressTag, LongLuckyPeriodManager.allPeriodGet);// 默认是7，也就是3个标记位都是1，表示3次lucky都给了
	}

	public override void Write(ES2Writer writer)
	{
		WriteCore(writer);

		writer.Write((ulong)_credits, _creditsTag);
		writer.Write(_userId, _userIdTag);
		writer.Write(_lastHourBonusDateTime.ToString(), _lastGetHourBonusDateTag);
		writer.Write(_lastDayBonusDateTime.ToString(), _lastGetDayBonusDateTag);
		writer.Write (_lastShowSpecialDateTime.ToString(), _lastShowSpecialDateTag);
		writer.Write(_lastCloseScorePageTime.ToString(), _lastGetCloseScorePageDateTag);
		writer.Write(_registrationTime.ToString(), _registrationTimeTag);
		writer.Write (_firstEnterGameTime.ToString (), _firstEnterGameTimeTag);
		writer.Write((int)_bonusDaysType, _dayBonusDaysTypeTag);
		writer.Write(_isFirstSpin, _isFirstSpinTag);
		writer.Write(_isFirstScore, _isFirstScoreTag);
		writer.Write(_isFirstLoadingFaceBook, _isFristLoadingFaceBookTag);
		writer.Write (_facebookBindEmail, _facebookBindEmailTag);
		writer.Write(_isFistStartGameBonus, _fistStartGameBonusTag);
		writer.Write((int)_vipPoint, _vipLevelTag);
		writer.Write(_userLevel, _userLevelTag);
		writer.Write(_userLevelProgress, _userLevelProgressTag);
		writer.Write(_lastArchiveMD5, _lastArchiveMD5Tag);
		writer.Write((int)_playerPayState, _playerPayStateTag);
		writer.Write(_noADS, _noADSTag);

		writer.Write(_lastGetChestDateTime.ToString(), _lastGetChestDateTag);
		writer.Write (_lastLoginDateTime.ToString (), _lastLoginDateTag);
		writer.Write(_loginDays, _loginDaysTag);
		writer.Write(_loginTimes, _loginTimesTag);
		writer.Write(_spinDays, _spinDaysTag);
		writer.Write(_lastSpinDate.ToString(), _lastSpinDateTag);
		writer.Write (_hasBoughtSpecialOffer.ToString (), _hasBoughtSpecialOfferTag);


		for(int i = 0; i < (int)JackpotPoolType.Max; ++i)
		{
			writer.Write(_jackpotDataArray[i], _jackpotTagArray[i]);
		}

		_jackpotDictSerialize = ConvertToJackpotDictSerialize (_jackpotDict);
		writer.Write<string, string> (_jackpotDictSerialize, _jackpotTag);

		writer.Write((int)_piggyBankCoins, _piggyBankCoinsTag);
		writer.Write(MailInforDic, _mailInforDicTag);

        writer.Write((int)_timesHasPaidInPiggy, _timesHasPaidInPiggyTag);
        writer.Write(_lastTimeLimitedStore.ToString(), _lastTimeLimitedStoreTag);
        writer.Write(_limitedStoreItemID, _limitedStoreItemIDTag);
        writer.Write(_hasBoughtTLItem, _hasBoughtTLItemTag);
		writer.Write(_facebookLikes, _facebookLikesTag);

		writer.Write(_paidAmount, _paidAmountTag); //still write the useless field for forward compatibility
		writer.Write(_buyNumber, _buyNumberTag);
		writer.Write(_totalPayAmount, _totalPayAmountTag);
		writer.Write(_historyMaxPaid, _historyMaxPayTag);
		writer.Write<DateTime>(_lastPayTime, _lastPayTimeTag);
		writer.Write<int> (_payProtectionLastBankruptBuytimes, _payProtectionLastBankruptBuytimesTag);
        writer.Write(_maxWinDuringActivity, _maxWinDuringActivityTag);
		writer.Write(_lastNoticeID, _lastNoticeIDTag);
        writer.Write(_hasUserGetChristmasMaxWinReward, _hasUserGetChristmasMaxWinRewardTag);
        writer.Write(_lastGetMaxWinActivityRewardDate, _lastGetMaxWinActivityRewardDateTag);
        writer.Write(_freeNodeIndex, _freeNodeIndexTag);
        writer.Write(_freeQueue, _freeQueueTag);
        writer.Write(_freeWinQueue, _freeWinQueueTag);
        writer.Write(_payNodeIndex, _payNodeIndexTag);
        writer.Write(_payQueue, _payQueueTag);
        writer.Write(_payWinQueue, _payWinQueueTag);
        writer.Write(_isAllVipMachineUnlock, _isAllVipMachineUnlockTag);
        writer.Write(_lastReceivedBroadcastId, _lastReceivedBroadcastIdTag);
		writer.Write(_lastPurchaseItemID, _lastPurchaseItemIDTag);
		writer.Write(_creditsWhenLastPurchase, _creditsWhenLastPurchaseTag);
		writer.Write(_luckyWhenLastPurchase, _luckyWhenLastPurchaseTag);
		writer.Write(_luckyAddPeriodProgress, _luckyAddPeriodProgressTag);
    }

	//add this function in android 1.10.0
	void TransferTotalPayAmount()
	{
		if(UserDeviceLocalData.Instance.TotalPayAmount_Deprecated > 0.0f)
		{
			_totalPayAmount += UserDeviceLocalData.Instance.TotalPayAmount_Deprecated;
			Save();
			UserDeviceLocalData.Instance.ClearTotalPayAmount();
		}
	}

	#region Public methods

	public void SetPiggyBankCoins(int coins, bool save)
	{
		_piggyBankCoins = coins;
		if(save)
			Save();
	}

	public void SetVIPPoint(int point, bool save)
	{
		_vipPoint = point;
		if(save)
			Save();
	}

	public void SetUserLevelData(LevelData ld, bool save)
	{
		_userLevel = ld;

		float lvProgress = UserLevelConfig.Instance.GetLevelProgress((int)_userLevel.Level, _userLevel.LevelPoint, false);
		_userLevelProgress = lvProgress;
		LogUtility.Log("SetUserLevelData : " + _userLevelProgress, Color.yellow);

		if(save)
			Save();
	}

	public void SetLastGetHourBonusDate(DateTime dt)
	{
		_lastHourBonusDateTime = dt;
		Save();
	}

	public void SetLastGetDayBonusDate(DateTime dt)
	{
		_lastDayBonusDateTime = dt;
		Save();
	}

	public void SetLastCloseScorePageDate(DateTime dt)
	{
		_lastCloseScorePageTime = dt;
	}

	public void AddBonusDay()
	{
		_bonusDaysType = _bonusDaysType + 1 > 4 ? 0 : _bonusDaysType + 1;
	}

	public void SetBonusDay(int day)
	{
		_bonusDaysType = day;
	}

	public void SetLastGetChestDateTime(DateTime dt)
	{
		_lastGetChestDateTime = dt;
	}

	public void SetLastLoginDateTime(DateTime dt)
	{
		_lastLoginDateTime = dt;
	}

	public bool AddCredits(ulong amount, FreeCreditsSource source, bool save)
	{
		_credits += amount;
        CitrusFramework.CitrusEventManager.instance.Raise(new AddCreditsEvent(source, amount));
		CitrusFramework.CitrusEventManager.instance.Raise(new LongLuckyPeriodEvent());
		if(save)
			Save();
		return true;
	}

	public void SetCredits(ulong c, bool save)
	{
		_credits = c;
		CitrusFramework.CitrusEventManager.instance.Raise(new LongLuckyPeriodEvent());
		if(save)
			Save();
	}

	public bool SubtractCredits(ulong amount, bool save)
	{
		bool result = false;
		if(_credits >= amount)
		{
			_credits -= amount;
			if(save)
				Save();
			result = true;
			CitrusFramework.CitrusEventManager.instance.Raise(new LongLuckyPeriodEvent());
		}
		return result;
	}

    public void SetTimesHasPaidInPiggy(int times)
    {
        _timesHasPaidInPiggy = times;
    }

    public void AddTimesHasPaidInPiggy()
    {
        _timesHasPaidInPiggy++;
    }

	public void CleanCredits()
	{
		_credits = 0;
		Save();
	}

    public void SetLastTimeLimitedStore(DateTime dt)
    {
        _lastTimeLimitedStore = dt;
    }

    public void SetLimitedStoreItemID(string id)
    {
        _limitedStoreItemID = id;
    }

    public void SetHasBoughtTLItem(bool flag)
    {
        _hasBoughtTLItem = flag;
    }

	public void AddBuyNumber(bool save)
	{
		_buyNumber++;
		if(save)
			Save();
	}

	public void SetBuyNumber(int num, bool save)
	{
		_buyNumber = num;
		if(save)
			Save();
	}

	public void RecordPayAmount(float amount, bool save)
	{
		_totalPayAmount += amount;
		_historyMaxPaid = Math.Max(_historyMaxPaid, amount);
		_lastPayTime = NetworkTimeHelper.Instance.GetNowTime ();

		if(save)
			Save();
	}

	public void SetTotalPayAmount(float amount, bool save)
	{
		_totalPayAmount = amount;
		if(save)
			Save();
	}

	public void SetHistoryMaxPaid(float maxPaid,bool save)
	{
		_historyMaxPaid = maxPaid;
		if (save)
			Save();
	}

	public void SetLastPayTime(DateTime date, bool save)
	{
		_lastPayTime = date;
		if(save)
			Save();
	}

	public void UpdateSpinDays()
	{
		DateTime now = NetworkTimeHelper.Instance.GetNowTime();
		if (!TimeUtility.IsSameDay(_lastSpinDate, now))
		{
			_spinDays++;
			_lastSpinDate = now;
		}
	}

	// 分阶段赠送lucky
	public void SetInfosWhenPurchase(int id, ulong credits, ulong lucky){
		_lastPurchaseItemID = id;
		_creditsWhenLastPurchase = credits;
		_luckyWhenLastPurchase = lucky;
		CoreDebugUtility.Log("SetInfosWhenPurchase item = " + id + " credits = " + credits + " lucky = " + lucky);
	}

	public void SetLuckyAddPeriodProgress(int offset){
		_luckyAddPeriodProgress |= 1 << offset;
		CoreDebugUtility.Log("set luckyadd period progress = " + _luckyAddPeriodProgress + " offset = " + offset);
	}

	public bool CanAddLuckyInPeriod(int period){
		bool result = ( _luckyAddPeriodProgress & (1 << period) ) == 0;
		CoreDebugUtility.Log("CanAddLuckyInPeriod = " + result + " period = " + period);
		return result;
	}

	public void TestLuckyPeriod(){
		CoreDebugUtility.Log("TestLuckyPeriod");
		_luckyAddPeriodProgress = LongLuckyPeriodManager.allPeriodGet;
		SetLuckyAddPeriodProgress(0);
		SetLuckyAddPeriodProgress(1);
		SetLuckyAddPeriodProgress(2);
		SetLuckyAddPeriodProgress(3);
		_luckyAddPeriodProgress = LongLuckyPeriodManager.PeriodPhase1;
		CoreDebugUtility.Log("_luckyAddPeriodProgress = " + _luckyAddPeriodProgress);
		CanAddLuckyInPeriod(0);
		CanAddLuckyInPeriod(1);
		CanAddLuckyInPeriod(2);
		CanAddLuckyInPeriod(3);
		SetLuckyAddPeriodProgress(1);
		CanAddLuckyInPeriod(0);
		CanAddLuckyInPeriod(1);
		CanAddLuckyInPeriod(2);
		CanAddLuckyInPeriod(3);
		SetLuckyAddPeriodProgress(2);
		CanAddLuckyInPeriod(0);
		CanAddLuckyInPeriod(1);
		CanAddLuckyInPeriod(2);
		CanAddLuckyInPeriod(3);
		_luckyAddPeriodProgress = LongLuckyPeriodManager.allPeriodGet;
		CoreDebugUtility.Log("_luckyAddPeriodProgress = " + _luckyAddPeriodProgress);
		CanAddLuckyInPeriod(0);
		CanAddLuckyInPeriod(1);
		CanAddLuckyInPeriod(2);
		CanAddLuckyInPeriod(3);
	}

    #endregion
}
