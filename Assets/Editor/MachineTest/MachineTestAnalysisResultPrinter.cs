using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum MachineTestAnalysisMode
{
	User,
	Machine
}

public class MachineTestAnalysisResultPrinter
{
	private static readonly string _delimiter = ",";

	MachineTestAnalysisMode _mode;
	MachineTestConfig _config;
	MachineTestUserResult _userResult;
	MachineTestMachineResult _machineResult;
	MachineTestAnalysisResult _analysisResult;
	private StreamWriter _streamWriter;

	public MachineTestAnalysisResultPrinter(MachineTestConfig config, MachineTestUserResult userResult, MachineTestAnalysisResult analysisResult)
	{
		_mode = MachineTestAnalysisMode.User;
		_config = config;
		_userResult = userResult;
		_analysisResult = analysisResult;
	}

	public MachineTestAnalysisResultPrinter(MachineTestConfig config, MachineTestMachineResult machineResult, MachineTestAnalysisResult analysisResult)
	{
		_mode = MachineTestAnalysisMode.Machine;
		_config = config;
		_machineResult = machineResult;
		_analysisResult = analysisResult;
	}

	public void WriteResult(string dir)
	{
		string fileName = "";
		if(_mode == MachineTestAnalysisMode.User)
		{
			int userId = _userResult.UserIndex + 1;
			fileName = dir + "analysis_" + _userResult.Machine.Name + "_" + userId.ToString() + ".txt";
		}
		else if(_mode == MachineTestAnalysisMode.Machine)
		{
			fileName = dir + "analysis_" + _machineResult.MachineName + ".txt";
		}
		else
		{
			Debug.Assert(false);
		}

		_streamWriter = FileStreamUtility.CreateFileStream(fileName);

		WriteConfig();
		WriteContent();

		FileStreamUtility.CloseFile(_streamWriter);
	}

	private void WriteConfig()
	{
		string s = "";
		Write("Init config:");

		if(_mode == MachineTestAnalysisMode.User)
		{
			int userId = _userResult.UserIndex + 1;
			s = string.Format("Machine:{0}, User:{1}, StartSeed:{2}", _userResult.Machine.Name, userId, _userResult.StartSeed);
		}
		else if(_mode == MachineTestAnalysisMode.Machine)
		{
			s = string.Format("Machine:{0}", _machineResult.MachineName);
		}
		Write(s);

		s = string.Format("InitLucky:{0}, InitCredit:{1}, Pay Protection:{2}", _config._initLucky, _config._initCredit, _config._isPayProtectionEnable);
		Write(s);

		s = string.Format("SpinCount:{0}", _config._spinCount);
		Write(s);

		if(_config._betMode == MachineTestBetMode.FixBetAmount)
		{
			s = string.Format("BetMode:{0}, BetAmount:{1}", _config._betMode, _config._betAmount);
			Write(s);
		}
		else if(_config._betMode == MachineTestBetMode.FixBetPercentage)
		{
			s = string.Format("BetMode:{0}, BetPercentage:{1}, MinBetAmount:{2}", _config._betMode, _config._betPercentage, _config._minBetAmountInPercentageMode);
			Write(s);
		}

		Write("");
	}

	private void WriteContent()
	{
		Write("Analysis result:");

		string s = string.Format("SpinCountBeforeReachLuckyThreshold:{0}", _analysisResult._spinCountBeforeReachLuckyThreshold);
		Write(s);

		s = string.Format("SpinCountBeforeLuckyZero:{0}", _analysisResult._spinCountBeforeLuckyZero);
		Write(s);

		for(int i = 0; i < (int)MachineTestLuckyMode.Count; i++)
		{
			MachineTestLuckyMode mode = (MachineTestLuckyMode)i;
			WriteSingleLuckyModeResult(mode, _analysisResult._luckyModeResults[i]);
		}
	}

	private void WriteSingleLuckyModeResult(MachineTestLuckyMode mode, MachineTestAnalysisLuckyModeResult luckyModeResult)
	{
		Write("LuckyMode=" + mode.ToString() + ":");

		string s = string.Format("TotalConsumedBetAmount:{0}, TotalWinAmount:{1}, RTP:{2}", luckyModeResult._totalConsumedBetAmount, luckyModeResult._totalWinAmount, luckyModeResult._rtp);
		Write(s);

		s = string.Format("SpinCountInCurrentMode:{0}", luckyModeResult._spinCountInCurrentMode);
		Write(s);

		s = string.Format("EnterSpecialSmallGameStateCount:{0}, EnterSpecialSmallGameStateCountProb:{1:N6}", 
			luckyModeResult._enterSpecialSmallGameStateCount, luckyModeResult._enterSpecialSmallGameStateCountProb);
		Write(s);

		s = string.Format("RespinCount:{0}, RespinCountProb:{1:N6}", 
			luckyModeResult._respinCount, luckyModeResult._respinCountProb);
		Write(s);

		Write("");

		if(mode != MachineTestLuckyMode.Hybrid && MachineTestAnalysisHelper.Instance.ShouldAnalysisDetail(MachineConfig.Instance))
		{
			//payout results
			bool payoutAssert = (luckyModeResult._payoutRowCounts.Count == luckyModeResult._payoutRowProbs.Count)
				&& (luckyModeResult._payoutRowCounts.Count == luckyModeResult._payoutRowProbDeviations.Count);
			Debug.Assert(payoutAssert);

			Write("Payout result:");
			Write("Id, RowCount, RowProb, ExpectProb, RowProbDeviation");
			for(int i = 0; i < luckyModeResult._payoutRowCounts.Count; i++)
			{
				float expectProb = MachineTestUtility.GetJoyOverallHit(MachineConfig.Instance, mode, SpinResultType.Win, i);
				s = string.Format("{0}, {1}, {2:N6}, {3:N6}, {4:N6}",
					i + 1, luckyModeResult._payoutRowCounts[i], luckyModeResult._payoutRowProbs[i], expectProb, luckyModeResult._payoutRowProbDeviations[i]);
				Write(s);
			}

			float expectWinTotalProb = MachineTestUtility.GetJoyTotalProb(MachineConfig.Instance, mode, SpinResultType.Win);
			s = string.Format("TotalProb:{0:N6}, ExpectTotalProb:{1:N6}, TotalProbDeviation:{2:N6}",
				luckyModeResult._payoutTotalProb, expectWinTotalProb, luckyModeResult._payoutTotalProbDeviation);
			Write(s);

			Write("");

			//nearHit results
			bool nearHitAssert = (luckyModeResult._nearHitRowCounts.Count == luckyModeResult._nearHitRowProbs.Count)
				&& (luckyModeResult._nearHitRowCounts.Count == luckyModeResult._nearHitRowProbDeviations.Count);
			Debug.Assert(nearHitAssert);

			Write("NearHit result:");
			Write("Id, RowCount, RowProb, ExpectProb, RowProbDeviation");
			for(int i = 0; i < luckyModeResult._nearHitRowCounts.Count; i++)
			{
				float expectProb = MachineTestUtility.GetJoyOverallHit(MachineConfig.Instance, mode, SpinResultType.NearHit, i);
				s = string.Format("{0}, {1}, {2:N6}, {3:N6}, {4:N6}",
					i + 1, luckyModeResult._nearHitRowCounts[i], luckyModeResult._nearHitRowProbs[i], expectProb, luckyModeResult._nearHitRowProbDeviations[i]);
				Write(s);
			}

			float expectNearHitTotalProb = MachineTestUtility.GetJoyTotalProb(MachineConfig.Instance, mode, SpinResultType.NearHit);
			s = string.Format("TotalProb:{0:N6}, ExpectTotalProb:{1:N6}, TotalProbDeviation:{2:N6}",
				luckyModeResult._nearHitTotalProb, expectNearHitTotalProb, luckyModeResult._nearHitTotalProbDeviation);
			Write(s);

			Write("");
		}
	}

	private void Write(string s)
	{
		FileStreamUtility.WriteFile(_streamWriter, s);
	}
}


