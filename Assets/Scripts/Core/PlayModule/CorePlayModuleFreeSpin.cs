using System.Collections;
using System.Collections.Generic;

public class CorePlayModuleFreeSpin : CorePlayModule
{
	private CoreFreeSpinData _freeSpinData;
	private int _respinCount;
	private FreeSpinType _freespinType;
	private bool _isTriggerAgain;

	public CoreFreeSpinData FreeSpinData { get { return _freeSpinData; } }
	public int RespinCount { get { return _respinCount; } }
	public bool IsTriggerAgain { get { return _isTriggerAgain; } }

	public CorePlayModuleFreeSpin(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
		:base(state, machine, generator){

		_freeSpinData = null;
		_respinCount = 0;
		_freespinType = _machineConfig.BasicConfig.FreeSpinType;
		_momentType = SmallGameMomentType.Front;
	}

	public override CoreSpinResult SpinHandler (CoreSpinInput spinInput)
	{
		_freeSpinData.RespinCount = _respinCount;
		spinInput.FreeSpinData = _freeSpinData;
		if(_machineConfig.BasicConfig.IsFreeSpinFixBonus)
		{
			spinInput.FixedSymbols = CoreUtility.FetchSymbols(_coreMachine.SpinResult.SymbolList, (CoreSymbol s) => {
				return _machineConfig.SymbolConfig.IsMatchSymbolType(s.SymbolData.SymbolType, SymbolType.Bonus);
			}, _machineConfig.BasicConfig.ReelCount);
		}

		CoreSpinResult result = _coreGenerator.Roll (spinInput);

		return result;
	}

	public override bool ShouldRespin ()
	{
		return true;
	}

	public override bool IsTriggerSmallGameState(){
		bool result =( _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.FreeSpin) || _isTriggerAgain;
		return result;
	}

	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult)
	{
		_coreMachine.SaveLastSmallGameState();

		_isTriggerAgain = false;

		if(_freespinType == FreeSpinType.FixCount)
		{
			if(CheckTriggerFreeSpin(_machineConfig, spinResult))
			{
				AddTotalFixCount(spinResult);
				_isTriggerAgain = true;
			}
			
			if(_respinCount >= _freeSpinData.TotalFixCount)
				_coreMachine.ChangeSmallGameState(SmallGameState.None);
		}
		else if(_freespinType == FreeSpinType.SpinUntilLose)
		{
			if(!spinResult.IsWinWithNonZeroRatio())
				_coreMachine.ChangeSmallGameState(SmallGameState.None);
		}
		else if(_freespinType == FreeSpinType.ReachBonusWild)
		{
			if(CheckTriggerStop(_machineConfig, spinResult, _machineConfig.BasicConfig.FreeSpinStopSymbolNames, _machineConfig.BasicConfig.FreeSpinStopCountOfBonusWild))
				_coreMachine.ChangeSmallGameState(SmallGameState.None);
		}
		else
		{
			CoreDebugUtility.Assert(false);
			if(!spinResult.IsWinWithNonZeroRatio())
				_coreMachine.ChangeSmallGameState(SmallGameState.None);
		}

		RefreshTriggerInfo(spinResult);

		return _coreMachine.LastSmallGameState != _coreMachine.SmallGameState;
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		return false;
	}

	public override void Enter ()
	{
		InitFreeSpinData(_coreMachine.SpinResult);

		RefreshTriggerInfo(_coreMachine.SpinResult);
	}

	public override void Exit ()
	{
		ResetFreeSpinData ();
		ClearRespinCount ();

		ClearTriggerInfo();
	}

	void InitFreeSpinData(CoreSpinResult spinResult)
	{
		float[] freeSpinHits = null;
		float[] freeSpinStopProbs = null;
		if(spinResult.Type == SpinResultType.Win)
		{
			if(_machineConfig.BasicConfig.IsMultiLine)
			{
				// For multiLine, freeSpinHits is not in Payout sheet, but in Basic sheet
				if(_coreMachine.LuckyManager.Mode == CoreLuckyMode.Normal)
					freeSpinHits = _machineConfig.BasicConfig.FreeSpinHits;
				else
					freeSpinHits = _machineConfig.BasicConfig.LuckyFreeSpinHits;

				freeSpinStopProbs = null;
			}
			else
			{
				freeSpinHits = spinResult.PayoutData.FreeSpinHits;
				freeSpinStopProbs = spinResult.PayoutData.FreeSpinStopProbs;
			}
		}
		else
		{
			//CoreDebugUtility.Assert(false);
			freeSpinHits = _machineConfig.BasicConfig.FreeSpinHitsNonWin;
			freeSpinStopProbs = null;
		}

		int fixCount = GetTriggerFixCount(_machineConfig, spinResult);
		_freeSpinData = new CoreFreeSpinData(freeSpinHits, freeSpinStopProbs, fixCount);
		_freeSpinData.InitForceWinIndexList (_coreMachine.Roller, _machineConfig.BasicConfig.FreeSpinForceWinTime);
	}

	void ResetFreeSpinData(){
		_freeSpinData = null;
	}

	void ClearRespinCount(){
		_respinCount = 0;
	}

	void IncreaseRespinCount(){
		++_respinCount;
	}

	void AddTotalFixCount(CoreSpinResult spinResult)
	{
		int fixCount = GetTriggerFixCount(_machineConfig, spinResult);
		_freeSpinData.AddTotalFixCount(fixCount);
	}

	public override void TryStartRound(){
		ClearRespinCount ();
	}

	public override void StartRespin(){
		IncreaseRespinCount ();
	}

	void RefreshTriggerInfo(CoreSpinResult spinResult)
	{
		_triggerPayoutData = null;
		_triggerSymbols.Clear();

		//we only need to consider UnorderCount here
		if(_triggerType == TriggerType.UnorderCount)
		{
			CoreDebugUtility.Assert(_machineConfig.BasicConfig.FreeSpinTriggerSymbolNames.Length > 0);

			_triggerSymbols = CoreHelper.GetSymbolsForAllVisibleStopIndexes(_machineConfig, spinResult, (string name) => {
				bool r = _machineConfig.BasicConfig.FreeSpinTriggerSymbolNames.Contains(name);
				return r;
			});
		}
	}

	void ClearTriggerInfo()
	{
		_triggerPayoutData = null;
		_triggerSymbols.Clear();
	}

	#region Static methods

	public static bool CheckTriggerFreeSpin(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		bool result = false;

		if(spinResult.IsMultiLine)
			result = CheckTriggerFreeSpinOfMultiLine(machineConfig, spinResult);
		else
			result = CheckTriggerFreeSpinOfSingleLine(machineConfig, spinResult);

		return result;
	}

	static bool CheckTriggerFreeSpinOfSingleLine(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		List<string> symbolNames = spinResult.GetSymbolNameList();
		bool result = IsTriggerFreeSpinForSymbolNames(machineConfig, symbolNames);
		return result;
	}

	static bool CheckTriggerFreeSpinOfMultiLine(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		bool result = false;
		if(machineConfig.BasicConfig.TriggerType == TriggerType.UnorderCount)
		{
			int totalCount = CorePlayModuleFreeSpin.GetFreeSpinTriggerUnorderCount(machineConfig, spinResult);
			result = totalCount >= machineConfig.BasicConfig.FreeSpinTriggerCount;
		}
		else if(machineConfig.BasicConfig.TriggerType == TriggerType.Payout)
		{
			MultiLineMatchInfo findInfo = ListUtility.FindFirstOrDefault(spinResult.MultiLineCheckResult.PayoutInfos,
				(MultiLineMatchInfo info) => {
					string[] symbolNames = machineConfig.ReelConfig.GetSymbolNames(info.StopIndexes);
					bool isTrigger = IsTriggerFreeSpinForSymbolNames(machineConfig, symbolNames);
					return isTrigger;
				});

			result = findInfo != null;
		}
		else
		{
			CoreDebugUtility.Assert(false, "TriggerType in MultiLine FreeSpin can be Payout or UnorderCount");
		}

		return result;
	}

	static bool IsTriggerFreeSpinForSymbolNames(MachineConfig machineConfig, IList<string> symbolNames)
	{
		int triggerCount = ListUtility.CountElements(symbolNames, (string name) => {
			return CorePlayModuleHelper.IsSymbolTriggerFreeSpin(machineConfig, name);
		});

		return machineConfig.BasicConfig.FreeSpinTriggerCount != 0 && triggerCount >= machineConfig.BasicConfig.FreeSpinTriggerCount;
	}

	static int GetFreeSpinTriggerUnorderCount(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		CoreDebugUtility.Assert(machineConfig.BasicConfig.TriggerType == TriggerType.UnorderCount, 
			"This function should only be called TriggerType.UnorderCount");

		int result = CoreHelper.GetIndexCountForAllVisibleStopIndexes(machineConfig, spinResult.StopIndexes, (string symbolName) => {
			bool r = CorePlayModuleHelper.IsSymbolTriggerFreeSpin(machineConfig, symbolName);
			return r;
		});
		return result;
	}

	static int GetTriggerFixCount(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		int result = 0;
		if(machineConfig.BasicConfig.FreeSpinType == FreeSpinType.FixCount)
		{
			if(machineConfig.BasicConfig.TriggerType == TriggerType.UnorderCount)
			{
				int triggerCount = GetFreeSpinTriggerUnorderCount(machineConfig, spinResult);
				int basicCount = machineConfig.BasicConfig.FreeSpinTriggerCount;
				CoreDebugUtility.Assert(triggerCount >= basicCount);

				int index = triggerCount - basicCount;
				CoreDebugUtility.Assert(index < machineConfig.BasicConfig.FreeSpinFixCounts.Length);
				result = machineConfig.BasicConfig.FreeSpinFixCounts[index];
			}
			else
			{
				result = machineConfig.BasicConfig.FreeSpinFixCounts[0];
			}
		}
		return result;
	}

	static bool CheckTriggerStop(MachineConfig machineConfig, CoreSpinResult spinResult, string[] triggerNames, int reference)
	{
		int triggerCount = ListUtility.CountElements(spinResult.SymbolList, (CoreSymbol symbol) => {
			//differentiate if designate triggerSymbolNames or not
			if(triggerNames != null)
				return ListUtility.IsContainElement(triggerNames, symbol.SymbolData.Name);
			else
				return machineConfig.SymbolConfig.IsMatchSymbolType(symbol.SymbolData.SymbolType, SymbolType.Bonus);
		});
		return triggerCount >= reference;
	}

	#endregion
}
