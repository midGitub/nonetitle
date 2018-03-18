using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MachineTestEngine
{
	private MachineTestConfig _config;
	private string _outputDir;

	public void Init(MachineTestConfig config)
	{
		_config = config;
	}

	public void RunSelectedMachines()
	{
		InitMakeOutputDir();

		for(int i = 0; i < _config._allMachines.Length; i++)
		{
			if(_config._selectMachines[i])
			{
				MachineTestMachineResult machineResult = RunSingleMachine(_config._allMachines[i]);
				PrintMachineResultAnalysis(machineResult);
			}
		}
	}

	private void InitMakeOutputDir()
	{
		string date = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
		_outputDir = "Assets/Test/MachineTest/Output/" + date + "/";
		Directory.CreateDirectory(_outputDir);
	}

	public MachineTestMachineResult RunSingleMachine(string machineName)
	{
		MachineTestMachineResult machineResult = new MachineTestMachineResult(machineName);

		for(int i = 0; i < _config._userCount; i++)
		{
			MachineTestUserResult userResult = RunSingleUser(machineName, i);
			machineResult.AddUserResult(userResult);
		}

		return machineResult;
	}

	private MachineTestUserResult RunSingleUser(string machineName, int userIndex)
	{
		uint startSeed = GetUserRandSeed(userIndex);

		// set longLucky first, then init machine
		UserBasicData.Instance.SetLongLucky(_config._initLucky, false);

		// reset pay protection
		bool bakcupPayProtectionState = ResetUserDataPayProtection();
		
		CoreMachine machine = new CoreMachine(machineName, startSeed);

		MachineTestUserResult userResult = RunSingleUserResult(machine, userIndex, startSeed);

		// recovery pay protection
		RecoveryUserDataPayProtection(bakcupPayProtectionState);

		return userResult;
	}

	private MachineTestUserResult RunSingleUserResult(CoreMachine machine, int userIndex, uint startSeed)
	{
		MachineTestUserResult userResult = new MachineTestUserResult(machine, userIndex, _config._spinCount, startSeed);

		MachineTestIndieGameManager indieGameManager = new MachineTestIndieGameManager(machine);
		MachineTestRound round = new MachineTestRound(machine, _config, indieGameManager);
		MachineTestInput input = null;
		MachineTestOutput output = null;
		for(int i = 0; i < _config._spinCount; i++)
		{
			bool canRun = true;
			do
			{
				input = round.ConstructInput(output);
				canRun = round.CanRun(input);
				if(canRun)
				{
					output = round.Run(input);
					MachineTestRoundResult roundResult = new MachineTestRoundResult(input, output);
					userResult.AddRoundResult(roundResult);
				}
				else
				{
					break;
				}
			} while(output._shouldRespin);

			if(!canRun)
				break;
		}

		return userResult;
	}

	private uint GetUserRandSeed(int userIndex)
	{
		uint result = CoreDefine.DefaultMachineRandSeed;
		if(_config._seedMode == MachineTestSeedMode.Random)
		{
			System.Random rand = new System.Random();
			result = (uint)rand.Next();
		}
		else
		{
			result = _config._startSeedForFixedMode + (uint)userIndex;
		}
		return result;
	}

	private void PrintMachineResultAnalysis(MachineTestMachineResult machineResult)
	{
		string machineDir = Path.Combine(_outputDir, machineResult.MachineName) + "/";
		Directory.CreateDirectory(machineDir);

		for(int i = 0; i < machineResult.UserResults.Count; i++)
		{
			MachineTestUserResult userResult = machineResult.UserResults[i];

			MachineTestUserResultPrinter userPrinter = new MachineTestUserResultPrinter(userResult);
			userPrinter.WriteResult(machineDir, MachineTestPrintFileNameMode.UserId);

			MachineTestAnalysisResult singleAnalysisResult = MachineTestAnalysisHelper.Instance.AnalyzeSingleUser(userResult);
			userResult.AnalysisResult = singleAnalysisResult;

			MachineTestAnalysisResultPrinter singleAnalysisPrinter = new MachineTestAnalysisResultPrinter(_config, userResult, singleAnalysisResult);
			singleAnalysisPrinter.WriteResult(machineDir);
		}

		MachineTestAnalysisResult analysisResult = MachineTestAnalysisHelper.Instance.AnalyzeSingleMachine(machineResult, _config._userCount);
		machineResult.AnalysisResult = analysisResult;

		MachineTestAnalysisResultPrinter analysisPrinter = new MachineTestAnalysisResultPrinter(_config, machineResult, analysisResult);
		analysisPrinter.WriteResult(machineDir);
	}

	#region change pay protection

	private bool ResetUserDataPayProtection(){
		bool result = UserBasicData.Instance.PayProtectionEnable;
		UserBasicData.Instance.DisablePayProtection();
		return result;
	}

	private void RecoveryUserDataPayProtection(bool recovery){
		UserBasicData.Instance.PayProtectionEnable = recovery;
	}

	#endregion
}
