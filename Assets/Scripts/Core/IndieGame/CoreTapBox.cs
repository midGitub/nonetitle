using System.Collections;
using System.Collections.Generic;

public class CoreTapBox
{
	CoreMachine _machine;
	BasicConfig _basicConfig;
	IRandomGenerator _roller;

	RollHelper _normalRatioProbsHelper;
	RollHelper _luckyRatioProbsHelper;

	float _totalWinRatio;
	List<float> _winRatios = new List<float>();
	int _tapIndex;

	public float TotalWinRatio { get { return _totalWinRatio; } }
	public List<float> WinRatios { get { return _winRatios; } }

	public CoreTapBox(CoreMachine machine)
	{
		_machine = machine;
		_basicConfig = _machine.MachineConfig.BasicConfig;
		_roller = _machine.Roller;

		_normalRatioProbsHelper = new RollHelper(_basicConfig.TapBoxRatioProbsOfNormal);
		_luckyRatioProbsHelper = new RollHelper(_basicConfig.TapBoxRatioProbsOfLucky);
	}

	public void Reset()
	{
		_totalWinRatio = 0.0f;
		_winRatios.Clear();
		_tapIndex = 0;
	}

	public float FetchWinRatio()
	{
		float ratio = GenerateWinRatio(_tapIndex);

		++_tapIndex;
		if(_tapIndex >= _basicConfig.TapBoxWinProbsOfNormal.Length)
			_tapIndex = _basicConfig.TapBoxWinProbsOfNormal.Length - 1;

		_winRatios.Add(ratio);
		_totalWinRatio += ratio;

		return ratio;
	}

	public ulong GetWinAmout(ulong betAmount)
	{
		ulong normalizedBetAmount = CoreUtility.GetNormalizedBetAmount(_machine.MachineConfig, betAmount);
		ulong result = (ulong)(_totalWinRatio * normalizedBetAmount);
		return result;
	}

	float GenerateWinRatio(int tapIndex)
	{
		float ratioResult = 0.0f;

		//1 consider lucky
		float[] winProbs;
		RollHelper ratioProbsHelper;
		CoreLuckyMode luckyMode = _machine.LuckyManager.Mode;
		if(luckyMode == CoreLuckyMode.Normal)
		{
			winProbs = _basicConfig.TapBoxWinProbsOfNormal;
			ratioProbsHelper = _normalRatioProbsHelper;
		}
		else
		{
			winProbs = _basicConfig.TapBoxWinProbsOfLucky;
			ratioProbsHelper = _luckyRatioProbsHelper;
		}

		//2 decide if win or not
		bool isWin = false;
		RollHelper winHelper = new RollHelper(winProbs[tapIndex]);
		int winIndex = winHelper.RollIndex(_roller);
		isWin = winIndex == 0;

		//3 choose win ratio
		if(isWin)
		{
			int ratioIndex = ratioProbsHelper.RollIndex(_roller);
			CoreDebugUtility.Assert(ratioIndex < _basicConfig.TapBoxWinRatios.Length);
			ratioResult = _basicConfig.TapBoxWinRatios[ratioIndex];
		}

		return ratioResult;
	}

	public string GetRatioCustomData(string delimitor)
	{
		List<string> ratioStrings = ListUtility.MapList(_winRatios, (float f) => f.ToString());
		string result = string.Join(delimitor, ratioStrings.ToArray());
		return result;
	}

	public void SubtractLucky(ulong winAmount)
	{
		//todo: it's a patch for safety
		if(winAmount > int.MaxValue)
		{
			winAmount = int.MaxValue;
			CoreDebugUtility.LogError("CoreTapBox: winAmount is greater than int.MaxValue");
		}
		int lucky = (int)winAmount;
		_machine.LuckyManager.LongLuckyManager.SubtractLongLucky(lucky);
	}

	#if UNITY_EDITOR

	// Support both TapBox and TapChip
	public MachineTestIndieGameResult SimulateUserPlay(ulong betAmount)
	{
		Reset();

		MachineTestIndieGameResult result = new MachineTestIndieGameResult();
		bool isLimitCountByTime = _basicConfig.IsPuzzleTapChip;
		int curCount = 0;

		while(true)
		{
			float r = FetchWinRatio();
			if(r == 0.0f)
				break;

			if(isLimitCountByTime)
			{
				++curCount;
				if(curCount >= CoreIndieGameDefine.MaxTapChipCount)
					break;
			}
		}

		result._winAmount = GetWinAmout(betAmount);
		result._customData = GetRatioCustomData(":");

		SubtractLucky(result._winAmount);

		return result;
	}

	#endif
}
