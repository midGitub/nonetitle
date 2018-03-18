using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemUtility
{
	public static void EnableScreenSleep()
	{
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}

	public static void DisableScreenSleep()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	public static void Set30FPS()
	{
		Application.targetFrameRate = 30;
	}

	public static void Set60FPS()
	{
		Application.targetFrameRate = 60;
	}
}
