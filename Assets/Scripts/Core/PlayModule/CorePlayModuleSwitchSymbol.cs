using System.Collections;
using System.Collections.Generic;

public class CorePlayModuleSwitchSymbol : CorePlayModule {

	private SymbolConfig _symbolConfig;
	private IRandomGenerator _roller;
	private CoreChecker _checker;
	private PayoutData _payoutData;

	public PayoutData PayoutData { get { return _payoutData; } }

	public CorePlayModuleSwitchSymbol(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
	: base(state, machine, generator){
		_momentType = SmallGameMomentType.Front;
		_symbolConfig = _machineConfig.SymbolConfig;
		_roller = _coreMachine.Roller;
		_checker = machine.Checker;
		_payoutData = null;

		CoreDebugUtility.Assert(_checker != null, "machine.Checker is non-null only for single line machine");
	}

	public override bool IsTriggerSmallGameState(){
		return _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.SwitchSymbol;
	}

	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		_coreMachine.SaveLastSmallGameState();

		CoreDebugUtility.Log("Switch symbol");

		if(CheckTriggerSwitchSymbol(spinResult)){
			RefreshTriggerInfo(spinResult);
			UpdateSwitchResult(spinResult);
		}
		else {
			_coreMachine.ChangeSmallGameState(SmallGameState.None);
		}

		return _coreMachine.LastSmallGameState != _coreMachine.SmallGameState;
	}

	private void UpdateSwitchResult(CoreSpinResult spinResult){
		_payoutData = null;
		CoreSpinResult result = GenerateSwitchResult(spinResult);
		_payoutData = GetSwitchSymbolPayout(result.JoyData, result.SymbolList);
		result.SetPayoutData(_payoutData);
		result.FillWinRatio();
		result.RefreshWinAmount();
		_coreMachine.SetSpinResult(result);
	}

	private bool CheckTriggerSwitchSymbol(CoreSpinResult spinResult){
		int triggerCount = ListUtility.CountElements(spinResult.SymbolList, (CoreSymbol symbol) => {
			return CorePlayModuleHelper.IsSymbolTriggerSwitchSymbol(_machineConfig, symbol.SymbolData.Name);
		});

		CoreDebugUtility.Log("check Trigger Switch Symbol count = "+triggerCount);
		return triggerCount >= _machineConfig.BasicConfig.SymbolSwitchTriggerCount && spinResult.Type == SpinResultType.Win;
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		return false;
	}

	public CoreSpinResult GenerateSwitchResult(CoreSpinResult result){
		List<CoreSymbol> switchSymbolList = GenerateSwitchSymbolList(result);
		result.FillSwitchSymbols(switchSymbolList);
		return result;
	}

	private PayoutData GetSwitchSymbolPayout(IJoyData data, List<CoreSymbol> list){
		CoreCheckResult result = _checker.CheckResultWithSymbols(data, list);
		return result.PayoutData;
	}

	private List<CoreSymbol> GenerateSwitchSymbolList(CoreSpinResult result){
		List<CoreSymbol> switchSymbolList = new List<CoreSymbol>();
		ListUtility.ForEach(result.SymbolList, (CoreSymbol symbol)=>{
			SymbolData data = Roll(symbol.SymbolData);
			CoreSymbol cloneSymbol = new CoreSymbol(symbol.ReelId, symbol.StopId, data);
			switchSymbolList.Add(cloneSymbol);
		});
		return switchSymbolList;
	}

	private SymbolData Roll(SymbolData data){
		RollHelper helper = new RollHelper(data.SwitchSymbolProbs);
		int i = helper.RollIndex(_roller);
		CoreDebugUtility.Log("Roll name " + data.Name + " index = " + i);
		string name = data.SwitchSymbolNames[i];
		CoreDebugUtility.Log(data.Name + " Switch to " + name);
		SymbolData switchData = _symbolConfig.GetSymbolData(name);
		return switchData;
	}

	public override void Enter(){
		RefreshTriggerInfo(_coreMachine.SpinResult);
		UpdateSwitchResult(_coreMachine.SpinResult);
	}

	public override void Exit(){
		_payoutData = null;
		ClearTriggerInfo();
	}

	void RefreshTriggerInfo(CoreSpinResult spinResult)
	{
		_triggerPayoutData = spinResult.PayoutData;
		_triggerSymbols.Clear();
		_triggerSymbols = CorePlayModuleHelper.GetTriggerSymbolsPayline(_machineConfig, spinResult, _machineConfig.BasicConfig.SymbolSwitchTriggerNames);
		CoreDebugUtility.Log("RefreshTriggerInfo count = " + _triggerSymbols.Count);
	}

	void ClearTriggerInfo()
	{
		_triggerPayoutData = null;
		_triggerSymbols.Clear();
	}

	public ulong GetWinAmount(ulong bet){
		if (_coreMachine.SpinResult != null){
			_coreMachine.SpinResult.FeedBetAndWinAmount(bet);
			return _coreMachine.SpinResult.WinAmount;
		}
		// if (_payoutData != null){
		// 	return (ulong)(bet * _payoutData.Ratio);
		// }
		return 0;
	}
}
