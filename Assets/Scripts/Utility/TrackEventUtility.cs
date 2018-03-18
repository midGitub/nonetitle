using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TrackEventUtility  {
	public static string ConstructTriggerString(CoreMachine machine){
		BasicConfig basicConfig = machine.MachineConfig.BasicConfig;
		TriggerType trigger = basicConfig.TriggerType;

		string triggerParam = "";

		if (trigger == TriggerType.Collect) {
			string max = basicConfig.CollectNum.ToString();
			int curNum =  UserMachineData.Instance.GetBetCollectNum (machine.Name, machine.SpinResult.BetAmount);
			string cur = curNum.ToString();
			triggerParam = "-" + cur + "/" + max;
		}

		return trigger.ToString () +  triggerParam;
	}

	public static string ConstructSmallGameType(CoreMachine machine){

		BasicConfig config = machine.MachineConfig.BasicConfig;

		// 目前都只有一个值
		StringBuilder builder = new StringBuilder ("");

//		ListUtility.ForEach (config.SmallGameTypes, (string s) => {
//			builder.Append(s);
//			builder.Append("-");
//		});

		if (config.HasFreeSpin && machine.SmallGameState == SmallGameState.FreeSpin) {
			FreeSpinType spinType = config.FreeSpinType;
			builder.Append ("Freespin-");
			builder.Append (spinType.ToString ());
			#if false
			if (spinType == FreeSpinType.FixCount) {
				CorePlayModuleFreeSpin module = machine.PlayModuleDict [SmallGameState.FreeSpin] as CorePlayModuleFreeSpin;
				if (module != null) {
					builder.Append (module.RespinCount.ToString ());
				}
			} else if (spinType == FreeSpinType.SpinUntilLose) {
				// no do
			}
			#endif
		} else if (config.HasFixWild && machine.SmallGameState == SmallGameState.FixWild) {
			CorePlayModuleFixWild module = machine.PlayModuleDict [SmallGameState.FixWild] as CorePlayModuleFixWild;
			if (module != null) {
				builder.Append ("FixWild-");
				builder.Append (module.FixReelCount.ToString ());
			} else {
				builder.Append("None");
			}
		} else if (config.HasSlide && machine.SpinResult.ShouldSlide) {
			builder.Append ("Slide");
		} else if (config.HasRewind && machine.SmallGameState == SmallGameState.Rewind) {
			builder.Append ("Rewind");
		} else if (config.IsJackpot) {
			CoreSpinResult result = machine.SpinResult;
			if (result.IsJackpotWin) {
				builder.Append (result.JackpotWinType);
			}else {
				builder.Append("None");
			}
		} else if (config.HasSwitchSymbol){
			if (CheckTriggerSwitchSymbol(machine.SpinResult, machine.MachineConfig)){
				builder.Append("Levelup");
			}else{
				builder.Append("None");
			}
		}
		else {
			builder.Append ("None");
		}
		return builder.ToString ();
	}

	public static string GetDoubleLevelUpStr(){
		if (DoubleLevelUpHelper.Instance.IsInDouble()){
			return "DoubleExp";
		}
		return "Normal";
	}

	private static bool CheckTriggerSwitchSymbol(CoreSpinResult spinResult, MachineConfig config){
		int triggerCount = ListUtility.CountElements(spinResult.SymbolList, (CoreSymbol symbol) => {
			return CorePlayModuleHelper.IsSymbolTriggerSwitchSymbol(config, symbol.SymbolData.Name);
		});
		return config.BasicConfig.SymbolSwitchTriggerCount != 0 && triggerCount >= config.BasicConfig.SymbolSwitchTriggerCount && spinResult.Type == SpinResultType.Win;
	}
}
