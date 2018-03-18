using System.Collections;
using System.Collections.Generic;
using System;

public class JackpotHelper  {
	private static Random _staticRandom;

	public static ulong CreateNextBonus(ulong bonus, ulong lowerThreshold, ulong upperThreshold, IRandomGenerator generator){
		#if true
		if (bonus > lowerThreshold) {
			lowerThreshold = (ulong)(1.2f * bonus);
		} 

		if (lowerThreshold > upperThreshold) {
			return (ulong)(bonus * (generator.NextFloat() * 0.1f + 1.1f));
		} else {
			return (ulong)(generator.NextFloat () * (upperThreshold - lowerThreshold) + lowerThreshold);
		}
		#else
		if (bonus > lowerThreshold) {
			lowerThreshold = (ulong)(1.2f * bonus);
		} 

		if (lowerThreshold > upperThreshold) {
			return (ulong)(bonus * (NextFloat() * 0.1f + 1.1f));
		} else {
			return (ulong)(NextFloat () * (upperThreshold - lowerThreshold) + lowerThreshold);
		}
		#endif
	}

	public static float GetRandom(float min , float max, IRandomGenerator generator){
		#if true
		float ratio = generator.NextFloat();
		float ret = min + (max - min) * ratio;
		return ret;
		#else
		float ratio = NextFloat();
		float ret = min + (max - min) * ratio;
		return ret;
		#endif
	}

	private static float NextFloat(){
		_staticRandom = new System.Random (unchecked((int)System.DateTime.Now.Ticks));
		return (float)_staticRandom.NextDouble();
	}

	public static float GetJackpotWinRatio(float winRatio, float deltaPoint, float jackpotPoint, float pointThreshold){
		if (pointThreshold == 0.0f)
			return 0.0f;
		
		return winRatio * deltaPoint * jackpotPoint / pointThreshold;
	} 
}
