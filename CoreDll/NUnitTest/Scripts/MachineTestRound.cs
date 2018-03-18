using System.Collections;
using System.Collections.Generic;
using System;
using NUnit.Framework;

public class MachineTestInput
{
	public int _credit;
	public int _betAmount;
	public int _lucky;
	public bool _isRespin;
}

public class MachineTestOutput
{
	public CoreSpinResult _spinResult;
	public int _creditChange;
	public int _remainCredit;
	public int _luckyChange;
	public int _remainLucky;
	public bool _shouldRespin;
	public bool _isChangeToSpecialSmallGameState;
}

public class MachineTestRound
{
	private CoreMachine _machine;
	private UserConfig _userConfig;

	public MachineTestRound(CoreMachine machine, UserConfig userConfig)
	{
		_machine = machine;
		_userConfig = userConfig;
	}

	public MachineTestOutput Run(MachineTestInput input)
	{
		CoreSpinResult spinResult = SpinWithSmallGameState(input);
		MachineTestOutput output = ConstructOutput(input, _machine, spinResult);
		CheckResult(input, output);
		return output;
	}

	private CoreSpinResult SpinWithSmallGameState(MachineTestInput input)
	{
		CoreSpinInput spinInput = new CoreSpinInput((ulong)input._betAmount, _machine.MachineConfig.BasicConfig.ReelCount, input._isRespin);
		CoreSpinResult spinResult = _machine.Spin(spinInput);
		_machine.CheckSwitchSmallGameState(spinResult);

		return spinResult;
	}

	private MachineTestOutput ConstructOutput(MachineTestInput input, CoreMachine machine, CoreSpinResult spinResult)
	{
		MachineTestOutput output = new MachineTestOutput();
		output._spinResult = spinResult;

		//when respin, not subtract
		output._creditChange = (input._isRespin) ? 0 : -input._betAmount;

		if(spinResult.Type == SpinResultType.Win)
			output._creditChange += (int)spinResult.WinAmount;

		output._remainCredit = input._credit + output._creditChange;

		output._luckyChange = _machine.LuckyManager.LongLuckyManager.GetSubtractLongLucky(spinResult);
		output._remainLucky = input._lucky - output._luckyChange;
		if(output._remainLucky < 0)
			output._remainLucky = 0;

		output._shouldRespin = machine.ShouldRespin();
		if(machine.LastSmallGameState == SmallGameState.None && machine.SmallGameState != SmallGameState.None)
			output._isChangeToSpecialSmallGameState = true;

		return output;
	}

	public MachineTestInput ConstructInput(MachineTestOutput output)
	{
		MachineTestInput input = new MachineTestInput();
		if(output != null)
		{
			input._credit = output._remainCredit;
			input._lucky = output._remainLucky;
			input._isRespin = output._shouldRespin;
		}
		else
		{
			input._credit = _userConfig._initCredits;
			input._lucky = _userConfig._initLucky;
			input._isRespin = false;
		}

		if(output != null && output._spinResult.IsRespin)
		{
			//keep betAmount
			input._betAmount = (int)output._spinResult.BetAmount;
		}
		else
		{
			input._betAmount = _userConfig._betAmount;
		}

		return input;
	}

	private void CheckResult(MachineTestInput input, MachineTestOutput output)
	{
		Assert.AreEqual(output._spinResult.WinAmount, (ulong)output._luckyChange);
	}
}

