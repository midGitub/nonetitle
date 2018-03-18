using System.Collections;

public static class CoreDebugUtility
{
	#if CORE_DLL

	public static void Assert(bool condition)
	{
	}

	public static void Assert(bool condition, object o)
	{
	}

	public static void Log(string s)
	{
		System.Console.WriteLine(s);
	}

	public static void LogError(string s)
	{
		System.Console.WriteLine(s);
	}

	#else

	public static void Assert(bool condition)
	{
		UnityEngine.Debug.Assert(condition);
	}

	public static void Assert(bool condition, object o)
	{
		UnityEngine.Debug.Assert(condition, o);
	}

	public static void Log(string s)
	{
		UnityEngine.Debug.Log(s);
	}

	public static void LogError(string s)
	{
		UnityEngine.Debug.LogError(s);
	}

	#endif
}

