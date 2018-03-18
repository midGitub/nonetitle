using System.Collections;
using System.Collections.Generic;
using System;

public static class TypeUtility
{
	public static T GetEnumFromString<T>(string s) where T : struct, IConvertible
	{
		T result = default(T);
		bool isFind = false;

		foreach(T t in Enum.GetValues(typeof(T)))
		{
			if(s.Equals(t.ToString()))
			{
				result = t;
				isFind = true;
				break;
			}
		}

		#if UNITY_EDITOR
		if(!isFind)
			CoreDebugUtility.LogError("GetEnumFromString fail, s:" + s + " ,type:" + typeof(T).ToString());
		#endif

		return result;
	}
}

