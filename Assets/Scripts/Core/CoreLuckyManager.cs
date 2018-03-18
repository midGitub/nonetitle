using System.Collections;
using System.Collections.Generic;
using System;

public class CoreLongLuckyManager
{
	private IRandomGenerator _roller;

	public CoreLongLuckyManager(IRandomGenerator roller)
	{
		_roller = roller;
	}

	public CoreLuckyMode CalculateMode(ulong betAmount)
	{
		CoreLuckyMode mode = CoreLuckyMode.Normal;
		int luckyThreshold = CoreConfig.Instance.LuckyConfig.LongLuckyThreshold;
		int luckyFactorThreshold = CoreConfig.Instance.LuckyConfig.LongLuckyFactorThreshold;
		int longLucky = UserBasicData.Instance.LongLucky;

		if((int)betAmount > longLucky * luckyFactorThreshold)
		{
			mode = CoreLuckyMode.Normal;
		}
		else
		{
			if(longLucky >= luckyThreshold)
			{
				mode = CoreLuckyMode.LongLucky;
			}
			else if(longLucky <= 0)
			{
				mode = CoreLuckyMode.Normal;
			}
			else
			{
				float factor = (float)longLucky / (float)luckyThreshold;
				factor *= factor;
				int r = RandomUtility.RollBinary(_roller);
				mode = (r == 0) ? CoreLuckyMode.Normal : CoreLuckyMode.LongLucky;
			}
		}

		return mode;
	}

	public void RefreshLucky(CoreSpinResult spinResult)
	{
		int sub = GetSubtractLongLucky(spinResult);
		int leftLucky = UserBasicData.Instance.LongLucky;
		//subtract only when betAmount <= leftLucky * factorThreshold
		if(sub > 0 && (int)spinResult.BetAmount <= leftLucky * CoreConfig.Instance.LuckyConfig.LongLuckyFactorThreshold)
		{
			SubtractLongLucky(sub);
			spinResult.LongLuckyChange = -sub;
		}
	}

	public int GetSubtractLongLucky(CoreSpinResult spinResult)
	{
		return spinResult.GetSubtractLongLucky();
	}

	//todo: the parameter type int might be not enough
	//it should be long or ulong
	public void SubtractLongLucky(int sub)
	{
		UserBasicData.Instance.SubtractLongLucky(sub, false);
	}

	public void AddLongLucky(int add)
	{
		UserBasicData.Instance.AddLongLucky (add, true);
	}
}

public class CoreShortLuckyManager
{
	private IRandomGenerator _roller;

	public CoreShortLuckyManager(IRandomGenerator roller)
	{
		_roller = roller;
	}

	public CoreLuckyMode CalculateMode(){
		CoreLuckyMode result = CoreLuckyMode.Normal;
		LuckyConfig config = CoreConfig.Instance.LuckyConfig;
		int shortLucky = UserBasicData.Instance.ShortLucky;

		// 第一次shortlucky的时候，强制在第N次发生shortlucky
		if (UserMachineData.Instance.TotalSpinCount <= config.FirstShortLuckyMax) {
			if (shortLucky >= config.FirstShortLuckyMax) {
				result = CoreLuckyMode.ShortLucky;
			} else {
				result = CoreLuckyMode.Normal;
			}
		} else {
			if(shortLucky <= config.ShortLuckyMin)
			{
				result = CoreLuckyMode.Normal;
			}
			else if(shortLucky >= config.ShortLuckyMax)
			{
				result = CoreLuckyMode.ShortLucky;
			}
			else
			{
				float normalizedFactor = (float)(shortLucky - config.ShortLuckyMin)/(float)(config.ShortLuckyMax - config.ShortLuckyMin);
				normalizedFactor = (float)Math.Pow(normalizedFactor, config.ShortLuckyExponent);
				RollHelper helper = new RollHelper(normalizedFactor);
				int r = helper.RollIndex(_roller);
				result = (r == 0) ? CoreLuckyMode.ShortLucky : CoreLuckyMode.Normal;
			}
		}
		return result;
	}

	public void ProcessBetAmount(ulong betAmount)
	{
		ulong lastBetAmount = UserTempData.Instance.LastBetAmount;
		if(lastBetAmount != 0 && lastBetAmount != betAmount)
			ResetShortLucky();
	}

	public void RefreshLucky(CoreSpinResult spinResult)
	{
		if(spinResult.IsWinWithNonZeroRatio())
			ResetShortLucky();
		else
			AddShortLucky();
	}

	private void ResetShortLucky()
	{
		UserBasicData.Instance.ResetShortLucky(false);
	}

	private void AddShortLucky()
	{
		UserBasicData.Instance.AddShortLucky(CoreConfig.Instance.LuckyConfig.ShortLuckyStep, false);
	}
}

public class CoreLuckyManager
{
	private CoreLongLuckyManager _longLuckyManager;
	private CoreShortLuckyManager _shortLuckyManager;
	private CoreLuckyMode _mode;

	public CoreLuckyMode Mode { get { return _mode; } }
	public CoreLongLuckyManager LongLuckyManager { get { return _longLuckyManager; } }
	public CoreShortLuckyManager ShortLuckyManager { get { return _shortLuckyManager; } }

	public Action NormalModeChangeHandler = null;// 变成NORMAL模式时的回调

	public CoreLuckyManager(IRandomGenerator roller, Action normalHandler = null)
	{
		_longLuckyManager = new CoreLongLuckyManager(roller);
		_shortLuckyManager = new CoreShortLuckyManager(roller);
		NormalModeChangeHandler = normalHandler;
	}

	public CoreLuckyMode RefreshMode(ulong betAmount)
	{
		CoreLuckyMode defaultMode = _mode;

		//process short first
		_shortLuckyManager.ProcessBetAmount(betAmount);
		_mode = _shortLuckyManager.CalculateMode();
		if(_mode == CoreLuckyMode.Normal)
			_mode = _longLuckyManager.CalculateMode(betAmount);
		//CoreDebugUtility.Log("Refresh lucky mode:" + _mode.ToString());

		if (defaultMode != _mode && _mode == CoreLuckyMode.Normal && NormalModeChangeHandler != null){
			NormalModeChangeHandler();
		}
		
		return _mode;
	}

	public void RefreshLucky(CoreSpinResult spinResult)
	{
		_longLuckyManager.RefreshLucky(spinResult);
		_shortLuckyManager.RefreshLucky(spinResult);
	}
}
