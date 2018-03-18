using System.Collections;
using System.Collections.Generic;

public class CorePlayModuleWheel : CorePlayModule
{
	public CorePlayModuleWheel(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
		: base(state, machine, generator){
		_momentType = SmallGameMomentType.Front;
	}

	public override bool IsTriggerSmallGameState(){
		return _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.Wheel;
	}

	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		//		LogUtility.Log ("check small game state wheel", Color.red);
		_coreMachine.SaveLastSmallGameState ();

		// payout
		if (_triggerType == TriggerType.Payout){
			if(CheckTriggerNormal(spinResult))
				_coreMachine.ChangeSmallGameState(SmallGameState.None);
			else
				RefreshTriggerInfo(spinResult);
		}

		return _coreMachine.LastSmallGameState != _coreMachine.SmallGameState;
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		return false;
	}

	public override void Enter ()
	{
		base.Enter ();

		RefreshTriggerInfo(_coreMachine.SpinResult);
	}

	public override void Exit ()
	{
		base.Exit ();

		ClearTriggerInfo();
	}

	bool CheckTriggerNormal(CoreSpinResult spinResult){
		bool result = !CheckTriggerWheel(_machineConfig, spinResult);
		return result;
	}

	void RefreshTriggerInfo(CoreSpinResult spinResult)
	{
		_triggerPayoutData = GetTriggeredWheelPayoutData(_machineConfig, spinResult);
		_triggerSymbols = CorePlayModuleHelper.GetTriggerSymbols(_machineConfig, spinResult, _triggerPayoutData);
	}

	void ClearTriggerInfo()
	{
		_triggerPayoutData = null;
		_triggerSymbols.Clear();
	}

	#region Static methods

	public static bool CheckTriggerWheel(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		PayoutData triggeredData = GetTriggeredWheelPayoutData(machineConfig, spinResult);
		return triggeredData != null;
	}

	public static PayoutData GetTriggeredWheelPayoutData(MachineConfig machineConfig, CoreSpinResult spinResult)
	{
		PayoutData result = null;
		if (spinResult.Type == SpinResultType.Win)
		{
			if(machineConfig.BasicConfig.IsMultiLine)
			{
				MultiLineMatchInfo matchInfo = ListUtility.FindFirstOrDefault(spinResult.MultiLineCheckResult.PayoutInfos,
					(MultiLineMatchInfo info) => {
						return HasWheelNames(info.PayoutData);
					});

				if(matchInfo != null)
					result = matchInfo.PayoutData;
			}
			else
			{
				if(HasWheelNames(spinResult.PayoutData))
					result = spinResult.PayoutData;
			}
		}
		return result;
	}

	static bool HasWheelNames(PayoutData data)
	{
		return data.WheelNames.Length > 0 && !string.IsNullOrEmpty(data.WheelNames[0]);
	}

	#endregion
}
