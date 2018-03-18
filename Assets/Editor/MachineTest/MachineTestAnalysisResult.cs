using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MachineTestLuckyMode
{
	Normal = 0,
	Lucky,
	Hybrid,
	Count
}

public class MachineTestAnalysisLuckyModeResult
{
	//for Normal, Lucky, Hybrid
	public ulong _totalConsumedBetAmount;
	public ulong _totalWinAmount;
	public float _rtp;
	public int _spinCountInCurrentMode;
	public int _enterSpecialSmallGameStateCount; // the count of transition SmallGameState.None -> Any other SmallGameState
	public int _respinCount;
	public float _enterSpecialSmallGameStateCountProb; // _enterSpecialSmallGameStateCount / _spinCountInCurrentMode
	public float _respinCountProb; // _respinCount / _spinCountInCurrentMode

	//only for Normal, Lucky
	public List<int> _payoutRowCounts;
	public List<int> _nearHitRowCounts;
	public List<float> _payoutRowProbs;
	public List<float> _nearHitRowProbs;
	public List<float> _payoutRowProbDeviations;
	public List<float> _nearHitRowProbDeviations;

	public float _payoutTotalProb;
	public float _nearHitTotalProb;
	public float _payoutTotalProbDeviation;
	public float _nearHitTotalProbDeviation;
}

public class MachineTestAnalysisResult
{
	public int _spinCountBeforeReachLuckyThreshold;
	public int _spinCountBeforeLuckyZero;

	public MachineTestAnalysisLuckyModeResult[] _luckyModeResults = new MachineTestAnalysisLuckyModeResult[(int)MachineTestLuckyMode.Count];
}

