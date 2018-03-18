using System.Collections;
using System.Collections.Generic;
using System;

public class CorePlayModule 
{
	// 机台对象
	protected CoreMachine _coreMachine;
	// 机台配置
	protected MachineConfig _machineConfig;
	// 生成器对象
	protected ICoreGenerator _coreGenerator;
	// 玩法状态
	protected SmallGameState _currentSmallGameState;

	// 触发状态
	protected TriggerType _triggerType;
	// Note 1: When _triggerType is Payout, _triggerPayoutData records current row of PayoutData,
	// Currently it's used for Wheel and TapBox, but it's supposed to be general used.
	// Note 2: Now it's used for FreeSpin while TriggerType is UnorderCount.
	protected PayoutData _triggerPayoutData;
	protected List<CoreSymbol> _triggerSymbols = new List<CoreSymbol>();

	// 函数容器
	protected Dictionary<string, Action> _coreDelegateCache = new Dictionary<string, Action> ();
	// 执行时刻
	protected SmallGameMomentType _momentType;

	public SmallGameState CurrentSmallGameState
	{
		get { return _currentSmallGameState; }
	}

	public PayoutData TriggerPayoutData
	{
		get { return _triggerPayoutData; }
	}

	public List<CoreSymbol> TriggerSymbols
	{
		get { return _triggerSymbols; }
	}

	public CorePlayModule(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
	{
		_coreMachine = machine;
		_machineConfig = _coreMachine.MachineConfig;
		_coreGenerator = generator;
		_currentSmallGameState = state;
		_triggerType = _machineConfig.BasicConfig.TriggerType;
		_momentType = SmallGameMomentType.None;
	}

	// 扩充执行函数
	public void AddDelegateCache(string name, Action callback){
		if (_coreDelegateCache.ContainsKey(name)) {
			CoreDebugUtility.Log ("delegate cache has same key = " + name);
		} else {
			_coreDelegateCache.Add (name, callback);
		}
	}

	// 执行函数
	public void ExecuteDelegate(string name){
		Action cb;
		if (_coreDelegateCache.TryGetValue (name, out cb)) {
			cb ();
		} else {
			CoreDebugUtility.Log ("delegate cache has not found = " + name);
		}
	}

	// 判断触发当前模式
	public virtual bool IsTriggerSmallGameState(){
		return false;
	}

	// 计算SPIN结果
	public virtual CoreSpinResult SpinHandler(CoreSpinInput spinInput)
	{
		CoreSpinResult result = _coreGenerator.Roll (spinInput);
		return result;
	}

	// 是否需要RESPIN
	public virtual bool ShouldRespin(){
		return false;
	}

	// 检查当前状态
	public bool CheckSwitchSmallGameState(CoreSpinResult spinResult, SmallGameMomentType momentType){
		bool result = false;
		SmallGameMomentType type = _momentType & momentType;
		if(type != SmallGameMomentType.None)
		{
			if (momentType == SmallGameMomentType.Front){
				result = CheckSwitchSmallGameStateFront(spinResult);
			}else if (momentType == SmallGameMomentType.Behind){
				result = CheckSwitchSmallGameStateBehind(spinResult);
			}
		}
			
		return result;
	}

	protected virtual bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		return false;
	}

	protected virtual bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		return false;
	}

	// 进入状态
	public virtual void Enter(){
	}

	// 出状态
	public virtual void Exit(){
	}
		
	public virtual void TryStartRound(){
	}

	public virtual void StartRespin(){
	}
}


