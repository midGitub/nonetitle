using System.Collections;
using System.Collections.Generic;

public class CoreRTPChecker
{
	delegate float PayoutWildFactorDelegate(PayoutData data);

	MachineConfig _machineConfig;
	Dictionary<PayoutType, PayoutWildFactorDelegate> _payoutWildFactorHandlerDict = new Dictionary<PayoutType, PayoutWildFactorDelegate>();

	//BasicReelCount should be 3
	List<List<int>> _reelIndexesCombinationList = new List<List<int>>() {
		//select one reel
		new List<int>(){ 0 }, new List<int>(){ 1 }, new List<int>(){ 2 },
		//select two reels
		new List<int>(){ 0, 1 }, new List<int>(){ 0, 2 }, new List<int>(){ 1, 2 }
	};

	#region Init

	public CoreRTPChecker(MachineConfig machineConfig)
	{
		_machineConfig = machineConfig;

		InitPayoutWildFactorHandlers();

		//when BasicReelCount is not 3, _reelIndexesCombinationList will change
		//CoreDebugUtility.Assert(_machineConfig.BasicConfig.BasicReelCount == 3);
	}

	private void InitPayoutWildFactorHandlers()
	{
		_payoutWildFactorHandlerDict[PayoutType.Ordered] = PayoutOrderedWildFactorHandler;
		_payoutWildFactorHandlerDict[PayoutType.All] = PayoutAllWildFactorHandler;
		_payoutWildFactorHandlerDict[PayoutType.Any] = PayoutAnyWildFactorHandler;
		_payoutWildFactorHandlerDict[PayoutType.UnOrdered] = PayoutUnOrderedWildFactorHandler;
	}

	#endregion

	#region Public

	public float CalculateRTP(CoreLuckySheetMode mode)
	{
		float result = 0.0f;
		if(_machineConfig.BasicConfig.IsMultiLine)
			result = CalculateMultiLineRTP(mode);
		else
			result = CalculateSingleLineRTP(mode);
		return result;
	}

	#endregion

	#region Single Line

	private float CalculateSingleLineRTP(CoreLuckySheetMode mode)
	{
		float result = 0.0f;
		PayoutConfig payoutConfig = _machineConfig.GetCurPayoutConfig(mode);
		foreach(PayoutData data in payoutConfig.Sheet.dataArray)
		{
			float wildFactor = ConjureWildFactor(data);
			float expect = data.Ratio * data.OverallHit * wildFactor;

//			#if DEBUG
//			CoreDebugUtility.Log("RTP single row, Id:" + data.Id.ToString() + ",wildFactor:" + wildFactor.ToString());
//			#endif

			result += expect;
		}
		return result;
	}

	private float ConjureWildFactor(PayoutData data)
	{
		PayoutWildFactorDelegate handler = _payoutWildFactorHandlerDict[data.PayoutType];
		CoreDebugUtility.Assert(handler != null);
		float result = handler(data);
		//todo: multiply reel 4
		return result;
	}

	private float PayoutOrderedWildFactorHandler(PayoutData data)
	{
		return 1.0f;
	}

	private float PayoutAllWildFactorHandler(PayoutData data)
	{
		float result = 0.0f;
		float totalWildsProb = 0.0f;
		ReelsWildConfig wildConfig = _machineConfig.PayoutConfig.GetReelsWildConfig(data);

		//1 wilds ratio
		for(int i = 0; i < _reelIndexesCombinationList.Count; i++)
		{
			List<int> selectReelIndexes = _reelIndexesCombinationList[i];
			float selectReelsProb = GetSelectReelsWithWildProb(wildConfig, selectReelIndexes);

			List<int> maxWildCountIndexes = ListUtility.MapList(selectReelIndexes, (int reelIndex) => {
				SingleReel reel = _machineConfig.ReelConfig.GetSingleReel(reelIndex);
				return reel.WildNameList.Count;
			});

			List<List<int>> wildIndexesCombinationList = GenIndexCombinationList(maxWildCountIndexes);
			for(int j = 0; j < wildIndexesCombinationList.Count; j++)
			{
				List<int> selectWildIndexes = wildIndexesCombinationList[j];
				float selectWildsProb = selectReelsProb;
				float wildsRatio = 1.0f;

				//selectWildIndexes.Count is 1 or 2
				for(int k = 0; k < selectWildIndexes.Count; k++)
				{
					int reelIndex = selectReelIndexes[k];
					int wildIndex = selectWildIndexes[k];
					float[] probs = wildConfig.ProbsList[reelIndex];
					float wildProb = probs[wildIndex];
					selectWildsProb *= wildProb;

					SingleReel reel = _machineConfig.ReelConfig.GetSingleReel(reelIndex);
					string symbolName = reel.WildNameList[wildIndex];
					int multiplier = _machineConfig.SymbolConfig.GetMultiplier(symbolName);
					wildsRatio *= multiplier;
				}

				totalWildsProb += selectWildsProb;
				result += selectWildsProb* wildsRatio;
			}
		}

		//2 non-wild ratio
		result += (1.0f - totalWildsProb) * 1.0f;

		return result;
	}

	private float PayoutAnyWildFactorHandler(PayoutData data)
	{
		return 1.0f;
	}

	private float PayoutUnOrderedWildFactorHandler(PayoutData data)
	{
		return 1.0f;
	}

	private float GetSelectReelsWithWildProb(ReelsWildConfig wildConfig, List<int> selectReelIndexes)
	{
		//I can't think of the case when it's not 1 or 2
		CoreDebugUtility.Assert(selectReelIndexes.Count == 1 || selectReelIndexes.Count == 2);

		List<int> allIndexes = ListUtility.CreateIntList(_machineConfig.BasicConfig.BasicReelCount);
		List<int> leftIndexes = ListUtility.SubtractList(allIndexes, selectReelIndexes);

		float selectLeftProb = ListUtility.FoldList(leftIndexes, 1.0f, (float acc, int index) => {
			return acc * (1.0f - wildConfig.ProbSumList[index]);
		});
		float result = selectLeftProb / (float)_machineConfig.BasicConfig.BasicReelCount;
		return result;
	}

	private List<List<int>> GenIndexCombinationList(List<int> maxIndexList)
	{
		List<List<int>> resultList = new List<List<int>>();
		List<int> cursorList = new List<int>(maxIndexList.Count);
		for(int i = 0; i < maxIndexList.Count; i++)
			cursorList.Add(0);

		bool isEnd = false;
		while(true)
		{
			resultList.Add(new List<int>(cursorList));

			++cursorList[0];
			for(int i = 0; i < maxIndexList.Count; i++)
			{
				if(cursorList[i] >= maxIndexList[i])
				{
					if(i < maxIndexList.Count - 1)
					{
						cursorList[i] = 0;
						++cursorList[i + 1];
					}
					else
					{
						isEnd = true;
						break;
					}
				}
			}

			if(isEnd)
				break;
		}

		return resultList;
	}

	#endregion

	#region Multi Line

	private float CalculateMultiLineRTP(CoreLuckySheetMode mode)
	{
		//todo
		return 0.0f;
	}

	#endregion
}
