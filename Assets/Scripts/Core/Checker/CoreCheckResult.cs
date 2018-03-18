using System.Collections;

public class CoreCheckResult : CoreBaseCheckResult
{
	private PayoutData _payoutData; //non-null when win
	private NearHitData _nearHitData; //non-null when nearHit
	private bool[] _matchFlags;

	public PayoutData PayoutData { get { return _payoutData; } set { _payoutData = value; } }
	public NearHitData NearHitData { get { return _nearHitData; } set { _nearHitData = value; } }
	public bool[] MatchFlags { get { return _matchFlags; } }

	public CoreCheckResult()
	{
		_matchFlags = new bool[MachineConfig.Instance.BasicConfig.ReelCount];
	}

	public bool IsWinWithNonZeroRatio()
	{
		return _type == SpinResultType.Win && (_payoutData != null && _payoutData.Ratio > 0.0f);
	}

	public bool IsWinWithZeroRatio()
	{
		return _type == SpinResultType.Win && (_payoutData != null && _payoutData.Ratio == 0.0f);
	}

	public bool CanBeNearHit(CoreSpinResult spinResult){
		return _type != SpinResultType.Win 
			|| (spinResult.HasFixedSymbol && _payoutData != null && _payoutData.NearHitOrLossForFixWild);
	}

	public bool CanBeLoss(CoreSpinResult spinResult){
		return _type == SpinResultType.Loss  
			|| (spinResult.HasFixedSymbol && _payoutData != null && _payoutData.NearHitOrLossForFixWild);
	}

	public override string ToString()
	{
		string s;
		if(_type == SpinResultType.Win)
		{
			s = string.Format("type:{0}, id:{1}, payoutType:{2}, matchFlags:({3}, {4}, {5})", 
				_type, _payoutData.Id, _payoutData.PayoutType, _matchFlags[0], _matchFlags[1], _matchFlags[2]);
		}
		else if(_type == SpinResultType.NearHit)
		{
			s = string.Format("type:{0}, id:{1}, payoutType:{2}, matchFlags:({3}, {4}, {5})", 
				_type, _nearHitData.Id, _nearHitData.PayoutType, _matchFlags[0], _matchFlags[1], _matchFlags[2]);
		}
		else
		{
			s = string.Format("type:{0}", _type);
		}

		return s;
	}
}
