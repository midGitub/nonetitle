using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deque<T> : List<T>
{
	public void AddFirst(T e)
	{
		this.Insert(0, e);
	}

	public void AddLast(T e)
	{
		this.Add(e);
	}

	public T GetFirst()
	{
		int count = this.Count;
		if(count == 0)
		{
			Debug.LogError("Deque: no element");
			return default(T);
		}

		return this[0];
	}

	public T GetLast()
	{
		int count = this.Count;
		if(count == 0)
		{
			Debug.LogError("Deque: no element");
			return default(T);
		}

		return this[count - 1];
	}

	public T PopFirst()
	{
		int count = this.Count;
		if(count == 0)
		{
			Debug.LogError("Deque: no element");
			return default(T);
		}

		T e = this[0];
		this.RemoveAt(0);
		return e;
	}

	public T PopLast()
	{
		int count = this.Count;
		if(count == 0)
		{
			Debug.LogError("Deque: no element");
			return default(T);
		}

		T e = this[count - 1];
		this.RemoveAt(count - 1);
		return e;
	}
}
