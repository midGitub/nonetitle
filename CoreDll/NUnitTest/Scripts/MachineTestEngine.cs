using System.Collections;
using System.Collections.Generic;
using System;
using NUnit.Framework;

public class MachineTestEngine
{
	private string _machineName;
	private UserConfig _userConfig;

	public MachineTestEngine(string machineName, UserConfig userConfig)
	{
		_machineName = machineName;
		_userConfig = userConfig;
	}

	public void RunSingleCase()
	{
		CoreMachine machine = InitCoreMachine();
		UserBasicData.Instance.SetLongLucky(_userConfig._initLucky, false);

		MachineTestRound round = new MachineTestRound(machine, _userConfig);
		MachineTestInput input = null;
		MachineTestOutput output = null;

		for(int i = 0; i < _userConfig._spinCount; i++)
		{
			do
			{
				input = round.ConstructInput(output);
				output = round.Run(input);
			} while(output._shouldRespin);
		}
	}

	private CoreMachine InitCoreMachine()
	{
		uint startSeed = _userConfig._startSeed;
		if(_userConfig._isRandomSeed)
		{
			Random rand = new Random();
			startSeed = (uint)rand.Next();
		}

		CoreMachine machine = new CoreMachine(_machineName, startSeed);
		return machine;
	}
}

