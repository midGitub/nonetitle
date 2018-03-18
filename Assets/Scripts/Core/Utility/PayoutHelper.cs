using System.Collections;
using System.Collections.Generic;

public static class PayoutHelper  {
	public static float GetRatio(ulong bet, BasicConfig config, PayoutData data){
		float ratio = 0.0f;

		if (config.IsPeriodRatioBet){
			int index = GetPeriodRatiosBetIndex(bet, config);
			// CoreDebugUtility.Log("get period ratio bet index = " + index);
			ratio = GetPeriodRatio(index, data);
		} else {
			ratio = data.Ratio;
		}

		return ratio;
	}
	
	private static int GetPeriodRatiosBetIndex(ulong bet, BasicConfig config){
		ulong[] betArray = config.PeriodRatioBets;
		for(int i = 0; i < betArray.Length; ++i){
			if (bet <= betArray[i]){
				return i;
			}
		}

		return betArray.Length;
	}

	private static float GetPeriodRatio(int index, PayoutData data){
		float result = 0.0f;
		if (data.PeriodRatios.Length == 0){
			// CoreDebugUtility.Log("no period ratios");
			return data.Ratio;
		}

		if (index < data.PeriodRatios.Length){
			result = data.PeriodRatios[index];
		}else{
			result = data.PeriodRatios[data.PeriodRatios.Length - 1];
		}

		return result;
	}

}
