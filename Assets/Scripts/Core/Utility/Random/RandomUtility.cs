using System.Collections;
using System.Collections.Generic;

public static class RandomUtility
{
	//Roll an integer which is between [start, end)
	public static int RollInt(IRandomGenerator generator, int start, int end)
	{
		CoreDebugUtility.Assert(end > start);

		int dice = generator.NextInt();
		int result = dice % (end - start) + start;
		return result;
	}

	//Roll an integer which is between [0, end)
	public static int RollInt(IRandomGenerator generator, int end)
	{
		return RollInt(generator, 0, end);
	}

	//Roll an integer which is 0 or 1
	public static int RollBinary(IRandomGenerator generator)
	{
		return RollInt(generator, 0, 2);
	}

	//Roll a list contains "count" integers which are between [start, end)
	public static List<int> RollIntList(IRandomGenerator generator, int start, int end, int count)
	{
		List<int> allList = ListUtility.CreateIntList(start, end);
		List<int> result = RollList(generator, allList, count);
		return result;
	}

	//Roll a list contains "count" integers which are between [0, end)
	public static List<int> RollIntList(IRandomGenerator generator, int end, int count)
	{
		return RollIntList(generator, 0, end, count);
	}

	//Roll a list contains "count" elements
	public static List<T> RollList<T>(IRandomGenerator generator, IList<T> list, int maxCount)
	{
		//It should be allowed that maxCount is greater than list.Count
		//CoreDebugUtility.Assert(maxCount <= list.Count);
		maxCount = (maxCount <= list.Count) ? maxCount : list.Count;

		List<T> allList = new List<T>(list);
		List<T> result = new List<T>();

		for(int i = 0; i < maxCount; i++)
		{
			int rollIndex = RollInt(generator, allList.Count);
			T element = allList[rollIndex];
			allList.RemoveAt(rollIndex);
			result.Add(element);
		}

		return result;
	}

	//Shuffle a list
	//Note that original list is not changed, but return a new shuffled list
	public static List<T> ShuffleList<T>(IRandomGenerator generator, IList<T> list)
	{
		return RollList(generator, list, list.Count);
	}

	//Roll one element
	public static T RollSingleElement<T>(IRandomGenerator generator, IList<T> list)
	{
		CoreDebugUtility.Assert(list.Count > 0);

		int rollIndex = RollInt(generator, list.Count);
		T result = list[rollIndex];
		return result;
	}

	public static int RollSingleElementIndex<T>(IRandomGenerator generator, IList<T> list){
		CoreDebugUtility.Assert(list.Count > 0);

		int rollIndex = RollInt(generator, list.Count);
		return rollIndex;
	}

	public static int RollSingleIntByRatios(IRandomGenerator generator, IList<int> list, IList<float> ratioList){
		CoreDebugUtility.Assert (list.Count > 0);
		CoreDebugUtility.Assert (ratioList.Count > 0);

		List<float> probs = InitProbList (ratioList);
		float ratio = 0.0f;
		int index = -1;
		bool hasIndex = false;
		while (!hasIndex) {
			ratio = generator.NextFloat ();
			index = FetchIndex (ratio, probs);
			hasIndex = ListUtility.IsAnyElementSatisfied (list, (int i) => {
				return index == i;
			});
		}
		return index;
	}

	public static List<float> InitProbList(IList<float> ratioList)
	{
		float curProb = 0.0f;
		List<float> list = new List<float> ();
		for(int i = 0; i < ratioList.Count; i++)
		{
			float num = ratioList[i];
			curProb += num;
			list.Add (curProb);
		}
		return list;
	}

	public static int FetchIndex(float num, IList<float> probsList)
	{
		int index = -1;
		for(int i = 0; i < probsList.Count; i++)
		{
			if(num < probsList[i])
			{
				index = i;
				break;
			}
		}
		return index;
	}

	public static IRandomGenerator CreateRandomGenerator(){
		int Seed = new System.Random().Next();
		IRandomGenerator iRG = new LCG((uint)Seed, null);
		return iRG;
	}
}

