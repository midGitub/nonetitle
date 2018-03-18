using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CitrusFramework;

public class CSVParser
{
	private int _count;
	private string[] _items;

	private char _listDelimiter = '|';
	private delegate T ConvertDelegate<T>(string s);

	public CSVParser(string[] items)
	{
		_count = 0;
		_items = items;
	}

	#region private methods

	private T GetGenericValue<T>(ConvertDelegate<T> handler, T defaultValue)
	{
		T result = defaultValue;
		try
		{
			if(_count < _items.Length)
			{
				string str = _items[_count].Trim();
				if(!string.IsNullOrEmpty(str))
					result = handler(str);
			}
		}
		catch(Exception e)
		{
			Debug.LogError(e.Message);
		}
		_count++;
		return result;
	}

	private List<T> GetGenericList<T>(ConvertDelegate<T> handler, List<T> defaultValue)
	{
		ConvertDelegate<List<T>> listHandler = (string s) => {
			List<T> result = new List<T>();
			string[] list = s.Split(_listDelimiter);
			for(int i = 0; i < list.Length; i++)
			{
				string l = list[i];
				if(!string.IsNullOrEmpty(l))
				{
					T r = handler(l.Trim());
					result.Add(r);
				}
			}
			return result;
		};

		return GetGenericValue<List<T>>(listHandler, defaultValue);
	}

	private int ConvertIntDelegate(string s)
	{
		return int.Parse(s);
	}

	private string ConvertStringDelegate(string s)
	{
		return s;
	}

	private float ConvertFloatDelegate(string s)
	{
		return float.Parse(s);
	}

	private bool ConvertBoolDelegate(string s)
	{
		return (s == "1") ? true : false;
	}

	#endregion

	#region Get single value

	public int GetInt
	{
		get
		{
			return GetGenericValue<int>(ConvertIntDelegate, 0);
		}
	}

	public string GetString
	{
		get
		{
			return GetGenericValue<string>(ConvertStringDelegate, "");
		}
	}

	public float GetFloat
	{
		get
		{
			return GetGenericValue<float>(ConvertFloatDelegate, 0.0f);
		}
	}

	public bool GetBool
	{
		get
		{
			return GetGenericValue<bool>(ConvertBoolDelegate, false);
		}
	}

	#endregion

	#region Get list value

	public List<int> GetIntList
	{
		get
		{
			return GetGenericList<int>(ConvertIntDelegate, new List<int>());
		}
	}

	public List<string> GetStringList
	{
		get
		{
			return GetGenericList<string>(ConvertStringDelegate, new List<string>());
		}
	}

	public List<float> GetFloatList
	{
		get
		{
			return GetGenericList<float>(ConvertFloatDelegate, new List<float>());
		}
	}

	public List<bool> GetBoolList
	{
		get
		{
			return GetGenericList<bool>(ConvertBoolDelegate, new List<bool>());
		}
	}

	#endregion
}
