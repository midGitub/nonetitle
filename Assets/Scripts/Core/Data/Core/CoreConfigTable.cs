using System;
using System.Collections.Generic;

public static class CoreConfigTable
{
	public static readonly string SubDir = "Core/";

	public static readonly ExcelSheetInfo[] SheetInfos = new ExcelSheetInfo[] {
		new ExcelSheetInfo(LuckyConfig.Name, typeof(LuckyData), typeof(LuckySheet)),
		new ExcelSheetInfo(MiscConfig.Name, typeof(MiscData), typeof(MiscSheet)),
		new ExcelSheetInfo(JackpotSettingConfig.Name, typeof(JackpotSettingData), typeof(JackpotSettingSheet)),
	};
}

