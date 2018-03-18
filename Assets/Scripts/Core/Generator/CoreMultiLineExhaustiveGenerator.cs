//#define DEBUG_TRIGGER_WIN
//#define DEBUG_FORCE_JACKPOT
using System.Collections;
using System.Collections.Generic;
using System;

public class CoreMultiLineExhaustiveGenerator : CoreMultiLineBaseGenerator
{
	private BasicConfig _basicConfig;
	private SymbolConfig _symbolConfig;
	private ReelConfig _reelConfig;
	private PayoutConfig _payoutConfig;
	private PayoutDistConfig _payoutDistConfig;
	private PayoutDistConfig _luckyPayoutDistConfig;
	private NearHitDistConfig _nearHitDistConfig;
	private NearHitDistConfig _luckyNearHitDistConfig;
	private PaylineConfig _paylineConfig;
	private RewardResultConfig _rewardResultConfig;

	private PayoutDistConfig _curPayoutDistConfig; //refer to _payoutDistConfig or _luckyPayoutDistConfig
	private NearHitDistConfig _curNearHitDistConfig; //refer to _nearHitDistConfig or _luckyNearHitDistConfig

	private int _totalReelCount;

	private RollHelper _spinRollHelper;
	private RollHelper _luckySpinRollHelper;
	private RollHelper _curSpinRollHelper; //refer to _spinRollHelper or _luckySpinRollHelper

	private RollHelper _winRollHelper;
	private RollHelper _luckyWinRollHelper;
	private RollHelper _curWinRollHelper; //refer to _winRollHelper or _luckyWinRollHelper

	private RollHelper _nearHitRollHelper;
	private RollHelper _luckyNearHitRollHelper;
	private RollHelper _curNearHitRollHelper; //refer to _nearHitRollHelper or _luckyNearHitRollHelper

	// free spin
	private RollHelper _freeSpinWinRollHelper;
	private RollHelper _luckyFreeSpinWinRollHelper;
	private RollHelper _curFreeSpinWinRollHelper; //refer to _freeSpinWinRollHelper or _luckyFreeSpinWinRollHelper

	//used to force a win when CoreLuckyMode.ShortLucky
	private List<PayoutDistData> _shortLuckyPayoutDistDatas;
	private RollHelper _shortLuckyWinRollHelper;

	#if DEBUG && DEBUG_TRIGGER_WIN
	int _curDebugTriggerIndex = 0;
//	int[] _debugTriggerPayoutIndexes = new int[] { 9450, 9450 }; // M15
//	int[] _debugTriggerPayoutIndexes = new int[] { 10636, 10341, 7591 }; // M25
//	int[] _debugTriggerPayoutIndexes = new int[] { 2225 }; // 25
//	int[] _debugTriggerPayoutIndexes = new int[] { 9870, 9870 }; // M31
//	int[] _debugTriggerPayoutIndexes = new int[] { 6591 }; // V5
//	int[] _debugTriggerPayoutIndexes = new int[] { 10000 }; // V4
	int[] _debugTriggerPayoutIndexes = new int[] { 1521 }; // M17
	#endif

	#region Init

	public CoreMultiLineExhaustiveGenerator(CoreMachine coreMachine, CoreMultiLineChecker checker, CoreLuckyManager luckyManager, MachineConfig machineConfig)
	{
		Init(coreMachine, checker, luckyManager, machineConfig);

		_basicConfig = _machineConfig.BasicConfig;
		_symbolConfig = _machineConfig.SymbolConfig;
		_reelConfig = _machineConfig.ReelConfig;
		_payoutConfig = _machineConfig.PayoutConfig;
		_payoutDistConfig = _machineConfig.PayoutDistConfig;
		_luckyPayoutDistConfig = _machineConfig.LuckyPayoutDistConfig;
		_nearHitDistConfig = _machineConfig.NearHitDistConfig;
		_luckyNearHitDistConfig = _machineConfig.LuckyNearHitDistConfig;
		_paylineConfig = _machineConfig.PaylineConfig;
		_rewardResultConfig = _machineConfig.RewardResultConfig;

		_curPayoutDistConfig = _payoutDistConfig;
		_curNearHitDistConfig = _nearHitDistConfig;

		_totalReelCount = _basicConfig.ReelCount;

		InitSpinRollHelpers();
		InitWinRollHelpers();
		InitFreeSpinWinRollHelpers();
		InitNearHitRollHelpers();

		InitShortLuckys();
	}

	private void InitSpinRollHelpers()
	{
		float winProb = _payoutDistConfig.TotalProb;
		float nearHitProb = _nearHitDistConfig.TotalProb;
		float lossProb = 1.0f - winProb - nearHitProb;

		float[] probArray = new float[]{winProb, nearHitProb, lossProb};
		_spinRollHelper = new RollHelper(probArray);

		winProb = _luckyPayoutDistConfig.TotalProb;
		nearHitProb = _luckyNearHitDistConfig.TotalProb;
		lossProb = 1.0f - winProb - nearHitProb;

		float[] luckyProbArray = new float[]{winProb, nearHitProb, lossProb};
		_luckySpinRollHelper = new RollHelper(luckyProbArray);

		_curSpinRollHelper = _spinRollHelper;
	}

	private void InitWinRollHelpers()
	{
		_winRollHelper = GenRollHelperFromJoy(_payoutDistConfig);
		_luckyWinRollHelper = GenRollHelperFromJoy(_luckyPayoutDistConfig);

		_curWinRollHelper = _winRollHelper;
	}

	private void InitFreeSpinWinRollHelpers()
	{
		if(_machineConfig.BasicConfig.HasFreeSpin)
		{
			_freeSpinWinRollHelper = new RollHelper(_payoutDistConfig.FreeSpinOverallHitArray);
			_luckyFreeSpinWinRollHelper = new RollHelper(_luckyPayoutDistConfig.FreeSpinOverallHitArray);

			_curFreeSpinWinRollHelper = _freeSpinWinRollHelper;
		}
	}

	private void InitNearHitRollHelpers()
	{
		_nearHitRollHelper = GenRollHelperFromJoy(_nearHitDistConfig);
		_luckyNearHitRollHelper = GenRollHelperFromJoy(_luckyNearHitDistConfig);

		_curNearHitRollHelper = _nearHitRollHelper;
	}

	private RollHelper GenRollHelperFromJoy(BaseJoyDistConfig joyConfig)
	{
		return new RollHelper(joyConfig.OverallHitArray);
	}

	private void InitShortLuckys()
	{
		//1 _shortLuckyPayoutDistDatas and probList
		_shortLuckyPayoutDistDatas = new List<PayoutDistData>();
		List<float> probList = new List<float>();
		PayoutDistData[] distDataArray = _luckyPayoutDistConfig.Sheet.dataArray;
		for(int i = 0; i < distDataArray.Length; i++)
		{
			if(distDataArray[i].IsShortLucky)
			{
				_shortLuckyPayoutDistDatas.Add(distDataArray[i]);
				probList.Add(distDataArray[i].OverallHit);
			}
		}

		//2 _shortLuckyWinRollHelper
		_shortLuckyWinRollHelper = new RollHelper(probList);
		_shortLuckyWinRollHelper.NormalizeProbs();
	}

	#endregion

	#region Roll

	protected override CoreSpinResult RollGeneralSpinResult(CoreSpinInput spinInput)
	{
		int payoutId = 0, nearHitId = 0;
		bool isJackpotWin = false;
		RollDistIndex(out payoutId, out nearHitId, out isJackpotWin, spinInput);
		RewardResultData resultData = RollRewardResult(payoutId, nearHitId);

		CoreSpinResult result = ConstructSpinResult(resultData, isJackpotWin, spinInput);
		return result;
	}

	void RollDistIndex(out int payoutId, out int nearHitId, out bool isJackpotWin, CoreSpinInput spinInput)
	{
		payoutId = 0;
		nearHitId = 0;
		isJackpotWin = false;

		SpinResultType spinType = SpinResultType.None;

		// free spin
		if(spinInput.FreeSpinData != null)
		{
			//By nichos: the logic here only support SpinUntilLose for now
			//To support more FreeSpinTypes, we might need to write more code
			//By nichos 20171121:
			//I think it support all FreeSpinTypes, but not 100% sure
			//CoreDebugUtility.Assert(_machineConfig.BasicConfig.FreeSpinType == FreeSpinType.SpinUntilLose);

			bool isFreeSpinWin = spinInput.FreeSpinData.RollFreeSpinWin(_spinRoller);
			spinType = isFreeSpinWin ? SpinResultType.Win : SpinResultType.NearHit;
		}
		else
		{
			float dice = _spinRoller.NextFloat();
			RollHelper rollHelper = GetHitRateIncreaseAfterPayRollHelper (_curSpinRollHelper, dice, spinInput);
			spinType = (SpinResultType)rollHelper.FetchIndex(dice);

			// check if cmd is on to force win
			//		#if UNITY_EDITOR
			//		spinIndex = CmdLineManager.Instance.ForceWinResultType (spinIndex);
			//		#endif

			// jackpot中奖判断逻辑
			string jackpotType = "";
			isJackpotWin = JackpotWinManager.Instance.CheckJackpotWin(_spinRoller, 
				ref jackpotType, _coreMachine.Name, spinInput);
			
			#if DEBUG && DEBUG_FORCE_JACKPOT
			isJackpotWin = true;
			#endif

			if(isJackpotWin)
			{
				CoreDebugUtility.Assert(_machineConfig.BasicConfig.IsJackpot);
				spinType = SpinResultType.Win;
			}
		}

		switch(spinType)
		{
			case SpinResultType.Win:
				{
					float totalProb = 0.0f;
					RollHelper winRollHelper = null;

					// free spin
					if(spinInput.FreeSpinData != null)
					{
						totalProb = _curPayoutDistConfig.FreeSpinTotalProb;
						winRollHelper = _curFreeSpinWinRollHelper;
					}
					else
					{
						totalProb = _curPayoutDistConfig.TotalProb;
						winRollHelper = _curWinRollHelper;
					}

					float winRatio = _spinRoller.NextFloat(0.0f, totalProb);
					int index = winRollHelper.FetchIndex(winRatio);

					// jackpot中奖data判断
					if (isJackpotWin)
					{
						int jackpotIndex = JackpotWinManager.Instance.GetJackpotMultilineData(_spinRoller);
						if(jackpotIndex >= 0)
							index = jackpotIndex;
					}
				
					payoutId = _curPayoutDistConfig.JoyDataArray[index].Id;
					int totalLength = _curPayoutDistConfig.Sheet.dataArray.Length;
					if(payoutId < _curPayoutDistConfig.Sheet.dataArray[0].Id || payoutId > _curPayoutDistConfig.Sheet.dataArray[totalLength - 1].Id)
					{
						CoreDebugUtility.LogError("RollDistIndex: roll payoutId out of range");
						CoreDebugUtility.Assert(false);
					}

//					#if UNITY_EDITOR
//					index = CmdLineManager.Instance.ForceWin (this, index);
//					if (CmdLineManager.Instance.EnableForceWin) {
//						CoreDebugUtility.Log("<color=red>force win index : </color>" + index);
//					}
//					#endif
				}
				break;

			case SpinResultType.NearHit:
				{
					//don't forget to subtract _curPayoutConfig.TotalProb
					float nearHitRatio = _spinRoller.NextFloat (0.0f,  _curNearHitDistConfig.TotalProb);
					CoreDebugUtility.Assert(nearHitRatio >= 0 && nearHitRatio < _curNearHitDistConfig.TotalProb);

					int index = _curNearHitRollHelper.FetchIndex(nearHitRatio);
					nearHitId = _curNearHitDistConfig.JoyDataArray[index].Id;
					int totalLength = _curNearHitDistConfig.Sheet.dataArray.Length;
					if(nearHitId < _curNearHitDistConfig.Sheet.dataArray[0].Id || nearHitId > _curNearHitDistConfig.Sheet.dataArray[totalLength - 1].Id)
					{
						CoreDebugUtility.LogError("RollDistIndex: roll nearHitId out of range");
						CoreDebugUtility.Assert(false);
					}

//					#if UNITY_EDITOR
//					index = CmdLineManager.Instance.ForceWin (this, index);
//					if (CmdLineManager.Instance.EnableForceWin) {
//						CoreDebugUtility.Log("<color=red>force win index : </color>" + index);
//					}
//					#endif
				}
				break;

			case SpinResultType.Loss:
				break;

			default:
				CoreDebugUtility.LogError("spinIndex is rolled out of range");
				break;
		}
	}

	RewardResultData RollRewardResult(int payoutId, int nearHitId)
	{
		RewardResultConfig config = _machineConfig.RewardResultConfig;
		List<int> optionIndexes = new List<int>();
		for(int i = 0; i < config.Sheet.dataArray.Length; i++)
		{
			RewardResultData data = config.Sheet.dataArray[i];
			if(data.PayoutId == payoutId && data.NearHitId == nearHitId)
				optionIndexes.Add(i);
		}

		if(optionIndexes.Count == 0)
		{
			string errorStr = string.Format("Fail to find the rows in RewardResult with PayoutId:{0} and NearHitId:{1}", payoutId, nearHitId);
			CoreDebugUtility.Assert(false, errorStr);
		}

		int index = RandomUtility.RollSingleElement(_spinRoller, optionIndexes);

		#if DEBUG && DEBUG_TRIGGER_WIN
		if(_curDebugTriggerIndex < _debugTriggerPayoutIndexes.Length)
		{
			index = _debugTriggerPayoutIndexes[_curDebugTriggerIndex];
			++_curDebugTriggerIndex;
		}
		#endif

		RewardResultData result = config.Sheet.dataArray[index];
		return result;
	}

	CoreSpinResult ConstructSpinResult(RewardResultData data, bool isJackpotWin = false, CoreSpinInput spinInput = null)
	{
		SpinResultType type = SpinResultType.None;
		if(data.PayoutId > 0)
			type = SpinResultType.Win;
		else if(data.NearHitId > 0)
			type = SpinResultType.NearHit;
		else
			type = SpinResultType.Loss;
		
		CoreSpinResult result = new CoreSpinResult(type, _coreMachine, spinInput);
		int[] stopIndexes = _rewardResultConfig.GetStopIndexes(data);
		result.IsJackpotWin = isJackpotWin;
		result.SetStopIndexes(stopIndexes);
		result.MultiLineCheckResult = _checker.CheckResultWithStopIndexes(stopIndexes);
		result.MultiLineRewardResultData = data;
		result.FillMultiLineWinRatio();
		result.FillFinalCoreSymbol (_coreMachine);
		result.SetJackpotWinMultiline (spinInput);
		return result;
	}

	protected override CoreSpinResult RollShortLuckySpinResult(CoreSpinInput spinInput)
	{
		int payoutId = 0, nearHitId = 0;
		bool isJackpotWin = false;
		int index = _shortLuckyWinRollHelper.RollIndex(_spinRoller);
		CoreDebugUtility.Assert(index >= 0 && index < _shortLuckyPayoutDistDatas.Count);
		payoutId = _shortLuckyPayoutDistDatas[index].Id;
		RewardResultData resultData = RollRewardResult(payoutId, nearHitId);

		CoreSpinResult result = ConstructSpinResult(resultData, isJackpotWin, spinInput);
		return result;
	}

	#endregion

	#region Lucky

	protected override void PreRoll()
	{
		if(_luckyManager.Mode == CoreLuckyMode.Normal)
		{
			_curPayoutDistConfig = _payoutDistConfig;
			_curNearHitDistConfig = _nearHitDistConfig;

			_curSpinRollHelper = _spinRollHelper;
			_curWinRollHelper = _winRollHelper;
			_curNearHitRollHelper = _nearHitRollHelper;

			if(_machineConfig.BasicConfig.HasFreeSpin)
				_curFreeSpinWinRollHelper = _freeSpinWinRollHelper;
		}
		else
		{
			_curPayoutDistConfig = _luckyPayoutDistConfig;
			_curNearHitDistConfig = _luckyNearHitDistConfig;

			_curSpinRollHelper = _luckySpinRollHelper;
			_curWinRollHelper = _luckyWinRollHelper;
			_curNearHitRollHelper = _luckyNearHitRollHelper;

			if(_machineConfig.BasicConfig.HasFreeSpin)
				_curFreeSpinWinRollHelper = _luckyFreeSpinWinRollHelper;
		}
	}

	#endregion


	#region pay protection

	private RollHelper GetHitRateIncreaseAfterPayRollHelper(RollHelper helper, float dice, CoreSpinInput spinInput){
		SpinResultType spinIndex = (SpinResultType)helper.FetchIndex (dice);
		if ((UserBasicData.Instance.PayProtectionEnable || spinInput.IsPayProtectionTest) && spinIndex != SpinResultType.Win) {
			float winProb = _curPayoutDistConfig.TotalProb + _coreMachine.PayProtectionHitRate;
			float nearHitProb = _curNearHitDistConfig.TotalProb;
			float lossProb = 1.0f - winProb - nearHitProb;

			float[] probArray = new float[]{winProb, nearHitProb, lossProb};
			CoreDebugUtility.Log ("multi GetHitRateIncreaseAfterPayRollHelper winProb nearHitProb lossProb is "+winProb + " " + nearHitProb + " " +lossProb);
			RollHelper hitRateIncreaseHelper = new RollHelper (probArray);

			return hitRateIncreaseHelper;
		} else {
			return helper;
		}
	}

	#endregion
}
