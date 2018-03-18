using System.Collections;
using System.Collections.Generic;
using System;

public class CoreMachine
{
	private delegate CoreSpinResult SpinHandler(CoreSpinInput option);
	//private delegate bool SwitchStateHandler(CoreSpinResult result);

	private string _name;
	private MachineConfig _machineConfig;
	private IRandomGenerator _roller;
	private CoreChecker	_checker;
	private CoreMultiLineChecker _multiLineChecker;
	private ICoreGenerator _generator;
	private CoreLuckyManager _luckyManager;
	private CoreSpinResult _lastSpinResult;
	private CoreSpinResult _spinResult;
	private SmallGameState _smallGameState; // refactor with FSM?
	private SmallGameState _lastSmallGameState;
	// 玩家保护机制的loss中奖率增加
	private float _payProtectionHitRate = 0.0f;

	//special games
	private CoreTapBox _tapBox;
	#if UNITY_EDITOR
	private CoreWheelGame _wheelGame;
	#endif

	private List<CoreReel> _reelList = new List<CoreReel>();

	public string Name { get { return _name; } }
	public MachineConfig MachineConfig { get { return _machineConfig; } }
	public IRandomGenerator Roller { get { return _roller; } }

	public CoreChecker Checker { get { return _checker; } }
	public CoreMultiLineChecker MultiLineChecker { get { return _multiLineChecker; } }
	public CoreBaseChecker BaseChecker {
		get {
			if(_machineConfig.BasicConfig.IsMultiLine)
				return _multiLineChecker;
			else
				return _checker;
		}
	}

	public List<CoreReel> ReelList { get { return _reelList; } }
	public CoreSpinResult LastSpinResult { get { return _lastSpinResult; } }
	public CoreSpinResult SpinResult { get { return _spinResult; } }
	public SmallGameState SmallGameState { get { return _smallGameState; } }
	public SmallGameState LastSmallGameState {get { return _lastSmallGameState; } }
	public CoreLuckyManager LuckyManager { get { return _luckyManager; } }

	private CorePlayModule _corePlayModule;
	private Dictionary<SmallGameState, CorePlayModule> _corePlayModuleDict = new Dictionary<SmallGameState, CorePlayModule> ();
	public CorePlayModule PlayModule { get { return _corePlayModule; } }
	public Dictionary<SmallGameState, CorePlayModule> PlayModuleDict{ get { return _corePlayModuleDict; } }
	public float PayProtectionHitRate{ get { return _payProtectionHitRate; } }

	//special games
	public CoreTapBox TapBox { get { return _tapBox; } }
	#if UNITY_EDITOR
	public CoreWheelGame WheelGame { get { return _wheelGame; } }
	#endif

	public CoreMachine(string name, uint randSeed)
	{
		_name = name;
		_machineConfig = new MachineConfig(name);

		_roller = new LCG(randSeed, SaveMachineSeed);
		_luckyManager = new CoreLuckyManager(_roller, ()=>{
			// 转变为NORMAL模式时，需要关闭付费保护机制
			UserBasicData.Instance.DisablePayProtection();
		});

		if(_machineConfig.BasicConfig.IsMultiLine)
		{
			_multiLineChecker = new CoreMultiLineChecker(_machineConfig);
			if(_machineConfig.BasicConfig.IsMultiLineExhaustive)
				_generator = new CoreMultiLineExhaustiveGenerator(this, _multiLineChecker, _luckyManager, _machineConfig);
			else if(_machineConfig.BasicConfig.IsMultiLineSymbolProb)
				_generator = new CoreMultiLineSymbolProbGenerator(this, _multiLineChecker, _luckyManager, _machineConfig);
			else
				CoreDebugUtility.Assert(false);
		}
		else
		{
			_checker = new CoreChecker(_machineConfig);
			_generator = new CoreGenerator(this, _checker, _luckyManager, _machineConfig);
		}

		InitReels();
		InitPlayModules();
		InitIndieGames();

		JackpotWinManager.Instance.SetCoreMachine (this);
	}

	private void InitPlayModules(){
		// 创建初始玩法模块
		CorePlayModule module = PlayModuleFactory.CreatePlayModule (SmallGameState.None, this, _generator);
		_corePlayModuleDict.Add (SmallGameState.None, module);
		// 创建配置中的玩法模块
		for (int i = 0; i < _machineConfig.BasicConfig.SmallGameTypes.Length; ++i) {
			if (_machineConfig.BasicConfig.SmallGameTypes [i].Equals ("None")) {
				continue;
			}
			string stateName = _machineConfig.BasicConfig.SmallGameTypes [i];
			SmallGameState state = TypeUtility.GetEnumFromString<SmallGameState>(stateName);
			module = PlayModuleFactory.CreatePlayModule (state, this, _generator);
			_corePlayModuleDict.Add (state, module);
		}

		_corePlayModule = _corePlayModuleDict[SmallGameState.None];
	}

	private void InitIndieGames()
	{
		if(_machineConfig.BasicConfig.HasTapBox){
			_tapBox = new CoreTapBox(this);
		}

		// This is only for machine test. In runtime mode, wheel logic is not in Core
		#if UNITY_EDITOR
		if (_machineConfig.BasicConfig.HasWheel){
			_wheelGame = new CoreWheelGame(this);
		}
		#endif
	}

	private void InitReels()
	{
		ReelConfig reelConfig = _machineConfig.ReelConfig;
		for(int i = 0; i < _machineConfig.BasicConfig.ReelCount; i++)
		{
			SingleReel config = reelConfig.GetSingleReel(i);
			int id = i + 1;

			CoreReel reel = new CoreReel(id, config, _machineConfig);
			_reelList.Add(reel);
		}
	}

	private void SaveMachineSeed(uint seed)
	{
		UserMachineData.Instance.SaveMachineSeed(_name, seed);
	}

	public void ExecuteDelegate(string name){
		_corePlayModule.ExecuteDelegate (name);
	}

	public void TryStartRound(){
		_corePlayModule.TryStartRound ();
	}

	public CoreSpinResult Spin(CoreSpinInput spinInput)
	{
		_spinResult = _corePlayModule.SpinHandler(spinInput);
		_spinResult.Recheck(spinInput);
		_lastSpinResult = _spinResult;
		RefreshPayProtection (_spinResult, spinInput);
		_luckyManager.RefreshLucky(_spinResult);
		UserTempData.Instance.LastBetAmount = spinInput.BetAmount;
		UserMachineData.Instance.IncreaseSpinCount(_name, false, _smallGameState);

		if(spinInput.IsRespin)
		{
			_spinResult.IsRespin = true;
			_corePlayModule.StartRespin();
		}

		DebugShowSpinResult(_spinResult);

		return _spinResult;
	}

	public void ChangeSmallGameState(SmallGameState s){
		if (s != _smallGameState) {
			CoreDebugUtility.Log ("Change SmallGameState: " + s.ToString ());
			_smallGameState = s;

			if (_corePlayModule != null) {
				_corePlayModule.Exit ();
			}
			_corePlayModule = _corePlayModuleDict [s];
			if (_corePlayModule != null) {
				_corePlayModule.Enter ();
			}
		}
	}

	public void SaveLastSmallGameState(){
		_lastSmallGameState = _smallGameState;
	}

	public bool ShouldRespin()
	{
		return _corePlayModule.ShouldRespin();
	}

	public void CheckAddJackpotPoint(WinType winType)
	{
		bool shouldRespin = ShouldRespin();
		JackpotWinManager.Instance.CheckAddJackpotPoint(shouldRespin, winType, _spinResult);
	}

	public bool CheckSwitchSmallGameState(CoreSpinResult spinResult, SmallGameMomentType momentType){
		return _corePlayModule.CheckSwitchSmallGameState(spinResult, momentType);
	}

	public bool IsTriggerSmallGameState(){
		return _corePlayModule.IsTriggerSmallGameState ();
	}

	private List<int> GetSpinIndexList(int rollIndex )
	{
		List<int> result = new List<int>();
		for(int i = 0; i < _reelList.Count; i++)
		{
			int symbolCount = _reelList[i].SymbolCount();

			int singleIndex = rollIndex % symbolCount;
			result.Add(singleIndex);

			rollIndex = rollIndex / symbolCount;
		}
		return result;
	}

	private void DebugShowSpinResult(CoreSpinResult result)
	{
		CoreDebugUtility.Log(result.ToString());
	}

	public void EndRound()
	{
		UserBasicData.Instance.Save();
		UserMachineData.Instance.Save(); //save machine seed
	}

	public void SetSpinResult(CoreSpinResult result){
		_spinResult = result;
	}

	#region pay protection

	private void AddPayProtectionHitRate(CoreSpinInput spinInput){
		if (UserBasicData.Instance.PayProtectionEnable || spinInput.IsPayProtectionTest) {
			_payProtectionHitRate += CoreConfig.Instance.LuckyConfig.HitRateIncreaseAfterPay;
		}
	}

	private void ResetPayProtectionHitRate(){
		_payProtectionHitRate = 0.0f;
	}

	private void RefreshPayProtection(CoreSpinResult result, CoreSpinInput spinInput){
		if (result.Type != SpinResultType.Win) {
			AddPayProtectionHitRate (spinInput);
		} else {
			ResetPayProtectionHitRate ();
		}
	}

	#endregion
}


