using System.Collections;
using System.Collections.Generic;

public class CoreSpinResult
{
	//Note:
	//1 Difference about _winRatio, _specialSmallGameWinRatio and TotalWinRatio
	//(1) _winRatio: ratio that wins from spin and hit on payline
	//(2) _specialSmallGameWinRatio: ratio that wins from special small games, such as wheel. But some 
	//    other small games like freespin is still in _winRatio
	//(3) TotalWinRatio = _winRatio + _specialSmallGameWinRatio
	//Take care to use them properly

	public static readonly int PaylineMidOffset = 1;

	private SpinResultType _type = SpinResultType.None;
	private bool _isMultiLine;
	private List<CoreSymbol> _symbolList;
	private List<CoreSymbol> _symbolListBefore;
	private List<CoreSymbol> _finalSymbolList;
	private List<int> _stopIndexes;
	private List<CoreSymbol> _nonPayoutLineSymbolList; // zhousen 非支付线线上symbol列表
	private List<CoreCollectData> _collectDataList;// 产生收集物
	private float _winRatio = 0.0f;
	private PayoutData _payoutData; //non-null when win
	private NearHitData _nearHitData; //non-null when nearHit
	private IJoyData _joyData; //refer to _payoutData or _nearHitData, for convenient use
	private bool _shouldSlide; //might be true in slide level when win
	private List<int> _slideOffsetList; //useful in slide level
	private List<bool> _isFixedList; //when fixed, don't spin the reel
	private List<bool> _isRestrictedList; //when restricted, the symbol should not be offset when calculating nearHit
	private bool _isReversedSpin;
	private ulong _betAmount;
	private ulong _winAmount;
	private CoreLuckyMode _luckyMode;
	private int _longLuckyChange;
	private bool[] _matchFlags;
	private List<bool[]> _winSymbolFlagsList;
	private bool _isRespin;
	private bool _isJackpotWin;
	private string _jackpotWinType;
	private NormalWinType _normalWinType;
	private float _specialSmallGameWinRatio = 0.0f;

	// 特殊倍数加成
	private float _specialMultiplierFactor;

	//multiLine
	private CoreMultiLineCheckResult _multiLineCheckResult;
	private RewardResultData _multiLineRewardResultData;

	private CoreMachine _coreMachine;
	private MachineConfig _machineConfig;
	private CoreChecker _checker;
	private CoreSpinInput _spinInput;
	public CoreSpinInput SpinInput { get { return _spinInput; } }
	public CoreMachine CoreMachine{ get { return _coreMachine; } }
	public SpinResultType Type { get { return _type; } set { _type = value; } }
	public bool IsMultiLine { get { return _isMultiLine; } }
	public List<CoreSymbol> SymbolList { get { return _symbolList; } set { _symbolList = value; } }
	public List<CoreSymbol> SymbolListBefore { get { return _symbolListBefore; } set { _symbolListBefore = value; } }
	public List<CoreSymbol> FinalSymbolList { get { return _finalSymbolList; } }
	public List<int> StopIndexes { get { return _stopIndexes; } }
	public List<CoreSymbol> NonPayoutLineSymbolList { get { return _nonPayoutLineSymbolList; } }
	public List<CoreCollectData> CollectDataList { get { return _collectDataList; } }
	public float WinRatio { get { return _winRatio; } set { _winRatio = value; } }
	public PayoutData PayoutData { get { return _payoutData; } }
	public NearHitData NearHitData { get { return _nearHitData; } }
	public IJoyData JoyData { get { return _joyData; } }
	public bool ShouldSlide { get { return _shouldSlide; } set { _shouldSlide = value; } }
	public List<int> SlideOffsetList { get { return _slideOffsetList; } }
	public List<bool> IsFixedList { get { return _isFixedList; } }
	public List<bool> IsRestrictedList { get { return _isRestrictedList; } }
	public bool IsReversedSpin { get { return _isReversedSpin; } set { _isReversedSpin = value; } }

	public ulong BetAmount { get { return _betAmount; } }
	public ulong NormalizedBetAmount
	{
		get { return CoreUtility.GetNormalizedBetAmount(_machineConfig, _betAmount); }
	}

	public ulong WinAmount { get { return _winAmount; } }
	public CoreLuckyMode LuckyMode { get { return _luckyMode; } }
	public int LongLuckyChange { get { return _longLuckyChange; } set { _longLuckyChange = value; } }
	public bool[] MatchFlags { get { return _matchFlags; } }
	public List<bool[]> WinSymbolFlagsList { get { return _winSymbolFlagsList; } }
	public bool IsRespin { get { return _isRespin; } set { _isRespin = value; } }
	public CoreMultiLineCheckResult MultiLineCheckResult { get { return _multiLineCheckResult; } set { _multiLineCheckResult = value; } }
	public RewardResultData MultiLineRewardResultData { get { return _multiLineRewardResultData; } set { _multiLineRewardResultData = value; } }
	public bool IsJackpotWin{ get { return _isJackpotWin; }  set { _isJackpotWin = value; } }
	public string JackpotWinType { get { return _jackpotWinType; } }
	public float SpecialMultiplierFactor { get { return _specialMultiplierFactor; } }
	public NormalWinType NormalWinType { get { return _normalWinType; } }
	public float SpecialSmallGameWinRatio { get { return _specialSmallGameWinRatio; } }

	public float TotalWinRatio
	{
		get
		{
			return _winRatio + _specialSmallGameWinRatio;
		}
	}

	public ulong ConsumedBetAmount
	{
		//Note: For now only when isRespin, the betAmount is not consumed.
		//This condition might change in the future.
		get 
		{
			ulong result = _isRespin ? 0 : _betAmount;
			return result;
		}
	}

	public bool HasFixedSymbol
	{
		get
		{
			return ListUtility.IsContainElement(_isFixedList, true);
		}
	}

	public CoreSpinResult(SpinResultType type, CoreMachine coreMachine, CoreSpinInput spinInput)
	{
		_type = type;

		_coreMachine = coreMachine;
		_machineConfig = _coreMachine.MachineConfig;
		_checker = _coreMachine.Checker;
		_spinInput = spinInput;

		_isMultiLine = _machineConfig.BasicConfig.IsMultiLine;
		_symbolList = new List<CoreSymbol>();
		_symbolListBefore = new List<CoreSymbol>();
		_finalSymbolList = new List<CoreSymbol> ();
		_stopIndexes = new List<int>();
		_nonPayoutLineSymbolList = new List<CoreSymbol> ();
		_collectDataList = new List<CoreCollectData> ();
		_slideOffsetList = new List<int>();
		_isFixedList = new List<bool>();
		_isRestrictedList = new List<bool>();
		_winSymbolFlagsList = new List<bool[]>();

		for(int i = 0; i < _machineConfig.BasicConfig.ReelCount; i++)
		{
			_symbolList.Add(null);
			_symbolListBefore.Add(null);
			_finalSymbolList.Add (null);
			_stopIndexes.Add(0);
			_slideOffsetList.Add(0);
			_isFixedList.Add(false);
			_isRestrictedList.Add(false);
			_winSymbolFlagsList.Add(new bool[CoreDefine.PaylineHorizonCount]);
		}

		_matchFlags = new bool[_machineConfig.BasicConfig.ReelCount];

		_isJackpotWin = false;
		_specialMultiplierFactor = 0.0f;
	}

	public void FillWinRatio()
	{
		if(_type == SpinResultType.Win)
		{
			// ratio区间
			_winRatio = PayoutHelper.GetRatio(_spinInput.BetAmount, _machineConfig.BasicConfig, _payoutData);

			if(!_payoutData.IsFixed)
			{
				for(int i = 0; i < _symbolList.Count; i++)
				{
					SymbolData symbolData = _symbolList[i].SymbolData;

					if(_slideOffsetList[i] != 0)
					{
						SingleReel reel = _machineConfig.ReelConfig.GetSingleReel(i);
						int afterSlideIndex = reel.GetNeighborStopIndex(_symbolList[i].StopIndex, _slideOffsetList[i]);
						string symbolName = reel.GetSymbolName(afterSlideIndex);
						symbolData = _machineConfig.SymbolConfig.GetSymbolData(symbolName);
					}

					int multiplier = _machineConfig.SymbolConfig.GetMultiplier(symbolData.Name);
					_winRatio *= multiplier;
				}
			}
			// 特殊加成系数
			if (GetSpecialMultiplierSymbolCount () > 0) {
				_specialMultiplierFactor = GetResultSpecialMultiplier ();
			} else {
				_specialMultiplierFactor = 0.0f;
			}
		}
		else
		{
			_winRatio = 0.0f;
			_specialMultiplierFactor = 0.0f;
		}

		RefreshNormalWinType();
	}

	public void FillMultiLineWinRatio()
	{
		_winRatio = _multiLineCheckResult.PayoutReward;

		RefreshNormalWinType();

		// assign _type here for SymbolProb
		if(_machineConfig.BasicConfig.IsMultiLineSymbolProb)
		{
			if(_multiLineCheckResult.PayoutInfos.Count > 0)
				_type = SpinResultType.Win;
			else
				_type = SpinResultType.Loss;
		}
	}

	void RefreshNormalWinType()
	{
		_normalWinType = PuzzleUtility.GetNormalWinType(TotalWinRatio);
	}

	public void AddSpecialSmallGameWinRatio(float addWinRatio)
	{
		_specialSmallGameWinRatio += addWinRatio;
		RefreshNormalWinType();
	}

	// 填充特别元素的基础倍率
	public void FillSpecialMultiplier(CoreMachine machine){
		if (machine.LastSpinResult != null 
			&& machine.LastSpinResult.SpecialMultiplierFactor > 0.0f){
			float multiplier = machine.LastSpinResult.SpecialMultiplierFactor;
			_winRatio *= multiplier;
		}
	}

	public void FillFixedSymbols(CoreSymbol[] symbols)
	{
		for(int i = 0; i < symbols.Length; i++)
		{
			if(symbols[i] != null)
			{
				_symbolList[i] = symbols[i];
				_isFixedList[i] = true;
				_isRestrictedList[i] = true;
			}
		}
	}

	public void Recheck(CoreSpinInput spinInput)
	{
		if(!_isMultiLine)
		{
			CoreCheckResult checkResult = _checker.CheckResultWithSymbols(_joyData, _symbolList);
			MergeFromCheckResult(checkResult);
			FillWinRatio();
		}
	}

	private void MergeFromCheckResult(CoreCheckResult checkResult)
	{
		_type = checkResult.Type;
		_payoutData = checkResult.PayoutData;
		_nearHitData = checkResult.NearHitData;
	}

	public List<string> GetSymbolNameList()
	{
		List<string> result = ListUtility.MapList(_symbolList, (CoreSymbol s) => {
			return s.SymbolData.Name;
		});
		return result;
	}

	public List<int> GetStopIndexList()
	{
		List<int> result = ListUtility.MapList(_symbolList, (CoreSymbol s) => {
			return s.StopIndex;
		});
		return result;
	}

	void CustomLog(string s, bool isLogNormal)
	{
		if(isLogNormal)
			CoreDebugUtility.Log(s);
		else
			CoreDebugUtility.LogError(s);
	}

	public void CompareToCheckResult(CoreCheckResult checkResult)
	{
		//when isAllRestricted is true, it might force the winType or payoutId change. For example,
		//In M30, when FreeSpin ends, the spinResult is restricted to 3 Box(Bonus) symbols.
		//So we don't logError in this case.
		bool isAllRestricted = ListUtility.IsAllElementsSatisfied(_isRestrictedList, (bool r) => {
			return r;
		});

		if(_type != checkResult.Type)
		{
			//WinWithZeroRatio is a special case. It might cause _type == NearHit while checkResult.Type == Win
			//This is ok
			if(!checkResult.IsWinWithZeroRatio())
			{
				CustomLog("### SpinResult type doesn't match", isAllRestricted);
				CustomLog("SpinResult: " + this.ToString(), isAllRestricted);
				CustomLog("CheckResult: " + checkResult.ToString(), isAllRestricted);
				//CoreDebugUtility.Assert(false);
			}
		}

		if(_type == SpinResultType.Win)
		{
			if(_payoutData.Id != checkResult.PayoutData.Id)
			{
				CustomLog("### SpinResult id doesn't match", isAllRestricted);
				CustomLog("SpinResult: " + this.ToString(), isAllRestricted);
				CustomLog("CheckResult: " + checkResult.ToString(), isAllRestricted);
				//CoreDebugUtility.Assert(false);
			}
		}
		else if(_type == SpinResultType.NearHit)
		{
			//todo: the nearHit Id might not be the same, but the result
			//could be still correct, so I might re-check the code later
//			if(_nearHitData.Id != checkResult.NearHitData.Id)
//				CoreDebugUtility.Assert(false);

			//But at least, spinResult should be NearHit
			if(_nearHitData == null)
			{
				CoreDebugUtility.Assert(false);
			}
		}
	}

	public ulong RefreshWinAmount()
	{
		_winAmount = (ulong)(NormalizedBetAmount * _winRatio);
		return _winAmount;
	}

	public void FeedBetAndWinAmount(ulong betAmount)
	{
		_betAmount = betAmount;
		RefreshWinAmount();
	}

	public void FeedLuckyMode(CoreLuckyMode mode)
	{
		_luckyMode = mode;
	}

	public void SetPayoutData(PayoutData data)
	{
		_payoutData = data;
		// 这里需要注释掉，switch symbol模式下，是有可能_joydata不为null的。
		// CoreDebugUtility.Assert(_joyData == null);
		_joyData = data;
	}

	public void SetNearHitData(NearHitData data)
	{
		_nearHitData = data;
		CoreDebugUtility.Assert(_joyData == null);
		_joyData = data;
	}

	public void SetStopIndex(int reelIndex, int stopIndex)
	{
		_stopIndexes[reelIndex] = stopIndex;
		_symbolList[reelIndex] = _machineConfig.SymbolConfig.CreateCoreSymbol(reelIndex, stopIndex);
	}

	public void SetStopIndexes(int[] stopIndexes)
	{
		for(int i = 0; i < stopIndexes.Length; i++)
		{
			SetStopIndex(i, stopIndexes[i]);
		}
	}

	public void FillNonPayoutLineSymbolList()
	{
		ListUtility.ForEach(_symbolList, (CoreSymbol s) => {
			List<CoreSymbol> symbolList = _machineConfig.ReelConfig.GetNonPayoutLineSymbolsFromStopIndex(s.ReelIndex, s.StopIndex);
			_nonPayoutLineSymbolList.AddRange(symbolList);
		});
	}

	public void RefreshJackpotWinAmount(ulong win){
		_winAmount += win;
	}

	public void SetJackpotWin(PayoutData data, CoreSpinInput spinInput){
		if (data != null) {
			bool checkJackpotBet = _machineConfig.BasicConfig.JackpotMinBet > 0;
			if (checkJackpotBet) {
				_isJackpotWin = !string.IsNullOrEmpty (data.JackpotType) && spinInput.BetAmount >= _machineConfig.BasicConfig.JackpotMinBet;
				_jackpotWinType = _isJackpotWin ? data.JackpotType + "Jackpot" : "none";
			}
		}
	}

	public void SetJackpotWinMultiline(CoreSpinInput spinInput){
		bool checkJackpotBet = _machineConfig.BasicConfig.JackpotMinBet > 0;
		if (checkJackpotBet) {
			_isJackpotWin = _isJackpotWin && spinInput.BetAmount >= _machineConfig.BasicConfig.JackpotMinBet;
			_jackpotWinType = _isJackpotWin ? "SingleJackpot" : "none";
		}
	}

	public int GetSubtractLongLucky()
	{
		int result = 0;
		if(_type == SpinResultType.Win && _winAmount > 0)
		{
			if(_isMultiLine)
			{
				_multiLineCheckResult.PayoutInfos.ForEach((MultiLineMatchInfo info) => {
					float winRatio = info.PayoutData.Ratio;
					string[] symbolNames = _machineConfig.ReelConfig.GetSymbolNames(info.StopIndexes);
					for(int i = 0; i < symbolNames.Length; i++)
					{
						int multiplier = _machineConfig.SymbolConfig.GetMultiplier(symbolNames[i]);
						winRatio *= multiplier;
					}
					result += (int)(info.PayoutData.LongLuckySubtractFactor * winRatio * _betAmount);
				});
			}
			else
			{
				result = (int)(_payoutData.LongLuckySubtractFactor * _winAmount);
			}
		}
		return result;
	}

	public int GetPayoutId()
	{
		int result = 0;
		if(_isMultiLine)
		{
			if(_machineConfig.BasicConfig.IsMultiLineExhaustive)
				result = _multiLineRewardResultData.PayoutId;
			else if(_machineConfig.BasicConfig.IsMultiLineSymbolProb)
				result = 0;
			else
				CoreDebugUtility.Assert(false);
		}
		else
		{
			result = _payoutData.Id;
		}
		return result;
	}

	public int GetNearHitId()
	{
		int result = 0;
		if(_isMultiLine)
		{
			if(_machineConfig.BasicConfig.IsMultiLineExhaustive)
				result = _multiLineRewardResultData.NearHitId;
			else if(_machineConfig.BasicConfig.IsMultiLineSymbolProb)
				result = 0;
			else
				CoreDebugUtility.Assert(false);
		}
		else
		{
			result = _nearHitData.Id;
		}
		return result;
	}

	public int GetJoyId()
	{
		int result = 0;
		if(_type == SpinResultType.Win)
			result = GetPayoutId();
		else
			result = GetNearHitId();
		return result;
	}

	public void RefreshWinSymbolFlagsList()
	{
		if(_isMultiLine)
		{
			ListUtility.ForEach(_multiLineCheckResult.PayoutInfos, (MultiLineMatchInfo info) => {
				for(int i = 0; i < _winSymbolFlagsList.Count; i++)
				{
					bool[] flags = _winSymbolFlagsList[i];
					int index = PaylineMidOffset + info.Payline[i];
					flags[index] = true;
				}
			});
		}
		else
		{
			for(int i = 0; i < _winSymbolFlagsList.Count; i++)
			{
				bool[] flags = _winSymbolFlagsList[i];
				flags[PaylineMidOffset] = _matchFlags[i];

				if (_machineConfig.BasicConfig.IsBonusValidOnNonPayline) {
					int[] offsets;
					if (IsUnOrderedMatch ()) {
						offsets = new int[]{ -1, 0, 1 };
					} else {
						offsets = new int[]{ -1, 1 };
					}
					for (int k = 0; k < offsets.Length; k++) {
						int offset = offsets [k];
						SingleReel reel = _machineConfig.ReelConfig.GetSingleReel (i);
						int neighborIndex = reel.GetNeighborStopIndex (_stopIndexes[i], offset);
						SymbolType type = reel.GetSymbolType (neighborIndex);
//						LogUtility.Log ("offsets k = "+k+" offset= "+offset+" neighborindex= "+neighborIndex, Color.magenta);
						if (IsUnOrderedMatch ()) {
							string symbolName = reel.GetSymbolName (neighborIndex);
//							CoreDebugUtility.Log ("symbolname is "+symbolName+" reel "+i+" offset "+offset+" joydatasymbol "+_joyData.Symbols [0]);
							if (symbolName.Equals (_joyData.Symbols [0])) {
								flags [PaylineMidOffset + offset] = true;
							} else {
								flags [PaylineMidOffset + offset] = false;	
							}
						} else {
							if (_machineConfig.SymbolConfig.IsMatchSymbolType (type, SymbolType.Bonus))
								flags [PaylineMidOffset + offset] = true;
						}
					}
				}
			}
		}
	}

	public bool IsWinWithNonZeroRatio()
	{
		return _type == SpinResultType.Win && _winRatio > 0.0f;
	}

	public bool IsWinWithZeroRatio()
	{
		return _type == SpinResultType.Win && _winRatio == 0.0f;
	}

	public bool IsUnOrderedMatch()
	{
		bool isPayout = false, isNearHit = false;
		if(IsUnOrderedPayout())
			isPayout = true;
		else if(IsUnOrderedNearHit())
			isNearHit = true;
		return isPayout || isNearHit;
	}

	public bool IsUnOrderedPayout()
	{
		bool result = (_payoutData != null && _payoutData.PayoutType == PayoutType.UnOrdered);
		return result;
	}

	public bool IsUnOrderedNearHit()
	{
		bool result = (_nearHitData != null && _nearHitData.PayoutType == PayoutType.UnOrdered);
		return result;
	}

	public int GetSymbolCount(SymbolType type)
	{
		int result = ListUtility.GetElementCount(_symbolList, (CoreSymbol s) => {
			return s.SymbolData.SymbolType == type;
		});
		return result;
	}

	public int GetSpecialMultiplierSymbolCount(){
		int specialSymbolCount = ListUtility.CountElements (this.SymbolList, (CoreSymbol symbol) => {
			return _machineConfig.SymbolConfig.IsMatchSymbolType(symbol.SymbolData.SymbolType, SymbolType.Cherry);
		});

		return specialSymbolCount;
	}

	public float GetResultSpecialMultiplier(){
		float result = 1.0f;

		int specialSymbolCount = GetSpecialMultiplierSymbolCount();

		if (specialSymbolCount > 0) {
			result *= (float)specialSymbolCount;
		}

		return result;
	}

	public void FillFinalCoreSymbol(CoreMachine machine){
		for (int i = 0; i < _stopIndexes.Count; ++i) {
			SingleReel reel = machine.ReelList [i].ReelConfig;
			int stopIndex = _stopIndexes [i];
			int offset = _slideOffsetList [i];
			int index = reel.GetNeighborStopIndex (stopIndex, offset);
			CoreSymbol symbol = machine.ReelList [i].SymbolList[index];
			_finalSymbolList [i] = symbol;
		}
	}

	public bool IsWinPayoutData(PayoutData data)
	{
		CoreDebugUtility.Assert(data != null);

		bool result = false;
		if(_isMultiLine)
		{
			result = ListUtility.IsAnyElementSatisfied(_multiLineCheckResult.PayoutInfos, (MultiLineMatchInfo info) => {
				bool r = info.PayoutData.Id == data.Id;
				return r;
			});
		}
		else
		{
			result = (_payoutData != null && _payoutData.Id == data.Id);
		}
		return result;
	}

	#region Collect

	public List<CoreCollectData> GetPayLineCollectList(){
		List<CoreCollectData> paylineList = ListUtility.FilterList (_collectDataList, (CoreCollectData data)=>{
			return ListUtility.IsAnyElementSatisfied(SymbolList, (CoreSymbol s)=>{
				return data.ReelIndex == s.ReelIndex && data.StopIndex == s.StopIndex;
			});
		});

		return paylineList;
	}

	#endregion

	#region switch symbol

	public void FillSwitchSymbols(List<CoreSymbol> list){
		_symbolListBefore.Clear();
		_symbolListBefore.AddRange(_symbolList);
		_symbolList.Clear();
		_symbolList.AddRange(list);
	}
	
	#endregion

	public override string ToString()
	{
		string s;

		List<string> symbolNames = ListUtility.MapList(_symbolList, (CoreSymbol symbol) => {
			return symbol.SymbolData.Name;
		});
		string symbolNameStr = string.Join(", ", symbolNames.ToArray());

		List<string> stopIds = ListUtility.MapList(_symbolList, (CoreSymbol symbol) => {
			return symbol.StopId.ToString();
		});
		string stopIdStr = string.Join(", ", stopIds.ToArray());

		List<string> slideOffsets = ListUtility.MapList(_slideOffsetList, (int offset) => {
			return offset.ToString();
		});
		string slideOffsetStr = string.Join(", ", slideOffsets.ToArray());

		List<string> isFixeds = ListUtility.MapList(_isFixedList, (bool isFixed) => {
			return isFixed ? "1" : "0";
		});
		string isFixedStr = string.Join(", ", isFixeds.ToArray());

		if(_type == SpinResultType.Win)
		{
			PayoutType payoutType = _isMultiLine ? PayoutType.Count : _payoutData.PayoutType;
			s = string.Format("type:{0}, id:{1}, payoutType:{2}, symbols:({3}), stopIds:({4}), winMultiplier:{5}, shouldSlide:{6}, slideOffset:{7}, isFixedList:{8}", 
				_type, GetPayoutId(), payoutType, symbolNameStr, stopIdStr, _winRatio, 
				_shouldSlide, slideOffsetStr, isFixedStr);
		}
		else if(_type == SpinResultType.NearHit)
		{
			PayoutType payoutType = _isMultiLine ? PayoutType.Count : _nearHitData.PayoutType;
			s = string.Format("type:{0}, id:{1}, payoutType:{2}, symbols:({3}), stopIds:({4}), winMultiplier:{5}, isFixedList:{6}", 
				_type, GetNearHitId(), payoutType, symbolNameStr, stopIdStr,
				_winRatio, isFixedStr);
		}
		else
		{
			s = string.Format("type:{0}, id:{1}, payoutType:{2}, symbols:({3}), stopIds:({4}), isFixedList:{5}", 
				_type, 0, 0, symbolNameStr, stopIdStr,
				_winRatio, isFixedStr);
		}

		return s;
	}
}
