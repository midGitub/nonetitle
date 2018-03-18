using System;
using System.Collections.Generic;

public class CoreForceWinManager
{
	MachineConfig _machineConfig;
	CoreMachine _machine;

	int _forceWinPayoutId;
	int _forceWinThresholdForNormal;
	int _forceWinThresholdForLucky;

	int _curSpinCount;

	public int ForceWinPayoutId { get { return _forceWinPayoutId; } }
	public int ForceWinPayoutIndex { get { return _forceWinPayoutId - 1; } }

	public CoreForceWinManager(MachineConfig machineConfig, CoreMachine machine)
	{
		_machineConfig = machineConfig;
		_machine = machine;

		_forceWinPayoutId = machineConfig.BasicConfig.ForceWinPayoutId;
		_forceWinThresholdForNormal = machineConfig.BasicConfig.ForceWinThresholdForNormal;
		_forceWinThresholdForLucky = machineConfig.BasicConfig.ForceWinThresholdForLucky;

		_curSpinCount = 0;
	}

	bool IsRunForceWinLogic()
	{
		return _forceWinPayoutId > 0;
	}

	public void FeedSpinResult(CoreSpinResult spinResult)
	{
		if(IsRunForceWinLogic())
		{
			PayoutData data = _machineConfig.PayoutConfig.Sheet.DataArray[_forceWinPayoutId - 1];
			if(spinResult.IsWinPayoutData(data))
				_curSpinCount = 0;
			else
				++_curSpinCount;
		}
	}

	public bool ShouldForceWin()
	{
		bool result = false;
		if(IsRunForceWinLogic())
		{
			int curThreshold = (_machine.LuckyManager.Mode == CoreLuckyMode.Normal) ? 
				_forceWinThresholdForNormal : _forceWinThresholdForLucky;
			// minus 1 because _curSpinCount is the previous spinCount which doesn't include this one
			result = _curSpinCount >= curThreshold - 1;
		}

		return result;
	}

	public void ClearForceWinFlag()
	{
		_curSpinCount = 0;
	}
}

