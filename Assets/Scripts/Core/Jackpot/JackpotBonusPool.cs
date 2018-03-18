using System.Collections;
using System.Collections.Generic;
using System;

public class JackpotBonusPool {
	//当前奖池类型
	protected JackpotType _jackpotType;
	// 当前奖池子类型
	protected JackpotPoolType _jackpotPoolType;

	// 当前奖金
	protected ulong _currentBonus;
	// 下次获奖时奖金数值
	protected ulong _nextWinBonus;

	// 随机时间数种子
	protected int _seed;
	// 随机数生成器
	protected IRandomGenerator _generator;
	// 上次的奖金增长量
	protected int _lastJackpotBonusDelta;
	// 是否触发奖金
	protected bool _lastTriggerBonus;
	protected bool _currentTriggerBonus;

	// 随机初始化起始值
	protected int _randomStartBonus;
	// 随机初始化结束值
	protected int _randomEndBonus;
	// 增加量下阈值
	protected int _deltaMin;
	// 增加量上阈值
	protected int _deltaMax;
	// 默认配置值
	protected int _defaultBonus;
	// 下阈值
	protected int _lowerBonusThreshold;
	// 上阈值
	protected int _upperBonusThreshold;

	// 参考用,用于生成自身的初始值
	protected int _referenceBonus;
	// jackpot 通知回调函数
	protected Action<string, ulong> _triggerJackpotAction = null; 

	protected string _machineName;

	public ulong CurrentBonus{
		get { return _currentBonus; }
	}

	public ulong NextBonus{
		get { return _nextWinBonus; }
	}

	public JackpotBonusPool(){
	}

	public void AddListener(Action<string, ulong> func){
		_triggerJackpotAction = func;
	}

	public void SetMachineName(string name){
		_machineName = name;
	}

	public virtual void Init(){
		_deltaMin = JackpotDefine.JACKPOT_DEFAULT_INCREASE_TABLE[(int)_jackpotPoolType][0];
		_deltaMax = JackpotDefine.JACKPOT_DEFAULT_INCREASE_TABLE[(int)_jackpotPoolType][1];
		_defaultBonus = JackpotDefine.JACKPOT_DEFAULT_TABLE[(int)_jackpotPoolType][0];
		_lowerBonusThreshold = JackpotDefine.JACKPOT_DEFAULT_TABLE[(int)_jackpotPoolType][1];
		_upperBonusThreshold = JackpotDefine.JACKPOT_DEFAULT_TABLE[(int)_jackpotPoolType][2];

		int Seed = new Random().Next();
		_generator = new LCG((uint)Seed, null);
	}

	public virtual int CreateDefaultBonus(int referenceBonus){
		return -1;
	}

	public virtual void SetBonusLinear(int current, int next, float deltaTime){
		int deltaBonus = (int)JackpotHelper.GetRandom (_deltaMin, _deltaMax, _generator);
		_currentBonus = (ulong)(current + deltaTime * deltaBonus);
		// 无需重置算法
		//_nextWinBonus = JackpotHelper.CreateNextBonus(_currentBonus, _lowerBonusThreshold, _upperBonusThreshold, _generator);
		_nextWinBonus = (ulong)next;
	}

	public virtual void RefreshBonus(){
		_lastTriggerBonus = _currentTriggerBonus;

		_lastJackpotBonusDelta = (int)JackpotHelper.GetRandom(_deltaMin, _deltaMax, _generator);
		_currentBonus += (ulong)_lastJackpotBonusDelta;
		if (_currentBonus >= _nextWinBonus) {
			#if DEBUG
			LogUtility.Log ("Trigger jackpot bonus cur = "+_currentBonus+" next = "+_nextWinBonus);
			LogUtility.Log ("default = " + _defaultBonus);
			LogUtility.Log ("jackpot type = " + _jackpotType);
			LogUtility.Log ("jackpot pool type = "+_jackpotPoolType);
			#endif
			_currentTriggerBonus = true;
			if (_triggerJackpotAction != null && IsSingleWinOrColossal()) {
				_triggerJackpotAction (_machineName, _currentBonus - (ulong)_defaultBonus);
			}
			ResetBonus ();
		} else {
			_currentTriggerBonus = false;
		}
	}

	private bool IsSingleWinOrColossal(){
		return _jackpotPoolType == JackpotPoolType.Single || _jackpotPoolType == JackpotPoolType.Colossal;
	}

	// 中奖重置
	public virtual void ResetBonus(){
		_currentBonus = (ulong)_defaultBonus;
		_nextWinBonus = JackpotHelper.CreateNextBonus (_currentBonus, (ulong)_lowerBonusThreshold, (ulong)_upperBonusThreshold, _generator);
	}

	public virtual bool IsTriggerBonus(){
		return _lastTriggerBonus != _currentTriggerBonus;
	}
}

public class JackpotBonusSinglePool : JackpotBonusPool{
	public JackpotBonusSinglePool(){
		_jackpotPoolType = JackpotPoolType.Single;
		base.Init ();
	}

	public override int CreateDefaultBonus(int referenceBonus){
		_randomStartBonus = JackpotDefine.SINGLE_JACKPOT_START_MIN;
		_randomEndBonus = JackpotDefine.SINGLE_JACKPOT_START_MAX;
		_currentBonus = (ulong)JackpotHelper.GetRandom (_randomStartBonus, _randomEndBonus, _generator);
		_nextWinBonus = JackpotHelper.CreateNextBonus (_currentBonus, (ulong)_lowerBonusThreshold, (ulong)_upperBonusThreshold, _generator);
		return (int)_currentBonus;
	}
}

public class JackpotBonusColossalPool : JackpotBonusPool{
	public JackpotBonusColossalPool(){
		_jackpotPoolType = JackpotPoolType.Colossal;
		base.Init ();
	}

	public override int CreateDefaultBonus (int referenceBonus)
	{
		_randomStartBonus = JackpotDefine.FOUR_JACKPOT_COLOSSAL_START_MIN;
		_randomEndBonus = JackpotDefine.FOUR_JACKPOT_COLOSSAL_START_MAX;
		_currentBonus = (ulong)JackpotHelper.GetRandom(_randomStartBonus, _randomEndBonus, _generator);
		_nextWinBonus = JackpotHelper.CreateNextBonus (_currentBonus, (ulong)_lowerBonusThreshold, (ulong)_upperBonusThreshold, _generator);
		return (int)_currentBonus;
	}
}

public class JackpotBonusMegaPool : JackpotBonusPool{
	public JackpotBonusMegaPool(){
		_jackpotPoolType = JackpotPoolType.Mega;
		base.Init ();
	}

	public override int CreateDefaultBonus (int referenceBonus)
	{
		float ratio = JackpotHelper.GetRandom(JackpotDefine.MEGA_JACKPOT_RATIO_MIN_RATIO, JackpotDefine.MEGA_JACKPOT_RATIO_MAX_RATIO, _generator);

		_randomStartBonus = (int)(referenceBonus * JackpotDefine.MEGA_JACKPOT_RATIO_MIN_RATIO);
		_randomEndBonus = (int)(referenceBonus * JackpotDefine.MEGA_JACKPOT_RATIO_MAX_RATIO);
		_currentBonus = (ulong)(referenceBonus * ratio);
		_nextWinBonus = JackpotHelper.CreateNextBonus (_currentBonus, (ulong)_lowerBonusThreshold, (ulong)_upperBonusThreshold, _generator);
		return (int)_currentBonus;
	}
}

public class JackpotBonusHugePool : JackpotBonusPool{
	public JackpotBonusHugePool(){
		_jackpotPoolType = JackpotPoolType.Huge;
		base.Init ();
	}

	public override int CreateDefaultBonus (int referenceBonus)
	{
		float ratio = JackpotHelper.GetRandom (JackpotDefine.HUGE_JACKPOT_RATIO_MIN_RATIO, JackpotDefine.HUGE_JACKPOT_RATIO_MAX_RATIO, _generator);

		_randomStartBonus = (int)(referenceBonus * JackpotDefine.HUGE_JACKPOT_RATIO_MIN_RATIO);
		_randomEndBonus = (int)(referenceBonus * JackpotDefine.HUGE_JACKPOT_RATIO_MAX_RATIO);
		_currentBonus = (ulong)(referenceBonus * ratio);
		_nextWinBonus = JackpotHelper.CreateNextBonus (_currentBonus, (ulong)_lowerBonusThreshold, (ulong)_upperBonusThreshold, _generator);
		return (int)_currentBonus;
	}
}

public class JackpotBonusBigPool : JackpotBonusPool{
	public JackpotBonusBigPool(){
		_jackpotPoolType = JackpotPoolType.Big;
		base.Init ();
	}

	public override int CreateDefaultBonus (int referenceBonus)
	{
		float ratio = JackpotHelper.GetRandom (JackpotDefine.BIG_JACKPOT_RATIO_MIN_RATIO, JackpotDefine.BIG_JACKPOT_RATIO_MAX_RATIO, _generator);

		_randomStartBonus = (int)(referenceBonus * JackpotDefine.BIG_JACKPOT_RATIO_MIN_RATIO);
		_randomEndBonus = (int)(referenceBonus * JackpotDefine.BIG_JACKPOT_RATIO_MAX_RATIO);
		_currentBonus = (ulong)(referenceBonus * ratio);
		_nextWinBonus = JackpotHelper.CreateNextBonus (_currentBonus, (ulong)_lowerBonusThreshold, (ulong)_upperBonusThreshold, _generator);
		return (int)_currentBonus;
	}
}
