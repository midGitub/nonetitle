using System.Collections;
using System.Collections.Generic;

public enum JackpotType{
	Single,
	FourJackpot,
}

public enum JackpotPoolType
{
	None = -1,
	Single,
	Colossal,
	Mega,
	Huge,
	Big,
	Max,
}

public class JackpotDefine  {
	

	public static readonly int SINGLE_JACKPOT_START_MIN = 30000000;
	public static readonly int SINGLE_JACKPOT_START_MAX = 300000000;

	public static readonly int FOUR_JACKPOT_COLOSSAL_START_MIN = 3000000;
	public static readonly int FOUR_JACKPOT_COLOSSAL_START_MAX = 10000000;

	public static readonly float MEGA_JACKPOT_RATIO_MIN_RATIO = 0.21f;
	public static readonly float MEGA_JACKPOT_RATIO_MAX_RATIO = 0.80f;

	public static readonly float HUGE_JACKPOT_RATIO_MIN_RATIO = 0.06f;
	public static readonly float HUGE_JACKPOT_RATIO_MAX_RATIO = 0.20f;

	public static readonly float BIG_JACKPOT_RATIO_MIN_RATIO = 0.01f;
	public static readonly float BIG_JACKPOT_RATIO_MAX_RATIO = 0.05f;

	public static readonly int SINGLE_JACKPOT_INCREASE_VALUE_MIN = 3000;
	public static readonly int SINGLE_JACKPOT_INCREASE_VALUE_MAX = 50000;

	public static readonly int FOUR_JACKPOT_COLOSSAL_INCREASE_VALUE_MIN = 10000;
	public static readonly int FOUR_JACKPOT_COLOSSAL_INCREASE_VALUE_MAX = 12000;

	public static readonly int FOUR_JACKPOT_MEGA_INCREASE_VALUE_MIN = 1000;
	public static readonly int FOUR_JACKPOT_MEGA_INCREASE_VALUE_MAX = 1200;

	public static readonly int FOUR_JACKPOT_HUGE_INCREASE_VALUE_MIN = 160;
	public static readonly int FOUR_JACKPOT_HUGE_INCREASE_VALUE_MAX = 200;

	public static readonly int FOUR_JACKPOT_BIG_INCREASE_VALUE_MIN = 50;
	public static readonly int FOUR_JACKPOT_BIG_INCREASE_VALUE_MAX = 60;

	public static readonly int SINGLE_JACKPOT_DEFAULT_VALUE = 30000000;
	public static readonly int SINGLE_JACKPOT_DEFAULT_LOWER_THRESHOLD = 200000000;
	public static readonly int SINGLE_JACKPOT_DEFAULT_UPPER_THRESHOLD = 500000000;

	public static readonly int FOUR_JACKPOT_COLOSSAL_DEFAULT_VALUE = 3000000;
	public static readonly int FOUR_JACKPOT_COLOSSAL_DEFAULT_LOWER_THRESHOLD = 6000000;
	public static readonly int FOUR_JACKPOT_COLOSSAL_DEFAULT_UPPER_THRESHOLD = 20000000;

	public static readonly int FOUR_JACKPOT_MEGA_DEFAULT_VALUE = 600000;
	public static readonly int FOUR_JACKPOT_MEGA_DEFAULT_LOWER_THRESHOLD = 1500000;
	public static readonly int FOUR_JACKPOT_MEGA_DEFAULT_UPPER_THRESHOLD = 10000000;

	public static readonly int FOUR_JACKPOT_HUGE_DEFAULT_VALUE = 150000;
	public static readonly int FOUR_JACKPOT_HUGE_DEFAULT_LOWER_THRESHOLD = 300000;
	public static readonly int FOUR_JACKPOT_HUGE_DEFAULT_UPPER_THRESHOLD = 2500000;

	public static readonly int FOUR_JACKPOT_BIG_DEFAULT_VALUE = 30000;
	public static readonly int FOUR_JACKPOT_BIG_DEFAULT_LOWER_THRESHOLD = 100000;
	public static readonly int FOUR_JACKPOT_BIG_DEFAULT_UPPER_THRESHOLD = 600000;

	public static readonly int[][] JACKPOT_DEFAULT_TABLE = 
	{
		new int[]{SINGLE_JACKPOT_DEFAULT_VALUE, SINGLE_JACKPOT_DEFAULT_LOWER_THRESHOLD, SINGLE_JACKPOT_DEFAULT_UPPER_THRESHOLD},
		new int[]{FOUR_JACKPOT_COLOSSAL_DEFAULT_VALUE, FOUR_JACKPOT_COLOSSAL_DEFAULT_LOWER_THRESHOLD, FOUR_JACKPOT_COLOSSAL_DEFAULT_UPPER_THRESHOLD},
		new int[]{FOUR_JACKPOT_MEGA_DEFAULT_VALUE, FOUR_JACKPOT_MEGA_DEFAULT_LOWER_THRESHOLD, FOUR_JACKPOT_HUGE_DEFAULT_UPPER_THRESHOLD},
		new int[]{FOUR_JACKPOT_HUGE_DEFAULT_VALUE, FOUR_JACKPOT_HUGE_DEFAULT_LOWER_THRESHOLD, FOUR_JACKPOT_HUGE_DEFAULT_UPPER_THRESHOLD},
		new int[]{FOUR_JACKPOT_BIG_DEFAULT_VALUE, FOUR_JACKPOT_BIG_DEFAULT_LOWER_THRESHOLD, FOUR_JACKPOT_BIG_DEFAULT_UPPER_THRESHOLD},
	};

	public static readonly int[][] JACKPOT_DEFAULT_INCREASE_TABLE = 
	{ 
		new int[]{SINGLE_JACKPOT_INCREASE_VALUE_MIN, SINGLE_JACKPOT_INCREASE_VALUE_MAX},
		new int[]{FOUR_JACKPOT_COLOSSAL_INCREASE_VALUE_MIN, FOUR_JACKPOT_COLOSSAL_INCREASE_VALUE_MAX},
		new int[]{FOUR_JACKPOT_MEGA_INCREASE_VALUE_MIN, FOUR_JACKPOT_MEGA_INCREASE_VALUE_MAX},
		new int[]{FOUR_JACKPOT_HUGE_INCREASE_VALUE_MIN, FOUR_JACKPOT_HUGE_INCREASE_VALUE_MAX},
		new int[]{FOUR_JACKPOT_BIG_INCREASE_VALUE_MIN, FOUR_JACKPOT_BIG_INCREASE_VALUE_MAX},
	};

	public static JackpotPoolType GetJackpotPoolType(string type){
		if (type.Equals ("Single"))
			return JackpotPoolType.Single;
		else if (type.Equals ("Colossal"))
			return JackpotPoolType.Colossal;
		else if (type.Equals ("Mega"))
			return JackpotPoolType.Mega;
		else if (type.Equals ("Huge"))
			return JackpotPoolType.Huge;
		else if (type.Equals ("Big"))
			return JackpotPoolType.Big;

		return JackpotPoolType.None;
	}
}
