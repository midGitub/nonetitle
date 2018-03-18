using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MachineTestBetMode
{
	FixBetAmount,
	FixBetPercentage
}

public enum MachineTestSeedMode
{
	Random,
	Fixed
}

[Serializable]
public class MachineTestConfig : ScriptableObject
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

	//machine config
	public string[] _allMachines = CoreDefine.AllMachineNames;
	public bool[] _selectMachines = new bool[CoreDefine.AllMachineNames.Length];

	//pay protection
	public bool _isPayProtectionEnable;
}
