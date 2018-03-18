using System.Collections;
using System.Collections.Generic;
using System;

public abstract class CoreMultiLineBaseGenerator : ICoreGenerator
{
	protected IRandomGenerator _spinRoller;

	protected CoreMachine _coreMachine;
	protected CoreMultiLineChecker _checker;
	protected CoreLuckyManager _luckyManager;
	protected CoreForceWinManager _forceWinManager;
	protected MachineConfig _machineConfig;

	protected void Init(CoreMachine coreMachine, CoreMultiLineChecker checker, CoreLuckyManager luckyManager, MachineConfig machineConfig)
	{
		_coreMachine = coreMachine;
		_spinRoller = _coreMachine.Roller;
		_checker = checker;
		_luckyManager = luckyManager;
		_machineConfig = machineConfig;

		InitForceWinManager();
	}

	void InitForceWinManager()
	{
		_forceWinManager = new CoreForceWinManager(_machineConfig, _coreMachine);
	}

	public CoreSpinResult Roll(CoreSpinInput spinInput)
	{
		_luckyManager.RefreshMode(spinInput.BetAmount);
		PreRoll();
		CoreSpinResult spinResult = RollSpinResult(spinInput);
		return spinResult;
	}

	protected CoreSpinResult RollSpinResult(CoreSpinInput spinInput)
	{
		CoreSpinResult result = null;
		CoreLuckyMode luckyMode = _luckyManager.Mode;

		if(luckyMode == CoreLuckyMode.Normal || luckyMode == CoreLuckyMode.LongLucky)
		{
			result = RollGeneralSpinResult(spinInput);
		}
		else if(luckyMode == CoreLuckyMode.ShortLucky)
		{
			result = RollShortLuckySpinResult(spinInput);
		}
		else
		{
			CoreDebugUtility.Assert(false);
		}

		result.FeedBetAndWinAmount(spinInput.BetAmount);
		result.FeedLuckyMode(luckyMode);
		result.RefreshWinSymbolFlagsList();
		result.FillNonPayoutLineSymbolList();
		_forceWinManager.FeedSpinResult(result);

		return result;
	}

	#region Abstract

	abstract protected void PreRoll();
	abstract protected CoreSpinResult RollGeneralSpinResult(CoreSpinInput spinInput);
	abstract protected CoreSpinResult RollShortLuckySpinResult(CoreSpinInput spinInput);

	#endregion
}

