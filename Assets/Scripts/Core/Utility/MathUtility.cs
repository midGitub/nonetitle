using System;
using System.Collections;
using System.Collections.Generic;

public static class MathUtility
{
	public static int GetLocateIndex<T>(IList<T> list, T element) where T : IComparable<T>
	{
		CoreDebugUtility.Assert(list.Count > 0);

		int result = 0;
		if(element.CompareTo(list[0]) < 0)
		{
			result = 0;
		}
		else if(element.CompareTo(list[list.Count - 1]) >= 0)
		{
			result = list.Count - 1;
		}
		else
		{
			for(int i = 0; i < list.Count - 1; i++)
			{
				if(element.CompareTo(list[i]) >= 0 && element.CompareTo(list[i + 1]) < 0)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	public static float Add(float t1, float t2)
	{
		return t1 + t2;
	}

	public static int Add(int t1, int t2)
	{
		return t1 + t2;
	}

	public static float Subtract(float t1, float t2)
	{
		return t1 - t2;
	}

	public static int Subtract(int t1, int t2)
	{
		return t1 - t2;
	}

	public static float Multiply(float t1, float t2)
	{
		return t1 * t2;
	}

	public static int Multiply(int t1, int t2)
	{
		return t1 * t2;
	}
}
