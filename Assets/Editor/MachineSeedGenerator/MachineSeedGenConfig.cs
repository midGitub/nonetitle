using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MachineSeedLimitationType
{
	Bankcrupt,
	CreditRange
}

[System.Serializable]
public class MachineSeedLimitationConfig
{
	public MachineSeedLimitationType _type;

	//used when _type == Bankcrupt
	public int _startSpinCount;
	public int _endSpinCount;

	//used when _type == CreditRange
	public int _spinCount;
	public long _minCredit;
	public long _maxCredit;
}

// This class is very similar to MachineTestConfig, but has difference
public class MachineSeedGenConfig : ScriptableObject
{
	//single user config
	public int _initLucky;
	public long _initCredit;

	public int _spinCount;
	public MachineTestBetMode _betMode;
	public int _betAmount;
	public float _betPercentage;
	public float _minBetAmountInPercentageMode;
	public int _stopCredit; //useless

	//all users config
	public int _userCount;
	public MachineTestSeedMode _seedMode;
	public uint _startSeedForFixedMode = CoreDefine.DefaultMachineRandSeed;

	public string _machineName;

	//pay protection
	public bool _isPayProtectionEnable;

	// Note:
	// The configs should be all satisfied, so they are AND operation
	// To support OR operation or other combinations, a new class should replace this one
	public List<MachineSeedLimitationConfig> _limitConfigs = new List<MachineSeedLimitationConfig>();

	public bool _isOutputUserResult;
	public bool _isOutputSeeds;
}
