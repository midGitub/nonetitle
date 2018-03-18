using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MachineTestMachineResult
{
	private string _machineName;
	private List<MachineTestUserResult> _userResults = new List<MachineTestUserResult>();
	private MachineTestAnalysisResult _analysisResult;

	public string MachineName { get { return _machineName; } }
	public List<MachineTestUserResult> UserResults { get { return _userResults; } }
	public MachineTestAnalysisResult AnalysisResult { get { return _analysisResult; } set { _analysisResult = value; } }

	public MachineTestMachineResult(string machineName)
	{
		_machineName = machineName;
	}

	public void AddUserResult(MachineTestUserResult userResult)
	{
		_userResults.Add(userResult);
	}
}

