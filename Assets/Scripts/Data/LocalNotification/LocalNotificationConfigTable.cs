using System;
using System.Collections.Generic;

public static class LocalNotificationConfigTable
{
	public static readonly string SubDir = "LocalNotification/";

	public static readonly ExcelSheetInfo[] SheetInfos = new ExcelSheetInfo[] {
		new ExcelSheetInfo(LocalNotificationConfig.Name, typeof(LocalNotificationData), typeof(LocalNotificationSheet)),
		new ExcelSheetInfo(LocalNotificationfestivalConfig.Name, typeof(LocalNotificationfestivalData), typeof(LocalNotificationfestivalSheet)),
	};
}

