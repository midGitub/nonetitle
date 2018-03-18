using System.Collections;
using System.Collections.Generic;

public abstract class CoreBaseChecker
{
	protected static readonly int WildcardStopIndex = -1;

	protected delegate bool PayoutCheckerDelegate(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData);

	protected MachineConfig _machineConfig;
	protected BasicConfig _basicConfig;
	protected SymbolConfig _symbolConfig;
	protected ReelConfig _reelConfig;
	protected PayoutConfig _curPayoutConfig; //refer to payoutConfig or luckyPayoutConfig
	protected NearHitConfig _curNearHitConfig; //refer to nearHitConfig or luckyNearHitConfig
	protected PayoutDistConfig _curPayoutDistConfig; //refer to payoutDistConfig or luckyPayoutDistConfig
	protected NearHitDistConfig _curNearHitDistConfig; //refer to nearHitDistConfig or luckyNearHitDistConfig
	protected PaylineConfig _paylineConfig;
	protected RewardResultConfig _rewardResultConfig;

	protected PayoutCheckerDelegate []_payoutCheckers = new PayoutCheckerDelegate[(int)PayoutType.Count];

	#region Init

	protected CoreBaseChecker()
	{
	}

	protected void Init(MachineConfig machineConfig)
	{
		_machineConfig = machineConfig;
		_basicConfig = _machineConfig.BasicConfig;
		_symbolConfig = _machineConfig.SymbolConfig;
		_reelConfig = _machineConfig.ReelConfig;
		_curPayoutConfig = _machineConfig.PayoutConfig;
		_curNearHitConfig = _machineConfig.NearHitConfig;
		_curPayoutDistConfig = _machineConfig.PayoutDistConfig;
		_curNearHitDistConfig = _machineConfig.NearHitDistConfig;
		_paylineConfig = _machineConfig.PaylineConfig;
		_rewardResultConfig = _machineConfig.RewardResultConfig;

		InitPayoutCheckers();
	}

	private void InitPayoutCheckers()
	{
		_payoutCheckers[(int)PayoutType.Ordered] = IsMatchPayoutOrdered;
		_payoutCheckers[(int)PayoutType.All] = IsMatchPayoutAll;
		_payoutCheckers[(int)PayoutType.Any] = IsMatchPayoutAny;
		_payoutCheckers[(int)PayoutType.UnOrdered] = IsMatchPayoutUnordered;
		_payoutCheckers[(int)PayoutType.Continuous] = IsMatchPayoutContinuous;
		_payoutCheckers[(int)PayoutType.Start] = IsMatchPayoutStart;
	}

	#endregion

	#region Public

	public void RefreshLuckyMode(CoreLuckyMode mode)
	{
		_curPayoutConfig = _machineConfig.GetCurPayoutConfig(mode);
		_curNearHitConfig = _machineConfig.GetCurNearHitConfig(mode);
		_curPayoutDistConfig = _machineConfig.GetCurPayoutDistConfig(mode);
		_curNearHitDistConfig = _machineConfig.GetCurNearHitDistConfig(mode);
	}

	#endregion

	#region Protected

	protected List<int> PerformSlideStopIndexes(IList<int> stopIndexes, out bool isSlide)
	{
		isSlide = false;
		List<int> result = new List<int>(stopIndexes);

		for(int i = 0; i < stopIndexes.Count; i++)
		{
			if(result[i] != CoreDefine.InvalidIndex)
			{
				SingleReel reel = _reelConfig.GetSingleReel(i);
				if(reel.ShouldSlideToNeighbor(result[i], SpinDirection.Up)){
					result[i] = reel.GetNeighborStopIndex(result[i], SpinDirection.Up);
					isSlide = true;
				}
				else if(reel.ShouldSlideToNeighbor(result[i], SpinDirection.Down)){
					result[i] = reel.GetNeighborStopIndex(result[i], SpinDirection.Down);
					isSlide = true;
				}
			}
		}

		return result;
	}

	protected PayoutData TryMatchJoyData(IList<PayoutData> dataList, IList<int> stopIndexes, bool[] matchFlags, string[] symbols, IJoyData spinData)
	{
		PayoutData result = null;
		//The dataArray is designed to have ordered items from high priority to low priority.
		//So when an high priority item is found, that's it!
		for(int i = 0; i < dataList.Count; i++)
		{
			PayoutData data = dataList[i];
			if(IsMatchJoyData(data, symbols, matchFlags, stopIndexes, spinData))
			{
				result = data;
				break;
			}
		}
		return result;
	}

	protected PayoutData CheckPayout(IList<int> stopIndexes, bool[] matchFlags, string[] symbols, IJoyData spinData)
	{
		PayoutData result = TryMatchJoyData(_curPayoutConfig.Sheet.dataArray, stopIndexes, matchFlags, symbols, spinData);
		return result;
	}

	protected NearHitData CheckNearHit(IList<int> stopIndexes, bool[] matchFlags, IJoyData spinData)
	{
		NearHitData result = null;
		NearHitData[] dataArray = _curNearHitConfig.Sheet.dataArray;
		//The dataArray is designed to have ordered items from high priority to low priority.
		//So when an high priority item is found, that's it!
		for(int i = 0; i < dataArray.Length; i++)
		{
			NearHitData data = dataArray[i];
			if(IsNearHit(data, stopIndexes, matchFlags, spinData))
			{
				result = data;
				break;
			}
		}
		return result;
	}

	protected bool CanMatchSymbols(string targetName, string realName)
	{	
		bool result = (targetName == realName)
			          || (targetName == CoreDefine.WildCardSymbol)
		              || (!string.IsNullOrEmpty (realName)
		              && _symbolConfig.IsMatchSymbolType (realName, SymbolType.Wild)
		              && _symbolConfig.CanWildChange (targetName))
		              || (!string.IsNullOrEmpty (realName)
		              && _symbolConfig.IsMatchSymbolType (realName, SymbolType.Wild7)
		              && _symbolConfig.CanWild7Change (targetName));
		
		return result;
	}

	protected bool IsMatchJoyData(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData)
	{
		PayoutCheckerDelegate checker = _payoutCheckers[(int)data.PayoutType];
		CoreDebugUtility.Assert(checker != null);

		bool result = checker(data, symbolNames, matchFlags, stopIndexes, spinData);
		return result;
	}

	protected bool IsMatchPayoutOrdered(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData)
	{
		bool result = true;
		for(int i = 0; i < data.Symbols.Length; i++)
		{
			matchFlags[i] = (data.Symbols[i] == symbolNames[i] || data.Symbols[i] == CoreDefine.WildCardSymbol);
			if(!matchFlags[i])
				result = false;
		}

		return result;
	}

	protected bool IsMatchPayoutAll(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData)
	{
		bool result = true;
		for(int i = 0; i < symbolNames.Length; i++)
		{
			string name = symbolNames[i];
			matchFlags[i] = !string.IsNullOrEmpty(name) && CanMatchSymbols(data.Symbols[0], name);
			if(!matchFlags[i])
			{
				result = false;
				break;
			}
		}

		return result;
	}

	// zhousen 实现无序的检查
	protected bool IsMatchPayoutUnordered(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData){
		bool result = true;

		// 需要判断stopIndex的上下包含data.symbol[0]的内容的个数

		string symbol = data.Symbols [0];
		int sum = 0;
		for (int i = 0; i < stopIndexes.Count; ++i) {
			if (stopIndexes [i] == CoreDefine.InvalidIndex)
				continue;
			
			SingleReel reel = _reelConfig.GetSingleReel (i);
			int last = reel.GetNeighborStopIndex (stopIndexes [i], -1);
			int next = reel.GetNeighborStopIndex (stopIndexes [i], 1);
			List<int> stopList = new List<int> (){last, stopIndexes[i], next};

//			LogUtility.Log ("unordered payout check stopindex is "+last + " " + stopIndexes[i] + " " +next, Color.magenta);
			int count = ListUtility.CountElements(stopList, (int index)=>{
				return reel.GetSymbolName(index).Equals(symbol);
			});
			sum += count;
		}

		result = (sum == data.Count);

		if (result) {
			for (int i = 0; i < matchFlags.Length; ++i) {
				matchFlags [i] = true;
			}
		}

		return result;
	}

	protected bool IsMatchPayoutAny(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData)
	{
		int count = 0;
		for(int i = 0; i < symbolNames.Length; i++)
		{
			string name = symbolNames[i];
			matchFlags[i] = ListUtility.IsAnyElementSatisfied(data.Symbols, (string targetName) => {
				return !string.IsNullOrEmpty(name) && CanMatchSymbols(targetName, name);
			});
			if(matchFlags[i])
				++count;
		}

		bool result = count >= data.Count;

		// check jackpot count
		PayoutData payoutData = spinData as PayoutData;
		PayoutData refData = data as PayoutData;
		if (payoutData != null && refData != null && payoutData.JackpotCount > 0){
			bool condition1 = refData.Count < payoutData.Count;// 轴匹配数量需要小于等于
			bool condition2 = (refData.Count == payoutData.Count && refData.JackpotCount <= payoutData.JackpotCount);// 如果等于，则需要jackpotcount小于等于我们的spindata
			result = result && (condition1 || condition2);
		}

		return result;
	}

	protected bool IsNearHit(IJoyData data, IList<int> stopIndexes, bool[] matchFlags, IJoyData spinData)
	{
		bool result = false;

		if (data.PayoutType != PayoutType.UnOrdered) {
			for(int i = 0; i < stopIndexes.Count; i++)
			{
				int reelIndex = i;
				int stopIndex = stopIndexes[reelIndex];
				SingleReel singleReel = _reelConfig.GetSingleReel(reelIndex);
				List<int> offsetStopIndexes = new List<int>(stopIndexes);

				SpinDirection[] dirs = new SpinDirection[]{ SpinDirection.Up, SpinDirection.Down };
				for(int k = 0; k < dirs.Length; k++)
				{
					SpinDirection dir = dirs[k];
					int neighborIndex = singleReel.GetNeighborStopIndex(stopIndex, dir);
					offsetStopIndexes[reelIndex] = neighborIndex;

					string[] symbolNames = _reelConfig.GetSymbolNames(offsetStopIndexes);
					if(IsMatchJoyData(data, symbolNames, matchFlags, stopIndexes, spinData))
					{
						result = true;
						break;
					}
				}

				if(result)
					break;
			}
		} else {
			int fitCount = 0;// 符合的symbol数
			int dataCount = data.Count;// 目标symbol数
			string fitSymbolName = data.Symbols [0];

			for (int i = 0; i < stopIndexes.Count; ++i) {
				if (stopIndexes [i] == CoreDefine.InvalidIndex)
					continue;
				
				SingleReel singleReel = _reelConfig.GetSingleReel(i);
				int neighbourUp = singleReel.GetNeighborStopIndex (stopIndexes [i], SpinDirection.Up);
				int neighbourDown = singleReel.GetNeighborStopIndex (stopIndexes [i], SpinDirection.Down);
				List<int> areaStopIndexes = new List<int> (){ stopIndexes[i], neighbourUp, neighbourDown };

				fitCount += ListUtility.CountElements (areaStopIndexes, (int stopIndex) => {
					return singleReel.GetSymbolName(stopIndex).Equals(fitSymbolName);
				});
			}

			if (fitCount + 1 == dataCount) {
				result = true;
			}
		}

		return result;
	}

	#endregion

	#region MultiLine Predicates

	protected bool IsMatchPayoutContinuous(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData)
	{
		bool result = false;
		bool[] tempMatchFlags = new bool[matchFlags.Length];

		for(int i = 0; i < symbolNames.Length - data.Count + 1; i++)
		{
			ListUtility.FillElements(tempMatchFlags, false);
			result = IsMatchPayoutFromIndex(data, symbolNames, tempMatchFlags, stopIndexes, i);
			if(result)
			{
				tempMatchFlags.CopyTo(matchFlags, 0);
				break;
			}
		}

		return result;
	}

	protected bool IsMatchPayoutStart(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, IJoyData spinData)
	{
		bool[] tempMatchFlags = new bool[matchFlags.Length];
		bool result = IsMatchPayoutFromIndex(data, symbolNames, tempMatchFlags, stopIndexes, 0);
		if(result)
			tempMatchFlags.CopyTo(matchFlags, 0);
		return result;
	}

	#endregion

	#region Private

	private bool IsMatchPayoutFromIndex(IJoyData data, string[] symbolNames, bool[] matchFlags, IList<int> stopIndexes, int startIndex)
	{
		CoreDebugUtility.Assert(data.PayoutType == PayoutType.Continuous || data.PayoutType == PayoutType.Start);

		int matchCount = 0;
		for(int i = startIndex; i < symbolNames.Length; i++)
		{
			string name = symbolNames[i];
			if(!string.IsNullOrEmpty(name))
			{
				matchFlags[i] = ListUtility.IsAnyElementSatisfied(data.Symbols, (string s) => {
					bool r = CanMatchSymbols(s, name);
					return r;
				});
			}
			if(matchFlags[i])
				++matchCount;
			else
				break;
		}

		bool result = matchCount >= data.Count;
		return result;
	}

	#endregion

	#region Abstract

	public abstract bool ShouldHype(CoreSpinResult spinResult, int reelIndex);

	#endregion
}
