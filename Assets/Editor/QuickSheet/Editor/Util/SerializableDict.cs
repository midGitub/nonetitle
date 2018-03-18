using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityQuickSheet
{
	[Serializable]
	public class SerializableDict<TKey, TValue>
	{
		[SerializeField]
		public List<TKey> keyList = new List<TKey>();

		[SerializeField]
		public List<TValue> valueList = new List<TValue>();

		public bool ContainsKey(TKey key)
		{
			return keyList.Contains(key);
		}

		public TValue GetValue(TKey key)
		{
			int index = GetIndexOfKey(key);
			Debug.Assert(index >= 0);
			return valueList[index];
		}

		public void SetKeyValue(TKey key, TValue value)
		{
			int index = GetIndexOfKey(key);
			if(index >= 0)
			{
				Debug.Assert(valueList.Count > index);
				valueList[index] = value;
			}
			else
			{
				keyList.Add(key);
				valueList.Add(value);
			}
		}

		public void RemoveKeyValue(TKey key)
		{
			int index = GetIndexOfKey(key);
			if(index >= 0)
			{
				keyList.RemoveAt(index);
				valueList.RemoveAt(index);
			}
			else
			{
				Debug.Assert(false);
			}
		}

		public void Clear()
		{
			keyList.Clear();
			valueList.Clear();
		}

		public int Count()
		{
			return keyList.Count;
		}

		private int GetIndexOfKey(TKey key)
		{
			return keyList.IndexOf(key);
		}
	}
}
