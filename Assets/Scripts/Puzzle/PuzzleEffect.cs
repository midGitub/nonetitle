using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using System;

public class PuzzleEffect : MonoBehaviour
{
	private static readonly string _effectPrefabPath = "Effect/Prefab/";
	private static readonly string _compositeEffectPath = "Game/";
	private static readonly float _symbolBlinkPeriod = 2.0f;

	public PuzzleMachine _machine;
	public GameObject _bigWinMachineEffect;
	public GameObject _epicWinMachineEffect;
	public GameObject _jackpotWinMachineEffect;
	public GameObject _respinPreEffect;

	//	public GameObject _hypeMachineEffect;
	public Text _bigWinNumberText;
	public Text _epicWinNumberText;
	public Text _jackpotWinNumberText;

    public Animator _bigWinDoubleRewardAnimator;
    public Animator _epicWinDoubleRewardAnimator;
    public Animator _jackpotWinDoubleRewardAnimator;

    private GameObject _idleReelSideEffect;
	private GameObject _spinReelSideEffect;
	private GameObject _spinReelSideEndEffect;
	private GameObject _respinReelSideEffect;
	private GameObject _normalWinReelSideEffect;
	private GameObject _bigWinReelSurroundingsEffect;
	private GameObject _smallGameEffectParent;
	private GameObject _smallGameSpecialEffectParent;
	private GameObject _specialReelBackEffectParent;

	public GameObject SmallGameSpecialEffect {
		get { return _smallGameSpecialEffectParent; }
	}

	private MachineConfig _machineConfig;

	private GameObject _currentEffect = null;
	private Action _forceQuitEffectCallback = null;

	private Coroutine _multiLineSymbolWinCoroutine;

	private bool _isShowingSpecialWinEffect;
	public bool IsShowingSpecialWinEffect { get { return _isShowingSpecialWinEffect; } }

	// 蝴蝶飞舞脚本
	private ButterflyHitController _butterflyHitController = null;

	// 蝴蝶闪烁脚本
	private ButterflyHitEffectController _butterflyHitEffectController = null;

	// 特殊背景特效
	private SpecialReelBackController _specialReelBackController = null;

	public GameObject SmallGameEffectParent
	{
		get { return _smallGameEffectParent; }
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	#region Init

	public void Init()
	{
		_machineConfig = _machine.CoreMachine.MachineConfig;

		InitIdleReelEffect();
		InitSpinReelEffect();
		InitSpinReelEndEffect();
		InitRespinReelEffect();
		InitNormalWinReelSideEffect();
		InitBigWinReelSurroundingsEffect();
		InitSmallGameEffectParent();
		InitSpecialMachineEffect ();
		InitSpecialReelBackEffect ();
		InitRespinPreEffect();

		StartIdleEffect();

		//		string path;
		//		GameObject prefab;
		//		GameObject obj;
		//
		//		_hypeMachineEffect.SetActive(false);
		//		path = _compositeEffectPath + _machineConfig.BasicConfig.HypeMachineEffect;
		//		prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		//		obj = Instantiate(prefab, _hypeMachineEffect.transform);
		//		obj.transform.localPosition = Vector3.zero;
		//		obj.transform.localScale = Vector3.one;
	}

	private void InitSpecialReelBackEffect(){
		if (!_machineConfig.BasicConfig.SpecialReelBackEffect.IsNullOrEmpty ()) {
			Transform parent = _machine._reelFrame.transform.FindDeepChild ("MultiLineBackParent");
			if (parent != null) {
				_specialReelBackEffectParent = parent.gameObject;
				string path = _effectPrefabPath + _machineConfig.BasicConfig.SpecialReelBackEffect;
				GameObject obj = UGUIUtility.CreateMachineAsset(path, _machine.MachineName, _specialReelBackEffectParent);
				_specialReelBackController = obj.GetComponent<SpecialReelBackController> ();
			}
		}
	}

	private void InitRespinPreEffect()
	{
		if(!_machineConfig.BasicConfig.RespinPreEffect.IsNullOrEmpty())
		{
			GameObject obj = AssetManager.Instance.LoadMachineAsset<GameObject>(_machineConfig.BasicConfig.RespinPreEffect, _machine.MachineName);
			obj = GameObject.Instantiate(obj);
			obj.transform.SetParent(_respinPreEffect.transform, false);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
		}

		_respinPreEffect.SetActive(false);
	}

	private void InitIdleReelEffect()
	{
		_idleReelSideEffect = _machine._reelFrame.transform.FindDeepChild("IdleReelSideEffect").gameObject;
		_idleReelSideEffect.SetActive(false);

		if(_machineConfig.BasicConfig.HasIdleReelSideEffect)
		{
			foreach(Transform t in _idleReelSideEffect.transform)
			{
				string path = GetEffectPath() + PuzzleEffectConfig.Get_FX_Machine_Idle_ReelSideLight(_machineConfig.Name);
				GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
				Debug.Assert(prefab != null);
				GameObject obj = Instantiate(prefab, t);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
			}
		}
	}

	private void InitSpinReelEffect()
	{
		_spinReelSideEffect = _machine._reelFrame.transform.FindDeepChild("SpinReelSideEffect").gameObject;
		_spinReelSideEffect.SetActive(false);

		if(_machineConfig.BasicConfig.HasSpinReelSideEffect)
		{
			foreach(Transform t in _spinReelSideEffect.transform)
			{
				string path = GetEffectPath() + PuzzleEffectConfig.Get_FX_Machine_Spin_ReelSideLight(_machineConfig.Name);
				GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
				Debug.Assert(prefab != null);
				GameObject obj = Instantiate(prefab, t);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
			}
		}
	}

	private void InitSpinReelEndEffect()
	{
		_spinReelSideEndEffect = _machine._reelFrame.transform.FindDeepChild("SpinReelSideEndEffect").gameObject;
		_spinReelSideEndEffect.SetActive(false);

		if(_machineConfig.BasicConfig.HasSpinReelSideEffect)
		{
			foreach(Transform t in _spinReelSideEndEffect.transform)
			{
				string path = GetEffectPath() + PuzzleEffectConfig.Get_FX_Machine_Spin_ReelSideLightEnd(_machineConfig.Name);
				GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
				Debug.Assert(prefab != null);
				GameObject obj = Instantiate(prefab, t);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
			}
		}
	}

	private void InitRespinReelEffect()
	{
		_respinReelSideEffect = _machine._reelFrame.transform.FindDeepChild("RespinReelSideEffect").gameObject;
		_respinReelSideEffect.SetActive(false);

		if(_machineConfig.BasicConfig.HasRespinReelSideEffect)
		{
			foreach(Transform t in _respinReelSideEffect.transform)
			{
				string path = GetEffectPath() + PuzzleEffectConfig.Get_FX_Machine_Respin_ReelSideLight(_machineConfig.Name);
				GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
				Debug.Assert(prefab != null);
				GameObject obj = Instantiate(prefab, t);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
			}
		}
	}

	private void InitNormalWinReelSideEffect()
	{
		_normalWinReelSideEffect = _machine._reelFrame.transform.FindDeepChild("NormalWinReelSideEffect").gameObject;
		_normalWinReelSideEffect.SetActive(false);

		if(_machineConfig.BasicConfig.HasNormalWinReelSideEffect)
		{
			foreach(Transform t in _normalWinReelSideEffect.transform)
			{
				string path = GetEffectPath() + PuzzleEffectConfig.Get_FX_Machine_NormalWin_ReelSideLight(_machineConfig.Name);
				GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
				Debug.Assert(prefab != null);
				GameObject obj = Instantiate(prefab, t);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = Vector3.one;
			}
		}
	}

	private void InitBigWinReelSurroundingsEffect()
	{
		_bigWinReelSurroundingsEffect = _machine._reelFrame.transform.FindDeepChild("BigWinReelSurroundingsEffect").gameObject;
		_bigWinReelSurroundingsEffect.SetActive(false);

		if(_machineConfig.BasicConfig.HasBigWinReelSurroundingsEffect)
		{
			string path = GetEffectPath() + PuzzleEffectConfig.Get_FX_Machine_BigWin_ReelSurroundings(_machineConfig.Name);
			GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
			Debug.Assert(prefab != null);
			GameObject obj = Instantiate(prefab, _bigWinReelSurroundingsEffect.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
		}
	}

	private string GetEffectPath()
	{
		return _effectPrefabPath + _machineConfig.Name + "/";
	}

	private string GetFreespinEffectPath(string effect){
		string path = "";
		if(!string.IsNullOrEmpty(effect))
		{
			path = _effectPrefabPath + effect;
		}
		else
		{
			path = _effectPrefabPath + "FX_FreeSpinWord";
		}
		return path;
	}

	private string GetRewindEffectPath(string effect){
		string path = "";
		if(!string.IsNullOrEmpty(effect))
		{
			path = _effectPrefabPath + effect;
		}
		else
		{
			path = _effectPrefabPath + "FX_RewindWord";
		}
		return path;
	}

	private string GetSwitchSymbolEffectPath(string effect){
		string path = "";
		if (!string.IsNullOrEmpty(effect)){
			path = _effectPrefabPath + effect;
		}
		return path;
	}

	private void CreateSmallGameEffect(string path){
		GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		Debug.Assert (prefab != null);
		if (prefab != null) {
			GameObject obj = Instantiate(prefab, _smallGameEffectParent.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
		}
	}

	private void InitSmallGameEffectParent()
	{
		_smallGameEffectParent = _machine._reelFrame.transform.FindDeepChild("SmallGameEffectParent").gameObject;
		BasicConfig config = _machine.CoreMachine.MachineConfig.BasicConfig;

		if(config.HasFreeSpin)
		{
			string path = GetFreespinEffectPath(config.SmallGameEffect);
			CreateSmallGameEffect (path);
		}
		else if(config.HasRewind)
		{
			string path = GetRewindEffectPath (config.SmallGameEffect);
			CreateSmallGameEffect (path);
		}
		else if (config.HasSwitchSymbol){

			string path = GetSwitchSymbolEffectPath (config.SmallGameEffect);
			CreateSmallGameEffect (path);
		}

		_smallGameEffectParent.SetActive(false);
	}

	private void InitButterflyBehaviour(ButterflyHitController controller, ButterflyHitEffectController effectController){
		GameObject startObj = _smallGameSpecialEffectParent.transform.Find ("ButterflyStartPos").gameObject;
		if (_machineConfig.BasicConfig.ButterFlyStartPos.Length > 0){
			float[] offsets = _machineConfig.BasicConfig.ButterFlyStartPos;
			startObj.transform.localPosition = new Vector3(offsets[0], offsets[1], offsets[2]);
		}
		// 取到freerspin的位置，取到winframe的位置
		if (controller != null){
			GameObject destObj = _machine._UIRoot.transform.FindDeepChild ("WinText").gameObject;
			Debug.Assert (destObj != null, "destObj has not found");
			if (destObj != null ) {
				// 因为puzzle reel frame在之后会有个缩放，这里需要延迟进行获取位置。
				UnityTimer.Start (this, 1.0f, () => {
					controller.SetFlyPathTransform (_machine , startObj.transform, destObj.transform);
				});
			}
		}

		if (effectController != null) {
			effectController.transform.position = startObj.transform.position;
			effectController.gameObject.SetActive (false);
		}
	}

	private void InitSpecialMachineEffect(){
		BasicConfig config = _machine.CoreMachine.MachineConfig.BasicConfig;
		Transform trans = _machine._reelFrame.transform.FindDeepChild("SmallGameSpecialEffectParent");
		if (trans == null)
			return;

		_smallGameSpecialEffectParent = trans.gameObject;

		string path = "";

		if (!config.ButterflyHitEffect.IsNullOrEmpty () && !config.ButterflyHitTwinkleEffect.IsNullOrEmpty ()) {
			path = _effectPrefabPath + config.ButterflyHitEffect;
			GameObject obj = UGUIUtility.CreateMachineAsset (path, _machine.MachineName, _smallGameSpecialEffectParent);
			_butterflyHitController = obj.GetComponent<ButterflyHitController> ();

			path = _effectPrefabPath + config.ButterflyHitTwinkleEffect;
			obj = UGUIUtility.CreateMachineAsset (path, _machine.MachineName, _smallGameSpecialEffectParent, false);
			_butterflyHitEffectController = obj.GetComponent<ButterflyHitEffectController> ();

			InitButterflyBehaviour (_butterflyHitController, _butterflyHitEffectController);
		}

		if (!config.SmallGameSpecialEffect.IsNullOrEmpty ()) {
			path = _effectPrefabPath + config.SmallGameSpecialEffect;
			UGUIUtility.CreateMachineAsset (path, _machine.MachineName, _smallGameSpecialEffectParent);
			_smallGameSpecialEffectParent.SetActive (false);
		}
	}

	#endregion

	#region Public

	public void StartIdleEffect()
	{
		_idleReelSideEffect.SetActive(true);
	}

	public void EndIdleEffect()
	{
		_idleReelSideEffect.SetActive(false);
	}

	public void StartSpinEffect()
	{
		if(_machine.CoreMachine.SmallGameState == SmallGameState.FreeSpin
			|| _machine.CoreMachine.SmallGameState == SmallGameState.Rewind)
			_respinReelSideEffect.SetActive(true);
		else
			_spinReelSideEffect.SetActive(true);
	}

	public void EndSpinEffect()
	{
		_spinReelSideEffect.SetActive(false);
		_spinReelSideEndEffect.SetActive(true);
		UnityTimer.Start(this, 0.5f, () =>
		{
			_spinReelSideEndEffect.SetActive(false);

			if(_machine.GetState() == MachineState.Idle)
				StartIdleEffect();
		});

		_respinReelSideEffect.SetActive(false);
	}

	public void StartWinEffect(WinType winType, NormalWinType normalWinType, bool shouldRespin)
	{
		ShowSymbolWinEffect(winType, shouldRespin);
		ShowSpecialSymbolLightEffect();

		if(shouldRespin)
		{
			ShowNormalWinEffect(normalWinType);
			PlayRespinWinSound();
		}
		else
		{
			if(winType == WinType.Normal && _machine.CoreMachine.SpinResult.TotalWinRatio > 0)
			{
				ShowNormalWinEffect(normalWinType);
				PlayNormalWinSound();

				// ShowTounamentMessage
				CitrusEventManager.instance.Raise(new CanShowTournamentRewardUIEvent());
			}
			else if(PuzzleUtility.IsSpecialWin(winType))
			{
				ShowSpecialWinEffect(winType);
				PlaySpecialWinSound(winType);
			}
			else
			{
				// ShowTounamentMessage
				CitrusEventManager.instance.Raise(new CanShowTournamentRewardUIEvent());
			}
		}
	}

	private void PlayRespinWinSound()
	{
		float ratio = _machine.CoreMachine.SpinResult.WinRatio;
		if(ratio > 0.0f)
		{
			AudioManager.Instance.PlaySound(AudioType.NormalRespinWin);
		}
	}

	private void PlaySpecialWinSound(WinType type)
	{
		switch(type)
		{
			case WinType.Big:
				AudioManager.Instance.PlaySound(AudioType.BigWin);
				break;
			case WinType.Epic:
			case WinType.Jackpot:
				AudioManager.Instance.PlaySound(AudioType.EpicWin);
				break;
			default:
				break;
		}
	}

	private void PlayNormalWinSound()
	{
		NormalWinType t = _machine.CoreMachine.SpinResult.NormalWinType;
		if(t == NormalWinType.Low)
			AudioManager.Instance.PlaySound(AudioType.NormalLowWin);
		else if(t == NormalWinType.High)
			AudioManager.Instance.PlaySound(AudioType.NormalHighWin);
	}

	public void EndWinEffect()
	{
		HideSymbolWinEffect();

		HideNormalWinEffect();
		HideSpecialWinEffect();

		HideSpecialSymbolLightEffect();
	}

	public void StartHypeEffect()
	{
		//		_hypeMachineEffect.SetActive(true);
	}

	public void EndHypeEffect()
	{
		//		_hypeMachineEffect.SetActive(false);
	}

	public void StartSmallGameEffect()
	{
		_smallGameEffectParent.SetActive(true);
	}

	public void EndSmallGameEffect()
	{
		_smallGameEffectParent.SetActive(false);
	}

	private bool ShouldShowSymbolWinEffect(CoreSpinResult spinResult)
	{
		bool result = spinResult.IsWinWithNonZeroRatio() 
			|| spinResult.IsUnOrderedPayout() 
			|| (_machine.CoreMachine.IsTriggerSmallGameState() && _machine.MachineConfig.BasicConfig.TriggerType != TriggerType.Collect);
		return result;
	}

	private void ShowSymbolWinEffect(WinType winType, bool shouldRespin)
	{
		CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
		if(ShouldShowSymbolWinEffect(spinResult))
		{
			if(_machine.MachineConfig.BasicConfig.IsMultiLine)
			{
				ShowMultiLineSymbolWinEffect(winType, shouldRespin, 0);

				//set post spin time, for auto spin
				//only wait for one period to make user not wait too long
//				float leastSymbolBlinkTime = spinResult.MultiLineCheckResult.PayoutInfos.Count * _symbolBlinkPeriod;
				float leastSymbolBlinkTime = _symbolBlinkPeriod;
				_machine.SpinData.SetSymbolBlinkTime(leastSymbolBlinkTime);
			}
			else
			{
				ShowSingleLineSymbolWinEffect(winType, shouldRespin);
			}
		}
	}

	private void ShowSingleLineSymbolWinEffect(WinType winType, bool shouldRespin)
	{
		CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel reel = _machine.ReelList[i];
			bool[] flags = spinResult.WinSymbolFlagsList[i];
			for(int k = 0; k < flags.Length; k++)
			{
				bool flag = flags[k];
				int offset = k - CoreSpinResult.PaylineMidOffset;
				PuzzleSymbol symbol = reel.GetOffsetMidAreaSymbol(offset);
//				LogUtility.Log ("show win eff symbol " + symbol.CoreSymbol.SymbolData.Name + " offset " + offset + " flag " + flag, Color.red);
				if(flag)
					symbol.ShowWinEffect(winType, shouldRespin);
				else
					symbol.StartWinNotMatchEffect();
			}

			//don't forget to stop some coroutines
			reel.StopHideReelSymbolEffectCoroutines();
		}
	}

	private void ShowMultiLineSymbolWinEffect(WinType winType, bool shouldRespin, int playIndex)
	{
		_multiLineSymbolWinCoroutine = null;

		CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
		int payoutCount = spinResult.MultiLineCheckResult.PayoutInfos.Count;

		HideMultiLineAllSymbolsEffect();

		//0: play all paylines
		//1 - payoutCount: play single payline
		if(playIndex == 0)
		{
			ShowMultiLineAllPaylinesEffect(winType, shouldRespin);
		}
		else
		{
			int infoIndex = playIndex - 1;
			Debug.Assert(infoIndex < payoutCount);
			MultiLineMatchInfo info = spinResult.MultiLineCheckResult.PayoutInfos[infoIndex];

			_machine._multiLineBack.HideAllPayline();
			ShowMultiLineSinglePaylineEffect(winType, shouldRespin, info);
		}

		ShowMultiLineAllTriggerPayoutSymbolsEffect(winType, shouldRespin);

		if(!shouldRespin)
		{
			playIndex = FindMultiLineNextShowableIndex(playIndex, spinResult);
			_multiLineSymbolWinCoroutine = UnityTimer.Start(this, _symbolBlinkPeriod, () => {
				ShowMultiLineSymbolWinEffect(winType, shouldRespin, playIndex);
			});
		}
	}

	private int FindMultiLineNextShowableIndex(int playIndex, CoreSpinResult spinResult)
	{
		int result = 0;
		int payoutCount = spinResult.MultiLineCheckResult.PayoutInfos.Count;

		while(true)
		{
			playIndex = (playIndex + 1) % (payoutCount + 1);
			if(playIndex == 0)
			{
				result = playIndex;
				break;
			}
			else
			{
				int infoIndex = playIndex - 1;
				MultiLineMatchInfo info = spinResult.MultiLineCheckResult.PayoutInfos[infoIndex];
				if(info.ShouldShowWinEffect())
				{
					result = playIndex;
					break;
				}
			}
		}

		return result;
	}

	private void ShowMultiLineAllPaylinesEffect(WinType winType, bool shouldRespin)
	{
		CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
		ListUtility.ForEach(spinResult.MultiLineCheckResult.PayoutInfos, 
			(MultiLineMatchInfo info) => {
				if(info.ShouldShowWinEffect())
					ShowMultiLineSinglePaylineEffect(winType, shouldRespin, info);
			});
	}

	private void ShowMultiLineAllTriggerPayoutSymbolsEffect(WinType winType, bool shouldRespin)
	{
		List<CoreSymbol> symbols = _machine.CoreMachine.PlayModule.TriggerSymbols;
		ListUtility.ForEach(symbols, (CoreSymbol s) => {
			PuzzleReel reel = _machine.ReelList[s.ReelIndex];
			PuzzleSymbol puzzleSymbol = reel.GetPuzzleSymbol(s.StopIndex);
			puzzleSymbol.ShowWinEffect(winType, shouldRespin);
		});
	}

	private void ShowMultiLineSinglePaylineEffect(WinType winType, bool shouldRespin, MultiLineMatchInfo matchInfo)
	{
		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			if(matchInfo.MatchFlags[i])
			{
				PuzzleReel reel = _machine.ReelList[i];
				int offset = matchInfo.Payline[i];
				PuzzleSymbol symbol = reel.GetOffsetMidAreaSymbol(offset);

				symbol.EndWinNotMatchEffect();
				symbol.ShowWinEffect(winType, shouldRespin);
			}
		}

		_machine._multiLineBack.ShowPayline(matchInfo);
	}

	private void HideMultiLineAllSymbolsEffect()
	{
		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel reel = _machine.ReelList[i];
			int maxOffset = reel.GetMaxVisibleSymbolOffset();
			for(int k = -maxOffset; k <= maxOffset; k++)
			{
				PuzzleSymbol symbol = reel.GetOffsetMidAreaSymbol(k);
				symbol.HideWinEffect();
				symbol.StartWinNotMatchEffect();
			}
		}
	}

	public void HideSymbolWinEffect()
	{
		CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
		bool respin = _machine.CoreMachine.ShouldRespin ();

		if (spinResult != null)
		{
			if (_machine.MachineConfig.BasicConfig.IsMultiLine && _multiLineSymbolWinCoroutine != null) {
				this.StopCoroutine (_multiLineSymbolWinCoroutine);
				_multiLineSymbolWinCoroutine = null;
			}

			for (int i = 0; i < _machine.ReelList.Count; i++) {
				PuzzleReel reel = _machine.ReelList [i];
				int maxOffset = reel.GetMaxVisibleSymbolOffset();
				for (int k = -maxOffset; k <= maxOffset; k++) {
					PuzzleSymbol symbol = reel.GetOffsetMidAreaSymbol (k);
					symbol.HideWinEffect ();
					symbol.EndWinNotMatchEffect ();
				}
			}

			if (_machine.MachineConfig.BasicConfig.IsMultiLine) {
				_machine._multiLineBack.HideAllPayline ();
			}
		} else {
			// 非获胜时关闭强元素动画
			if (! respin) {
				HideStrongSpecialSymbolEffect ();
			}
		}
	}

	public void HideStrongSpecialSymbolEffect(){
		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel reel = _machine.ReelList[i];
			for(int k = -1; k <= 1; k++)
			{
				PuzzleSymbol symbol = reel.GetOffsetMidAreaSymbol(k);
				if (symbol.CoreSymbol.SymbolData.HasStrongSpecialEffect) {
					symbol.HideStrongSpecialEffect ();
				}
			}
		}
	}

	#endregion

	#region Other

	private void ShowNormalWinEffect(NormalWinType normalWinType)
	{
		if (_normalWinReelSideEffect != null)
			_normalWinReelSideEffect.SetActive(true);

		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel r = _machine.ReelList[i];
			if(normalWinType == NormalWinType.Low)
				r.ShowNormalWinFrameLightEffect();
			else if(normalWinType == NormalWinType.High)
				r.ShowBigWinFrameLightEffect();
		}

		//		if (_machine.CoreMachine.SpinResult.WinRatio >= GameConfig.Instance.MiscConfig.NormalWinHighThreshold)
		//			AudioManager.Instance.PlaySound(AudioType.NormalWinHigh);
		//		else
		//			AudioManager.Instance.PlaySound(AudioType.NormalWinLow);
	}

	private void HideNormalWinEffect()
	{
		if (_normalWinReelSideEffect != null)
			_normalWinReelSideEffect.SetActive(false);

		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel r = _machine.ReelList[i];
			r.HideNormalWinFrameLightEffect();
			r.HideBigWinFrameLightEffect();
		}
	}

	private void ShowSpecialSymbolLightEffect()
	{
		PayoutData data = _machine.CoreMachine.SpinResult.PayoutData;
		if(data == null)
			return;

		if(data.PayoutType == PayoutType.UnOrdered)
		{
			string symbolName = data.Symbols[0];
			CoreSpinResult result = _machine.CoreMachine.SpinResult;

			for(int i = 0; i < _machine.ReelList.Count; ++i)
			{
				PuzzleReel r = _machine.ReelList[i];
				SingleReel singleReel = _machine.MachineConfig.ReelConfig.GetSingleReel(i);
				List<int> visibleIndexList = singleReel.GetVisibleStopIndexes(result.StopIndexes[i]);
				bool isSpecial = ListUtility.IsAnyElementSatisfied(visibleIndexList, (int stopindex) =>
				{
					return r.GetPuzzleSymbol(stopindex).CoreSymbol.SymbolData.Name.Equals(symbolName);
				});
				if(isSpecial)
				{
					r.ShowSpecialSymbolLightEffect(true);
				}
			}
		}
	}

	private void HideSpecialSymbolLightEffect()
	{
		for(int i = 0; i < _machine.ReelList.Count; ++i)
		{
			PuzzleReel r = _machine.ReelList[i];
			r.ShowSpecialSymbolLightEffect(false);
		}
	}

	private void ShowSpecialWinEffect(WinType type)
	{
		// zhousen
		//LogUtility.Log("ShowSpecialWinEffect : "+type, Color.yellow);

		_isShowingSpecialWinEffect = true;

		_bigWinReelSurroundingsEffect.SetActive(true);

	    bool isDoubleWin = _machine._specialMode == SpecialMode.DoubleWin;

        if (type == WinType.Big)
		{
			_bigWinMachineEffect.SetActive(true);
			SetSpecialWinNumberText(_bigWinNumberText, isDoubleWin, _bigWinDoubleRewardAnimator);
		}
		else if(type == WinType.Epic)
		{
			_epicWinMachineEffect.SetActive(true);
			SetSpecialWinNumberText(_epicWinNumberText, isDoubleWin, _epicWinDoubleRewardAnimator);
		}
		else if(type == WinType.Jackpot)
		{
			_jackpotWinMachineEffect.SetActive(true);
			SetSpecialWinNumberText(_jackpotWinNumberText, isDoubleWin, _jackpotWinDoubleRewardAnimator);
		}
		else
			Debug.Assert(false);

		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel r = _machine.ReelList[i];
			r.ShowBigWinFrameLightEffect();
		}
	}

	private void HideSpecialWinEffect()
	{
		_isShowingSpecialWinEffect = false;

		_bigWinReelSurroundingsEffect.SetActive(false);

		_bigWinMachineEffect.SetActive(false);
		_epicWinMachineEffect.SetActive(false);
		_jackpotWinMachineEffect.SetActive(false);

		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel r = _machine.ReelList[i];
			r.HideBigWinFrameLightEffect();
		}

		AudioManager.Instance.StopSound(AudioType.BigWin);
		AudioManager.Instance.StopSound(AudioType.MegaWin);
		AudioManager.Instance.StopSound(AudioType.EpicWin);
		//		AudioManager.Instance.StopSound(AudioType.CreditsTick);

		if(_currentEffect != null)
			_currentEffect.GetComponentInChildren<ShareButton>().ShareFinishedEvent.RemoveAllListeners();
		_currentEffect = null;
	}

	private void SetSpecialWinNumberText(Text text, bool doubleWin, Animator animator)
	{
        animator.gameObject.SetActive(doubleWin);

		ulong winAmount = _machine.GameData.WinAmount;
		int winRatio = (int)(winAmount / _machine.GameData.BetAmount);
		float tickTime = _machine.PuzzleConfig.GetNumberTickTime(_machine);

		NumberTickHandler handler = text.gameObject.GetComponent<NumberTickHandler>();
		handler._completeHandler += TickTweenerOverCallback;//  we can quit effect when ticktweener is over
		handler.StartTick(0, winAmount, tickTime);

		//don't forget to set tickTime
	    if (doubleWin)
	    {
	        UnityTimer.Start(this, tickTime, () =>animator.SetTrigger("play"));

            //1.4 is doublewin anim's length
            tickTime += 1.4f;
	        UnityTimer.Start(this, tickTime, () => text.text = StringUtility.FormatNumberString(winAmount*2, true, false));
	    }
		_machine.SpinData.SetNumberTickTime(tickTime);
	}

    void PlayDoubleWinAnim(Animator animator)
    {
        animator.SetTrigger("play");
    }

    private void RemoveCurrentWinEffect()
	{
		if(_currentEffect != null)
		{
			if(_currentEffect == _bigWinMachineEffect)
			{
				AudioManager.Instance.StopSound (AudioType.BigWin);
			}
			else if(_currentEffect == _epicWinMachineEffect)
			{
				AudioManager.Instance.StopSound(AudioType.EpicWin);
			}
			else if(_currentEffect == _jackpotWinMachineEffect)
			{
				AudioManager.Instance.StopSound(AudioType.EpicWin);
			}
			_currentEffect.GetComponentInChildren<ShareButton>().ShareFinishedEvent.RemoveListener(OnClickQuitEffect);
			_currentEffect.SetActive(false);
			_currentEffect = null;
		}
	}

	private void OnClickQuitEffect()
	{
		bool isWindowOpen = false;// 是否有弹窗

		if(_currentEffect != null)
		{
			_machine.SetSpinMode(SpinMode.Normal);

			if (_currentEffect == _bigWinMachineEffect || _currentEffect == _epicWinMachineEffect)
            {
                CitrusEventManager.instance.Raise(new ManualCloseBigWinEvent());
				isWindowOpen = TLStoreController.Instance.ShowTLStoreAfterBigWin;
                if (isWindowOpen)
                    LogUtility.Log("DiceUi and ScoreApp ui won't show because need show timelimit store ui this time", Color.yellow);
            }

			if (!isWindowOpen){
				ulong winAmount = _machine.GameData.WinAmount;
				float winRatio = _machine.GameData.GetWinBetRatio();
				CitrusEventManager.instance.Raise(new SpinEndEvent(winAmount, winRatio));
				isWindowOpen = DiceManager.Instance.CanPlay(winAmount, winRatio);
                if (isWindowOpen)
                    LogUtility.Log("ScoreApp ui won't show because need show dice ui this time", Color.yellow);
            }

			if (_currentEffect == _epicWinMachineEffect && !isWindowOpen){
				ScoreAppSystem.Instance.EpicWinEvent();
				isWindowOpen = true;
			}
			// 这个要挪到最后，因为上面要判断一次是否是epicwin
			RemoveCurrentWinEffect();
		}

		_isShowingSpecialWinEffect = false;
		// ShowTournamentMessage
		CitrusEventManager.instance.Raise(new CanShowTournamentRewardUIEvent());
	}

	private void TickTweenerOverCallback(NumberTickHandler num)
	{
		// 出现特效界面，不能触发下一轮的spin
		_machine.SpinData.IsUserTriggeredNextSpin = false;
		if(num == _bigWinNumberText.gameObject.GetComponent<NumberTickHandler>())
		{
			_currentEffect = _bigWinMachineEffect;
			var sb = _bigWinMachineEffect.GetComponentInChildren<ShareButton>();
			sb.ChangeShareText(SharePlace.BigWin, StringUtility.FormatNumberStringWithComma(_machine.GameData.WinAmount));
			sb.ShareFinishedEvent.AddListener(OnClickQuitEffect);
		}
		else if(num == _epicWinNumberText.gameObject.GetComponent<NumberTickHandler>())
		{
			_currentEffect = _epicWinMachineEffect;
			var sb = _epicWinMachineEffect.GetComponentInChildren<ShareButton>();
			sb.ChangeShareText(SharePlace.EpicWin, StringUtility.FormatNumberStringWithComma(_machine.GameData.WinAmount));
			sb.ShareFinishedEvent.AddListener(OnClickQuitEffect);
		}
		else if(num == _jackpotWinNumberText.gameObject.GetComponent<NumberTickHandler>())
		{
			_currentEffect = _jackpotWinMachineEffect;
			var sb = _jackpotWinMachineEffect.GetComponentInChildren<ShareButton>();
			sb.ChangeShareText(SharePlace.JACKPOT, StringUtility.FormatNumberStringWithComma(_machine.GameData.WinAmount));
			sb.ShareFinishedEvent.AddListener(OnClickQuitEffect);
		}
	}

	#endregion

	#region smallgameeffect
	public void StartButterflyHit(Callback<bool> endCallback = null){
		if (_butterflyHitController != null) {
			_butterflyHitController.StartFly (endCallback);
		}
	}

	public void StartButterflyHitTwinkle(){
		if (_butterflyHitEffectController != null) {
			_butterflyHitEffectController.Show ();
		}
	}

	public void ShowSmallGameSpecialEffect(bool show){
		if (_smallGameSpecialEffectParent != null) {
			_smallGameSpecialEffectParent.SetActive (show);
		}
	}

	public void StartSpecialReelBackEffectAnimation(string clip){
		if (_specialReelBackController != null) {
			_specialReelBackController.Play (clip);
		}
	}

	public void StartSpecialReelBackEffectAnimationTrigger(string trigger){
		if (_specialReelBackController != null) {
			_specialReelBackController.PlayTrigger (trigger);
		}
	}

	public void StopSpecialReelBackEffect(){
		// TODO: 目前不用
	}

	public void StartRespinPreEffect()
	{
		_respinPreEffect.SetActive(true);
		UnityTimer.Start(this, 2.0f, () => {
			_respinPreEffect.SetActive(false);
		});
	}

	#endregion
}
