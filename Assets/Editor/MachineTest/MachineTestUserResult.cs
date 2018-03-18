using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MachineTestUserResult
{
	private List<MachineTestRoundResult> _roundResults;
	private CoreMachine _machine;
	private int _userIndex;
	private uint _startSeed;
	private MachineTestAnalysisResult _analysisResult;

	public List<MachineTestRoundResult> RoundResults { get { return _roundResults; } }
	public CoreMachine Machine { get { return _machine; } }
	public int UserIndex { get { return _userIndex; } }
	public uint StartSeed { get { return _startSeed; } }
	public MachineTestAnalysisResult AnalysisResult { get { return _analysisResult; } set { _analysisResult = value; } }

	public MachineTestUserResult(CoreMachine machine, int userIndex, int spinCount, uint startSeed)
	{
		_machine = machine;
		_userIndex = userIndex;
		_startSeed = startSeed;
		_roundResults = new List<MachineTestRoundResult>(spinCount);
	}

	public void AddRoundResult(MachineTestRoundResult r)
	{
		_roundResults.Add(r);
	}
}


