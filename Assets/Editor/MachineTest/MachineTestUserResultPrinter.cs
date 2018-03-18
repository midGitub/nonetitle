using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum MachineTestPrintFileNameMode
{
	UserId,
	Seed
}

public class MachineTestUserResultPrinter
{
	private static readonly string _delimiter = ",";

	MachineTestUserResult _userResult;
	private StreamWriter _streamWriter;

	public MachineTestUserResultPrinter(MachineTestUserResult userResult)
	{
		_userResult = userResult;
	}

	public void WriteResult(string dir, MachineTestPrintFileNameMode mode)
	{
		string postfix = "";
		if(mode == MachineTestPrintFileNameMode.UserId)
		{
			int userId = _userResult.UserIndex + 1;
			postfix = userId.ToString();
		}
		else if(mode == MachineTestPrintFileNameMode.Seed)
		{
			postfix = _userResult.StartSeed.ToString();
		}
		else
		{
			Debug.Assert(false);
		}

		string fileName = dir + _userResult.Machine.Name + "_" + postfix + ".csv";

		_streamWriter = FileStreamUtility.CreateFileStream(fileName);

		WriteHeader();
		WriteContent();

		FileStreamUtility.CloseFile(_streamWriter);
	}

	private void WriteHeader()
	{
		string reelStr = "";
		string isFixedStr = "";
		if(_userResult.Machine.MachineConfig.BasicConfig.ReelCount == CoreDefine.Reel3)
		{
			reelStr = "Reel1,Reel2,Reel3,";
			isFixedStr = "IsFixed1,IsFixed2,IsFixed3,";
		}
		else if(_userResult.Machine.MachineConfig.BasicConfig.ReelCount == CoreDefine.Reel4)
		{
			reelStr = "Reel1,Reel2,Reel3,Reel4,";
			isFixedStr = "IsFixed1,IsFixed2,IsFixed3,IsFixed4,";
		}
		else if(_userResult.Machine.MachineConfig.BasicConfig.ReelCount == CoreDefine.Reel5)
		{
			reelStr = "Reel1,Reel2,Reel3,Reel4,Reel5,";
			isFixedStr = "IsFixed1,IsFixed2,IsFixed3,IsFixed4,IsFixed5,";
		}
		else
		{
			Debug.Assert(false);
		}

		string s = "ID," + reelStr + "ResultType,ResultId,Lucky,CurrentCredit," + 
			"Bet,Amount,CreditChange,RemainCredit,LuckyChange,RemainLucky,IsRespin,"
			+ isFixedStr + "IsTriggerIndieGame,IndieGameWinAmount,IndieGameCustomData";
		
		FileStreamUtility.WriteFile(_streamWriter, s);
	}

	private void WriteContent()
	{
		for(int i = 0; i < _userResult.RoundResults.Count; i++)
		{
			int id = i + 1;
			MachineTestRoundResult roundResult = _userResult.RoundResults[i];
			string s = ConstructResult(id, roundResult._input, roundResult._output);
			FileStreamUtility.WriteFile(_streamWriter, s);
		}
	}

	private string ConstructResult(int id, MachineTestInput input, MachineTestOutput output)
	{
		CoreSpinResult spinResult = output._spinResult;
		SpinResultType spinResultType = spinResult.Type;

		List<string> symbolNames = ListUtility.MapList(spinResult.SymbolList, (CoreSymbol s) => {
			return s.SymbolData.Name;
		});
		string symbolNameStr = string.Join(_delimiter, symbolNames.ToArray());

		int resultType = CoreUtility.SpinResultTypeToInt(spinResultType);

		int resultId = 0;
		if(spinResultType == SpinResultType.Win)
			resultId = spinResult.GetPayoutId();
		else if(spinResultType == SpinResultType.NearHit)
			resultId = spinResult.GetNearHitId();

		int lucky = CoreUtility.LuckyModeToInt(spinResult.LuckyMode);
		long curCredit = input._credit;
		int consumedBetAmount = (int)spinResult.ConsumedBetAmount;
		float winRatio = spinResult.WinRatio;
        long creditChange = output._creditChange;
        long remainCredit = output._remainCredit;
		int luckyChange = output._luckyChange;
		int remainLucky = output._remainLucky;
		int isRespin = spinResult.IsRespin ? 1 : 0;
		List<string> isFixedStrList = ListUtility.MapList(spinResult.IsFixedList, (bool b) => {
			return b ? "1" : "0";
		});
		string isFixedStr = string.Join(_delimiter, isFixedStrList.ToArray());
		int isTriggerIndieGame = output._isTriggerIndieGame ? 1 : 0;
		ulong indieGameWinAmount = output._indieGameWinAmount;
		string indieGameCustomData = output._indieGameCustomData;

		string result = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
			id, symbolNameStr, resultType, resultId, lucky, curCredit, consumedBetAmount, winRatio, 
			creditChange, remainCredit, luckyChange, remainLucky, isRespin, isFixedStr, isTriggerIndieGame, 
			indieGameWinAmount, indieGameCustomData);
		return result;
	}
}


