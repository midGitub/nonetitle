using System.Collections;
using System.Collections.Generic;
using System;

public class MultiLineMatchInfo
{
	private MachineConfig _machineConfig;
	private int[] _stopIndexes;
	private int[] _payline;
	private PayoutData _payoutData;
	private bool[] _matchFlags;
	private float _reward;

	public int[] StopIndexes { get { return _stopIndexes; } }
	public int[] Payline { get { return _payline; } }
	public PayoutData PayoutData { get { return _payoutData; } }
	public bool[] MatchFlags { get { return _matchFlags; } }
	public float Reward { get { return _reward; } }

	public MultiLineMatchInfo(MachineConfig machineConfig, int[] stopIndexes, int[] payline, PayoutData payoutData, bool[] matchFlags)
	{
		_machineConfig = machineConfig;
		_stopIndexes = stopIndexes;
		_payline = payline;
		_payoutData = payoutData;
		_matchFlags = matchFlags;

		RefreshReward();
	}

	public override string ToString()
	{
		return string.Format("Reward={0}, StopIndexes=[{1},{2},{3}], Payline=[{4},{5},{6}], PayoutData Id={7}", 
			_reward, _stopIndexes[0], _stopIndexes[1], _stopIndexes[2], _payline[0], _payline[1], _payline[2], _payoutData.Id);
	}

	private void RefreshReward()
	{
		_reward = _payoutData.Ratio;
		if(!_payoutData.IsFixed)
		{
			for(int i = 0; i < _stopIndexes.Length; i++)
			{
				int index = _stopIndexes[i];
				SingleReel reel = _machineConfig.ReelConfig.GetSingleReel(i);
				string symbolName = reel.GetSymbolName(index);
				float multiplier = _machineConfig.SymbolConfig.GetMultiplier(symbolName);
				if (_matchFlags [i]) {
					_reward *= multiplier;
				}
			}
		}
	}

	public bool ShouldShowWinEffect()
	{
		//should always be true
		bool result = _payoutData != null;
		return result;
	}
}

public class CoreMultiLineCheckResult : CoreBaseCheckResult
{
	private MachineConfig _machineConfig;
	private List<MultiLineMatchInfo> _payoutInfos = new List<MultiLineMatchInfo>();
	private List<MultiLineMatchInfo> _nearHitInfos = new List<MultiLineMatchInfo>();
	private float _payoutReward;
	private float _nearHitReward;

	public List<MultiLineMatchInfo> PayoutInfos { get { return _payoutInfos; } }
	public List<MultiLineMatchInfo> NearHitInfos { get { return _nearHitInfos; } }
	public float PayoutReward { get { return _payoutReward; } }
	public float NearHitReward { get { return _nearHitReward; } }

	public CoreMultiLineCheckResult(MachineConfig machineConfig)
	{
		_machineConfig = machineConfig;
	}

	public void AddPayoutInfo(int[] stopIndexes, int[] payline, PayoutData payoutData, bool[] matchFlags)
	{
		MultiLineMatchInfo info = new MultiLineMatchInfo(_machineConfig, stopIndexes, payline, payoutData, matchFlags);
		_payoutInfos.Add(info);
	}

	public void AddNearHitInfo(int[] stopIndexes, int[] payline, PayoutData payoutData, bool[] matchFlags)
	{
		MultiLineMatchInfo info = new MultiLineMatchInfo(_machineConfig, stopIndexes, payline, payoutData, matchFlags);
		_nearHitInfos.Add(info);
	}

	public void RefreshResult()
	{
		Func<float, MultiLineMatchInfo, float> accFunc = (float acc, MultiLineMatchInfo info) => acc + info.Reward;

		_payoutReward = ListUtility.FoldList(_payoutInfos, 0.0f, accFunc);
		_nearHitReward = ListUtility.FoldList(_nearHitInfos, 0.0f, accFunc);

		if(_payoutInfos.Count > 0)
			_type = SpinResultType.Win;
		else if(_nearHitInfos.Count > 0)
			_type = SpinResultType.NearHit;
		else
			_type = SpinResultType.Loss;
	}

	public void DebugPrint()
	{
		CoreDebugUtility.Log("+++++++++++++++++++++");
		string s = string.Format("PayoutReward:{0}, NearHitReward:{1}", _payoutReward, _nearHitReward);
		CoreDebugUtility.Log(s);
		if(_payoutInfos.Count > 0)
		{
			CoreDebugUtility.Log("PayoutInfos:");
			for(int i = 0; i < _payoutInfos.Count; i++)
			{
				MultiLineMatchInfo info = _payoutInfos[i];
				CoreDebugUtility.Log(info.ToString());
			}
		}
		if(_nearHitInfos.Count > 0)
		{
			CoreDebugUtility.Log("NearHitInfos:");
			for(int i = 0; i < _nearHitInfos.Count; i++)
			{
				MultiLineMatchInfo info = _nearHitInfos[i];
				CoreDebugUtility.Log(info.ToString());
			}
		}
		CoreDebugUtility.Log("---------------------");
	}
}
