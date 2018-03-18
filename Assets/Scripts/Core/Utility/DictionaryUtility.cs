using System;
using System.Collections;
using System.Collections.Generic;

public static class DictionaryUtility
{
	public static void DictionaryWhere<T1, T2>(Dictionary<T1, T2> oldDic, Dictionary<T1, T2> backDic, Predicate<T2> pred)
	{
		backDic.Clear();
		var keylist = new List<T1>(oldDic.Keys);
		for(int i = 0; i < keylist.Count; i++)
		{
			if(pred(oldDic[keylist[i]]))
			{
				backDic[keylist[i]] = oldDic[keylist[i]];
			}
		}
	}

	public static T1 DictionaryFirst<T1, T2>(Dictionary<T1, T2> Dic, Predicate<T2> pred)
	{
		var keylist = new List<T1>(Dic.Keys);
		for(int i = 0; i < keylist.Count; i++)
		{
			T1 currKey = keylist[i];
			T2 currValue = Dic[currKey];
			if(pred(currValue))
			{
				return currKey;
			}
		}

		return default(T1);
	}

}
