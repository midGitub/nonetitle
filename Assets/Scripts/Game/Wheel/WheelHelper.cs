using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WheelHelper
{
	public static WheelData RandomCreateWheelData(WheelConfig config){
		// 随机数种子
		int Seed = new System.Random().Next();
		IRandomGenerator iRG = new LCG((uint)Seed, null);
		int rum = RandomUtility.RollInt(iRG, 0, (int)config.ProbSum);
		LogUtility.Log("Random wheel data num is" + rum, Color.blue);

		WheelData data = config.GetFetchData ((float)rum);
		return data;
	}

	public static WheelData RandomCreateWheelData(WheelConfig config, IRandomGenerator generator)
	{
		int rum = RandomUtility.RollInt(generator, 0, (int)config.ProbSum);
		LogUtility.Log("Random wheel data num is" + rum, Color.blue);

		WheelData data = config.GetFetchData ((float)rum);
		return data;
	}

	public static float GetTotalRatio(WheelData[] datas)
	{
		float result = ListUtility.FoldList(datas, 1.0f, (float acc, WheelData data) => {
			float ratio = GetSingleRatio(data);
			return acc * ratio;
		});
		return result;
	}

	public static float GetSingleRatio(WheelData data)
	{
		float result = ListUtility.FoldList(data.Ratio, MathUtility.Multiply);
		return result;
	}

	public static int GetRatioCount(WheelData data)
	{
		return data.Ratio.Length;
	}
}
