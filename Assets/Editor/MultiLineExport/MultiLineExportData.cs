using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiLineExportData
{
	public float _payoutReward;
	public float _nearHitReward;
	public int[] _stopIndexes;

	public MultiLineExportData(float payoutReward, float nearHitReward, int[] stopIndexes)
	{
		_payoutReward = payoutReward;
		_nearHitReward = nearHitReward;
		_stopIndexes = stopIndexes;
	}
}
