using System;
using System.Collections;
using System.Collections.Generic;

public static class ListUtility
{
	#region Functional

	//Map
	public static List<U> MapList<T, U>(IList<T> list, Func<T, U> handler)
	{
		List<U> result = new List<U>(list.Count);
		for(int i = 0; i < list.Count; i++)
		{
			U e = handler(list[i]);
			result.Add(e);
		}
		return result;
	}

	//Filter
	public static List<T> FilterList<T>(IList<T> list, Predicate<T> pred)
	{
		List<T> result = new List<T>();
		for(int i = 0; i < list.Count; i++)
		{
			T e = list[i];
			if(pred(e))
				result.Add(e);
		}
		return result;
	}

	//Fold 1
	public static T FoldList<T>(IList<T> list, Func<T, T, T> func)
	{
		if(list.Count == 0)
			return default(T);

		T acc = list[0];
		for(int i = 1; i < list.Count; i++)
			acc = func(acc, list[i]);
		return acc;
	}

	//Fold 2
	public static S FoldList<T, S>(IList<T> list, S acc, Func<S, T, S> func)
	{
		for(int i = 0; i < list.Count; i++)
			acc = func(acc, list[i]);
		return acc;
	}

	//ForEach
	public static void ForEach<T>(IList<T> list, Action<T> handler)
	{
		for(int i = 0; i < list.Count; i++)
		{
			handler(list[i]);
		}
	}

	#endregion

	//Generate list contains [start, end)
	public static List<int> CreateIntList(int start, int end)
	{
		List<int> result = new List<int>(end - start);
		for(int n = start; n < end; n++)
			result.Add(n);
		return result;
	}

	public static List<int> CreateIntList(int value, int start, int end)
	{
		List<int> result = new List<int>(end - start);
		for(int i = 0; i < end - start; ++i)
		{
			result.Add(value);
		}
		return result;
	}

	//Generate list contains [0, end)
	public static List<int> CreateIntList(int end)
	{
		return CreateIntList(0, end);
	}

	//Copy list
	public static List<T> CreateList<T>(IList<T> list, int count)
	{
		List<T> result = new List<T>(count);
		for(int i = 0; i < count; i++)
			result.Add(list[i]);
		return result;
	}

	//intersection: l1 intersect l2
	public static List<T> IntersectList<T>(IList<T> l1, IList<T> l2)
	{
		Predicate<T> pred = (T e) =>
		{
			return l2.Contains(e);
		};
		List<T> result = FilterList(l1, pred);
		return result;
	}

	//subtraction: l1 - l2
	public static List<T> SubtractList<T>(List<T> l1, IList<T> l2)
	{
		Predicate<T> pred = (T e) =>
		{
			return !l2.Contains(e);
		};
		List<T> result = FilterList(l1, pred);
		return result;
	}

	//If all elements are the same
	public static bool IsAllElementsSame<T>(IList<T> list) where T : IComparable<T>
	{
		bool result = false;
		if(list.Count > 0)
		{
			Predicate<T> pred = (T t) =>
			{
				return t.CompareTo(list[0]) == 0;
			};
			result = IsAllElementsSatisfied(list, pred);
		}
		return result;
	}

	//If all elements are the same with particular element
	public static bool IsAllElementsSame<T>(IList<T> list, T element) where T : IComparable<T>
	{
		bool result = false;
		if(list.Count > 0)
		{
			Predicate<T> pred = (T t) =>
			{
				return t.CompareTo(element) == 0;
			};
			result = IsAllElementsSatisfied(list, pred);
		}
		return result;
	}

	//If all elements satisfy a predicate
	public static bool IsAllElementsSatisfied<T>(IList<T> list, Predicate<T> pred)
	{
		bool result = true;
		for(int i = 0; i < list.Count; i++)
		{
			if(!pred(list[i]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	//If any element satisfy a predicate
	public static bool IsAnyElementSatisfied<T>(IList<T> list, Predicate<T> pred)
	{
		bool result = false;
		for(int i = 0; i < list.Count; i++)
		{
			if(pred(list[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	//Fill the list with an single element
	public static void FillElements<T>(IList<T> list, T element)
	{
		for(int i = 0; i < list.Count; i++)
			list[i] = element;
	}

	//Find all indexes for a particular element
	public static List<int> FindAllIndexes<T>(IList<T> list, T element) where T : IComparable<T>
	{
		List<int> result = FindAllIndexes(list, (T e) =>
		{
			return element.CompareTo(e) == 0;
		});
		return result;
	}

	//Find all indexes for a predicate
	public static List<int> FindAllIndexes<T>(IList<T> list, Predicate<T> pred)
	{
		List<int> result = new List<int>();
		for(int i = 0; i < list.Count; i++)
		{
			if(pred(list[i]))
				result.Add(i);
		}
		return result;
	}

	//Find the index for a predicate
	public static int Find<T>(IList<T> list, Predicate<T> pred)
	{
		int result = -1;
		for(int i = 0; i < list.Count; i++)
		{
			if(pred(list[i]))
			{
				result = i;
				break;
			}
		}
		return result;
	}

	//If a litst contains a particular element
	public static bool IsContainElement<T>(IList<T> list, T element)
	{
		bool result = false;
		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].Equals(element))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	//If a list contains all elements in another list
	public static bool IsContainAllElements<T>(IList<T> superList, IList<T> subList)
	{
		bool result = IsAllElementsSatisfied(subList, (T e) =>
		{
			return superList.Contains(e);
		});
		return result;
	}

	//If a list contains any elements in another list
	public static bool IsContainAnyElements<T>(IList<T> superList, IList<T> subList)
	{
		bool result = IsAnyElementSatisfied(subList, (T e) =>
		{
			return superList.Contains(e);
		});
		return result;
	}

	//Count the number of one element
	public static int GetElementCount<T>(IList<T> list, T element) where T : IComparable<T>
	{
		int count = 0;
		for(int i = 0; i < list.Count; i++)
		{
			if(element.CompareTo(list[i]) == 0)
				++count;
		}
		return count;
	}

	//Count the number for predicate
	public static int GetElementCount<T>(IList<T> list, Predicate<T> pred)
	{
		int count = 0;
		for(int i = 0; i < list.Count; i++)
		{
			if(pred(list[i]))
				++count;
		}
		return count;
	}

	//Find any repeated elements and return all their indexes
	public static List<int> FindAnyRepeatedElementIndexes<T>(IList<T> list) where T : IComparable<T>
	{
		CoreDebugUtility.Assert(list.Count > 0);

		List<int> result = new List<int>();
		bool isRepeated = false;
		T repeatedElement = list[0];
		for(int i = 0; i < list.Count; i++)
		{
			int c = GetElementCount(list, list[i]);
			if(c > 1)
			{
				isRepeated = true;
				repeatedElement = list[i];
				break;
			}
		}

		if(isRepeated)
		{
			for(int i = 0; i < list.Count; i++)
			{
				if(repeatedElement.CompareTo(list[i]) == 0)
					result.Add(i);
			}
		}

		return result;
	}

	//If two lists are the same
	public static bool IsEqualLists<T>(IList<T> l1, IList<T> l2) where T : IComparable<T>
	{
		bool result = false;
		if(l1.Count == l2.Count)
		{
			result = true;
			for(int i = 0; i < l1.Count; i++)
			{
				if(l1[i].CompareTo(l2[i]) != 0)
				{
					result = false;
					break;
				}
			}
		}

		return result;
	}

	//count element number for a predicate
	public static int CountElements<T>(IList<T> list, Predicate<T> pred)
	{
		int result = 0;
		for(int i = 0; i < list.Count; i++)
		{
			if(pred(list[i]))
				++result;
		}
		return result;
	}

	//get sub-list
	public static List<T> SubList<T>(IList<T> list, int start, int end)
	{
		List<T> result = new List<T>();
		for(int i = start; i < end; i++)
			result.Add(list[i]);
		return result;
	}

	public static T FindFirstOrDefault<T>(IList<T> list, Predicate<T> pred)
	{
		for(int i = 0; i < list.Count; i++)
		{
			if(pred(list[i]))
			{
				return list[i];
			}
		}
		return default(T);
	}

	public static T First<T>(IList<T> list)
	{
		return list[0];
	}

	public static T Last<T>(IList<T> list)
	{
		return list[list.Count - 1];
	}


	// index
	public static List<int> IndexList<T>(IList<T> list, Predicate<T> pred)
	{
		List<int> result = new List<int>();
		for(int i = 0; i < list.Count; ++i)
		{
			if(pred(list[i]))
			{
				result.Add(i);
			}
		}
		return result;
	}

}

