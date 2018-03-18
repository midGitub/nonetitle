using System.Collections;
using System.Collections.Generic;
using System;

// Note: The functions CheckTriggerXYZ could be refactored later. The Check logic should be
// moved to respective modules. A completed example is CheckTriggerFreeSpin.
public class CorePlayModuleNormal : CorePlayModule
{
	private CoreCollectSpinData _collectSpinData;

	public CorePlayModuleNormal(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
		:base(state, machine, generator)
	{
		Init ();
	}

	public void Init(){
		if (_triggerType == TriggerType.Collect) {
			_collectSpinData = new CoreCollectSpinData (_coreMachine.Name, 0, _machineConfig.BasicConfig.CollectNum, 100UL);
		}
		_momentType = SmallGameMomentType.Front | SmallGameMomentType.Behind;
	}

	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		bool isSwitched = false;
		_coreMachine.SaveLastSmallGameState();

		// freespin
		if(_machineConfig.BasicConfig.HasFreeSpin)
		{
			if(CheckTriggerFreeSpin(spinResult))
			{
				_coreMachine.ChangeSmallGameState(SmallGameState.FreeSpin);
				isSwitched = true;
			}
		}

		// rewind
		if(!isSwitched && _machineConfig.BasicConfig.HasRewind)
		{
			if(CheckTriggerRewind(spinResult))
			{
				_coreMachine.ChangeSmallGameState(SmallGameState.Rewind);
				isSwitched = true;
			}
		}

		// fixwild
		if(!isSwitched && _machineConfig.BasicConfig.HasFixWild)
		{
			Predicate<CoreSpinResult> checkFunc = null;
			if(_machineConfig.BasicConfig.FixWildType == FixWildType.FishHunting)
				checkFunc = CheckTriggerFixwildFishHunting;
			else
				checkFunc = CheckTriggerFixwild;

			if(checkFunc(spinResult))
			{
				_coreMachine.ChangeSmallGameState(SmallGameState.FixWild);
				isSwitched = true;
			}
		}

		// collect
		if(!isSwitched && _triggerType == TriggerType.Collect)
		{
			List<CoreCollectData> paylineList = spinResult.GetPayLineCollectList();
			_collectSpinData.AddCollectNum(paylineList.Count);

			if(_collectSpinData.IsFinishCollect())
			{
				if(_machineConfig.BasicConfig.HasFreeSpin)
				{
					_coreMachine.ChangeSmallGameState(SmallGameState.FreeSpin);
					isSwitched = true;
				}
			}
		}

		// payout
		if(!isSwitched && (_triggerType == TriggerType.Payout || _triggerType == TriggerType.UnorderCount))
		{
			if(_machineConfig.BasicConfig.HasWheel && CheckTriggerWheel(spinResult))
			{
				_coreMachine.ChangeSmallGameState(SmallGameState.Wheel);
				isSwitched = true;
			}
			else if(_machineConfig.BasicConfig.HasTapBox && CheckTriggerTapBox(spinResult))
			{
				_coreMachine.ChangeSmallGameState(SmallGameState.TapBox);
				isSwitched = true;
			}
			else if (_machineConfig.BasicConfig.HasSwitchSymbol && CheckTriggerSwitchSymbol(spinResult))
			{
				_coreMachine.ChangeSmallGameState(SmallGameState.SwitchSymbol);
				isSwitched = true;
			}
		}

		return isSwitched;
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult)
	{
		bool isSwitched = false;

		if(_triggerType == TriggerType.Collect)
		{
			List<CoreCollectData> paylineList = spinResult.GetPayLineCollectList();
			if(_collectSpinData.IsFinishCollect())
			{
				if(_machineConfig.BasicConfig.HasTapBox)
				{
					_coreMachine.ChangeSmallGameState(SmallGameState.TapBox);
					isSwitched = true;
				}
			}
		}

		return isSwitched;
	}

	private bool CheckTriggerRewind(CoreSpinResult spinResult)
	{
		bool result = false;
		if(spinResult.Type == SpinResultType.Win)
		{
			float[] rewindProbs = spinResult.PayoutData.RewindHits;
			int respinCount = 0;
			int respinIndex = (respinCount < rewindProbs.Length) ? respinCount : (rewindProbs.Length - 1);
			float prob = rewindProbs[respinIndex];
			RollHelper helper = new RollHelper(prob);
			int index = helper.RollIndex(_coreMachine.Roller);
			result = index == 0;
		}
		return result;
	}

	private bool CheckTriggerFreeSpin(CoreSpinResult spinResult)
	{
		bool result = CorePlayModuleFreeSpin.CheckTriggerFreeSpin(_machineConfig, spinResult);
		return result;
	}

	private bool CheckTriggerFixwild(CoreSpinResult spinResult){
		int triggerCount = ListUtility.CountElements (spinResult.SymbolList, (CoreSymbol symbol) => {
			return CorePlayModuleHelper.IsSymbolTriggerFixWild(_machineConfig, symbol.SymbolData.Name);
		});

		if (triggerCount > 0 && triggerCount != _machineConfig.BasicConfig.ReelCount) {
			return true;
		}
		return false;
	}

	private bool CheckTriggerFixwildFishHunting(CoreSpinResult spinResult){
		int triggerCount = ListUtility.CountElements (spinResult.SymbolList, (CoreSymbol symbol) => {
			return CorePlayModuleHelper.IsSymbolTriggerFixWild(_machineConfig, symbol.SymbolData.Name);
		});

		if (triggerCount > 0 && triggerCount != _machineConfig.BasicConfig.ReelCount && spinResult.Type == SpinResultType.Win && spinResult.WinRatio != 0.0f) {
			return true;
		}
		return false;
	}

	private bool CheckTriggerWheel(CoreSpinResult spinResult){
		bool result = CorePlayModuleWheel.CheckTriggerWheel(_machineConfig, spinResult);
		return result;
	}

	private bool CheckTriggerTapBox(CoreSpinResult spinResult)
	{
		bool result = ListUtility.IsAllElementsSatisfied(spinResult.SymbolList, (CoreSymbol symbol) => {
			return CorePlayModuleHelper.IsSymbolTriggerTapBox(_machineConfig, symbol.SymbolData.Name);
		});
		return result;
	}

	private bool CheckTriggerSwitchSymbol(CoreSpinResult spinResult){
		int triggerCount = ListUtility.CountElements(spinResult.SymbolList, (CoreSymbol symbol) => {
			return CorePlayModuleHelper.IsSymbolTriggerSwitchSymbol(_machineConfig, symbol.SymbolData.Name);
		});

		CoreDebugUtility.Log("check Trigger Switch Symbol count = "+triggerCount);
		return _machineConfig.BasicConfig.SymbolSwitchTriggerCount != 0 && triggerCount >= _machineConfig.BasicConfig.SymbolSwitchTriggerCount && spinResult.Type == SpinResultType.Win;
	}

	public override void Enter(){
	}

	public override void Exit(){
		if (_triggerType == TriggerType.Collect){
			_collectSpinData.ResetFinishCollect();
		}
	}

	public override CoreSpinResult SpinHandler (CoreSpinInput spinInput)
	{
		if (_triggerType == TriggerType.Collect){
			ulong betAmount = spinInput.BetAmount;
			int collectNum = UserMachineData.Instance.GetBetCollectNum (_coreMachine.Name, betAmount);
			SetCurrentCollectParam (betAmount, collectNum);
		}
		return base.SpinHandler (spinInput);
	}

	public void SetCurrentCollectParam(ulong betAmount, int collectNum){
		_collectSpinData.ChangeBetAmount (betAmount);
		_collectSpinData.ChangelCollectNum (collectNum);
	}
}