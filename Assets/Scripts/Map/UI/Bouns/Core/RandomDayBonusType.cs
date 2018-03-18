using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomDayBonusType
{
	private static int GetOneDayRandomNum(int daysType)
	{
		// 随机数种子
		int Seed = new System.Random().Next();
		IRandomGenerator iRG = new LCG((uint)Seed, null);
		int rum = RandomUtility.RollInt(iRG, 0, DailyBonusConfig.Instance.GetSumProbability((DailyType)daysType));
		Debug.Log("Random num"+rum);
		return rum;
	}

	public static DailyBonusData GetRandomDailyBonusData(int daysType)
	{
		int rnum = GetOneDayRandomNum(daysType);
		int sum = 0;
		for (int i = 0; i < DailyBonusConfig.Instance.DailyBonusList.Count; i++)
		{
			sum += DailyBonusConfig.Instance.GetBonusProbability(i, (DailyType)daysType);
			if (sum == 0)
			{
				continue;
			}
			if (rnum <= sum)
			{
				return DailyBonusConfig.Instance[i];
			}
		}

		Debug.Log("GetRandomDailyBonusData 没有选取到任何的数据");
		return DailyBonusConfig.Instance[0];
	}
}
