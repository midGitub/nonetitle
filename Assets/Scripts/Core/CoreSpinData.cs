using System.Collections;
using System.Collections.Generic;

public enum SpinDataType
{
	None = -1,
	FreeSpin,
	FixWild,
	Bonus,
	Collect,
	Max,
}

public class CoreSpinData{

}

public class CoreNormalSpinData : CoreSpinData{
}

public class CoreFreeSpinData : CoreSpinData{
	private float[] _freeSpinHits;
	private float[] _freeSpinStopProbs;
	private int _respinCount;
	private List<int> _forceWinIndexList = new List<int> ();
	private int _totalFixCount;

	public float[] FreeSpinHits { get { return _freeSpinHits; } }
	public float[] FreeSpinStopProbs { get { return _freeSpinStopProbs; } }
	public int RespinCount { get { return _respinCount; } set { _respinCount = value; } }
	public List<int> ForceWinIndexList { get { return _forceWinIndexList; } }
	public int TotalFixCount { get { return _totalFixCount; } }

	public CoreFreeSpinData(float[] freeSpinHits, float[] freeSpinStopProbs, int totalFixCount)
	{
		_freeSpinHits = freeSpinHits;
		_freeSpinStopProbs = freeSpinStopProbs;
		_respinCount = 0;
		_totalFixCount = totalFixCount;
	}

	public void InitForceWinIndexList(IRandomGenerator generator, int forceCount){
		if (_totalFixCount == 0)
			return;
		
		List<int> indexes = RandomUtility.RollIntList (generator, _totalFixCount, forceCount);
		_forceWinIndexList.AddRange (indexes);
	}

	public bool IsCurrentForceWinIndex(){
		bool result = _forceWinIndexList.Contains(_respinCount);
		if (result) {
			_forceWinIndexList.Remove (_respinCount);
		}
		return result;
	}

	public bool RollFreeSpinWin(IRandomGenerator roller)
	{
		int winIndex = (_respinCount < _freeSpinHits.Length) ? _respinCount : _freeSpinHits.Length - 1;
		float prob = _freeSpinHits[winIndex];
		RollHelper spinHelper = new RollHelper(prob);
		int rollIndex = spinHelper.RollIndex(roller);
		bool isWin = rollIndex == 0;
		return isWin;
	}

	public void AddTotalFixCount(int count)
	{
		_totalFixCount += count;
	}
}

public class CoreFixWildSpinData : CoreSpinData{
	private float[] _freeSpinHits;
	// 当前转得次数
	private int _spinCount;
	// 固定轴数量
	private int _fixReelCount;
	// 中奖率hit
	private float[] _fixhit = new float[2];


	public float[] FreeSpinHits { get { return _freeSpinHits; } }
	public int SpinCount { get { return _spinCount; } set { _spinCount = value; } }
	public int FixReelCount { get { return _fixReelCount; } set { _fixReelCount = value; } }
	public float[] FixHit {
		get { return _fixhit; }
		set { _fixhit = value; }
	}

	public CoreFixWildSpinData(float[] freeSpinHits, int fixreelCount, float hit, float hit2)
	{
		_freeSpinHits = freeSpinHits;
		_spinCount = 0;
		_fixReelCount = fixreelCount;
		_fixhit[0] = hit;
		_fixhit [1] = hit2;
	}
}

public class CoreBonusSpinData : CoreSpinData{}

public class CoreCollectSpinData : CoreSpinData{
	// 机器名
	private string _name = "";
	// 当前收集的数量
	private int _collectNum = 0;
	// 收集最大数量 
	private int _collectMax = 0;
	// 当前下注
	private ulong _betAmount = 100UL;
	// 已经完成收集
	private bool _isFinish = false;

	public CoreCollectSpinData(string machineName, int collectNum, int collectMax, ulong betAmount){
		_name = machineName;
		_collectNum = collectNum;
		_collectMax = collectMax;
		_betAmount = betAmount;
		_isFinish = false;
	}

	public int GetCollectNum(){
		return _collectNum;
	}

	public ulong GetBetAmount(){
		return _betAmount;
	}

	public bool IsFinishCollect(){
		return _isFinish;
	}

	public void ResetFinishCollect(){
		_isFinish = false;
	}

	public void AddCollectNum(int count){
		_collectNum += count;

		if (_collectNum >= _collectMax) {
			_collectNum = 0;
			_isFinish = true;
		}

		// 记录到user basic data 本次收集数量
		if (count != 0){
			UserMachineData.Instance.SetBetCollectNum (_name, _betAmount, _collectNum);	
		}
	}

	public void ChangeBetAmount(ulong betAmount){
		_betAmount = betAmount;
	}

	public void ChangelCollectNum(int collectNum){
		_collectNum = collectNum;
	}

	public void ChangeCollectMax(int collectMax){
		_collectMax = collectMax;
	}
}

public class CoreWheelSpinData : CoreSpinData{}