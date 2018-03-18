using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MachineTestAnalysisHelper : SimpleSingleton<MachineTestAnalysisHelper>
{
	private MachineConfig _machineConfig;

	#region Init

	private delegate bool IsLuckyModePredicate(MachineTestRoundResult roundResult);
	private static Dictionary<MachineTestLuckyMode, IsLuckyModePredicate> _luckyPredicateDict;
	private static Dictionary<MachineTestLuckyMode, IsLuckyModePredicate> _luckyNoRespinPredicateDict;

	private static bool IsLuckyModeNormal(MachineTestRoundResult roundResult)
	{
		return roundResult._output._spinResult.LuckyMode == CoreLuckyMode.Normal;
	}

	private static bool IsLuckyModeLucky(MachineTestRoundResult roundResult)
	{
		CoreLuckyMode mode = roundResult._output._spinResult.LuckyMode;
		return mode == CoreLuckyMode.LongLucky || mode == CoreLuckyMode.ShortLucky;
	}

	private static bool IsLuckyModeHybrid(MachineTestRoundResult roundResult)
	{
		return true;
	}

	private static bool IsRespinRound(MachineTestRoundResult roundResult)
	{
		return roundResult._output._spinResult.IsRespin;
	}

	private static bool IsLuckyModeNormalWithNoRespin(MachineTestRoundResult roundResult)
	{
		return IsLuckyModeNormal(roundResult) && !IsRespinRound(roundResult);
	}

	private static bool IsLuckyModeLuckyWithNoRespin(MachineTestRoundResult roundResult)
	{
		return IsLuckyModeLucky(roundResult) && !IsRespinRound(roundResult);
	}

	private static bool IsLuckyModeHybridWithNoRespin(MachineTestRoundResult roundResult)
	{
		return IsLuckyModeHybrid(roundResult) && !IsRespinRound(roundResult);
	}

	public MachineTestAnalysisHelper()
	{
		_luckyPredicateDict = new Dictionary<MachineTestLuckyMode, IsLuckyModePredicate>() {
			{MachineTestLuckyMode.Normal, IsLuckyModeNormal},
			{MachineTestLuckyMode.Lucky, IsLuckyModeLucky},
			{MachineTestLuckyMode.Hybrid, IsLuckyModeHybrid}
		};

		_luckyNoRespinPredicateDict = new Dictionary<MachineTestLuckyMode, IsLuckyModePredicate>() {
			{MachineTestLuckyMode.Normal, IsLuckyModeNormalWithNoRespin},
			{MachineTestLuckyMode.Lucky, IsLuckyModeLuckyWithNoRespin},
			{MachineTestLuckyMode.Hybrid, IsLuckyModeHybridWithNoRespin}
		};
	}

	#endregion

	#region Public

	public MachineTestAnalysisResult AnalyzeSingleUser(MachineTestUserResult userResult)
	{
		_machineConfig = MachineConfig.Instance;

		MachineTestAnalysisResult result = new MachineTestAnalysisResult();

		result._spinCountBeforeReachLuckyThreshold = GetSpinCountBeforeReachLuckyThreshold(userResult.RoundResults);
		result._spinCountBeforeLuckyZero = GetSpinCountBeforeLuckyZero(userResult.RoundResults);

		for(int i = 0; i < (int)MachineTestLuckyMode.Count; i++)
		{
			MachineTestLuckyMode mode = (MachineTestLuckyMode)i;
			result._luckyModeResults[(int)mode] = GetLuckyModeResult(mode, userResult.RoundResults);
		}

		return result;
	}

	public MachineTestAnalysisResult AnalyzeSingleMachine(MachineTestMachineResult machineResult, int userCount)
	{
		_machineConfig = MachineConfig.Instance;

		MachineTestAnalysisResult result = new MachineTestAnalysisResult();

		result._spinCountBeforeReachLuckyThreshold = GetAverageSpinCountBeforeReachLuckyThreshold(machineResult.UserResults);
		result._spinCountBeforeLuckyZero = GetAverageSpinCountBeforeLuckyZero(machineResult.UserResults);

		for(int i = 0; i < (int)MachineTestLuckyMode.Count; i++)
		{
			MachineTestLuckyMode mode = (MachineTestLuckyMode)i;
			List<MachineTestRoundResult> roundResults = new List<MachineTestRoundResult>(userCount * machineResult.UserResults.Count);
			for(int k = 0; k < machineResult.UserResults.Count; k++)
			{
				roundResults.AddRange(machineResult.UserResults[k].RoundResults);
			}

			result._luckyModeResults[(int)mode] = GetLuckyModeResult(mode, roundResults);
		}

		return result;
	}

	public bool ShouldAnalysisDetail(MachineConfig machineConfig)
	{
		bool result = !machineConfig.BasicConfig.IsMultiLineSymbolProb;
		return result;
	}

	#endregion

	#region AnalysisResult

	int GetSpinCountBeforeReachLuckyThreshold(List<MachineTestRoundResult> roundResults)
	{
		int threshold = CoreConfig.Instance.LuckyConfig.LongLuckyThreshold;
		int index = ListUtility.Find(roundResults, (MachineTestRoundResult r) => {
			return r._input._lucky < threshold;
		});
		return index;
	}

	int GetSpinCountBeforeLuckyZero(List<MachineTestRoundResult> roundResults)
	{
		int index = ListUtility.Find(roundResults, (MachineTestRoundResult r) => {
			return r._input._lucky == 0;
		});
		return index;
	}

	int GetAverageSpinCountBeforeReachLuckyThreshold(List<MachineTestUserResult> userResults)
	{
		float result = ListUtility.FoldList(userResults, 0.0f, (float acc, MachineTestUserResult r) => {
			return acc + r.AnalysisResult._spinCountBeforeReachLuckyThreshold;
		});
		result /= userResults.Count;
		return (int)result;
	}

	int GetAverageSpinCountBeforeLuckyZero(List<MachineTestUserResult> userResults)
	{
		float result = ListUtility.FoldList(userResults, 0.0f, (float acc, MachineTestUserResult r) => {
			return acc + r.AnalysisResult._spinCountBeforeLuckyZero;
		});
		result /= userResults.Count;
		return (int)result;
	}

	#endregion

	#region AnalysisLuckyModeResult

	MachineTestAnalysisLuckyModeResult GetLuckyModeResult(MachineTestLuckyMode mode, List<MachineTestRoundResult> roundResults)
	{
		MachineTestAnalysisLuckyModeResult result = new MachineTestAnalysisLuckyModeResult();
		IsLuckyModePredicate luckyPred = _luckyPredicateDict[mode];
		IsLuckyModePredicate luckyNoRespinPred = _luckyNoRespinPredicateDict[mode];

		result._totalConsumedBetAmount = GetTotalConsumedBetAmount(luckyPred, roundResults);
		result._totalWinAmount = GetTotalWinAmount(luckyPred, roundResults);
		result._rtp = GetRTP(result._totalConsumedBetAmount, result._totalWinAmount);
		result._spinCountInCurrentMode = GetSpinCountInCurrentMode(luckyPred, roundResults);
		result._enterSpecialSmallGameStateCount = GetEnterSpecialSmallGameStateCount(luckyPred, roundResults);
		result._respinCount = GetRespinCount(luckyPred, roundResults);
		result._enterSpecialSmallGameStateCountProb = GetEnterSpecialSmallGameStateCountProb(result);
		result._respinCountProb = GetRespinCountProb(result);

		if(mode != MachineTestLuckyMode.Hybrid && ShouldAnalysisDetail(_machineConfig))
		{
			result._payoutRowCounts = GetJoyRowCounts(luckyNoRespinPred, mode, SpinResultType.Win, roundResults);
			result._nearHitRowCounts = GetJoyRowCounts(luckyNoRespinPred, mode, SpinResultType.NearHit, roundResults);

			result._payoutRowProbs = GetJoyRowProbs(result._payoutRowCounts, result._spinCountInCurrentMode);
			result._nearHitRowProbs = GetJoyRowProbs(result._nearHitRowCounts, result._spinCountInCurrentMode);

			result._payoutRowProbDeviations = GetJoyRowProbDeviations(mode, SpinResultType.Win, result._payoutRowProbs);
			result._nearHitRowProbDeviations = GetJoyRowProbDeviations(mode, SpinResultType.NearHit, result._nearHitRowProbs);

			result._payoutTotalProb = GetJoyTotalProb(result._payoutRowProbs);
			result._nearHitTotalProb = GetJoyTotalProb(result._nearHitRowProbs);

			result._payoutTotalProbDeviation = GetJoyTotalProbDeviation(mode, SpinResultType.Win, result._payoutTotalProb);
			result._nearHitTotalProbDeviation = GetJoyTotalProbDeviation(mode, SpinResultType.NearHit, result._nearHitTotalProb);
		}

		return result;
	}

	ulong GetTotalConsumedBetAmount(IsLuckyModePredicate pred, List<MachineTestRoundResult> roundResults)
	{
		ulong result = ListUtility.FoldList<MachineTestRoundResult, ulong>(roundResults, 0, (ulong acc, MachineTestRoundResult rr) => {
			if(pred(rr))
				acc += (ulong)rr._output._spinResult.ConsumedBetAmount;
			return acc;
		});
		return result;
	}

	ulong GetTotalWinAmount(IsLuckyModePredicate pred, List<MachineTestRoundResult> roundResults)
	{
		ulong result = ListUtility.FoldList<MachineTestRoundResult, ulong>(roundResults, 0, (ulong acc, MachineTestRoundResult rr) => {
			if(pred(rr))
				acc += rr._output._spinResult.WinAmount;
			return acc;
		});
		return result;
	}

	float GetRTP(ulong betAmount, ulong winAmount)
	{
		return ((float)winAmount) / ((float)betAmount);
	}

	int GetEnterSpecialSmallGameStateCount(IsLuckyModePredicate pred, List<MachineTestRoundResult> roundResults)
	{
		int result = ListUtility.FoldList<MachineTestRoundResult, int>(roundResults, 0, (int acc, MachineTestRoundResult rr) => {
			if(pred(rr) && rr._output._isChangeToSpecialSmallGameState)
				++acc;
			return acc;
		});
		return result;
	}

	int GetRespinCount(IsLuckyModePredicate pred, List<MachineTestRoundResult> roundResults)
	{
		int result = ListUtility.FoldList<MachineTestRoundResult, int>(roundResults, 0, (int acc, MachineTestRoundResult rr) => {
			if(pred(rr) && IsRespinRound(rr))
				++acc;
			return acc;
		});
		return result;
	}

	float GetEnterSpecialSmallGameStateCountProb(MachineTestAnalysisLuckyModeResult luckyModeResult)
	{
		float result = 0.0f;
		int manualSpinCount = luckyModeResult._spinCountInCurrentMode - luckyModeResult._enterSpecialSmallGameStateCount;
		if(manualSpinCount > 0)
			result = (float)luckyModeResult._enterSpecialSmallGameStateCount / (float)manualSpinCount;
		return result;
	}

	float GetRespinCountProb(MachineTestAnalysisLuckyModeResult luckyModeResult)
	{
		float result = 0.0f;
		int manualSpinCount = luckyModeResult._spinCountInCurrentMode - luckyModeResult._respinCount;
		if(manualSpinCount > 0)
			result = (float)luckyModeResult._respinCount / (float)manualSpinCount;
		return result;
	}

	int GetSpinCountInCurrentMode(IsLuckyModePredicate pred, List<MachineTestRoundResult> roundResults)
	{
		int result = ListUtility.FoldList(roundResults, 0, (int acc, MachineTestRoundResult r) => {
			if(pred(r))
				++acc;
			return acc;
		});
		return result;
	}

	List<int> GetJoyRowCounts(IsLuckyModePredicate pred, MachineTestLuckyMode mode, SpinResultType resultType, List<MachineTestRoundResult> roundResults)
	{
		int rowCount = MachineTestUtility.GetJoyRowCount(_machineConfig, mode, resultType);
		List<int> result = new List<int>(rowCount);
		for(int i = 0; i < rowCount; i++)
			result.Add(0);

		foreach(var r in roundResults)
		{
			if(pred(r) && r._output._spinResult.Type == resultType)
			{
				int id = r._output._spinResult.GetJoyId();
				if(id > result.Count)
				{
					Debug.LogError("index out of range");
					Debug.Assert(false);
				}
				++result[id - 1];
			}
		}

		return result;
	}

	List<float> GetJoyRowProbs(List<int> rowCounts, int spinCount)
	{
		List<float> result = new List<float>(rowCounts.Count);
		
		for(int i = 0; i < rowCounts.Count; i++)
		{
			float v = 0.0f;
			if(spinCount > 0)
				v = (float)rowCounts[i] / (float)spinCount;
			result.Add(v);
		}
		return result;
	}

	List<float> GetJoyRowProbDeviations(MachineTestLuckyMode mode, SpinResultType resultType, List<float> rowProbs)
	{
		int joyRowCount = MachineTestUtility.GetJoyRowCount(_machineConfig, mode, resultType);
		List<float> result = new List<float>(joyRowCount);

		for(int i = 0; i < rowProbs.Count; i++)
		{
			float expectProb = MachineTestUtility.GetJoyOverallHit(_machineConfig, mode, resultType, i);
			float v = rowProbs[i] - expectProb;
			result.Add(v);
		}
		return result;
	}

	float GetJoyTotalProb(List<float> rowProbs)
	{
		float result = ListUtility.FoldList(rowProbs, MathUtility.Add);
		return result;
	}

	float GetJoyTotalProbDeviation(MachineTestLuckyMode mode, SpinResultType resultType, float totalProb)
	{
		float expectProb = MachineTestUtility.GetJoyTotalProb(_machineConfig, mode, resultType);
		float result = totalProb - expectProb;
		return result;
	}

	#endregion
}

