using System;
using System.Collections.Generic;

public static class LocalizationConfigTable
{
	public static readonly string SubDir = "Localization/";

	public static readonly ExcelSheetInfo[] SheetInfos = new ExcelSheetInfo[] {
		new ExcelSheetInfo(LocalizationConfig.Name, typeof(ContentData), typeof(ContentSheet)),
	};
}

