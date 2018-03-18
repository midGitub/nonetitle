using System.Collections.Generic;
using System;

public static class ExcelQueryUtility
{
	public static void Assert(bool condition, string message)
	{
		#if !CORE_DLL
		UnityEngine.Debug.Assert(condition, message);
		#endif
	}

	public static void Log(object message)
	{
		#if !CORE_DLL
		UnityEngine.Debug.Log(message);
		#endif
	}

	public static void LogWarning(object message)
	{
		#if !CORE_DLL
		UnityEngine.Debug.LogWarning(message);
		#endif
	}

	public static void LogError(object message)
	{
		#if !CORE_DLL
		UnityEngine.Debug.LogError(message);
		#endif
	}
}

