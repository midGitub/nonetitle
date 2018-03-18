using System.Collections;
using System.Collections.Generic;
using System;

public static class CorePlayModuleHelper
{
	//Note by nichos:
	//For now, PayoutData.WheelNames is used only for wheel.
	//But it should be extended to be used for triggering any small games like free spin.
	//This extension should be done in the future.

	//Note by nichos:
	//The class has some functions like IsSymbolTriggerXXX, but it could be refactored as 
	//config in CoreSmallGameDefine.
	//More generally speaking, small game codes are not abstract enough for now and should
	//be refactored in the future.

	public static List<CoreSymbol> GetTriggerSymbols(MachineConfig machineConfig, CoreSpinResult spinResult, PayoutData triggerPayoutData)
	{
		if(triggerPayoutData != null && triggerPayoutData.Symbols.Length > 0)
		{
			// Note: comment this code
			// Because sometimes the config is Bonus,Bonus,Bonus Unordered, such as in M10
			//CoreDebugUtility.Assert(triggerPayoutData.Symbols.Length == 1, "triggerPayoutData only support 1 symbolName for now");

			List<CoreSymbol> result = CoreHelper.GetSymbolsForAllVisibleStopIndexes(machineConfig, spinResult, (string name) => {
				return name == triggerPayoutData.Symbols[0];
			});
			return result;
		}
		else
		{
			return new List<CoreSymbol>();
		}
	}

	public static List<CoreSymbol> GetTriggerSymbolsPayline(MachineConfig machineConfig, CoreSpinResult spinResult, string[] names){

		if(names.Length > 0)
		{
			List<CoreSymbol> result = CoreHelper.GetSymbolForPaylineStopIndexes(machineConfig, spinResult, (string name) => {
				return ListUtility.IsContainElement<string>(names, name);
			});
			return result;
		}
		else
		{
			return new List<CoreSymbol>();
		}
	}

	#region General small games

	public static bool IsSymbolTriggerFreeSpin(MachineConfig machineConfig, string symbolName)
	{
		bool result = false;

		//check if designate triggerSymbolNames or not
		if(machineConfig.BasicConfig.FreeSpinTriggerSymbolNames != null)
			result = ListUtility.IsContainElement(machineConfig.BasicConfig.FreeSpinTriggerSymbolNames, symbolName);
		else
			result = machineConfig.SymbolConfig.IsMatchSymbolType(symbolName, SymbolType.Bonus);
		
		return result;
	}

	public static bool IsSymbolTriggerFixWild(MachineConfig machineConfig, string symbolName)
	{
		bool result = machineConfig.SymbolConfig.IsMatchSymbolType(symbolName, SymbolType.Wild);
		return result;
	}

	public static bool IsSymbolTriggerTapBox(MachineConfig machineConfig, string symbolName)
	{
		bool result = machineConfig.SymbolConfig.IsMatchSymbolType(symbolName, SymbolType.Bonus);
		return result;
	}

	public static bool IsSymbolTriggerSwitchSymbol(MachineConfig machineConfig, string symbolName)
	{
		bool result = false;

		//check if designate triggerSymbolNames or not
		if(machineConfig.BasicConfig.SymbolSwitchTriggerNames != null)
		{
			return ListUtility.IsAnyElementSatisfied(machineConfig.BasicConfig.SymbolSwitchTriggerNames, (string s)=>{
				SymbolType refType = TypeUtility.GetEnumFromString<SymbolType>(s);
				SymbolType type = machineConfig.SymbolConfig.GetSymbolType(symbolName);
				return type.Equals(refType);
			});
		}
		else
			return machineConfig.SymbolConfig.IsMatchSymbolType(symbolName, SymbolType.BonusWild);
		
		return result;
	}

	#endregion
}
