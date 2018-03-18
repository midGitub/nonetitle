//todo: temp patch, don't roll BonusWild when Loss/NearHit in freeSpin machine
//Caution: this path can't be removed. The reason is that we allow this case:
//Rolling a NearHit result but get a row in Payout row as long as the attribute "NearHitOrLoss" is true
//So without this patch, in M2, since NearHitOrLoss is true, the BonusWild can still appear when rolling NearHit,
//And this is not what we want
#define kPatchFreeSpinBonusWildBug

using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class CoreGenerator : ICoreGenerator
{
	//input Joy config and data, output symbol names
	//
	//params:
	//fixedSymbols: input
	//baseJoyConfig: input
	//data: input
	//type: input
	//matchFlags: output (Caution)
	//
	//return:
	//stopIndexes: output
	private delegate int[] PayoutGeneratorDelegate(IList<CoreSymbol> fixedSymbols, BaseJoyConfig baseJoyConfig, IJoyData data, SpinResultType type, bool[] matchFlags, CoreSpinSymbolRestriction restriction);

	private IRandomGenerator _spinRoller;

	private CoreMachine _coreMachine;
	private CoreChecker _checker;
	private CoreLuckyManager _luckyManager;
	private CoreForceWinManager _forceWinManager;

	private MachineConfig _machineConfig;
	private BasicConfig _basicConfig;
	private SymbolConfig _symbolConfig;
	private ReelConfig _reelConfig;
	private PayoutConfig _payoutConfig;
	private PayoutConfig _luckyPayoutConfig;
	private NearHitConfig _nearHitConfig;
	private NearHitConfig _luckyNearHitConfig;

	private PayoutConfig _curPayoutConfig; //refer to _payoutConfig or _luckyPayoutConfig
	private NearHitConfig _curNearHitConfig; //refer to _nearHitConfig or _luckyNearHitConfig

	private int _basicReelCount;
	private int _totalReelCount;

	private RollHelper _spinRollHelper;
	private RollHelper _luckySpinRollHelper;
	private RollHelper _winRollHelper;
	private RollHelper _luckyWinRollHelper;
	private RollHelper _nearHitRollHelper;
	private RollHelper _luckyNearHitRollHelper;

	#if DEBUG
	private RollHelper _debugHugeWinSpinRollHelper;
	private RollHelper _debugHugeWinWinRollHelper;
	#endif

	private RollHelper _curSpinRollHelper; //refer to _spinRollHelper or _luckySpinRollHelper
	private RollHelper _curWinRollHelper; //refer to _winRollHelper or _luckyWinRollHelper
	private RollHelper _curNearHitRollHelper; //refer to _nearHitRollHelper or _luckyNearHitRollHelper

	//used to force a win when CoreLuckyMode.ShortLucky
	private List<PayoutData> _shortLuckyPayoutDatas;
	private RollHelper _shortLuckyWinRollHelper; 

	// 每根轴上能够生成的符合数量的unorder symbol数列表
	// 轴索引  symbol名  roll的数量  符合数量的stopindexes
	private List<Dictionary<string, Dictionary<int, List<int>>>> _unorderSymbolDictList = new List<Dictionary<string, Dictionary<int, List<int>>>> ();

	private PayoutGeneratorDelegate []_payoutGenerators = new PayoutGeneratorDelegate[(int)PayoutType.Count];

	#region Init

	public CoreGenerator(CoreMachine coreMachine, CoreChecker checker, CoreLuckyManager luckyManager, MachineConfig machineConfig)
	{
		_coreMachine = coreMachine;
		_checker = checker;
		_luckyManager = luckyManager;
		_machineConfig = machineConfig;
		_basicConfig = _machineConfig.BasicConfig;
		_symbolConfig = _machineConfig.SymbolConfig;
		_reelConfig = _machineConfig.ReelConfig;
		_payoutConfig = _machineConfig.PayoutConfig;
		_luckyPayoutConfig = _machineConfig.LuckyPayoutConfig;
		_nearHitConfig = _machineConfig.NearHitConfig;
		_luckyNearHitConfig = _machineConfig.LuckyNearHitConfig;

		_curPayoutConfig = _payoutConfig;
		_curNearHitConfig = _nearHitConfig;

		_basicReelCount = _basicConfig.BasicReelCount;
		_totalReelCount = _basicConfig.ReelCount;

		_spinRoller = _coreMachine.Roller;

		InitForceWinManager();
		InitSpinRollHelpers();
		InitWinRollHelpers();
		InitNearHitRollHelpers();

		#if DEBUG
		InitDebugHugeWinRollHelpers();
		#endif

		InitShortLuckys();

		InitPayoutGenerators();

		InitUnorderSymbolDict ();
	}

	private void InitForceWinManager()
	{
		_forceWinManager = new CoreForceWinManager(_machineConfig, _coreMachine);
	}

	private void InitUnorderSymbolDict(){
		List<string> unorderNameList = new List<string> ();
		PayoutData[] datas = _payoutConfig.Sheet.dataArray;

		ListUtility.ForEach (datas, (PayoutData data) => {
			if (data.PayoutType == PayoutType.UnOrdered){
				if (!unorderNameList.Contains (data.Symbols [0])) {
					unorderNameList.Add (data.Symbols[0]);
				}
			}
		});

		for (int i = 0; i < _basicConfig.ReelCount; ++i) {
			Dictionary<string , Dictionary<int, List<int>>> allDict = new Dictionary<string, Dictionary<int, List<int>>>();
			ListUtility.ForEach (unorderNameList, (string symbolName) => {
				Dictionary<int, List<int>> dict = _reelConfig.GetSingleReel (i).ConstructUnorderSymbolCountDict(symbolName);
				allDict.Add(symbolName, dict);
			});
			_unorderSymbolDictList.Add (allDict);
		}
	}

	private void InitSpinRollHelpers()
	{
		float winProb = _payoutConfig.TotalProb;
		float nearHitProb = _nearHitConfig.TotalProb;
		float lossProb = 1.0f - winProb - nearHitProb;

		float[] probArray = new float[]{winProb, nearHitProb, lossProb};
		CoreDebugUtility.Log ("winProb nearHitProb lossProb is "+winProb + " " + nearHitProb + " " +lossProb);
		_spinRollHelper = new RollHelper(probArray);

		winProb = _luckyPayoutConfig.TotalProb;
		nearHitProb = _luckyNearHitConfig.TotalProb;
		lossProb = 1.0f - winProb - nearHitProb;

		float[] luckyProbArray = new float[]{winProb, nearHitProb, lossProb};
		CoreDebugUtility.Log ("lucky winProb nearHitProb lossProb is "+winProb + " " + nearHitProb + " " +lossProb);
		_luckySpinRollHelper = new RollHelper(luckyProbArray);

		_curSpinRollHelper = _spinRollHelper;
	}

	private void InitWinRollHelpers()
	{
		_winRollHelper = GenRollHelperFromJoy(_payoutConfig);
		_luckyWinRollHelper = GenRollHelperFromJoy(_luckyPayoutConfig);

		_curWinRollHelper = _winRollHelper;
	}

	private void InitNearHitRollHelpers()
	{
		_nearHitRollHelper = GenRollHelperFromJoy(_nearHitConfig);
		_luckyNearHitRollHelper = GenRollHelperFromJoy(_luckyNearHitConfig);

		_curNearHitRollHelper = _nearHitRollHelper;
	}

	private RollHelper GenRollHelperFromJoy(BaseJoyConfig joyConfig)
	{
		return new RollHelper(joyConfig.OverallHitArray);
	}

	private void InitShortLuckys()
	{
		//1 _shortLuckyPayoutDatas and probList
		_shortLuckyPayoutDatas = new List<PayoutData>();
		List<float> probList = new List<float>();
		PayoutData[] dataArray = _luckyPayoutConfig.Sheet.dataArray;
		for(int i = 0; i < dataArray.Length; i++)
		{
			if(dataArray[i].IsShortLucky)
			{
				_shortLuckyPayoutDatas.Add(dataArray[i]);
				probList.Add(dataArray[i].OverallHit);
			}
		}

		//2 _shortLuckyWinRollHelper
		_shortLuckyWinRollHelper = new RollHelper(probList);
		_shortLuckyWinRollHelper.NormalizeProbs();
	}

	private void InitPayoutGenerators()
	{
		// In single line machine, these PayoutTypes are not supported:
		// Continuous, Start,
		_payoutGenerators[(int)PayoutType.Ordered] = PayoutOrderedHandler;
		_payoutGenerators[(int)PayoutType.All] = PayoutAllHandler;
		_payoutGenerators[(int)PayoutType.Any] = PayoutAnyHandler;
		_payoutGenerators[(int)PayoutType.UnOrdered] = PayoutUnorderedHandler;
	}

	#endregion

	#region Roll

	public CoreSpinResult Roll(CoreSpinInput spinInput)
	{
		RefreshLuckyMode(spinInput.BetAmount);
		CoreSpinResult spinResult = RollSpinResult(spinInput);

		#if UNITY_EDITOR
		CoreCheckResult checkResult = _checker.CheckResultWithSymbols(spinResult.JoyData, spinResult.SymbolList);
		spinResult.CompareToCheckResult(checkResult);
		#endif

		return spinResult;
	}

	private CoreSpinResult RollSpinResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = null;
		CoreLuckyMode mode = _luckyManager.Mode;
		if(mode == CoreLuckyMode.Normal || mode == CoreLuckyMode.LongLucky)
		{
			if (spinInput.FreeSpinData != null) {
				result = RollFreeSpinResult (spinInput);
			} else if (spinInput.FixWildSpinData != null) {
				result = RollFixWildResult (spinInput);
			} else if (_machineConfig.BasicConfig.IsTriggerType(TriggerType.Collect)){
				result = RollCollectSpinResult (spinInput);
			} else {
				result = RollGeneralSpinResult (spinInput);
			}
		}
		else if(mode == CoreLuckyMode.ShortLucky)
		{
			result = RollShortLuckySpinResult(spinInput);
		}
		else
		{
			CoreDebugUtility.Assert(false);
		}

		result.FeedBetAndWinAmount(spinInput.BetAmount);
		result.FeedLuckyMode(_luckyManager.Mode);
		result.RefreshWinSymbolFlagsList();
		_forceWinManager.FeedSpinResult(result);

		return result;
	}

	// Note by nichos:
	// The jackpot roll algorithm is tightly coupled with the general roll algorithm
	// To make the code better, the two algorithms should be completely in two functions and decoupled
	// We could refactor this in the future
	// Besides, the Force Win algorithm can also be extracted to another function
	private CoreSpinResult RollGeneralSpinResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = null;
		float dice = _spinRoller.NextFloat();

		//1 handle pay roll
		RollHelper rollHelper = GetHitRateIncreaseAfterPayRollHelper (_curSpinRollHelper, dice, spinInput);

		//2 handle debug HugeWin mode
		#if DEBUG
		rollHelper = GetDebugSpinRollHelperForHugeWinMode(rollHelper);
		#endif

		int spinIndex = rollHelper.FetchIndex(dice);

		// check if cmd is on to force win
		#if !CORE_DLL
		spinIndex = CmdLineManager.Instance.ForceWinResultType (spinIndex);
		#endif

		// jackpot中奖判断逻辑
		string jackpotType = "";
		// 这里的dice不能用hitRate，因为我们不希望jackpot的概率受到付费保护机制影响
		bool isJackpotWin = JackpotWinManager.Instance.CheckJackpotWin(_spinRoller, ref jackpotType, _coreMachine.Name, spinInput);
		if(isJackpotWin)
			spinIndex = (int)SpinResultType.Win;

		// force win
		if(!isJackpotWin)
		{
			if(_forceWinManager.ShouldForceWin())
				spinIndex = (int)SpinResultType.Win;
		}

		switch((SpinResultType)spinIndex)
		{
			case SpinResultType.Win:
				{
					#if DEBUG
					RollHelper winHelper = GetDebugWinRollHelperForHugeWinMode(_curWinRollHelper);
					#else
					RollHelper winHelper = _curWinRollHelper;
					#endif

					float winRatio = _spinRoller.NextFloat(0.0f, winHelper.GetTotalProb());
					int index = winHelper.FetchIndex(winRatio);

					// jackpot中奖data判断
					int jackpotIndex = JackpotWinManager.Instance.GetJackpotPayoutData (jackpotType, _curPayoutConfig.Sheet.dataArray, spinInput);
					if(jackpotIndex >= 0)
						index = jackpotIndex;

					// force win
					if(jackpotIndex < 0)
					{
						if(_forceWinManager.ShouldForceWin())
						{
							index = _forceWinManager.ForceWinPayoutIndex;
							_forceWinManager.ClearForceWinFlag();
						}
					}

					#if !CORE_DLL
					index = CmdLineManager.Instance.ForceWin (this, index);
					if (CmdLineManager.Instance.EnableForceWin) {
						CoreDebugUtility.Log("<color=red>force win index : </color>" + index);
					}
					#endif

					CoreDebugUtility.Assert(index >= 0, "Random index should >= 0");
					PayoutData payoutData = _curPayoutConfig.Sheet.dataArray[index];
					// CoreDebugUtility.Log("Construct payoutdata id = "+payoutData.Id);
					result = ConstructWinResult(spinInput, payoutData);
				}
				break;

			case SpinResultType.NearHit:
				{
					float nearHitRatio = _spinRoller.NextFloat(0.0f, _curNearHitRollHelper.GetTotalProb());
					int index = _curNearHitRollHelper.FetchIndex(nearHitRatio);

					#if !CORE_DLL
					index = CmdLineManager.Instance.ForceWin (this, index);
					if (CmdLineManager.Instance.EnableForceWin) {
						CoreDebugUtility.Log("<color=red>force win index : </color>" + index);
					}
					#endif

					CoreDebugUtility.Assert(index >= 0, "Random index should >= 0");
					NearHitData nearHitData = _curNearHitConfig.Sheet.dataArray[index];
					result = ConstructNearHitResult(spinInput, nearHitData);
				}
				break;

			case SpinResultType.Loss:
				result = ConstructLossResult(spinInput);
				break;

			default:
				CoreDebugUtility.LogError("spinIndex is rolled out of range");
				break;
		}

		return result;
	}

	private CoreSpinResult RollFreeSpinResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = null;
		bool isWin = false;
		bool isStopByBonusWild = false;
		CoreFreeSpinData freeSpinData = spinInput.FreeSpinData;
		FreeSpinType type = _machineConfig.BasicConfig.FreeSpinType;
		#if false // 不需要用幸运模板
		if (_machineConfig.BasicConfig.IsFreeSpinLuckyPayout) {
			RefreshLuckyMode (spinInput.BetAmount, true, CoreLuckyMode.LongLucky);
		}
		#endif

		//1 roll stop or not
		if(type == FreeSpinType.ReachBonusWild)
		{
			float[] freeSpinStopProbs = freeSpinData.FreeSpinStopProbs;
			int respinIndex = (freeSpinData.RespinCount < freeSpinStopProbs.Length) ? freeSpinData.RespinCount : freeSpinStopProbs.Length - 1;
			float stopProb = freeSpinStopProbs[respinIndex];
			RollHelper stopHelper = new RollHelper(stopProb);
			int rollStopIndex = stopHelper.RollIndex(_spinRoller);
			isStopByBonusWild = rollStopIndex == 0;

			int stopCount = _machineConfig.BasicConfig.FreeSpinStopCountOfBonusWild;
			string[] stopSymbols = _machineConfig.BasicConfig.FreeSpinStopSymbolNames;
			CoreDebugUtility.Assert(stopSymbols != null, "FreeSpinStopSymbolName = null");
			CoreDebugUtility.Assert(stopCount >= 1);
			if(isStopByBonusWild)
				spinInput.SymbolRestriction = new CoreSpinSymbolRestriction(stopSymbols, CompareType.GreaterEqual, stopCount);
			else
				spinInput.SymbolRestriction = new CoreSpinSymbolRestriction(stopSymbols, CompareType.LessEqual, stopCount - 1);
		}

		//2 roll win or not
		isWin = freeSpinData.RollFreeSpinWin(_spinRoller);

		// 强制中奖
		if (type == FreeSpinType.FixCount) {
			if (freeSpinData.IsCurrentForceWinIndex ()) {
				isWin = true;
			}
		}

		//3 roll payout
		if(isWin)
		{
			float[] overallHitArray = null;
			if(isStopByBonusWild)
				overallHitArray = _curPayoutConfig.FreeSpinStopOverallHitArray;
			else
				overallHitArray = _curPayoutConfig.FreeSpinOverallHitArray;
			
			RollHelper winHelper = new RollHelper(overallHitArray);
			winHelper.NormalizeProbs();
			int payoutIndex = winHelper.RollIndex(_spinRoller);
			PayoutData payoutData = _curPayoutConfig.Sheet.dataArray[payoutIndex];
			result = ConstructWinResult(spinInput, payoutData);
		}
		else
		{
			RollHelper nearHelper = new RollHelper(_curNearHitConfig.OverallHitArray);
			nearHelper.NormalizeProbs();
			int nearIndex = nearHelper.RollIndex(_spinRoller);
			NearHitData nearHitData = _curNearHitConfig.Sheet.dataArray[nearIndex];
			result = ConstructNearHitResult(spinInput, nearHitData);
		}

		return result;
	}

	// fixwild玩法
	private CoreSpinResult RollFixWildResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = null;

		//1 roll win or not
		CoreFixWildSpinData fixWildData = spinInput.FixWildSpinData;
		
		#if false
		float dice = _spinRoller.NextFloat();
		int spinIndex = _curSpinRollHelper.FetchIndex(dice);
		bool isWin = (spinIndex == 0);
		#else
		float hit;
		if (fixWildData.FixReelCount == 1) {
			hit = fixWildData.FixHit[0];
		} else  {
			hit = fixWildData.FixHit[1];
		}
		RollHelper spinHelper = new RollHelper(hit);
		int rollIndex = spinHelper.RollIndex(_spinRoller);
		bool isWin = rollIndex == 0;
		#endif

		//2 roll payout
		if(isWin)
		{
			RollHelper winHelper = null;
			if (fixWildData.FixReelCount == 1) {
				winHelper = new RollHelper (_curPayoutConfig.Fix1ReelOverallHitArray);
			} else  {
				winHelper = new RollHelper (_curPayoutConfig.Fix2ReelOverallHitArray);
			}
			winHelper.NormalizeProbs();
			int payoutIndex = winHelper.RollIndex(_spinRoller);
//			LogUtility.Log ("fix wild win fixcount = "+fixWildData.FixReelCount+" payoutIndex = "+payoutIndex, Color.magenta);
			PayoutData payoutData = _curPayoutConfig.Sheet.dataArray[payoutIndex];
			result = ConstructWinResult(spinInput, payoutData);
		}
		else
		{
			RollHelper nearHelper = null;
			if (fixWildData.FixReelCount == 1) {
				nearHelper = new RollHelper(_curNearHitConfig.Fix1ReelOverallHitArray);
			} else{
				nearHelper = new RollHelper(_curNearHitConfig.Fix2ReelOverallHitArray);
			} 
			nearHelper.NormalizeProbs();
			int nearIndex = nearHelper.RollIndex(_spinRoller);
//			LogUtility.Log ("fix wild nearhit fixcount = "+fixWildData.FixReelCount+" nearIndex = "+nearIndex, Color.magenta);
			NearHitData nearHitData = _curNearHitConfig.Sheet.dataArray[nearIndex];
			result = ConstructNearHitResult(spinInput, nearHitData);
		}

		return result;
	}

	// 收集玩法
	private string RandomGetCollectionName()
	{
		// 挑选一个随机收集物
		int collectTypeCount = _machineConfig.BasicConfig.CollectSymbol.Length;
		int collectIndex = RandomUtility.RollInt (_spinRoller, collectTypeCount);
		string collectName = _machineConfig.BasicConfig.CollectSymbol [collectIndex];

		return collectName;
	}

	private void RandomAttachCollect(CoreSymbol symbol, string name, CoreSpinResult result)
	{
	// 检查是否可以ATTACH
		if (!_symbolConfig.CheckSymbolCanApplyCollect (symbol.SymbolData.Name, name)) {
			return;
		}

		CoreLuckyMode mode = _luckyManager.Mode;
		float winRatio = 0.0f;
		if (mode == CoreLuckyMode.Normal) {
			winRatio = _machineConfig.BasicConfig.NormalCollectRate;
		} else {
			winRatio = _machineConfig.BasicConfig.LuckyCollectRate;
		}
		RollHelper winhelper = new RollHelper (winRatio);
		int rollIndex = winhelper.RollIndex (_spinRoller);
		bool canCollect = rollIndex == 0;
		if (canCollect) {
			CoreCollectData data = new CoreCollectData (symbol.ReelIndex, symbol.StopIndex);
//			LogUtility.Log ("attach collect reelindex = "+symbol.ReelIndex+" stopindex = "+symbol.StopIndex, Color.red);
			data.AttachCollectSymbol (name);
			result.CollectDataList.Add (data);
		}
	}

	private CoreSpinResult RollCollectSpinResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = RollGeneralSpinResult (spinInput);

		return result;
	}

	private void FillRandomCollect(CoreSpinResult result){

		// 计算支付线上的symbol添加概率
		int randomNum = RandomUtility.RollInt (_spinRoller, _totalReelCount+1);
		if (randomNum != 0) {
			List<int> hasCollectSymbolIndexArray = RandomUtility.RollIntList (_spinRoller, _totalReelCount, randomNum);
			for (int i = 0; i < hasCollectSymbolIndexArray.Count; ++i) {
				RandomAttachCollect (result.SymbolList [hasCollectSymbolIndexArray [i]], RandomGetCollectionName (), result);
			}
		}

		// 获得非支付线上symnbol
		List<CoreSymbol> nonPayoutLineSymbols = result.NonPayoutLineSymbolList;

		// 目前只取一个
		List<int> nonPayoutLineHasCollectList = RandomUtility.RollIntList (_spinRoller, nonPayoutLineSymbols.Count, 1);
		for (int i = 0; i < nonPayoutLineHasCollectList.Count; ++i) {
			RandomAttachCollect (nonPayoutLineSymbols [nonPayoutLineHasCollectList [i]], RandomGetCollectionName (), result);
		}
	}

	private bool canFillCollect(){
		bool trigger = _machineConfig.BasicConfig.IsTriggerType (TriggerType.Collect);
		bool notInFreeSpin = _coreMachine.SmallGameState != SmallGameState.FreeSpin;
		return trigger && notInFreeSpin;
	}

	private CoreSpinResult RollShortLuckySpinResult(CoreSpinInput spinInput)
	{
		int index = _shortLuckyWinRollHelper.RollIndex(_spinRoller);
		CoreDebugUtility.Assert(index >= 0 && index < _shortLuckyPayoutDatas.Count);
		PayoutData data = _shortLuckyPayoutDatas[index];
		CoreSpinResult result = ConstructWinResult(spinInput, data);
		return result;
	}

	#endregion

	#region Win

	private bool IsSpinResultCanNotBeAny(IList<CoreSymbol> symbolList, IJoyData data)
	{
		bool result = false;
		int fixSymbolNum = ListUtility.CountElements(symbolList, (CoreSymbol s) => {
			return s != null;
		});
		if(fixSymbolNum >= 2 && data.PayoutType == PayoutType.Any)
		{
			bool isAllWild = ListUtility.IsAllElementsSatisfied(symbolList, (CoreSymbol s) => {
                if (s != null)
					return _symbolConfig.IsMatchSymbolWildType(s.SymbolData.Name);
                else
                    return false;
			});
			result = !isAllWild;
		}
		return result;
	}

	private int[] GenerateStopIndexesForCanNotAny(IList<CoreSymbol> fixedSymbols, IJoyData data){
		int[] stopIndexes = new int[_totalReelCount];
		for(int i = 0; i < _totalReelCount; ++i){
			if (fixedSymbols[i] != null) {
				stopIndexes [i] = fixedSymbols[i].StopIndex;
			} else {
				stopIndexes [i] = CoreDefine.InvalidIndex;
			}
		}

		for (int i = 0; i < _totalReelCount; ++i) {
			if (stopIndexes [i] == CoreDefine.InvalidIndex) {
				int reelIndex = i;
				SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
				List<string> optionNameList = ListUtility.IntersectList(reel.SymbolNameList, data.Symbols);

				string rolledName = RandomUtility.RollSingleElement(_spinRoller, optionNameList);
				stopIndexes[reelIndex] = RollStopIndex(reelIndex, rolledName);
			}
		}

		return stopIndexes;
	}

	private CoreSpinResult ConstructWinResult(CoreSpinInput spinInput, PayoutData data)
	{
		CoreSpinResult result = new CoreSpinResult(SpinResultType.Win, _coreMachine, spinInput);
		result.SetPayoutData(data);
		result.FillFixedSymbols(spinInput.FixedSymbols);
		ApplySymbolRestrictionToSpinResult(result, spinInput.SymbolRestriction);

		PayoutGeneratorDelegate generator = _payoutGenerators[(int)data.PayoutType];
		CoreDebugUtility.Assert(generator != null);

		int[] stopIndexes;
		List<CoreSymbol> fixedSymbols = new List<CoreSymbol>(result.SymbolList);
		bool isCanNotBeAny = IsSpinResultCanNotBeAny(fixedSymbols, data);
		if (isCanNotBeAny) {
			// 无法handle的情况下，从之前的结果里直接提取stopindexes
			stopIndexes = GenerateStopIndexesForCanNotAny(fixedSymbols, data);
		} else {
			stopIndexes = generator (fixedSymbols, _curPayoutConfig, data, SpinResultType.Win, result.MatchFlags, spinInput.SymbolRestriction);
		}
		stopIndexes = GenerateTotalStopIndexes(_curPayoutConfig, data, stopIndexes, result.MatchFlags);
		result.SetStopIndexes(stopIndexes);
		RollSlideInfoInResult(result, data);
		result.FillWinRatio();
		result.FillSpecialMultiplier (_coreMachine);
		result.FillNonPayoutLineSymbolList();
		result.FillFinalCoreSymbol (_coreMachine);
		result.SetJackpotWin (data, spinInput);
		if (canFillCollect()) {
			FillRandomCollect (result);
		}
		return result;
	}

	private int[] PayoutOrderedHandler(IList<CoreSymbol> fixedSymbols, BaseJoyConfig baseJoyConfig, IJoyData data, SpinResultType type, bool[] matchFlags, CoreSpinSymbolRestriction restriction)
	{
		int[] stopIndexes = new int[_basicReelCount];
		for(int i = 0; i < _basicReelCount; i++)
		{
			if(fixedSymbols[i] != null)
				stopIndexes[i] = fixedSymbols[i].StopIndex;
			else
				stopIndexes[i] = RollStopIndex(i, data.Symbols[i]);
			matchFlags[i] = true;
		}
		return stopIndexes;
	}

	private int[] PayoutAllHandler(IList<CoreSymbol> fixedSymbols, BaseJoyConfig baseJoyConfig, IJoyData data, SpinResultType type, bool[] matchFlags, CoreSpinSymbolRestriction restriction)
	{
		#if UNITY_EDITOR
		CoreDebugUtility.Assert(data.Count == _basicReelCount, "For PayoutType.All, count must be equal to reel count");
		#endif

		//1 fill all symbols
		int[] stopIndexes = new int[_basicReelCount];
		for(int i = 0; i < _basicReelCount; i++)
		{
			if(fixedSymbols[i] != null)
				stopIndexes[i] = fixedSymbols[i].StopIndex;
			else
				stopIndexes[i] = RollStopIndex(i, data.Symbols[0]);
			matchFlags[i] = true;
		}

		//2 change some to wild
		if(baseJoyConfig.IsPureSymbols(data, CoreDefine.SevenBarTypes))
		{
			ReelsWildConfig wildConfig = baseJoyConfig.GetReelsWildConfig(data);
			List<int> wildIndexList = ListUtility.CreateIntList(_basicReelCount);
			RollWilds(stopIndexes, wildConfig, data, type, fixedSymbols, wildIndexList, restriction);
		}

		return stopIndexes;
	}

	// 以jackpot总点数来构建结果的逻辑
	#region jackpotCount
	private int[] FillAnyCountSymbolsWithJackpotCount(IJoyData data, int fixedAnyCount, SpinResultType type, IList<int> optionReelIndexList, int[] stopIndexes, bool[] matchFlags, IList<int> wildIndexList){
		PayoutData payoutData = data as PayoutData;
		int leftJackpotCount = payoutData.JackpotCount;
		int jackpotSingleMax = _symbolConfig.JackpotCountMax;// 单个symbol里面最高的jackpotCount数

		// 这里逻辑copy自payoutanyhandler
		int anyCount = data.Count;
		int leftAnyCount = anyCount - fixedAnyCount;
		List<int> reelIndexList = RandomUtility.RollList(_spinRoller, optionReelIndexList, leftAnyCount);
		for(int i = 0; i < reelIndexList.Count; i++)
		{
			int reelIndex = reelIndexList[i];
			SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
			List<string> optionNameList = ListUtility.IntersectList(reel.SymbolNameList, data.Symbols);

			//when roll nearHit in slide level, avoid slide symbols
			if(type == SpinResultType.NearHit && _basicConfig.HasSlide)
			{
				List<string> slideSymbols = _symbolConfig.GetSlideSymbolNames();
				optionNameList = ListUtility.SubtractList(optionNameList, slideSymbols);
			}

			stopIndexes[reelIndex] = RollSingleStopIndex(stopIndexes, reelIndex, optionNameList, data);
			CoreDebugUtility.Log("reelindex = "+reelIndex+" stopIndex = "+stopIndexes[reelIndex]);
			if(stopIndexes[reelIndex] == CoreDefine.InvalidIndex)
			{
				CoreDebugUtility.Log ("stop indexs is "+stopIndexes[0] +" "+stopIndexes[1]+" "+stopIndexes[2]);
				for (int m = 0; m < data.Symbols.Length; ++m){
					CoreDebugUtility.Log ("datasymbols is "+data.Symbols[m]);
				}

				CoreDebugUtility.LogError("RollSingleStopIndex returns -1");
				CoreDebugUtility.Assert(false);
			}

			// 对应jackpot count的逻辑
			int leftReelNum = reelIndexList.Count - i - 1;// 剩余轴数

			// 获得当前symbol的jackpot count数
			string symbolName = reel.GetSymbolName(stopIndexes[reelIndex]);
			SymbolData symbolData = _symbolConfig.GetSymbolData(symbolName);
			int jackpotCount = symbolData.JackpotCount;

			bool checkJackpotCountValid = false;
			while(!checkJackpotCountValid && optionNameList.Count > 0){
				int leftCount = leftJackpotCount - jackpotCount;// 除去这一次的jackpotcount后的剩余点数
				bool condition1 = (i == reelIndexList.Count - 1 && leftJackpotCount == jackpotCount);// 最后一轴，并且正好转到相应的jackpotCount
				bool condition2 = (i < reelIndexList.Count - 1 // 非最后一轴
					&& jackpotCount < leftJackpotCount  // 转到的jackpotCount比剩余的要小
					&& jackpotSingleMax * leftReelNum >= leftCount // 剩下的轴足够转出剩余点数
					&& leftCount >= leftReelNum);// 剩下的点数不能比轴数小

				if (condition1 || condition2){
					checkJackpotCountValid = true;
					leftJackpotCount -= jackpotCount;
				}else{
					optionNameList.Remove(symbolName);// 去掉之前的name, 重新roll
					stopIndexes[reelIndex] = RollSingleStopIndex(stopIndexes, reelIndex, optionNameList, data);
					CoreDebugUtility.Assert(stopIndexes[reelIndex] != CoreDefine.InvalidIndex);
					symbolName = reel.GetSymbolName(stopIndexes[reelIndex]);
					symbolData = _symbolConfig.GetSymbolData(symbolName);
					jackpotCount = symbolData.JackpotCount;
				}
			}
		
			matchFlags[reelIndex] = true;
			wildIndexList.Add(reelIndex);
		}

		return stopIndexes;
	}

	private bool CheckJackpotCount(IJoyData data){
		PayoutData payoutData = data as PayoutData;
		if (payoutData == null){
			return false;
		}
		return payoutData.JackpotCount > 0;
	}

	#endregion

	private int[] FillAnyCountSymbols(IJoyData data, int fixedAnyCount, SpinResultType type, IList<int> optionReelIndexList, int[] stopIndexes, bool[] matchFlags, IList<int> wildIndexList){
		//3 fill anyCount symbols
		int anyCount = data.Count;
		int leftAnyCount = anyCount - fixedAnyCount;
		List<int> reelIndexList = RandomUtility.RollList(_spinRoller, optionReelIndexList, leftAnyCount);
		for(int i = 0; i < reelIndexList.Count; i++)
		{
			int reelIndex = reelIndexList[i];
			SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
			List<string> optionNameList = ListUtility.IntersectList(reel.SymbolNameList, data.Symbols);

			//when roll nearHit in slide level, avoid slide symbols
			if(type == SpinResultType.NearHit && _basicConfig.HasSlide)
			{
				List<string> slideSymbols = _symbolConfig.GetSlideSymbolNames();
				optionNameList = ListUtility.SubtractList(optionNameList, slideSymbols);
			}

			stopIndexes[reelIndex] = RollSingleStopIndex(stopIndexes, reelIndex, optionNameList, data);
			if(stopIndexes[reelIndex] == CoreDefine.InvalidIndex)
			{
				CoreDebugUtility.Log ("stop indexs is "+stopIndexes[0] +" "+stopIndexes[1]+" "+stopIndexes[2]);
				for (int m = 0; m < data.Symbols.Length; ++m){
					CoreDebugUtility.Log ("datasymbols is "+data.Symbols[m]);
				}

				CoreDebugUtility.LogError("RollSingleStopIndex returns -1");
				CoreDebugUtility.Assert(false);
			}

			matchFlags[reelIndex] = true;

			wildIndexList.Add(reelIndex);
		}

		return stopIndexes;
	}

	private int[] PayoutAnyHandler(IList<CoreSymbol> fixedSymbols, BaseJoyConfig baseJoyConfig, IJoyData data, SpinResultType type, bool[] matchFlags, CoreSpinSymbolRestriction restriction)
	{
		int[] stopIndexes = new int[_basicReelCount];
		ListUtility.FillElements(stopIndexes, CoreDefine.InvalidIndex);
		List<int> wildIndexList = new List<int>(); //the indexes which can be rolled wild

		//1 find the reels which have particular symbols
		List<int> optionReelIndexList = new List<int>();
		for(int i = 0; i < _basicReelCount; i++)
		{
			SingleReel reel = _reelConfig.GetSingleReel(i);
			if(ListUtility.IsContainAnyElements(reel.SymbolNameList, data.Symbols))
				optionReelIndexList.Add(i);
		}

		//2 handle fixedSymbols
		int fixedAnyCount = 0;
		for(int i = 0; i < fixedSymbols.Count; i++)
		{
			if(fixedSymbols[i] != null)
			{
				stopIndexes[i] = fixedSymbols[i].StopIndex;
				optionReelIndexList.Remove(i);
				if(data.Symbols.Contains(fixedSymbols[i].SymbolData.Name))
					fixedAnyCount++;

				matchFlags[i] = true;
			}
		}

		//3 fill anyCount symbols
		int anyCount = data.Count;
		int leftAnyCount = anyCount - fixedAnyCount;

		bool isJackpotCount = CheckJackpotCount(data);
		if (isJackpotCount){
			stopIndexes = FillAnyCountSymbolsWithJackpotCount(data, fixedAnyCount, type, optionReelIndexList, stopIndexes, matchFlags, wildIndexList);
		}else{
			stopIndexes = FillAnyCountSymbols(data, fixedAnyCount, type, optionReelIndexList, stopIndexes, matchFlags, wildIndexList);
		}

		//4 fill empty symbols
		if(anyCount < _basicReelCount)
		{
			List<string> missAvoidList = new List<string>(data.Symbols);
			for(int i = 0; i < _basicReelCount; i++)
			{
				int reelIndex = i;
				if(stopIndexes[reelIndex] == CoreDefine.InvalidIndex)
				{
					SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
					List<string> optionNameList = new List<string>(reel.NonWildNameList);
					optionNameList = ListUtility.SubtractList(optionNameList, missAvoidList);

					stopIndexes[reelIndex] = RollSingleStopIndex(stopIndexes, reelIndex, optionNameList, data);
					if(stopIndexes[reelIndex] == CoreDefine.InvalidIndex)
					{
						CoreDebugUtility.LogError("RollSingleStopIndex returns -1");
						CoreDebugUtility.Assert(false);
					}
				}
			}
		}

		//5 change some to wild
//		if(CanRollWild(baseJoyConfig, data))
		if(true)
		{
			ReelsWildConfig wildConfig = baseJoyConfig.GetReelsWildConfig(data);
			//maxWildCount is only 1, otherwise, the combination constitutes a pure wild payout
			RollWilds(stopIndexes, wildConfig, data, type, fixedSymbols, wildIndexList, restriction);
		}

		return stopIndexes;
	}

	private int[] PayoutUnorderedHandler(IList<CoreSymbol> fixedSymbols, BaseJoyConfig baseJoyConfig, IJoyData data, SpinResultType type, bool[] matchFlags, CoreSpinSymbolRestriction restriction)
	{
//		LogUtility.Log ("payout unordered handler", Color.yellow);

		//1 fill all symbols
		int[] stopIndexes = new int[_basicReelCount];
		int leftCount = data.Count;
		string symbolName = data.Symbols [0];

		// 每个轴能够构建的symbol数容器
		List<List<int>> canConstructSymbolNumList = new List<List<int>> ();

		ListUtility.ForEach(_unorderSymbolDictList, (Dictionary<string, Dictionary<int, List<int>>> alldict)=>{
			List<int> indexes = new List<int>();
			if (alldict.ContainsKey(symbolName)){
				foreach(KeyValuePair<int, List<int>> pair in alldict[symbolName]){
					indexes.Add(pair.Key);
				}
			}
			canConstructSymbolNumList.Add(indexes);
		});

		// 将来需要用的 SingleReel
		SingleReel[] singleReels = new SingleReel[_basicReelCount];

		for(int i = 0; i < _basicReelCount; i++)
		{
			if (fixedSymbols [i] != null) {
				stopIndexes [i] = fixedSymbols [i].StopIndex;
			} else {
				stopIndexes [i] = -1;
			}
			matchFlags[i] = true;
		}

		// 非固定轴的索引
		List<int> leftReelIndexList = ListUtility.IndexList(stopIndexes, (int i)=>{
			return i == -1;
		});

		// 能够roll出指定symbol的轴索引
		List<int> canRollSymbolReelIndexList = ListUtility.FilterList (leftReelIndexList, (int i) => {
			return canConstructSymbolNumList[i].Contains(1);
		});

		// 无法roll出指定symbol的轴索引
		List<int> cannotRollSymbolReelIndexList = ListUtility.SubtractList (leftReelIndexList, canRollSymbolReelIndexList);

		// 每个轴规定需要roll出的symbol数
		List<int> rollSymbolNum = ListUtility.CreateIntList(-1, 0, _basicReelCount);

		FillUnorderedRollSymbolNum (canRollSymbolReelIndexList, leftCount, rollSymbolNum, canConstructSymbolNumList);

		for(int i = 0; i < rollSymbolNum.Count; ++i){
			if (rollSymbolNum[i] != -1){
				stopIndexes [i] = RandomUtility.RollSingleElement (_spinRoller, _unorderSymbolDictList [i] [symbolName][rollSymbolNum [i]]);
			}
		}

		// 如果leftCount达成，强制把剩余可roll轴设置为roll出非symbol
		for (int i = 0; i < canRollSymbolReelIndexList.Count; ++i) {
			SingleReel reel = _reelConfig.GetSingleReel(canRollSymbolReelIndexList [i]);
			int stopindex = RollSuitableStopIndexInUnorderMode(_spinRoller, _unorderSymbolDictList [canRollSymbolReelIndexList [i]] [symbolName] [0], reel);
			stopIndexes[canRollSymbolReelIndexList[i]] = stopindex;
			// CoreDebugUtility.Log("canRollSymbolReelIndexlist  reelIndex = "+canRollSymbolReelIndexList[i] + " stopIndex = "+stopindex);
		}

		// 不可roll的轴随机roll出非symbol
		for (int i = 0; i < cannotRollSymbolReelIndexList.Count; ++i) {
			SingleReel reel = _reelConfig.GetSingleReel(cannotRollSymbolReelIndexList [i]);
			int stopindex = RollSuitableStopIndexInUnorderMode(_spinRoller, _unorderSymbolDictList [cannotRollSymbolReelIndexList [i]] [symbolName] [0], reel);
			stopIndexes[cannotRollSymbolReelIndexList[i]] = stopindex;
			// CoreDebugUtility.Log("cannotRollSymbolReelIndexList  reelIndex = "+cannotRollSymbolReelIndexList[i] + " stopIndex = "+stopindex);
		}

		return stopIndexes;
	}

	private int RollSuitableStopIndexInUnorderMode(IRandomGenerator generator, List<int> list, SingleReel reel){
		List<int> indexList = new List<int>(list);
		int index = CoreDefine.InvalidIndex;
		while (indexList.Count > 0){
			index = RandomUtility.RollSingleElement(generator, indexList);
			SymbolType type = reel.GetSymbolType(index);
			if (!type.Equals(SymbolType.BonusWild)){ // unorder下中间不能出bonuswild
				return index;
			}else{
				indexList.Remove(index);
			}
		}

		return index;
	}

	// canrollsybollist 能够转出symbol的轴列表
	// leftcount 剩余需要转得symbol数
	// rollSymbolNum 输出结果，表示每个轴期待转得symbol数
	// symbolRandomList 表示每个轴能够转出symbol的个数集合(0, 1, 2, 3等， 从小到大排列）
	private void FillUnorderedRollSymbolNum (List<int> canrollsymbollist, int leftcount, List<int> rollSymbolNum, List<List<int>> symbolRandomList){
		if (leftcount == 0) {
//			LogUtility.Log ("Fill Unordered Roll symbol num is zero", Color.red);
			return;
		}

		if (canrollsymbollist.Count == 0)
		{
			// 这里如果leftcount 还有，但是轴却已经没了，那就是有错误
			CoreDebugUtility.Assert(false, "can roll symbol list count is zero");
			return ;
		}
	
		int randomReelIndex = -1;
		int randomRollSymbolNum = -1;

		if (canrollsymbollist.Count == 1){
			// 最后一根轴
			randomReelIndex = canrollsymbollist[0];
//			LogUtility.Log ("can roll symbol list count is 1 random index is " + randomindex + " leftcount is " + leftcount, Color.red);
			rollSymbolNum[randomReelIndex] = leftcount;
			canrollsymbollist.Remove(randomReelIndex);
			return;
		}

		if (canrollsymbollist.Count > leftcount) {
			randomReelIndex = RandomUtility.RollSingleElement (_spinRoller, canrollsymbollist);
			do {
				randomRollSymbolNum = RandomUtility.RollSingleElement (_spinRoller, symbolRandomList [randomReelIndex]);
			} while(randomRollSymbolNum > leftcount);

		} else if (canrollsymbollist.Count == leftcount) {
			randomReelIndex = RandomUtility.RollSingleElement (_spinRoller, canrollsymbollist);
			// 强制每根轴都转一个bonus
			randomRollSymbolNum = 1;
		} else {// 假如剩余轴数比期望roll出的symbol数少
			randomReelIndex = RandomUtility.RollSingleElement (_spinRoller, canrollsymbollist);
			int maxIndex = symbolRandomList [randomReelIndex].Count;
			do {
				--maxIndex;
				randomRollSymbolNum = symbolRandomList [randomReelIndex] [maxIndex];// roll一个最大的symbol数
			} while(randomRollSymbolNum > leftcount && maxIndex >= 0);
		}

		leftcount -= randomRollSymbolNum;
		canrollsymbollist.Remove(randomReelIndex);
		// 要记录一下当前轴需要roll一个包含多少symbol的stopindex
		rollSymbolNum[randomReelIndex] = randomRollSymbolNum;
		//	LogUtility.Log ("random index is " + randomReelIndex + " roll count : " + randomRollSymbolNum + " leftcount is " + leftcount, Color.red);
		FillUnorderedRollSymbolNum(canrollsymbollist, leftcount, rollSymbolNum, symbolRandomList);
	}

	private Predicate<int[]> GeneratePassPredicate(IJoyData data)
	{
		Predicate<int[]> passPred = (int[] stopIndexes) => {
			bool result = true;
			string[] symbolNames = _reelConfig.GetSymbolNames(stopIndexes);
			CoreCheckResult checkResult = _checker.CheckResultWithStopIndexesAndSymbolNames(data, stopIndexes, symbolNames, CoreCheckMode.PayoutOnly);
			if(checkResult.Type == SpinResultType.Win)
			{
				//if checkResult's PayoutData has not higher priority, then pass
				result = checkResult.PayoutData.Id >= data.Id;

				// jackpot count类型的判断
				PayoutData payoutData = data as PayoutData;
				if (payoutData != null && payoutData.JackpotCount > 0){
					result = result || checkResult.PayoutData.JackpotCount >= payoutData.JackpotCount || checkResult.PayoutData.Count >= payoutData.Count;
				}
//				if(!result)
//					CoreDebugUtility.Log("checkResult actual payout Id:" + checkResult.PayoutData.Id.ToString() + ", expect Id:" + data.Id.ToString());
			}
			return result;
		};
		return passPred;
	}

	private int RollSingleStopIndex(int[] stopIndexes, int reelIndex, List<string> optionNameList, IJoyData data)
	{
		Predicate<int[]> passPred = GeneratePassPredicate(data);
		int result = RollSingleStopIndexFromPredicate(stopIndexes, reelIndex, optionNameList, passPred);
		return result;
	}

	private int RollSingleStopIndexFromPredicate(int[] stopIndexes, int reelIndex, List<string> optionNameList, Predicate<int[]> passPred)
	{
		bool isFind = false;
		int result = CoreDefine.InvalidIndex;
		SingleReel singleReel = _reelConfig.GetSingleReel(reelIndex);

		while(optionNameList.Count > 0)
		{
			int[] tryStopIndexes = new int[stopIndexes.Length];
			stopIndexes.CopyTo(tryStopIndexes, 0);

			string rolledName = RandomUtility.RollSingleElement(_spinRoller, optionNameList);
			List<int> indexList = new List<int>(singleReel.GetStopIndexesForSymbolName(rolledName));
			while(indexList.Count > 0){
				int stopIndex = RollStopIndex(indexList);
				tryStopIndexes[reelIndex] = stopIndex;

				if(passPred(tryStopIndexes)) {
					result = stopIndex;
					isFind = true;
					break;
				} else {
					indexList.Remove(stopIndex);
				}
			}

			if(isFind){
				break;
			}else{
				optionNameList.RemoveAll((string s) => {
					return s == rolledName;
				});
			}
		}
		
		return result;
	}

	private int RollStopIndex(int reelIndex, string symbolName)
	{
		SingleReel singleReel = _reelConfig.GetSingleReel(reelIndex);
		List<int> indexList = singleReel.GetStopIndexesForSymbolName(symbolName);

//		indexList = ListUtility.FilterList(indexList, (int index) => {
//			return !singleReel.ShouldSlideToNeighbor(index);
//		});

		if(indexList == null || indexList.Count == 0)
		{
			CoreDebugUtility.LogError("Error: RollStopIndex returns empty list");
			CoreDebugUtility.Assert(false);
			return CoreDefine.InvalidIndex;
		}

		int stopIndex = RandomUtility.RollSingleElement(_spinRoller, indexList);
		return stopIndex;
	}

	private int RollStopIndex(List<int> indexList){
		if(indexList == null || indexList.Count == 0)
		{
			CoreDebugUtility.LogError("Error: RollStopIndex returns empty list");
			CoreDebugUtility.Assert(false);
			return CoreDefine.InvalidIndex;
		}

		int stopIndex = RandomUtility.RollSingleElement(_spinRoller, indexList);
		return stopIndex;
	}

	private int RollStopIndex(int reelIndex, string[] symbolNames){
		int result = CoreDefine.InvalidIndex;
		SingleReel singleReel = _reelConfig.GetSingleReel(reelIndex);
		List<int> rollList = ListUtility.CreateIntList (symbolNames.Length);

		while (result == CoreDefine.InvalidIndex && rollList.Count > 0) {
			int index = RandomUtility.RollSingleElement (_spinRoller, rollList);
			result = RollStopIndex(reelIndex, symbolNames[index]);
			rollList.Remove (index);
		}

		return result;
	}

	private int[] GenerateTotalStopIndexes(BaseJoyConfig baseJoyConfig, IJoyData data, int[] basicStopIndexes, bool[] matchFlags)
	{
		if(_basicReelCount == _totalReelCount)
		{
			return basicStopIndexes;
		}
		else
		{
			int[] result = new int[_totalReelCount];
			basicStopIndexes.CopyTo(result, 0);

			CoreDebugUtility.Assert(_totalReelCount - _basicReelCount == 1); //if it's broken, it means the logic below needs rewritten

			int fillReelIndex = _basicReelCount;
			ReelsWildConfig wildConfig = baseJoyConfig.GetReelsWildConfig(data);
			string wildName = RollWildName(wildConfig, fillReelIndex);
			int stopIndex = RollStopIndex(fillReelIndex, wildName);
			result[fillReelIndex] = stopIndex;
			matchFlags[fillReelIndex] = true;

			return result;
		}
	}

	private void ApplySymbolRestrictionToSpinResult(CoreSpinResult spinResult, CoreSpinSymbolRestriction restriction)
	{
		if(restriction != null && 
			(restriction._compareType == CompareType.Equal || restriction._compareType == CompareType.GreaterEqual))
		{
			int expectSymbolCount = restriction._count;

			//1 find option reels
			List<int> reelIndexList = new List<int>();
			for(int i = 0; i < _basicReelCount; i++)
			{
				//only when the symbol is not ready (ex. fixed symbol), consider the reel
				if(spinResult.SymbolList[i] == null)
				{
					SingleReel r = _reelConfig.GetSingleReel(i);
					if(r.IsContainAnySymbol(restriction._symbolNames))
						reelIndexList.Add(i);
				}
			}

			//2 subtract ready symbols which satisfies restriction
			for(int i = 0; i < _basicReelCount; i++)
			{
				if(spinResult.SymbolList[i] != null)
				{
					if (restriction._symbolNames.Contains(spinResult.SymbolList[i].SymbolData.Name))
						--expectSymbolCount;
				}
			}

			CoreDebugUtility.Assert(reelIndexList.Count >= expectSymbolCount);

			//3 remove part of reels
			int removeCount = reelIndexList.Count - expectSymbolCount;
			for(int i = 0; i < removeCount; i++)
			{
				int removeIndex = RandomUtility.RollInt(_spinRoller, reelIndexList.Count);
				reelIndexList.RemoveAt(removeIndex);
			}

			//4 force the reels to set restricted symbols
			for(int i = 0; i < reelIndexList.Count; i++)
			{
				int reelIndex = reelIndexList[i];
				int stopIndex = RollStopIndex(reelIndex, restriction._symbolNames);
				spinResult.SetStopIndex(reelIndex, stopIndex);
				spinResult.IsRestrictedList[reelIndex] = true;
			}
		}
	}

	#endregion

	#region Roll wild

	private int RollWildIndex(ReelsWildConfig config, int reelIndex)
	{
		float[] probs = config.ProbsList[reelIndex];
		RollHelper helper = new RollHelper(probs);
		int index = helper.RollIndex(_spinRoller);
		return index;
	}

	private int RollWildIndexExclude(ReelsWildConfig config, int reelIndex, string refName = "Wild7"){
		float[] probs = config.ProbsList[reelIndex];
		RollHelper helper = new RollHelper(probs);
		int index = -1;
		SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
		List<string> wildNameList = reel.WildNameList;
		string wildName = "";

		do{
			index = helper.RollIndex(_spinRoller);
			CoreDebugUtility.Assert(index < wildNameList.Count, "RollWildNameExcludeWild7 : Rolled index out of range");
			if (index == -1){
				break;
			}
			wildName = wildNameList[index];
		}while(wildName.Equals(refName));

		return index;
	}

	private int RollWildIndexExclude(ReelsWildConfig config, int reelIndex, SymbolType refType = SymbolType.Wild7){
		float[] probs = config.ProbsList[reelIndex];
		RollHelper helper = new RollHelper(probs);
		int index = -1;
		SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
		List<string> wildNameList = reel.WildNameList;
		string wildName = "";
		SymbolType symbolType;

		do{
			index = helper.RollIndex(_spinRoller);
			CoreDebugUtility.Assert(index < wildNameList.Count, "RollWildNameExcludeWild7 : Rolled index out of range");
			if (index == -1){
				break;
			}
			wildName = wildNameList[index];
			List<int> stopIndexes = reel.GetStopIndexesForSymbolName(wildName);
			if (stopIndexes.Count > 0){
				symbolType = reel.GetSymbolType(stopIndexes[0]);
			}else{
				break;
			}
		}while(symbolType.Equals(refType));

		return index;
	}

	private string RollWildName(ReelsWildConfig config, int reelIndex)
	{
		string result = "";
		int index = RollWildIndex(config, reelIndex);

		if(index >= 0)
		{
			SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
			List<string> wildNameList = reel.WildNameList;
			CoreDebugUtility.Assert(index < wildNameList.Count, "Rolled index out of range");
			result = wildNameList[index];
		}

		return result;
	}

	private string RollWildNameExclude(ReelsWildConfig config, int reelIndex, string refName = "Wild7"){
		string result = "";
		int index = RollWildIndexExclude(config, reelIndex, refName);

		if(index >= 0)
		{
			SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
			List<string> wildNameList = reel.WildNameList;
			CoreDebugUtility.Assert(index < wildNameList.Count, "RollWildNameExcludeWild7 Rolled index out of range");
			result = wildNameList[index];
		}

		return result;
	}

	private string RollWildNameExclude(ReelsWildConfig config, int reelIndex, SymbolType refType = SymbolType.Wild7){
		string result = "";
		int index = RollWildIndexExclude(config, reelIndex, refType);

		if(index >= 0)
		{
			SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
			List<string> wildNameList = reel.WildNameList;
			CoreDebugUtility.Assert(index < wildNameList.Count, "RollWildNameExcludeWild7 Rolled index out of range");
			result = wildNameList[index];
		}

		return result;
	}

	private bool RollWilds(int[] stopIndexes, ReelsWildConfig config, IJoyData data, SpinResultType type, IList<CoreSymbol> fixedSymbols, 
		List<int> wildIndexList, CoreSpinSymbolRestriction restriction)
	{
		bool isRolled = false;

		//1 fill symbolNames
		string[] symbolNames = _reelConfig.GetSymbolNames(stopIndexes);

		//2 find the reel indexes
		//2(a) rule out fixedSymbols
		for(int i = 0; i < fixedSymbols.Count; i++)
		{
			if(fixedSymbols[i] != null)
				wildIndexList.Remove(i);
		}

		//2(b) shuffle
		wildIndexList = RandomUtility.ShuffleList(_spinRoller, wildIndexList);

		//3 roll wilds
		for(int i = 0; i < wildIndexList.Count; i++)
		{
			//first, check restriction, break if reach the upper bound
			if (restriction != null && restriction._compareType == CompareType.LessEqual) {
				int restrictSymbolCount = GetSymbolNameCount(stopIndexes, restriction._symbolNames);
				if(restrictSymbolCount >= restriction._count)
				{
					CoreDebugUtility.Assert(restrictSymbolCount == restriction._count); //can't be greater
					break;
				}
			}

			int reelIndex = wildIndexList[i];
			//wild can be rolled to another wild, but bonusWild can't be rolled to wild
			if((_symbolConfig.CanWildChange(symbolNames[reelIndex]) || _symbolConfig.GetSymbolType(symbolNames[reelIndex]) == SymbolType.Wild)
				|| (_symbolConfig.CanWild7Change(symbolNames[reelIndex]) ||  _symbolConfig.GetSymbolType(symbolNames[reelIndex]) == SymbolType.Wild7))
			{
				int stopIndex = RollSingleWild(stopIndexes, reelIndex, data, config, type);

				//Note: after changing NearhitOrLoss to NearHitOrLossForFixWild, the patch here isn't needed any more
//				#if kPatchFreeSpinBonusWildBug
//				if(stopIndex != CoreDefine.InvalidIndex && _machineConfig.BasicConfig.HasFreeSpin && type == SpinResultType.NearHit)
//				{
//					SingleReel reel = _machineConfig.ReelConfig.GetSingleReel(reelIndex);
//					string symbolName = reel.GetSymbolName(stopIndex);
//					SymbolType symbolType = _machineConfig.SymbolConfig.GetSymbolType(symbolName);
//					if(_machineConfig.SymbolConfig.IsMatchSymbolType(symbolType, SymbolType.BonusWild))
//						stopIndex = CoreDefine.InvalidIndex;
//				}
//				#endif

				if(stopIndex != CoreDefine.InvalidIndex)
				{
					stopIndexes[reelIndex] = stopIndex;
					isRolled = true;

					//CoreDebugUtility.Log("Roll a wild, reelIndex:" + reelIndex.ToString() + ", stopIndex:" + stopIndex);
				}
			}
		}

		return isRolled;
	}

	private int GetSymbolNameCount(int[] indexes, string[] names){
		int result = 0;
		for(int m = 0; m < indexes.Length; ++m){
			if (indexes[m] != CoreDefine.InvalidIndex){
				SingleReel reel = _reelConfig.GetSingleReel(m);
				string name = reel.GetSymbolName(indexes[m]);
				if (names.Contains(name)){
					++result;
				}
			}
		}

		return result;
	}

	// 当前symbol类型是否符合排除条件
	private bool ShouldExclude(int[] stopIndexes, int reelIndex, SymbolType[] excludeSymbols){
		if (stopIndexes [reelIndex] == CoreDefine.InvalidIndex)
			return false;
		
		SingleReel reel = _reelConfig.GetSingleReel (reelIndex);
		SymbolType symbolType = reel.GetSymbolType (stopIndexes [reelIndex]);
		bool isFind = ListUtility.IsAnyElementSatisfied(excludeSymbols, (SymbolType type)=>{
			return symbolType.Equals(type);
		});

		return isFind;
	}

	private int RollSingleWild(int[] stopIndexes, int reelIndex, IJoyData data, ReelsWildConfig config, SpinResultType type)
	{
		int result = CoreDefine.InvalidIndex;
		string wildName = "";

		if (ShouldExclude(stopIndexes, reelIndex, CoreDefine.Wild7ExcludeSymbols)) {
			// zhousen 这里来区分wild7和wild
			wildName = RollWildNameExclude (config, reelIndex, SymbolType.Wild7);
		} else {
			wildName = RollWildName(config, reelIndex);
		}

		if(!string.IsNullOrEmpty(wildName))
		{
			//when nearHit in slide level, avoid slide symbol
			bool canReplace = true;
			if(_basicConfig.HasSlide && type == SpinResultType.NearHit)
			{
				SymbolData d = _symbolConfig.GetSymbolData(wildName);
				if(d.SlideType != SlideType.None)
					canReplace = false;
			}

			if(canReplace)
			{
				List<string> optionNameList = new List<string>();
				optionNameList.Add(wildName);
				int stopIndex = RollSingleStopIndex(stopIndexes, reelIndex, optionNameList, data);

				if(stopIndex != CoreDefine.InvalidIndex)
					result = stopIndex;
			}
		}
		return result;
	}

	#endregion

	#region Roll slide

	private void RollSlideInfoInResult(CoreSpinResult result, PayoutData data)
	{
		if(_basicConfig.HasSlide)
		{
			SlideConfig slideConfig = _machineConfig.PayoutConfig.GetSlideConfig(data);

			for(int i = 0; i < result.SymbolList.Count; i++)
			{
				if(i < _basicReelCount)
				{
					SingleReel singleReel = _reelConfig.GetSingleReel(i);
					CoreSymbol symbol = result.SymbolList[i];
					SlideType slideType = symbol.SymbolData.SlideType;
					if(slideType != SlideType.None)
					{
						float prob = slideConfig.ProbList[i];
						SpinDirection dir = RollSlideDirection(slideType, prob);
						if(dir == SpinDirection.Up || dir == SpinDirection.Down)
						{
							CoreSymbol originSymbol = result.SymbolList[i];
							int curIndex = singleReel.GetNeighborStopIndex(originSymbol.StopIndex, dir);
							result.SetStopIndex(i, curIndex);
							result.ShouldSlide = true;

							if(dir == SpinDirection.Up)
								result.SlideOffsetList[i] = -1;
							else
								result.SlideOffsetList[i] = 1;
						}
						else
						{
							result.SlideOffsetList[i] = 0;
						}
					}
				}
			}
		}
	}

	private SpinDirection RollSlideDirection(SlideType slideType, float prob)
	{
		SpinDirection dir = SpinDirection.None;
		float dice = _spinRoller.NextFloat();
		if(dice <= prob)
		{
			//slide
			if(slideType == SlideType.UpDown)
			{
				int d = RandomUtility.RollBinary(_spinRoller);
				if(d == 0)
					dir = SpinDirection.Up;
				else
					dir = SpinDirection.Down;
			}
			else if(slideType == SlideType.Up)
			{
				dir = SpinDirection.Up;
			}
			else if(slideType == SlideType.Down)
			{
				dir = SpinDirection.Down;
			}
			else
			{
				CoreDebugUtility.Assert(false);
			}
		}

		return dir;
	}

	#endregion

	#region NearHit

	private CoreSpinResult ConstructNearHitResult(CoreSpinInput spinInput, NearHitData data)
	{
		CoreSpinResult result = new CoreSpinResult(SpinResultType.NearHit, _coreMachine, spinInput);
		result.SetNearHitData(data);
		result.FillFixedSymbols(spinInput.FixedSymbols);
		ApplySymbolRestrictionToSpinResult(result, spinInput.SymbolRestriction);

		PayoutGeneratorDelegate handler = _payoutGenerators[(int)data.PayoutType];
		CoreDebugUtility.Assert(handler != null);

		int[] stopIndexes;
		List<CoreSymbol> fixedSymbols = new List<CoreSymbol>(result.SymbolList);
		bool isCanNotBeAny = IsSpinResultCanNotBeAny(fixedSymbols, data);
		if (isCanNotBeAny) {
			// 无法handle的情况下，从之前的结果里直接提取stopindexes
			stopIndexes = GenerateStopIndexesForCanNotAny(fixedSymbols, data);
		} else {
			stopIndexes = handler(fixedSymbols, _curNearHitConfig, data, SpinResultType.NearHit, result.MatchFlags, spinInput.SymbolRestriction);
		}
		CoreDebugUtility.Log ("nearhit stopindexes = "+stopIndexes[0]+" "+stopIndexes[1]+" "+stopIndexes[2]);
		stopIndexes = GenerateTotalStopIndexes(_curNearHitConfig, data, stopIndexes, result.MatchFlags);
		result.SetStopIndexes(stopIndexes);
		OffsetNearHitResult(result, data);
		result.FillNonPayoutLineSymbolList();
		result.FillFinalCoreSymbol (_coreMachine);

		if (canFillCollect()) {
			FillRandomCollect (result);
		}
		return result;
	}

	private void OffsetNearHitResult(CoreSpinResult result, NearHitData data)
	{
		//Note:
		//For NearHit, offset only one symbol might not be enough since it might become a Win after offset.
		//I had this case in M4 machine. So the offset should be looped until it forms a NearHit.
		List<int> optionReelIndexList =  ListUtility.CreateIntList(_basicReelCount);
		for(int i = 0; i < result.SymbolList.Count; i++)
		{
			if(result.IsRestrictedList[i])
				optionReelIndexList.Remove(i);
		}

		//Note:
		//This check is necessary in this case:
		//In M30, the FreeSpin ending is triggered by 3 Box(Bonus) symbols. Before this function is called,
		//ApplySymbolRestrictionToSpinResult is called and set all 3 symbols to Box and result.IsRestrictedList[3]
		//to true. So in here, there is no reel to offset and we could just skip the logic followed.
		//If we remove this check, there would be some LogError printed in the end of this function
		if(optionReelIndexList.Count == 0)
		{
			CoreDebugUtility.Log("Skip offset NearHit symbols, since all symbols on payline are restricted and fixed");
			return;
		}

		List<int> fallbackOptionReelIndexList = new List<int>(optionReelIndexList);
		bool isFind = false;

		//1 try offset only one symbol
		while(optionReelIndexList.Count > 0)
		{
//			int reelIndex = RandomUtility.RollSingleElement(_spinRoller, optionReelIndexList);
			int reelIndex = RandomUtility.RollSingleIntByRatios(_spinRoller, optionReelIndexList, _basicConfig.NearHitProbs);
			SingleReel reel = _reelConfig.GetSingleReel(reelIndex);

			int factor = RandomUtility.RollSingleElement (_spinRoller, new List<int> (){ -1, 1 });
			List<int> offsets = _curNearHitConfig.GetNeighborOffsets (data);;
			offsets = ListUtility.MapList (offsets, (int offset) => {
				return factor * offset;
			});

			while(offsets.Count > 0)
			{
				List<int> stopIndexes = new List<int>(result.StopIndexes);
//				int offset = RandomUtility.RollSingleElement(_spinRoller, offsets);
				int offset = offsets[0];
				int neighborIndex = reel.GetNeighborStopIndex(stopIndexes[reelIndex], offset);
				stopIndexes[reelIndex] = neighborIndex;

				string[] symbolNames = _reelConfig.GetSymbolNames(stopIndexes);
				CoreCheckResult checkResult = _checker.CheckResultWithStopIndexesAndSymbolNames(data, stopIndexes, symbolNames, CoreCheckMode.PayoutOnly);

				if (checkResult.CanBeNearHit(result))
				{
					isFind = true;
					result.SetStopIndex(reelIndex, neighborIndex);
					break;
				}
				else
				{
					offsets.Remove(offset);
				}
			}

			if(isFind)
				break;
			else
				optionReelIndexList.Remove(reelIndex);
		}

		//2 when offset one symbol fails, fallback to offset multiple symbols
		if(!isFind)
		{
			CoreDebugUtility.LogError("Just warning: NearHit offset one symbol fails, fallback to offset multiple symbols");
			CoreDebugUtility.LogError("Expected spinResult: " + result.ToString());

			while(fallbackOptionReelIndexList.Count > 0)
			{
				int reelIndex = RandomUtility.RollSingleElement(_spinRoller, fallbackOptionReelIndexList);
				List<int> neighbourOffsets = _curNearHitConfig.GetNeighborOffsets (data);;
				int stopIndex = RollNeighborSymbolIndex(reelIndex, result.StopIndexes[reelIndex], neighbourOffsets);
				result.SetStopIndex(reelIndex, stopIndex);

				CoreCheckResult checkResult = _checker.CheckResultWithSymbols(data, result.SymbolList, CoreCheckMode.PayoutOnly);
				if(checkResult.CanBeNearHit(result))
				{
					isFind = true;
					break;
				}

				fallbackOptionReelIndexList.Remove(reelIndex);
			}

			if(!isFind)
				CoreDebugUtility.LogError("Fail to fallback to offset multiple symbols");
		}
	}

	private int RollNeighborSymbolIndex(int reelIndex, int stopIndex, List<int> neighborOffsets)
	{
		SingleReel singleReel = _reelConfig.GetSingleReel(reelIndex);
		//avoid offset to a neighbor symbol which can slide
		List<int> neighborIndexList = new List<int>();
		if(_basicConfig.HasSlide)
		{
			SpinDirection[] dirs = { SpinDirection.Up, SpinDirection.Down };
			int[] offsets = { 1, -1 };
			for(int i = 0; i < dirs.Length; i++)
			{
				int neighborIndex = singleReel.GetNeighborStopIndex(stopIndex, dirs[i]);
				if(!singleReel.ShouldSlideToNeighbor(neighborIndex))
					neighborIndexList.Add(offsets[i]);
			}
		}
		else
		{
			neighborIndexList.AddRange (neighborOffsets);
		}

		if(neighborIndexList.Count == 0)
		{
			string s = string.Format("Error: The symbol which has slide symbols above and below it, " +
			"ReelIndex: {0}, StopIndex: {1}", reelIndex, stopIndex);
			CoreDebugUtility.LogError(s);
			//CoreDebugUtility.Assert(false);
			neighborIndexList.AddRange (neighborOffsets);
		}

		int indexOffset = RandomUtility.RollSingleElement(_spinRoller, neighborIndexList);
		int symbolCount = singleReel.SymbolCount;

		int result = stopIndex + indexOffset;
		if(result < 0)
			result += symbolCount;
		result = result % symbolCount;

		return result;
	}

	#endregion

	#region Loss

	private CoreSpinResult ConstructLossResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = new CoreSpinResult(SpinResultType.Loss, _coreMachine, spinInput);
		result.FillFixedSymbols(spinInput.FixedSymbols);

		int[] stopIndexes = LossSymbolStopIndexesHandler(result);
		stopIndexes = LossGenerateTotalSymbolIndexes(stopIndexes);
		result.SetStopIndexes(stopIndexes);
		FillSlideInfoInResult(result, stopIndexes);
		result.FillNonPayoutLineSymbolList();
		result.FillFinalCoreSymbol (_coreMachine);

		if (canFillCollect()) {
			FillRandomCollect (result);
		}
		return result;
	}

	private int[] LossSymbolStopIndexesHandler(CoreSpinResult spinResult)
	{
		int[] stopIndexes = new int[_basicReelCount];
		ListUtility.FillElements(stopIndexes, CoreDefine.InvalidIndex);

		bool isFind = LossGenerateSingleSymbolIndex(spinResult, stopIndexes, stopIndexes, 0);
		CoreDebugUtility.Assert(isFind, "Loss generate fail");

		return stopIndexes;
	}

	//recursive function
	private bool LossGenerateSingleSymbolIndex(CoreSpinResult spinResult, int[] curIndexes, int[] resultIndexes, int reelIndex)
	{
		if(reelIndex == _basicReelCount)
		{
			curIndexes.CopyTo(resultIndexes, 0);
			return true;
		}

		bool result = false;
		int[] copyIndexes = new int[curIndexes.Length];
		curIndexes.CopyTo(copyIndexes, 0);

		SingleReel reel = _reelConfig.GetSingleReel(reelIndex);
		List<int> optionIndexList = ListUtility.CreateIntList(reel.SymbolCount);

		#if kPatchFreeSpinBonusWildBug
		if(_machineConfig.BasicConfig.HasFreeSpin)
		{
			optionIndexList = ListUtility.FilterList(optionIndexList, (int index) => {
				string name = reel.GetSymbolName(index);
				SymbolType type = _symbolConfig.GetSymbolType(name);
				return !_symbolConfig.IsMatchSymbolType(type, SymbolType.BonusWild);
			});
		}
		#endif

		while(optionIndexList.Count > 0)
		{
			int rollStopIndex = RandomUtility.RollSingleElement(_spinRoller, optionIndexList);

			copyIndexes[reelIndex] = rollStopIndex;
			string[] symbolNames = _reelConfig.GetSymbolNames(copyIndexes);
			CoreCheckResult checkResult = _checker.CheckResultWithStopIndexesAndSymbolNames(null, copyIndexes, symbolNames);
			//todo: this is ugly, refactor it later...
			if(checkResult.CanBeLoss(spinResult))
			{
				result = LossGenerateSingleSymbolIndex(spinResult, copyIndexes, resultIndexes, reelIndex + 1);
				if(result)
					break;
			}
			else
			{
				optionIndexList.Remove(rollStopIndex);
			}
		}

		return result;
	}

	private int[] LossGenerateTotalSymbolIndexes(int[] stopIndexes)
	{
		if(_basicReelCount == _totalReelCount)
		{
			return stopIndexes;
		}
		else
		{
			int[] result = new int[_totalReelCount];
			stopIndexes.CopyTo(result, 0);

			CoreDebugUtility.Assert(_totalReelCount - _basicReelCount == 1); //if it's broken, it means the logic below needs rewritten

			int fillIndex = _basicReelCount;
			SingleReel reel = _reelConfig.GetSingleReel(fillIndex);
			int wildIndex = RandomUtility.RollInt(_spinRoller, reel.SymbolNameList.Count);
			result[fillIndex] = wildIndex;

			return result;
		}
	}

	private void FillSlideInfoInResult(CoreSpinResult result, int[] stopIndexes)
	{
		if(_basicConfig.HasSlide)
		{
			for(int i = 0; i < stopIndexes.Length; i++)
			{
				int index = stopIndexes[i];
				SingleReel singleReel = _reelConfig.GetSingleReel(i);

				if(singleReel.ShouldSlideToNeighbor(index, SpinDirection.Up))
				{
					result.ShouldSlide = true;
					result.SlideOffsetList[i] = 1;
				}
				else if(singleReel.ShouldSlideToNeighbor(index, SpinDirection.Down))
				{
					result.ShouldSlide = true;
					result.SlideOffsetList[i] = -1;
				}
				else
				{
					result.SlideOffsetList[i] = 0;
				}
			}
		}
	}

	#endregion

	#region Lucky
	private void RefreshLuckyMode(ulong betAmount, bool forceLuckyMode = false, CoreLuckyMode luckyMode = CoreLuckyMode.LongLucky)
	{
		CoreLuckyMode mode = _luckyManager.RefreshMode(betAmount);

		if (forceLuckyMode) {
			mode = luckyMode;
		}

		_curPayoutConfig = _machineConfig.GetCurPayoutConfig(mode);
		_curNearHitConfig = _machineConfig.GetCurNearHitConfig(mode);

		if(mode == CoreLuckyMode.Normal)
		{
			_curSpinRollHelper = _spinRollHelper;
			_curWinRollHelper = _winRollHelper;
			_curNearHitRollHelper = _nearHitRollHelper;
		}
		else
		{
			_curSpinRollHelper = _luckySpinRollHelper;
			_curWinRollHelper = _luckyWinRollHelper;
			_curNearHitRollHelper = _luckyNearHitRollHelper;
		}

		//don't forget to refresh checker
		_checker.RefreshLuckyMode(mode);
	}

	#endregion

	#region update hitrate increase after pay

	private RollHelper GetHitRateIncreaseAfterPayRollHelper(RollHelper helper, float dice, CoreSpinInput spinInput){
		SpinResultType spinIndex = (SpinResultType)helper.FetchIndex (dice);
		if ((UserBasicData.Instance.PayProtectionEnable || spinInput.IsPayProtectionTest) && spinIndex != SpinResultType.Win) {
			float winProb = _curPayoutConfig.TotalProb + _coreMachine.PayProtectionHitRate;
			float nearHitProb = _curNearHitConfig.TotalProb;
			float lossProb = 1.0f - winProb - nearHitProb;

			float[] probArray = new float[]{winProb, nearHitProb, lossProb};
			CoreDebugUtility.Log ("GetHitRateIncreaseAfterPayRollHelper winProb nearHitProb lossProb is "+winProb + " " + nearHitProb + " " +lossProb);
			RollHelper hitRateIncreaseHelper = new RollHelper (probArray);

			return hitRateIncreaseHelper;
		} else {
			return helper;
		}
	}

	#endregion

	#region cmddebug

	#if DEBUG

	public int GetIndexByWinType(WinType type, NormalWinType normalType)
	{
		float bigRatio = CoreUtility.GetWinTypeThreshold (WinType.Big);
		float epicRatio = CoreUtility.GetWinTypeThreshold (WinType.Epic);
		float normalRatio = CoreUtility.GetWinTypeThreshold (WinType.Normal);

		PayoutData[] array = _payoutConfig.Sheet.dataArray;

		float start = 0.0f, end = 0.0f;

		if (type == WinType.Big) {
			start = bigRatio;
			end = epicRatio;
		} else if (type == WinType.Epic) {
			start = epicRatio;
			end = float.MaxValue;
		} else if (type == WinType.Normal) {
			if (normalType == NormalWinType.High) {
				start = normalRatio;
				end = bigRatio;
			} else if (normalType == NormalWinType.Low) {
				start = 0.0f;
				end = normalRatio;
			} else {
				CoreDebugUtility.Assert (false, "can't find normaltype ratio");
			}
		}else {
			return -1;
		}

		for (int i = 0; i < array.Length; ++i) {
			if (array [i].Ratio >= start && array[i].Ratio < end) {
				if (Math.Abs(array[i].Ratio-0.0f)<0.001f)
					continue;
				return i;
			}
		}

		return -1;
	}

	private void InitDebugHugeWinRollHelpers()
	{
		float[] spinProbArray = new float[]{
			CoreDebugManager.Instance.WinProbInHugeWinMode,
			CoreDebugManager.Instance.NearHitProbInHugeWinMode,
			CoreDebugManager.Instance.LossProbInHugeWinMode
		};
		_debugHugeWinSpinRollHelper = new RollHelper(spinProbArray);

		int count = _payoutConfig.JoyDataArray.Length;
		float[] winProbArray = new float[count];
		float singleProb = CoreDebugManager.Instance.WinProbInHugeWinMode / count;
		ListUtility.FillElements(winProbArray, singleProb);
		_debugHugeWinWinRollHelper = new RollHelper(winProbArray);
	}

	private RollHelper GetDebugSpinRollHelperForHugeWinMode(RollHelper defaultHelper)
	{
		RollHelper result = null;
		if(CoreDebugManager.Instance.IsHugeWinMode)
			result = _debugHugeWinSpinRollHelper;
		else
			result = defaultHelper;
		return result;
	}

	private RollHelper GetDebugWinRollHelperForHugeWinMode(RollHelper defaultHelper)
	{
		RollHelper result = null;
		if(CoreDebugManager.Instance.IsHugeWinMode)
			result = _debugHugeWinWinRollHelper;
		else
			result = defaultHelper;
		return result;
	}

	#endif

	#endregion
}

