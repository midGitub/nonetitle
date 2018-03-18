using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineTestInput
{
	public long _credit;
	public ulong _betAmount;
	public int _lucky;
	public bool _isRespin;
	public bool _isPayProtection;
}

public class MachineTestOutput
{
	public CoreSpinResult _spinResult;
	public long _creditChange;
	public long _remainCredit;
	public int _luckyChange;
	public int _remainLucky;
	public bool _shouldRespin;
	public bool _isChangeToSpecialSmallGameState;
	public bool _isPayProtection;
	public bool _isTriggerIndieGame;
	public ulong _indieGameWinAmount;
	public string _indieGameCustomData = "";
}

public class MachineTestRoundResult
{
	public MachineTestInput _input;
	public MachineTestOutput _output;

	public MachineTestRoundResult(MachineTestInput input, MachineTestOutput output)
	{
		_input = input;
		_output = output;
	}
}

public class MachineTestRound
{
	CoreMachine _machine;
	MachineTestConfig _config;
	MachineTestIndieGameManager _indieGameManager;

	public MachineTestRound(CoreMachine machine, MachineTestConfig config, MachineTestIndieGameManager indieGameManager)
	{
		_machine = machine;
		_config = config;
		_indieGameManager = indieGameManager;
	}

	private CoreSpinResult Spin(MachineTestInput input)
	{
		CoreSpinInput spinInput = new CoreSpinInput(input._betAmount, _machine.MachineConfig.BasicConfig.ReelCount, input._isRespin);
		spinInput.IsPayProtectionTest = input._isPayProtection;
		CoreSpinResult spinResult = _machine.Spin(spinInput);

		_machine.CheckSwitchSmallGameState(spinResult, SmallGameMomentType.Front);
		_machine.CheckSwitchSmallGameState(spinResult, SmallGameMomentType.Behind);

		return spinResult;
	}

	public MachineTestOutput Run(MachineTestInput input)
	{
		CoreSpinResult spinResult = Spin(input);
		MachineTestIndieGameResult indieGameResult = _indieGameManager.Run(input);
		MachineTestOutput output = ConstructOutput(input, spinResult, indieGameResult);
		return output;
	}

	public bool CanRun(MachineTestInput input)
	{
		bool result = true;
		if(_config._stopCredit >= 0)
			result = input._credit >= _config._stopCredit;
		return result;
	}

	private MachineTestOutput ConstructOutput(MachineTestInput input, CoreSpinResult spinResult, 
		MachineTestIndieGameResult indieGameResult)
	{
		MachineTestOutput output = new MachineTestOutput();
		output._spinResult = spinResult;

		//when respin, not subtract
		output._creditChange = (input._isRespin) ? 0 : -(int)input._betAmount;

		if(spinResult.Type == SpinResultType.Win)
			output._creditChange += (int)spinResult.WinAmount;

		//consider indie game
		if(indieGameResult != null)
			output._creditChange += (long)indieGameResult._winAmount;

		output._remainCredit = input._credit + output._creditChange;

		output._luckyChange = _machine.LuckyManager.LongLuckyManager.GetSubtractLongLucky(spinResult);
		output._remainLucky = input._lucky - output._luckyChange;
		if(output._remainLucky < 0)
			output._remainLucky = 0;

		output._shouldRespin = _machine.ShouldRespin();
		if(_machine.LastSmallGameState == SmallGameState.None && _machine.SmallGameState != SmallGameState.None)
			output._isChangeToSpecialSmallGameState = true;

		output._isPayProtection = input._isPayProtection;

		if(indieGameResult != null)
		{
			output._isTriggerIndieGame = true;
			output._indieGameWinAmount = indieGameResult._winAmount;
			output._indieGameCustomData = indieGameResult._customData;
		}

		return output;
	}

	private ulong GetBetAmount(int curCredit)
	{
		ulong result = 0;
		if(_config._betMode == MachineTestBetMode.FixBetAmount)
			result = (ulong)_config._betAmount;
		else if(_config._betMode == MachineTestBetMode.FixBetPercentage)
			result = (ulong)(curCredit * _config._betPercentage / 100.0f);
		else
			Debug.Assert(false);
		return result;
	}

	public MachineTestInput ConstructInput(MachineTestOutput output)
	{
		MachineTestInput input = new MachineTestInput();
		if(output != null)
		{
			input._credit = output._remainCredit;
			input._lucky = output._remainLucky;
			input._isRespin = output._shouldRespin;
			input._isPayProtection = output._isPayProtection;
		}
		else
		{
			input._credit = _config._initCredit;
			input._lucky = _config._initLucky;
			input._isRespin = false;
			input._isPayProtection = _config._isPayProtectionEnable;
		}

		if(output != null && output._spinResult.IsRespin)
		{
			//keep betAmount
			input._betAmount = output._spinResult.BetAmount;
		}
		else
		{
			input._betAmount = GetBetAmount((int)input._credit);
		}

		return input;
	}
}
