using System.Collections.Generic;
using UnityEngine;
using System;

public class DiceHelper : SimpleSingleton<DiceHelper> {

    public readonly int SingleDiceMinPoint = 1;
    public readonly int SingleDiceMaxPoint = 6;

    private IRandomGenerator _Roller = new LCG((uint)new System.Random().Next(), null);

	public bool UserInNoDisturbState()
	{
		bool result = false;

		DateTime lastCloseDicePageDate = UserDeviceLocalData.Instance.LastCloseDicePageDate;
		DateTime lastPayForDiceDate = UserDeviceLocalData.Instance.LastPayForDiceDate;

		bool inClosePageCoolDownTime = System.DateTime.Now.CompareTo(lastCloseDicePageDate.AddDays(3)) < 0;
		bool inPayForDiceCoolDownTime = System.DateTime.Now.CompareTo(lastPayForDiceDate.AddDays(2)) < 0;
		if(inClosePageCoolDownTime || inPayForDiceCoolDownTime)
		{
			result = true;
			Debug.Log("closePageCoolDown: " + inClosePageCoolDownTime  + "   lastCloseDicePageDate : " +lastCloseDicePageDate
							+ "  payForDiceCoolDown:" + inPayForDiceCoolDownTime + "  lastPayForDiceDate" + lastPayForDiceDate);
		}

		return result;
	}

	public int PlayDiceResult(DiceData data)
	{
		int result = 0;

		List<int> indexList = ListUtility.CreateIntList(0, data.GameResult.Length);
		List<string> ratioStrList = new List<string>(data.Probability.Split(','));
		List<float> ratioList = ListUtility.MapList(ratioStrList, (string s) =>  {return float.Parse(s);});

		int resultIndex = RandomUtility.RollSingleIntByRatios(_Roller, indexList, ratioList);
		result = resultIndex < indexList.Count ? data.GameResult [resultIndex] : default(int);

		return result;
	}

	//originalResult means result not relate to diceCount, it's a total number, should be divide to each dice
	public List<int> CalculateResultByDiceCount(int diceCount, int originalResult)
	{
		List<int> result = ListUtility.CreateIntList(diceCount);
		int restPoints = originalResult;
		int restDiceNum = 0;

		Debug.Assert(originalResult >= diceCount * SingleDiceMinPoint, "DiceHelper: Error On OriginalResult");

		for(int i = 0; i < result.Count; i++ )
		{
			restDiceNum = result.Count - 1 - i;
			int restMaxCapcity = restDiceNum * SingleDiceMaxPoint;
			int restMinCapcity = restDiceNum * SingleDiceMinPoint;

			//if roll result is less than minRollPointThisTime, mean that rest dice's point must more than SingleDiceMaxPoint
			int minRollPointThisTime = restPoints > restMaxCapcity ? restPoints - restMaxCapcity : SingleDiceMinPoint;
			int maxRollPointThisTime = restPoints - restMinCapcity > SingleDiceMaxPoint ? SingleDiceMaxPoint : restPoints - restMinCapcity;
				
			if (i != result.Count - 1 && restPoints > 0) 
			{
				int random = RandomUtility.RollInt(_Roller, minRollPointThisTime, maxRollPointThisTime + 1);
				result[i] = random;
				restPoints -= random;
			} 
			else 
			{
				result[i] = restPoints;
			}
		}

		return result;
	}
}
