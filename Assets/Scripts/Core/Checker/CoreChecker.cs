using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Important:
// This class is only used for single line machine. For multi line machine, use CoreMultiLineChecker
public class CoreChecker : CoreBaseChecker
{
	#region Init

	public CoreChecker(MachineConfig machineConfig)
	{
		base.Init(machineConfig);
	}

	#endregion

	#region Public

#if false // 留着参考用，等确定新代码无误后删除
	public CoreCheckResult CheckResultWithStopIndexes(IList<int> stopIndexes, CoreCheckMode mode = CoreCheckMode.All)
	{
		//only check BasicReelCount
		if(stopIndexes.Count > CoreDefine.BasicReelCount)
			stopIndexes = ListUtility.SubList(stopIndexes, 0, CoreDefine.BasicReelCount);

		CoreCheckResult result = new CoreCheckResult();

		if(_basicConfig.HasSlide)
			stopIndexes = PerformSlideStopIndexes(stopIndexes);

		string[] symbolNames = _reelConfig.GetSymbolNames(stopIndexes);

		PayoutData payoutData = CheckPayout(stopIndexes, result.MatchFlags, symbolNames);
		if(payoutData != null)
		{
			result.Type = SpinResultType.Win;
			result.PayoutData = payoutData;
		}
		else
		{
			if(mode == CoreCheckMode.All)
			{
				NearHitData nearHitData = CheckNearHit(stopIndexes, result.MatchFlags);
				if(nearHitData != null)
				{
					result.Type = SpinResultType.NearHit;
					result.NearHitData = nearHitData;
				}
				else
				{
					result.Type = SpinResultType.Loss;
				}
			}
		}
		return result;
	}
#endif
	public CoreCheckResult CheckResultWithSymbols(IJoyData spinData, IList<CoreSymbol> symbols, CoreCheckMode mode = CoreCheckMode.All){
		List<int> stopIndexes = ListUtility.MapList(symbols, (CoreSymbol symbol)=>{
			return symbol.StopIndex;
		});
		List<string> symbolNames = ListUtility.MapList(symbols, (CoreSymbol symbol)=>{
			return symbol.SymbolData.Name;
		});
		return CheckResultWithStopIndexesAndSymbolNames(spinData, stopIndexes, symbolNames, mode);
	}

	public CoreCheckResult CheckResultWithStopIndexesAndSymbolNames(IJoyData spinData, IList<int> stopIndexes, IList<string> symbolNames, CoreCheckMode mode = CoreCheckMode.All)
	{
		int basicReelCount = _machineConfig.BasicConfig.BasicReelCount;
		string[] names = new string[basicReelCount];
		List<int> indexes = new List<int>();

		//Note: only check the front 3 symbols
		for(int i = 0; i < basicReelCount; i++)
		{
			names[i] = symbolNames[i];
			indexes.Add(stopIndexes[i]);
		}

		CoreCheckResult result = new CoreCheckResult();

		if(_basicConfig.HasSlide){
			bool isSlide = false;
			indexes = PerformSlideStopIndexes(indexes, out isSlide);
			if (isSlide){
				names = _reelConfig.GetSymbolNames(indexes);
			}
		}

		PayoutData payoutData = CheckPayout(indexes, result.MatchFlags, names, spinData);
		if(payoutData != null)
		{
			result.Type = SpinResultType.Win;
			result.PayoutData = payoutData;
		}
		else
		{
			if(mode == CoreCheckMode.All)
			{
				NearHitData nearHitData = CheckNearHit(indexes, result.MatchFlags, spinData);
				if(nearHitData != null)
				{
					result.Type = SpinResultType.NearHit;
					result.NearHitData = nearHitData;
				}
				else
				{
					result.Type = SpinResultType.Loss;
				}
			}
		}
		return result;
	}

	public override bool ShouldHype(CoreSpinResult spinResult, int reelIndex)
	{
		bool result = false;
		IList<int> stopIndexes = spinResult.StopIndexes;

		if(spinResult.IsFixedList[reelIndex])
		{
			result = false;
		}
		else
		{
			//Unordered type has different way from other Payout type
			if (spinResult.JoyData != null && spinResult.JoyData.PayoutType == PayoutType.UnOrdered)
			{
				//1 payout data list
				List<PayoutData> payoutDatas = ListUtility.FilterList(_curPayoutConfig.Sheet.dataArray, (PayoutData d) => {
					return d.PayoutType == PayoutType.UnOrdered && d.Count >= 3;
				});

				//2 try match
				bool[] matchMasks = new bool[stopIndexes.Count];
				matchMasks[0] = matchMasks[1] = true;
				matchMasks[2] = false;
				PayoutData matchData = TryMatchPartJoyData(payoutDatas, stopIndexes, matchMasks, spinResult.JoyData);
				result = matchData != null;
			}
			else
			{
				List<CoreSymbol> checkSymbols = new List<CoreSymbol>(){ spinResult.SymbolList[0], spinResult.SymbolList[1] };
				result = ListUtility.IsAllElementsSatisfied(checkSymbols, CoreUtility.CanSymbolHypeAsWildOrHigh7) 
					|| ListUtility.IsAllElementsSatisfied(checkSymbols, CoreUtility.CanSymbolHypeAsBonus);
			}
		}

		return result;
	}

	//check if part of symbols are matched. It's very different from TryMatchJoyData()
	//matchMasks: input, fill in it to designate which symbols you wanna match
	public PayoutData TryMatchPartJoyData(IList<PayoutData> dataList, IList<int> stopIndexes, bool[] matchMasks, IJoyData spinData)
	{
		CoreDebugUtility.Assert(!_basicConfig.IsMultiLine, "don't call this function in multiLine machine");

		PayoutData result = null;
		//The dataArray is designed to have ordered items from high priority to low priority.
		//So when an high priority item is found, that's it!
		for(int i = 0; i < dataList.Count; i++)
		{
			PayoutData data = dataList[i];
			string[] symbolNames = _reelConfig.GetSymbolNames(stopIndexes);
			bool[] matchFlags = new bool[stopIndexes.Count];
			IsMatchJoyData(data, symbolNames, matchFlags, stopIndexes, spinData);

			bool canMatch = true;
			for(int k = 0; k < matchMasks.Length; k++)
			{
				if(matchMasks[k])
					canMatch &= matchFlags[k];
			}

			if(canMatch)
			{
				result = data;
				break;
			}
		}
		return result;
	}

	#endregion


}
