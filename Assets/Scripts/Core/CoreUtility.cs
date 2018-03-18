using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public static class CoreUtility
{
	public static SpinDirection ReverseDirection(SpinDirection dir)
	{
		if(dir == SpinDirection.Up)
			dir = SpinDirection.Down;
		else if(dir == SpinDirection.Down)
			dir = SpinDirection.Up;

		return dir;
	}

	public static int SpinResultTypeToInt(SpinResultType type)
	{
		int result = -1;
		if(type == SpinResultType.Loss)
			result = 0;
		else if(type == SpinResultType.Win)
			result = 1;
		else if(type == SpinResultType.NearHit)
			result = 2;
		else
			CoreDebugUtility.Assert(false);
		return result;
	}

	public static int LuckyModeToInt(CoreLuckyMode mode)
	{
		int result = 0;
		if(mode == CoreLuckyMode.Normal)
			result = 0;
		else if(mode == CoreLuckyMode.LongLucky)
			result = 1;
		else if(mode == CoreLuckyMode.ShortLucky)
			result = 2;
		else
			CoreDebugUtility.Assert(false);
		return result;
	}

	public static int GetSpinResultRowId(CoreSpinResult spinResult)
	{
		int result = 0;
		if(spinResult.Type == SpinResultType.Win)
			result = spinResult.GetPayoutId();
		else if(spinResult.Type == SpinResultType.NearHit)
			result = spinResult.GetNearHitId();
		else if(spinResult.Type == SpinResultType.Loss)
			result = 0;
		else
			CoreDebugUtility.Assert(false);
		return result;
	}

	public static CoreSymbol[] FetchSymbols(List<CoreSymbol> symbols, Predicate<CoreSymbol> pred, int reelCount)
	{
		CoreSymbol[] result = new CoreSymbol[reelCount];

		for(int i = 0; i < symbols.Count; i++)
		{
			CoreSymbol s = symbols[i];
			if(pred(s))
				result[i] = s;
		}
		return result;
	}

	public static int GetWinTypeThreshold(WinType winType){
		if (winType == WinType.Big) {
			return CoreConfig.Instance.MiscConfig.BigWinThreshold;
		} else if (winType == WinType.Epic) {
			return CoreConfig.Instance.MiscConfig.EpicWinThreshold;
		}else {
			return CoreConfig.Instance.MiscConfig.NormalWinHighThreshold;
		}
	}

	public static bool IsMatchSymbolType(SymbolType srcType, SymbolType destType)
	{
		SymbolType []matches = CoreDefine.SymbolTypeMatchDict[srcType];
		return ListUtility.IsContainElement(matches, destType);
	}

	public static bool CanSymbolHypeAsWildOrHigh7(CoreSymbol symbol)
	{
		bool result = IsMatchSymbolType(symbol.SymbolData.SymbolType, SymbolType.Wild);
		if(!result)
			result = (symbol.SymbolData.Name == "High7");
		return result;
	}

	public static bool CanSymbolHypeAsBonus(CoreSymbol symbol)
	{
		bool result = IsMatchSymbolType(symbol.SymbolData.SymbolType, SymbolType.Bonus);
		return result;
	}

	// For reel 5 machine, NormalizedBetAmount is BetAmount divided by payline count
	public static ulong GetNormalizedBetAmount(MachineConfig machineConfig, ulong betAmount)
	{
		ulong result = betAmount;
		if(machineConfig.BasicConfig.ShouldNormalizeBetAmount)
			result /= (ulong)machineConfig.PaylineConfig.PaylineCount;
		return result;
	}
}
