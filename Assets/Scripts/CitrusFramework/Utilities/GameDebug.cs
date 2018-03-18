//#define Debug

using UnityEngine;
using System.Collections;

namespace CitrusFramework
{
	public class GameDebug
	{
		public static void Log(object log)
		{
			#if DEBUG
//			Debug.Log(log + "_" + Time.realtimeSinceStartup);
			Debug.Log(log);
			#endif
		}

		public static void Log(string title, string category, object value)
		{
			#if DEBUG
			Debug.Log("[" + category + "]  [" + title + "]  " + value);
			#endif
		}
		
		public static void LogError(object log)
		{
//			#if DEBUG
			Debug.LogError(log);
//			#endif
		}
		
		public static void LogWarning(object log)
		{
			#if DEBUG
//			Debug.LogWarning(log + "_" + Time.realtimeSinceStartup);
			Debug.LogWarning(log);
			#endif
		}

		public void Assert(bool cond)
		{
			if (cond == false)
			{
				Debug.LogError(UnityEngine.StackTraceUtility.ExtractStackTrace());
			}
		}

		public static  void Assert(bool cond, string log)
		{
		if (cond == false)
			{
				Debug.LogError(log);
				Debug.LogError(UnityEngine.StackTraceUtility.ExtractStackTrace());
			}
		}

		public void Assert(bool cond, string format, params object[] args)
		{
			if (cond == false)
			{
				Debug.LogError(string.Format(format, args));
				Debug.LogError(UnityEngine.StackTraceUtility.ExtractStackTrace());
            }
        }

		public static void Warning(bool cond, string log)
		{
			if (cond == false)
			{
				Debug.LogWarning(log);
			}
		}
    }
}

