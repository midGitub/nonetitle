using System.Collections;
using System.Collections.Generic;
using System;

public class CorePlayModuleFixWild : CorePlayModule
{
	// fixwild数据
	private CoreFixWildSpinData _fixWildData;
	// 触发的次数
	private int _lastWildTriggerCount = 0;
	// 固定轴数
	private int _fixReelCount = 0;
	// 上一次锁住的滚轴ID
	private List<int> _lastFixReelIndexList = new List<int>();
	// 锁住的滚轴ID
	private List<int> _fixReelIndexList = new List<int>();

	// FixWild类型
	private FixWildType _type;

	public List<int> LastFixReelIndexList {
		get{ return _lastFixReelIndexList; }
	}

	public List<int> FixReelIndexList{
		get{ return _fixReelIndexList; }
	}

	public int FixReelCount {
		get { return _fixReelCount; }
	}

	public CorePlayModuleFixWild(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
		: base(state, machine, generator){
		_fixWildData = null;
		_lastWildTriggerCount = 0;
		_fixReelCount = 0;
		_type = _machineConfig.BasicConfig.FixWildType;
		_momentType = SmallGameMomentType.Front;
	}

	public override CoreSpinResult SpinHandler (CoreSpinInput spinInput)
	{
		_fixWildData.SpinCount = _lastWildTriggerCount;
		_fixWildData.FixReelCount = _fixReelCount;

		spinInput.FixWildSpinData = _fixWildData;
		spinInput.FixedSymbols = CoreUtility.FetchSymbols (_coreMachine.SpinResult.SymbolList, (CoreSymbol s) => {
			return _machineConfig.SymbolConfig.IsMatchSymbolType(s.SymbolData.SymbolType, SymbolType.Wild);
		}, _machineConfig.BasicConfig.ReelCount);

		CoreSpinResult result = _coreGenerator.Roll (spinInput);
		return result;
	}

	public override bool IsTriggerSmallGameState(){
		return _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.FixWild;
	}


	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		Func<CoreSpinResult, int, bool> checkFunc = null;
		if (_type == FixWildType.FishHunting) {
			checkFunc = CheckStateNormalFishHunting;
		} else {
			checkFunc = CheckStateNormalFrozen;
		}

//		LogUtility.Log ("check small game state fix wild", Color.red);
		_coreMachine.SaveLastSmallGameState ();

		int fixReelCount = ListUtility.CountElements (spinResult.SymbolList, (CoreSymbol symbol) => {
			return _machineConfig.SymbolConfig.IsMatchSymbolType (symbol.SymbolData.SymbolType, SymbolType.Wild);
		});

		if (checkFunc (spinResult, fixReelCount)) {
			_coreMachine.ChangeSmallGameState (SmallGameState.None);
		} else {
			_fixReelCount = fixReelCount;
			_lastWildTriggerCount += 1;

			List<int> fixReelIndexList = ListUtility.IndexList (_coreMachine.SpinResult.SymbolList, (CoreSymbol symbol) => {
				return _machineConfig.SymbolConfig.IsMatchSymbolType (symbol.SymbolData.SymbolType, SymbolType.Wild);
			});
			_lastFixReelIndexList = ListUtility.SubtractList (fixReelIndexList, _fixReelIndexList);
			_fixReelIndexList.AddRange (_lastFixReelIndexList);
			if (_fixWildData != null) {
				_fixWildData.SpinCount = _lastWildTriggerCount;
				_fixWildData.FixReelCount = _fixReelCount;
			}
		}

		return _coreMachine.SmallGameState != _coreMachine.LastSmallGameState;
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		return false;
	}

	private void InitFixWildData(CoreSpinResult spinResult, int fixReelCount)
	{
		float[] freeSpinHits = null;
		float fixhit = 0.0f;
		float fixhit2 = 0.0f;
		if (spinResult.Type == SpinResultType.Win) {
			fixhit = spinResult.PayoutData.Fix1Hit;
			fixhit2 = spinResult.PayoutData.Fix2Hit;
		} else {
			fixhit = _machineConfig.BasicConfig.FixHitNonWin;
//			freeSpinHits = _machineConfig.BasicConfig.FreeSpinHitsNonWin;
		}
		_fixWildData = new CoreFixWildSpinData(freeSpinHits, fixReelCount, fixhit, fixhit2);
	}

	private void ResetFixWildData(){
		_fixWildData = null;
	}

	public override void Enter ()
	{
		int fixReelCount = ListUtility.CountElements (_coreMachine.SpinResult.SymbolList, (CoreSymbol symbol) => {
			return _machineConfig.SymbolConfig.IsMatchSymbolType (symbol.SymbolData.SymbolType, SymbolType.Wild);
		});
		_fixReelCount = fixReelCount;
		_lastWildTriggerCount = 0;
		InitFixWildData (_coreMachine.SpinResult, fixReelCount);
		// 添加锁住轴的index
		List<int> fixReelIndexList = ListUtility.IndexList (_coreMachine.SpinResult.SymbolList, (CoreSymbol symbol) => {
				return _machineConfig.SymbolConfig.IsMatchSymbolType (symbol.SymbolData.SymbolType, SymbolType.Wild);
			});
		_lastFixReelIndexList.Clear ();
		_fixReelIndexList.Clear ();
		_lastFixReelIndexList.AddRange (fixReelIndexList);
		_fixReelIndexList.AddRange (fixReelIndexList);
	}

	public override void Exit ()
	{
		_lastWildTriggerCount = 0;
		_fixReelCount = 0;
		ResetFixWildData ();
		_lastFixReelIndexList.Clear ();
	}

	private bool CheckStateNormalFrozen(CoreSpinResult result, int fixCount){
		return fixCount <= _fixReelCount || fixCount == _machineConfig.BasicConfig.ReelCount;
	}

	private bool CheckStateNormalFishHunting(CoreSpinResult result, int fixCount){
		return fixCount == _machineConfig.BasicConfig.ReelCount || result.Type != SpinResultType.Win || result.WinRatio == 0.0f;
	}
}
