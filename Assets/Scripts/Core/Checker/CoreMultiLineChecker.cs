using System.Collections;
using System.Collections.Generic;

public class CoreMultiLineChecker : CoreBaseChecker
{
	#region Init

	public CoreMultiLineChecker(MachineConfig machineConfig)
	{
		base.Init(machineConfig);
	}

	#endregion

	#region Public

	public CoreMultiLineCheckResult CheckResultWithStopIndexes(IList<int> stopIndexes, IJoyData spinData = null)
	{
		CoreMultiLineCheckResult result = new CoreMultiLineCheckResult(_machineConfig);

		for(int i = 0; i < _machineConfig.PaylineConfig.AlllineList.Count; i++)
		{
			int[] payline = _machineConfig.PaylineConfig.AlllineList[i];
			int[] tempIndexes = new int[stopIndexes.Count];
			stopIndexes.CopyTo(tempIndexes, 0);

			_machineConfig.ReelConfig.OffsetStopIndexes(tempIndexes, payline);

			bool[] matchFlags = new bool[stopIndexes.Count];

			string[] symbolNames = _reelConfig.GetSymbolNames(tempIndexes);

			//fill wildcard symbols for hype feature in 5 reels
			for(int k = 0; k < tempIndexes.Length; k++)
			{
				if(tempIndexes[k] == WildcardStopIndex)
					symbolNames[k] = CoreDefine.WildCardSymbol;
			}

			PayoutData data = CheckPayout(tempIndexes, matchFlags, symbolNames, spinData);
			if(data != null)
			{
				if(_machineConfig.PaylineConfig.IsPayline(payline))
					result.AddPayoutInfo(tempIndexes, payline, data, matchFlags);
				else
					result.AddNearHitInfo(tempIndexes, payline, data, matchFlags);
			}
		}

		result.RefreshResult();
		return result;
	}

	public override bool ShouldHype(CoreSpinResult spinResult, int reelIndex)
	{
		List<int> indexes = new List<int>(spinResult.StopIndexes);
		for(int i = reelIndex; i < indexes.Count; i++)
			indexes[i] = WildcardStopIndex;
		
		CoreMultiLineCheckResult checkResult = CheckResultWithStopIndexes(indexes);
		float threshold = GetMultiLineHypeThreshold();
		bool result = checkResult.PayoutReward >= threshold;
		return result;
	}

	private float GetMultiLineHypeThreshold(){
		if (_machineConfig.BasicConfig.MultilineHypeThreshold > 0.0f){
			return _machineConfig.BasicConfig.MultilineHypeThreshold;
		}
		return CoreConfig.Instance.MiscConfig.MultiLineHypeThreshold;
	}

	#endregion
}
