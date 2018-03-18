using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PuzzleUtility
{
	public static WinType GetWinType(float winRatio)
	{
		WinType winType = WinType.None;
		if(winRatio > 0.0)
		{
			if(winRatio >= CoreConfig.Instance.MiscConfig.EpicWinThreshold)
				winType = WinType.Epic;
			else if(winRatio >= CoreConfig.Instance.MiscConfig.BigWinThreshold)
				winType = WinType.Big;
			else
				winType = WinType.Normal;
		}

		return winType;
	}

	public static NormalWinType GetNormalWinType(float winRatio)
	{
		NormalWinType result = NormalWinType.None;
		MiscConfig miscConfig = CoreConfig.Instance.MiscConfig;
		if(winRatio == 0.0f)
		{
			result = NormalWinType.None;
		}
		else if(winRatio > 0.0f && winRatio < miscConfig.NormalWinHighThreshold)
		{
			result = NormalWinType.Low;
		}
		else if(winRatio >= miscConfig.NormalWinHighThreshold)
		{
			result = NormalWinType.High;
		}
		else
		{
			Debug.Assert(false);
		}
		return result;
	}

	public static bool IsSpecialWin(WinType type)
	{
		return type == WinType.Big || type == WinType.Epic || type == WinType.Jackpot;
	}

	public static bool CanSmallGameStateHype(SmallGameState s)
	{
		bool result = false;
		if(PuzzleDefine.CanSmallGameStateHypeDict.ContainsKey(s))
			result = PuzzleDefine.CanSmallGameStateHypeDict[s];
		else
			Debug.Assert(false);
		return result;
	}

	public static string GetReel4EffectName(string name)
	{
		return name + "2";
	}

	public static bool CanAutoSpinInWinType(WinType type)
	{
		bool result = type == WinType.None || type == WinType.Normal || type == WinType.Big;
		return result;
	}

	public static bool CanAutoSpinSmallGameState(SmallGameState state){
//		return state != SmallGameState.Wheel;
		return true;
	}

	public static bool CanPlaySpecialSymbolSound(SymbolConfig symbolConfig, SymbolData data)
	{
		SymbolType type = data.SymbolType;
		bool result = symbolConfig.IsMatchSymbolWildType (type)
			|| symbolConfig.IsMatchSymbolType(type, SymbolType.Bonus)
			|| symbolConfig.IsMatchSymbolType(type, SymbolType.Jackpot)
			|| data.Name == "High7";
		return result;
	}

	public static bool CanPlaySpecialNormalSymbolSound(SymbolConfig symbolConfig, SymbolData data){
		SymbolType type = data.SymbolType;
		bool result = symbolConfig.IsMatchSymbolWildType (type)
			|| data.Name == "High7";
		return result;
	}

	public static bool CanPlaySpecialStrongSymbolSound(SymbolConfig symbolConfig, SymbolData data){
		SymbolType type = data.SymbolType;
		bool result = symbolConfig.IsMatchSymbolType (type, SymbolType.Bonus)
	        || symbolConfig.IsMatchSymbolType (type, SymbolType.Jackpot);
		
		return result;
	}

	public static bool CanPlaySpecialSymbolEffect(SymbolConfig symbolConfig, SymbolData data, CoreMachine machine){
		SymbolType type = data.SymbolType;
		bool result = symbolConfig.IsMatchSymbolType (type, SymbolType.Bonus)
		            || symbolConfig.IsMatchSymbolType (type, SymbolType.Jackpot)
					|| ( symbolConfig.IsMatchSymbolType(type, SymbolType.Wild) && machine.MachineConfig.BasicConfig.CanPlaySpecialSymbolEffect );
		
		return result;
	}

	// 验证special symbol
	public static bool CheckSpecialSymbol(SymbolConfig symbolConfig, SymbolData data, CoreMachine machine, bool isPayline){
		SymbolType type = data.SymbolType;
		// m9机台的话，如果是bonus需要在支付线才会触发special effect
		if (machine.Name.Equals("M9")){
			if (symbolConfig.IsMatchSymbolType (type, SymbolType.Bonus)){
				return isPayline;
			}
		}

		return true;
	}

	public static bool IsSpecialMachine(CoreMachine machine){
		bool result = machine.MachineConfig.BasicConfig.IsSpecialMachine;
		return result;
	}

	// Note:
	// Subordinate reel is less important than other reels
	// In 4 reels machine, the Reel4 is subordinate.
	// But in 5 reels machine, all reels are identical so Reel4 is not subordinate
	public static bool IsSubordinateReel4(MachineConfig machineConfig, int reelId)
	{
		bool result = machineConfig.BasicConfig.IsFourReel && reelId == CoreDefine.Reel4;
		return result;
	}
}
