using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface ICSVSheet
{
	bool Load(string text);
	void Reset();
}

//public class CSVSheet<T> : ICSVSheet where T : CSVData, new ()
//{
//	protected List<T> _dataList = new List<T>();
//	public List<T> DataList { get { return _dataList; } }
//
//	public virtual bool Load(string text)
//	{
//		Reset();
//
//		bool result = true;
//		try
//		{
//			string []rows = text.Split('\n', '\r');
//			//start from the first row
//			for(int i = 1; i < rows.Length; i++)
//			{
//				string row = rows[i];
//				if(string.IsNullOrEmpty(row))
//					continue;
//
//				string []items = row.Split(new char[]{','});
//				T data = new T();
//				if(data.Parse(items))
//					_dataList.Add(data);
//				else
//					result = false;
//			}
//		}
//		catch(Exception e)
//		{
//			result = false;
//			Debug.LogError("Error when loading csv:" + e.Message);
//		}
//
//		return result;
//	}
//
//	public virtual void Reset()
//	{
//		_dataList.Clear();
//	}
//}

public class CSVSheet<T> : ICSVSheet where T : new ()
{
	protected List<T> _dataList = new List<T>();
	public List<T> DataList { get { return _dataList; } }

	public virtual bool Load(string text)
	{
		return false;
	}

	public virtual void Reset()
	{
		_dataList.Clear();
	}
}


