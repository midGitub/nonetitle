//#define DEBUG_FORCE_JACKPOT
using System.Collections;
using System.Collections.Generic;
using System;

public class CoreMultiLineSymbolProbGenerator : CoreMultiLineBaseGenerator
{
	#if DEBUG && DEBUG_FORCE_JACKPOT
//	static readonly string _debugJackpotType = "Colossal";
	static readonly string _debugJackpotType = "Huge";
	#endif

	static readonly int _maxRollCountToAvoidJackpot = 3;

	private BasicConfig _basicConfig;
	private SymbolConfig _symbolConfig;
	private ReelConfig _reelConfig;
	private PayoutConfig _payoutConfig;
	private PaylineConfig _paylineConfig;

	private int _totalReelCount;

	private RollHelper _normalSpinRollHelper;
	private RollHelper _luckySpinRollHelper;
	private RollHelper _curSpinRollHelper; //refer to _normalSpinRollHelper or _luckySpinRollHelper

	#region Init

	public CoreMultiLineSymbolProbGenerator(CoreMachine coreMachine, CoreMultiLineChecker checker, CoreLuckyManager luckyManager, MachineConfig machineConfig)
	{
		Init(coreMachine, checker, luckyManager, machineConfig);

		_basicConfig = _machineConfig.BasicConfig;
		_symbolConfig = _machineConfig.SymbolConfig;
		_reelConfig = _machineConfig.ReelConfig;
		_payoutConfig = _machineConfig.PayoutConfig;
		_paylineConfig = _machineConfig.PaylineConfig;

		_totalReelCount = _basicConfig.ReelCount;
	}

	#endregion

	#region Roll

	protected override CoreSpinResult RollGeneralSpinResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = new CoreSpinResult(SpinResultType.None, _coreMachine, spinInput);
		bool isJackpotWin = false;
		int[] stopIndexes = RollStopIndexes(spinInput, out isJackpotWin);
		result.IsJackpotWin = isJackpotWin;
		result.SetStopIndexes(stopIndexes);
		result.MultiLineCheckResult = _checker.CheckResultWithStopIndexes(stopIndexes);
		result.FillMultiLineWinRatio();
		result.FillFinalCoreSymbol (_coreMachine);
		result.SetJackpotWinMultiline (spinInput);
		return result;
	}

	int[] RollStopIndexes(CoreSpinInput spinInput, out bool isJackpotWin)
	{
		int[] result = null;
		
		string jackpotType = "";
		isJackpotWin = JackpotWinManager.Instance.CheckJackpotWin(_spinRoller, 
			ref jackpotType, _coreMachine.Name, spinInput);

		#if DEBUG && DEBUG_FORCE_JACKPOT
		isJackpotWin = true;
		jackpotType = _debugJackpotType;
		#endif

		if(isJackpotWin)
		{
			result = RollJackpotStopIndexes(jackpotType, spinInput);
		}
		else
		{
			if(_forceWinManager.ShouldForceWin())
				result = RollForceWinStopIndexes(spinInput);
			else
				result = RollGeneralStopIndexes(spinInput, out isJackpotWin);
		}

		return result;
	}

	int[] RollGeneralStopIndexes(CoreSpinInput spinInput, out bool isJackpotWin)
	{
		int[] indexes = new int[_basicConfig.ReelCount];
		// We should avoid jackpot win, but we shouldn't loop too many times
		int curRollCount = 0;
		bool isNoJackpot = false;

		do
		{
			for(int i = 0; i < _basicConfig.ReelCount; i++)
			{
				SingleReel reel = _reelConfig.GetSingleReel(i);
				RollHelper rollHelper = reel.GetCurRollHelper(_luckyManager.Mode);
				indexes[i] = rollHelper.RollIndex(_spinRoller);

				#if DEBUG
				CoreDebugUtility.Assert(indexes[i] >= 0 && indexes[i] < reel.SymbolCount);
				#endif
			}

			++curRollCount;

			if(!IsResultJackpotWin(indexes))
			{
				isNoJackpot = true;
				break;
			}
		} while(curRollCount <= _maxRollCountToAvoidJackpot);

		isJackpotWin = !isNoJackpot;

		if(isJackpotWin)
		{
			CoreDebugUtility.LogError("Try to roll general stopIndexes but always get jackpot!");
			CoreDebugUtility.Assert(false);
		}

		return indexes;
	}

	int[] RollJackpotStopIndexes(string jackpotType, CoreSpinInput spinInput)
	{
		int[] result = null;
		int jackpotIndex = JackpotWinManager.Instance.GetJackpotPayoutData (jackpotType, _payoutConfig.Sheet.dataArray, spinInput);
		if(jackpotIndex >= 0)
		{
			PayoutData data = _payoutConfig.Sheet.dataArray[jackpotIndex];
			result = ConstructMultiLineResultFromPayoutData(data);
		}
		else
		{
			CoreDebugUtility.LogError("Try to roll jackpot but no jackpot payout is available");
			CoreDebugUtility.Assert(false);
			bool isJackpotWin = false;
			result = RollGeneralStopIndexes(spinInput, out isJackpotWin);
		}

		return result;
	}

	int[] RollForceWinStopIndexes(CoreSpinInput spinInput)
	{
		PayoutData data = _payoutConfig.Sheet.dataArray[_forceWinManager.ForceWinPayoutIndex];
		int[] result = ConstructMultiLineResultFromPayoutData(data);
		_forceWinManager.ClearForceWinFlag();
		return result;
	}

	int[] ConstructMultiLineResultFromPayoutData(PayoutData data)
	{
		int[] stopIndexes = ConstructSingleLineResultFromPayoutData(data);

		int[] payline = RandomUtility.RollSingleElement(_spinRoller, _paylineConfig.PaylineList);
		for(int i = 0; i < stopIndexes.Length; i++)
		{
			SingleReel reel = _machineConfig.ReelConfig.GetSingleReel(i);
			stopIndexes[i] = reel.GetNeighborStopIndex(stopIndexes[i], -payline[i]);
		}

		return stopIndexes;
	}

	// todo by nichos:
	// This function is not perfect and only supports PayoutType.Ordered for now,
	// It's just a temporary simple implementation.
	// The todo part is:
	// The code is very similar to CoreGenerator.PayoutOrderedHandler,
	// So we should extract the common logic from this class and CoreGenerator, 
	// but it might take 1 day to complete the work
	int[] ConstructSingleLineResultFromPayoutData(PayoutData data)
	{
		//todo: only support Ordered for now
		CoreDebugUtility.Assert(data.PayoutType == PayoutType.Ordered);

		int[] stopIndexes = new int[_totalReelCount];
		for(int i = 0; i < _totalReelCount; i++)
		{
			string symbolName = data.Symbols[i];
			List<int> indexList = null;
			SingleReel singleReel = _reelConfig.GetSingleReel(i);
			if(symbolName == CoreDefine.WildCardSymbol)
				indexList = ListUtility.CreateIntList(singleReel.SymbolCount);
			else
				indexList = singleReel.GetStopIndexesForSymbolName(symbolName);

			if(indexList == null || indexList.Count == 0)
			{
				CoreDebugUtility.LogError("Error: Fail to find indexList");
				CoreDebugUtility.Assert(false);
			}

			stopIndexes[i] = RandomUtility.RollSingleElement(_spinRoller, indexList);
		}
		return stopIndexes;
	}

	protected override CoreSpinResult RollShortLuckySpinResult(CoreSpinInput spinInput)
	{
		// Designers have confirmed not to support short lucky for SymbolProb
		CoreSpinResult result = RollGeneralSpinResult(spinInput);
		return result;
	}

	#endregion

	#region Utility

	public bool IsResultJackpotWin(int[] stopIndexes)
	{
		bool result = false;
		if(_machineConfig.BasicConfig.IsJackpot)
		{
			CoreMultiLineCheckResult checkResult = _checker.CheckResultWithStopIndexes(stopIndexes);
			result = ListUtility.IsAnyElementSatisfied(checkResult.PayoutInfos, (MultiLineMatchInfo info) => {
				bool r = info.PayoutData != null && !string.IsNullOrEmpty(info.PayoutData.JackpotType);
				return r;
			});
		}

		return result;
	}

	#endregion

	#region Lucky

	protected override void PreRoll()
	{
		//nothing
	}

	#endregion
}

