using System.Collections.Generic;
using System;

public static class CoreHelper
{
	#region Unorder

	//Predicate<string> param: symbolName
	public static int GetIndexCountForAllVisibleStopIndexes(MachineConfig machineConfig, List<int> stopIndexes, Predicate<string> pred)
	{
		List<List<int>> allVisibleIndexes = machineConfig.ReelConfig.GetAllVisibleStopIndexes(stopIndexes);
		int totalCount = 0;

		for(int i = 0; i < allVisibleIndexes.Count; i++)
		{
			List<int> curIndexes = allVisibleIndexes[i];
			int count = ListUtility.CountElements(curIndexes, (int index) => {
				string symbolName = machineConfig.ReelConfig.GetSymbolName(i, index);
				bool isTriggered = pred(symbolName);
				return isTriggered;
			});
			totalCount += count;
		}

		return totalCount;
	}

	public static List<CoreSymbol> GetSymbolsForAllVisibleStopIndexes(MachineConfig machineConfig, CoreSpinResult spinResult, Predicate<string> pred)
	{
		List<CoreSymbol> allSymbols = new List<CoreSymbol>();
		allSymbols.AddRange(spinResult.SymbolList);
		allSymbols.AddRange(spinResult.NonPayoutLineSymbolList);
		List<CoreSymbol> resultSymbols = ListUtility.FilterList(allSymbols, (CoreSymbol symbol) => {
			bool r = pred(symbol.SymbolData.Name);
			return r;
		});
		return resultSymbols;
	}

	public static List<CoreSymbol> GetSymbolForPaylineStopIndexes(MachineConfig machineConfig, CoreSpinResult spinResult, Predicate<string> pred){
		List<CoreSymbol> allSymbols = new List<CoreSymbol>(spinResult.SymbolList);
		List<CoreSymbol> resultSymbols = ListUtility.FilterList(allSymbols, (CoreSymbol symbol) => {
			bool r = pred(symbol.SymbolData.Name);
			return r;
		});
		return resultSymbols;
	}

	#endregion
}

