using System.Collections;
using System.Collections.Generic;

public class CorePlayModuleTapBox : CorePlayModule
{
	public CorePlayModuleTapBox(SmallGameState state, CoreMachine machine, ICoreGenerator generator, SmallGameMomentType momentType)
		: base(state, machine, generator){
		_momentType = momentType;
	}

	public override bool IsTriggerSmallGameState(){
		return _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.TapBox;
	}

	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		if(_momentType == SmallGameMomentType.Front)
		{
			_coreMachine.SaveLastSmallGameState();
			_coreMachine.ChangeSmallGameState(SmallGameState.None);
			return _coreMachine.LastSmallGameState != _coreMachine.SmallGameState;
		}
		else if(_momentType == SmallGameMomentType.Behind)
		{
			return false;
		}
		else
		{
			CoreDebugUtility.Assert(false);
			return false;
		}
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		_coreMachine.SaveLastSmallGameState();
		_coreMachine.ChangeSmallGameState(SmallGameState.None);
		return _coreMachine.LastSmallGameState != _coreMachine.SmallGameState;
	}

	public override void Enter ()
	{
		base.Enter();

		RefreshTriggerInfo(_coreMachine.SpinResult);
	}

	public override void Exit ()
	{
		base.Exit();

		ClearTriggerInfo();
	}

	void RefreshTriggerInfo(CoreSpinResult spinResult)
	{
		//not support multi line now
		CoreDebugUtility.Assert(!spinResult.IsMultiLine);

		if(_coreMachine.MachineConfig.BasicConfig.TriggerType == TriggerType.Payout)
		{
			_triggerPayoutData = spinResult.PayoutData;
			_triggerSymbols = CorePlayModuleHelper.GetTriggerSymbols(_machineConfig, spinResult, _triggerPayoutData);
		}
	}

	void ClearTriggerInfo()
	{
		_triggerPayoutData = null;
		_triggerSymbols.Clear();
	}
}
