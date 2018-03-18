using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MachineSeedGenEngine
{
	static readonly string _outputUserResultDir = "Assets/Editor/MachineSeedGenerator/Output/";
	static readonly string _outputSeedsDir = "Assets/Resources/" + MachineSeedConfig.SeedFileDir;

	MachineTestEngine _testEngine = new MachineTestEngine();

	public void Run(MachineSeedGenConfig genConfig, MachineTestConfig testConfig)
	{
		_testEngine.Init(testConfig);
		MachineTestMachineResult machineResult = _testEngine.RunSingleMachine(genConfig._machineName);
		List<uint> seedList = FilterMachineSeeds(machineResult, genConfig._limitConfigs);
		OutputPrintMachineResult(machineResult, genConfig);
		OutputMachineSeeds(genConfig, seedList);

		Debug.Log("Seed generator engine done");
	}

	public void UpdateGenConfigToTestConfig(MachineSeedGenConfig genConfig, MachineTestConfig testConfig, bool isCheckMachineName)
	{
		testConfig._initLucky = genConfig._initLucky;
		testConfig._initCredit = genConfig._initCredit;
		testConfig._spinCount = genConfig._spinCount;
		testConfig._betMode = genConfig._betMode;

		testConfig._spinCount = genConfig._spinCount;
		testConfig._betMode = genConfig._betMode;
		testConfig._betAmount = genConfig._betAmount;
		testConfig._betPercentage = genConfig._betPercentage;
		testConfig._minBetAmountInPercentageMode = genConfig._minBetAmountInPercentageMode;
		testConfig._stopCredit = genConfig._stopCredit;

		testConfig._userCount = genConfig._userCount;
		testConfig._seedMode = genConfig._seedMode;
		testConfig._startSeedForFixedMode = genConfig._startSeedForFixedMode;

		testConfig._isPayProtectionEnable = genConfig._isPayProtectionEnable;

		if(isCheckMachineName)
		{
			if(testConfig._allMachines.Contains(genConfig._machineName))
			{
				int index = ListUtility.Find(testConfig._allMachines, (string name) => {
					return name == genConfig._machineName;
				});

				Debug.Assert(index >= 0 && index < testConfig._allMachines.Length);

				testConfig._selectMachines[index] = true;
			}
			else
			{
				Debug.LogError("machineName is wrong: " + genConfig._machineName);
			}
		}
	}

	public void UpdateTestConfigToGenConfig(MachineTestConfig testConfig, MachineSeedGenConfig genConfig)
	{
		genConfig._initLucky = testConfig._initLucky;
		genConfig._initCredit = testConfig._initCredit;
		genConfig._spinCount = testConfig._spinCount;
		genConfig._betMode = testConfig._betMode;

		genConfig._spinCount = testConfig._spinCount;
		genConfig._betMode = testConfig._betMode;
		genConfig._betAmount = testConfig._betAmount;
		genConfig._betPercentage = testConfig._betPercentage;
		genConfig._minBetAmountInPercentageMode = testConfig._minBetAmountInPercentageMode;
		genConfig._stopCredit = testConfig._stopCredit;

		genConfig._userCount = testConfig._userCount;
		genConfig._seedMode = testConfig._seedMode;
		genConfig._startSeedForFixedMode = testConfig._startSeedForFixedMode;

		genConfig._isPayProtectionEnable = testConfig._isPayProtectionEnable;

//		int index = ListUtility.Find(testConfig._selectMachines, (bool s) => {
//			return true;
//		});
//
//		if(index >= 0 && index < testConfig._allMachines.Length)
//		{
//			genConfig._machineName = testConfig._allMachines[index];
//		}
//		else
//		{
//			Debug.LogError("Find no machine name");
//		}
	}

	List<uint> FilterMachineSeeds(MachineTestMachineResult machineResult, List<MachineSeedLimitationConfig> limitConfigs)
	{
		List<uint> result = new List<uint>();

		for(int i = 0; i < machineResult.UserResults.Count; i++)
		{
			MachineTestUserResult userResult = machineResult.UserResults[i];
			bool isPass = IsUserResultPassLimitConfigs(userResult, limitConfigs);
			if(isPass)
				result.Add(userResult.StartSeed);
		}

		return result;
	}

	bool IsUserResultPassLimitConfigs(MachineTestUserResult userResult, List<MachineSeedLimitationConfig> limitConfigs)
	{
		bool result = ListUtility.IsAllElementsSatisfied(limitConfigs, (MachineSeedLimitationConfig config) => {
			bool r = IsUserResultPassSingleLimitConfig(userResult, config);
			return r;
		});
		return result;
	}

	bool IsUserResultPassSingleLimitConfig(MachineTestUserResult userResult, MachineSeedLimitationConfig limitConfig)
	{
		bool result = false;
		List<MachineTestRoundResult> roundResults = userResult.RoundResults;

		if(limitConfig._type == MachineSeedLimitationType.Bankcrupt)
		{
			Debug.Assert(limitConfig._startSpinCount < limitConfig._endSpinCount);

			for(int i = limitConfig._startSpinCount; i < limitConfig._endSpinCount && i < roundResults.Count; i++)
			{
				MachineTestRoundResult roundResult = roundResults[i];
				if(roundResult._output._remainCredit <= 0)
				{
					result = true;
					break;
				}
			}
		}
		else if(limitConfig._type == MachineSeedLimitationType.CreditRange)
		{
			if(limitConfig._spinCount <= roundResults.Count)
			{
				MachineTestRoundResult roundResult = roundResults[limitConfig._spinCount - 1];
				long remainCredit = roundResult._output._remainCredit;
				result = (remainCredit >= limitConfig._minCredit && remainCredit <= limitConfig._maxCredit);
			}
			else
			{
				// just treat it as not passed
				result = false;
			}
		}
		else
		{
			Debug.Assert(false);
		}

		return result;
	}

	void OutputPrintMachineResult(MachineTestMachineResult machineResult, MachineSeedGenConfig genConfig)
	{
		if(genConfig._isOutputUserResult)
		{
			string machineDir = Path.Combine(_outputUserResultDir, machineResult.MachineName) + "/";
			if(Directory.Exists(machineDir))
			{
				DirectoryInfo info = new DirectoryInfo(machineDir);
				foreach (FileInfo file in info.GetFiles())
					file.Delete();
			}

			Directory.CreateDirectory(machineDir);

			for(int i = 0; i < machineResult.UserResults.Count; i++)
			{
				MachineTestUserResult userResult = machineResult.UserResults[i];

				MachineTestUserResultPrinter userPrinter = new MachineTestUserResultPrinter(userResult);
				userPrinter.WriteResult(machineDir, MachineTestPrintFileNameMode.Seed);
			}
		}
	}

	void OutputMachineSeeds(MachineSeedGenConfig genConfig, List<uint> seedList)
	{
		if(genConfig._isOutputSeeds)
		{
			string[] seedArray = new string[seedList.Count];
			for(int i = 0; i < seedList.Count; i++)
				seedArray[i] = seedList[i].ToString();

			string content = string.Join(MachineSeedConfig.SeedFileDelimitor.ToString(), seedArray);

			string fileName = MachineSeedConfig.GetSeedFileName(genConfig._machineName, true);
			string filePath = Path.Combine(_outputSeedsDir, fileName);

			FileStreamUtility.DeleteFile(filePath);
			StreamWriter writer = FileStreamUtility.CreateFileStream(filePath);
			FileStreamUtility.WriteFile(writer, content);
			FileStreamUtility.CloseFile(writer);
		}
	}
}


