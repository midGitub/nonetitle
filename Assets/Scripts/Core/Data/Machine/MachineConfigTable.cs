using System;
using System.Collections.Generic;

public static class MachineConfigTable
{
	public static readonly string SubDir = "Machine/";

	public static readonly ExcelSheetInfo[] SheetInfos = new ExcelSheetInfo[] {
		new ExcelSheetInfo(BasicConfig.Name, typeof(BasicData), typeof(BasicSheet)),
		new ExcelSheetInfo(SymbolConfig.Name, typeof(SymbolData), typeof(SymbolSheet)),
		new ExcelSheetInfo(ReelConfig.Name, typeof(ReelData), typeof(ReelSheet)),
		new ExcelSheetInfo(PayoutConfig.Name, typeof(PayoutData), typeof(PayoutSheet)),
		new ExcelSheetInfo(PayoutConfig.LuckySheetName, typeof(PayoutData), typeof(PayoutSheet)),
		new ExcelSheetInfo(NearHitConfig.Name, typeof(NearHitData), typeof(NearHitSheet)),
		new ExcelSheetInfo(NearHitConfig.LuckySheetName, typeof(NearHitData), typeof(NearHitSheet)),
		new ExcelSheetInfo(PayoutDistConfig.Name, typeof(PayoutDistData), typeof(PayoutDistSheet)),
		new ExcelSheetInfo(PayoutDistConfig.LuckySheetName, typeof(PayoutDistData), typeof(PayoutDistSheet)),
		new ExcelSheetInfo(NearHitDistConfig.Name, typeof(NearHitDistData), typeof(NearHitDistSheet)),
		new ExcelSheetInfo(NearHitDistConfig.LuckySheetName, typeof(NearHitDistData), typeof(NearHitDistSheet)),
		new ExcelSheetInfo(PaylineConfig.Name, typeof(PaylineData), typeof(PaylineSheet)),
		new ExcelSheetInfo(RewardResultConfig.Name, typeof(RewardResultData), typeof(RewardResultSheet)),

		//for wheel, ex. M15
		new ExcelSheetInfo("Wheel", typeof(WheelData), typeof(WheelSheet)),
		new ExcelSheetInfo("LuckyWheel", typeof(WheelData), typeof(WheelSheet)),
		new ExcelSheetInfo("Wheel2", typeof(WheelData), typeof(WheelSheet)),
		new ExcelSheetInfo("LuckyWheel2", typeof(WheelData), typeof(WheelSheet)),
	};
}

