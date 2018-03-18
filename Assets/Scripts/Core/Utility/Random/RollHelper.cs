using System.Collections;
using System.Collections.Generic;

public class RollHelper
{
	private List<float> _probList = null;
	
	public RollHelper(List<float> list)
	{
		_probList = new List<float>(list);
		InitProbList();
	}

	public RollHelper(float[] array)
	{
		_probList = new List<float>();
		_probList.AddRange(array);
		InitProbList();
	}

	public RollHelper(float singleVal)
	{
		_probList = new List<float>();
		_probList.Add(singleVal);
		InitProbList();
	}

	private void InitProbList()
	{
		float curProb = 0.0f;
		for(int i = 0; i < _probList.Count; i++)
		{
			float num = _probList[i];
			curProb += num;
			_probList[i] = curProb;
		}
	}

	public int FetchIndex(float num)
	{
		int index = -1;
		for(int i = 0; i < _probList.Count; i++)
		{
			if(num < _probList[i])
			{
				index = i;
				break;
			}
		}
		return index;
	}

	public int RollIndex(IRandomGenerator generator)
	{
		float num = generator.NextFloat();
		int index = FetchIndex(num);
		return index;
	}

	//make _probList sums all elements to 1.0f
	public void NormalizeProbs()
	{
		float sumRev = 1.0f / _probList[_probList.Count - 1];
		for(int i = 0; i < _probList.Count; i++)
			_probList[i] = _probList[i] * sumRev;
	}

	public float GetTotalProb()
	{
		CoreDebugUtility.Assert(_probList.Count > 0);
		return _probList[_probList.Count - 1];
	}
}

