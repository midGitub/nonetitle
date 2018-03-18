//#define DEBUG_MACHINE

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using CitrusFramework;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//MachineState notes:
//Idle: Machine is idle. The player can start spin only in this state
//Spinning: Machine is spinning
//SmallGameFront: From start SmallGameFront to end SmallGameFront. The tricky point is: 
//			 For FreeSpin and Rewind, the state only lasts between two function calls in one frame
//			 For Slide, the state starts from PerformSlide() until the slide action ends
//PostSpin: From EndSpin() to EndPostSpin(). In this state, number is ticking
//SmallGameBehind: Similar to SmallGameFront, but it's processed after PostSpin. Some special small games occurs here
//PreSpin: From scheduling Spin() callback to Spin() is called. In this state, the player can't interrupt the logic
public enum MachineState
{
	Idle,
	Spinning,
	SmallGameFront,
	PostSpin,
	SmallGameBehind,
	PreSpin
}

public enum SpinMode
{
	Normal,
	Auto
}

public enum SpecialMode
{
    Normal,
    DoubleWin,
    FreeMaxBet,
}

public class PuzzleMachine : MonoBehaviour
{
	private static readonly string _effectPrefabPath = "Effect/Prefab/";
	private static readonly string _imageDir = "Images/Machines/";
	// 上一次spin结果为胜利时的等待时间
	private static readonly float _respinPreWaitTime = 1.0f;
	// 上一次spin结果非胜利时的等待时间
	private static readonly float _respinPreWaitTimeFail = 0.1f;
	// 判断delay是否在合理区间
	private static readonly float _butterflyHitDelayValidThreshold = 99.0f;

	public delegate void MachineEventDelegate(PuzzleMachine machine);

	public GameScene _scene;
	public GameObject _UIRoot;
	public MachineBar _machineBar;
	public PuzzleReelFrame _reelFrame;
	public PuzzleEffect _effect;
	public GameObject _backgroundParent;
	public GameObject _backgroundEffectParent;
	private GameObject _backgroundEffect;
	public PuzzleConfig _puzzleConfig;
	public GameObject _leaderboardFrame;
	public PuzzleMultiLineFrame _multiLineFrame;
	public PuzzleMultiLineBack _multiLineBack;
    public MachineRewardAdManager _machineRewardAdMng;

	private PuzzleDebugPanel _debugPanel;

	public event MachineEventDelegate StartSpinEventHandler = delegate { };
	public event MachineEventDelegate EndSpinEventHandler = delegate { };
    public event MachineEventDelegate TryEndRoundHandler = delegate { };
    public event MachineEventDelegate EndRoundEventHandler = delegate { };

    public event MachineEventDelegate EnterSpecialModeHandler = delegate { };
    public event MachineEventDelegate DuringSpecialModeHandler = delegate { };
    public event MachineEventDelegate EndSpecialModeHandler = delegate { };

    private string _name;
	private CoreMachine _coreMachine;
	private MachineConfig _machineConfig;
	private BasicConfig _basicConfig;
	private List<PuzzleReel> _reelList = new List<PuzzleReel>();
	private List<PuzzleSmallGameHandler> _smallGameHandlers = new List<PuzzleSmallGameHandler>();
	private PuzzleSpinManager _spinManager;
	private PuzzleResourceManager _resourceManager;
	private PuzzleReelSpinConfig _puzzleReelSpinConfig;

	public MachineState _state;
	public SpinMode _spinMode;
    public SpecialMode _specialMode;
	public PuzzleSpinData _spinData; //valid for every spin and cleared when every spin starts

	private PuzzleSmallGameStage _curSmallGameStage;
	private GameData _gameData;
	private bool _shouldRespin;
	private CoreSpinInput _spinInput;

	public MachineConfig MachineConfig { get { return _machineConfig; } }
	public GameData GameData { get { return _gameData; } }
	public CoreMachine CoreMachine { get { return _coreMachine; } }
	public List<PuzzleReel> ReelList { get { return _reelList; } }
	public PuzzleSpinData SpinData { get { return _spinData; } }
	public bool ShouldRespin { get { return _shouldRespin; } }
	public PuzzleConfig PuzzleConfig { get { return _puzzleConfig; } }
	public PuzzleReelSpinConfig PuzzleReelSpinConfig { get { return _puzzleReelSpinConfig; } }
	public string MachineName { get { return _name; } }
	public PuzzleResourceManager ResourceManager { get { return _resourceManager; } }
	public bool IsCollecting { get { return _collecting; } }

	private Coroutine _startRoundCoroutine;
	// 是否在收集过程中
	private bool _collecting;
	// 是否在冻结
	private bool _isFrozen;
	// 非支付线上的收集物列表
	private List<CoreCollectData> _nonPaylineCollectList;
	// 龙卷轴特效
	private Coroutine _dragonReelEffectCoroutine;
	// Wheel
	private Dictionary<string, GameObject> _wheelPrefabDict = new Dictionary<string, GameObject>();
	// TapBox
	private GameObject _tapBoxObject;
	// TapChip
	private GameObject _tapChipObject;

	// 全屏特效
	private BackgroundEffectBehaviour _backgroundEffectBehaviour = null;
	private GameObject _backgroundIdleEffect;
	private GameObject _backgroundSpinEffect;
	private GameObject _backgroundFreespinEffect;

	private MachineTheme _machineTheme;
		
#if DEBUG_MACHINE
	public int _debugSpinCount = 0;
#endif

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
	}

	#region Init

	public void Init(string name)
	{
		_name = name;

		CoreConfig nothing = CoreConfig.Instance; //call it to force load game data from disk

		uint seed = UserMachineData.Instance.GetMachineSeed(name);
//		uint seed = 1;
		_coreMachine = new CoreMachine(name, seed);
		_machineTheme = CoreDefine.MachineThemeDict [_coreMachine.Name];
		_machineConfig = _coreMachine.MachineConfig;
		_basicConfig = _machineConfig.BasicConfig;
		_curSmallGameStage = PuzzleSmallGameStage.None;
		_state = MachineState.Idle;
		_spinMode = SpinMode.Normal;
        _specialMode = SpecialMode.Normal; 
		_spinManager = new PuzzleSpinManager(this);
		_spinData = new PuzzleSpinData();

		InitGameData();
		InitPuzzleReelSpinConfig();
		InitResourceManager();
		InitReelFrame();
		InitMachineBar();
		InitSmallGameHandlers();
		InitImages();
		InitEffect();
		InitMultiLine();
		InitDebugPanel();
		InitResolutionAdaption();
		InitBGM();
		InitWheelPrefabDict();
		InitTapBox();
		InitTapChip();
        InitTryEndRoundHandler();
        InitMachineRewardAdManager();
		InitFreeSpinControllers();

		UserMachineData.Instance.CurrentMachine = _name;

		AnalysisManager.Instance.EnterMachine(_name);

		InitSpecialMachine ();

		CitrusEventManager.instance.AddListener<EnterMainMapSceneEvent>(EnterMainMapSceneDelegate);

		StartPlayBackgroundEffect ();

		// test
		// Test();
	}

	private void InitResourceManager()
	{
		_resourceManager = new PuzzleResourceManager(this, _machineConfig);
	}

	private void InitReelFrame()
	{
		//1 reelFrame
		if(!_basicConfig.IsThreeReel)
		{
			string pathStr = "Game/ReelFrame" + _basicConfig.ReelCount.ToString();
			GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(pathStr);
			GameObject newReelFrame = Instantiate(prefab, _reelFrame.transform.parent);
			int reelSiblingIndex = _reelFrame.transform.GetSiblingIndex();

			newReelFrame.transform.localPosition = _reelFrame.transform.localPosition;
			newReelFrame.transform.localRotation = _reelFrame.transform.localRotation;
			newReelFrame.transform.localScale = _reelFrame.transform.localScale;
			Destroy(_reelFrame.gameObject);

			_reelFrame = newReelFrame.GetComponent<PuzzleReelFrame>();
			_reelFrame.transform.SetSiblingIndex(reelSiblingIndex);
		}

		_reelFrame.Init(this);

		//2 reelList
		_reelList = _reelFrame._reelList;
		Debug.Assert(_reelList.Count == _basicConfig.ReelCount);

		for (int i = 0; i < _reelList.Count; i++)
		{
			PuzzleReel reel = _reelList[i];
			CoreReel coreReel = _coreMachine.ReelList[i];
			reel.Init(this, coreReel);
		}
	}
     
	private void InitGameData()
	{
		_gameData = new GameData(this);
	}

	private void InitPuzzleReelSpinConfig()
	{
		string name = _basicConfig.IsFiveReel ? "Game/PuzzleReelSpinConfig_Reel5" : "Game/PuzzleReelSpinConfig";
		GameObject obj = AssetManager.Instance.LoadAsset<GameObject>(name);
		_puzzleReelSpinConfig = obj.GetComponent<PuzzleReelSpinConfig>();
	}

	private void InitMachineBar()
	{
		_machineBar.Init();

		if (_basicConfig.IsTriggerType (TriggerType.Collect)) {
			_machineBar.ChangeBetAmountHandler += UpdateCollectNum;
		}

		if (_basicConfig.IsJackpot) {
			_machineBar.ChangeBetAmountHandler += ShowJackpotTip;
		}

		_machineBar.NoChangeBetAmountHandler += ShowBetUnlockTip;
	}

	private void InitSmallGameHandlers()
	{
		// 1 add PuzzleSmallGameMomentType.Front
		if (_basicConfig.HasSlide)
		{
			PuzzleSmallGameHandler slideHandler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.Slide, CheckSlide, PerformSlide);
			_smallGameHandlers.Add(slideHandler);
		}

		if (_basicConfig.HasFreeSpin)
		{
			PuzzleSmallGameHandler freeSpinHandler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.FreeSpin, CheckFreeSpin, PerformFreeSpin);
			_smallGameHandlers.Add(freeSpinHandler);
		}

		if (_basicConfig.HasRewind)
		{
			PuzzleSmallGameHandler rewindHandler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.Rewind, CheckRewind, PerformRewind);
			_smallGameHandlers.Add(rewindHandler);
		}

		if (_basicConfig.HasFixWild)
		{
			PuzzleSmallGameHandler fixwildHandler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.FixWild, CheckFixWild, PerformFixWild);
			_smallGameHandlers.Add(fixwildHandler);
		}

		if (_basicConfig.HasWheel){
			PuzzleSmallGameHandler wheelHandler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.Wheel, CheckWheel, PerformWheel);
			_smallGameHandlers.Add(wheelHandler);
		}

		if (_basicConfig.IsTriggerType (TriggerType.Collect)) {
			PuzzleSmallGameHandler collecthandler = new PuzzleSmallGameHandler (PuzzleSmallGameStage.Collect, CheckCollect, PerformCollect);
			_smallGameHandlers.Add (collecthandler);
		}

		if(_basicConfig.IsPuzzleTapBox)
		{
			PuzzleSmallGameHandler handler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.TapBox, CheckTapBox, PerformTapBox);
			_smallGameHandlers.Add(handler);
		}

		if(_basicConfig.IsPuzzleTapChip)
		{
			PuzzleSmallGameHandler handler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.TapChip, CheckTapChip, PerformTapChip);
			_smallGameHandlers.Add(handler);
		}

		if (_basicConfig.HasSwitchSymbol)
		{
			PuzzleSmallGameHandler handler = new PuzzleSmallGameHandler(PuzzleSmallGameStage.SwitchSymbol, CheckSwitchSymbol, PerformSwitchSymbol);
			_smallGameHandlers.Add(handler);
		}

		// 2 add PuzzleSmallGameMomentType.Behind
	}

	private void InitImages()
	{
		InitBackgroundPrefab ();
		InitBackgroundEffect ();
	}

	private void InitEffect()
	{
		_effect.Init();
	}

	private void InitBackgroundPrefab()
	{
		GameObject backgroundPrefab = null;
		if (_basicConfig.BackgroundPrefab != "") {
			backgroundPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (_basicConfig.BackgroundPrefab, MachineName);
		} else {
			backgroundPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> ("Game/BackgroundM1", "M1");
			Debug.LogError("backgroundprefab is null: " + _machineConfig.Name);
		}
		GameObject obj = Instantiate (backgroundPrefab);
		obj.transform.SetParent (_backgroundParent.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
	}

	private GameObject CreateBackgroundEffect(string assetName)
	{
		GameObject result = null;
		if(!assetName.IsNullOrEmpty())
		{
			string path = _effectPrefabPath + assetName;
			GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, MachineName);
			if(prefab != null)
			{
				result = UGUIUtility.CreateObj(prefab, _backgroundEffectParent);
				result.SetActive(false);
			}
			else
			{
				Debug.LogError("Fail to load BackgroundEffect: " + assetName);
			}
		}
		return result;
	}

	private void InitBackgroundEffect(){
		if (!_basicConfig.BackgroundEffectPrefab.IsNullOrEmpty ()) {
			string path = _effectPrefabPath + _basicConfig.BackgroundEffectPrefab;
			GameObject backgroundEffectPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, MachineName);
			if(backgroundEffectPrefab != null)
			{
				_backgroundEffect = UGUIUtility.CreateObj(backgroundEffectPrefab, _backgroundEffectParent);
				if(_backgroundEffect != null)
				{
					_backgroundEffect.SetActive(false);
				}

				if(_basicConfig.BackgroundEffectStartOn)
				{
					ShowBackgroundEffect(true);
				}
			}
			else
			{
				Debug.LogError("Fail to load BackgroundEffect: " + path);
			}
		}

		_backgroundIdleEffect = CreateBackgroundEffect (_basicConfig.BackgroundEffectIdlePrefab);
		_backgroundSpinEffect = CreateBackgroundEffect (_basicConfig.BackgroundEffectSpinPrefab);
		_backgroundFreespinEffect = CreateBackgroundEffect (_basicConfig.BackgroundEffectFreespinPrefab);

		if (_backgroundIdleEffect != null || _backgroundSpinEffect != null || _backgroundFreespinEffect != null) {
			if (_backgroundEffectParent.GetComponent<BackgroundEffectBehaviour> () == null) {
				_backgroundEffectParent.AddComponent<BackgroundEffectBehaviour> ();
			}
			_backgroundEffectBehaviour = _backgroundEffectParent.GetComponent<BackgroundEffectBehaviour> ();
			_backgroundEffectBehaviour.SetBackgroundEffect (BackgroundType.Idle, _backgroundIdleEffect);
			_backgroundEffectBehaviour.SetBackgroundEffect (BackgroundType.Spin, _backgroundSpinEffect);
			_backgroundEffectBehaviour.SetBackgroundEffect (BackgroundType.Freespin, _backgroundFreespinEffect);
		}
	}

	private void InitMultiLine()
	{
		if(_basicConfig.IsMultiLine)
		{
			int paylineCount = _machineConfig.PaylineConfig.PaylineCount;

			string multiFramePrefab = "";
			if(!_basicConfig.MultiFrameAsset.IsNullOrEmpty())
			{
				multiFramePrefab = _basicConfig.MultiFrameAsset;
			}
			else
			{
				multiFramePrefab = "Game/MultiLineFrame";
				if(paylineCount == CoreDefine.MaxPaylineCountOfMultiLine3Reels)
					multiFramePrefab += paylineCount;
			}

			string multiBackPrefab = "";
			if(!_basicConfig.MultiLineBackAsset.IsNullOrEmpty())
			{
				multiBackPrefab = _basicConfig.MultiLineBackAsset;
			}
			else
			{
				multiBackPrefab = "Game/MultiLineBack";
				if (paylineCount == CoreDefine.MaxPaylineCountOfMultiLine3Reels)
					multiBackPrefab += paylineCount;
			}

			//multiLineFrame
			GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(multiFramePrefab);
			GameObject multiLineFrame = Instantiate(prefab, _reelFrame.transform);
			RectTransform t = multiLineFrame.GetComponent<RectTransform>();
			multiLineFrame.transform.localPosition = Vector3.zero;
			t.offsetMax = new Vector2(0.0f, 0.0f);
			t.offsetMin = new Vector2(0.0f, 0.0f);
			multiLineFrame.transform.localScale = Vector3.one;
			_multiLineFrame = multiLineFrame.GetComponent<PuzzleMultiLineFrame>();
			_multiLineFrame.Init(this);

			//multiLineBack
			prefab = AssetManager.Instance.LoadAsset<GameObject>(multiBackPrefab);
			GameObject multiLineBack = Instantiate(prefab, _reelFrame._multiLineBackParent.transform);
			t = multiLineBack.GetComponent<RectTransform>();
			multiLineBack.transform.localPosition = Vector3.zero;
			t.offsetMax = new Vector2(0.0f, 0.0f);
			t.offsetMin = new Vector2(0.0f, 0.0f);
			multiLineBack.transform.localScale = Vector3.one;
			_multiLineBack = multiLineBack.GetComponent<PuzzleMultiLineBack>();
			_multiLineBack.Init(this);
		}
	}

	private void InitDebugPanel()
	{
		#if DEBUG
		GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>("Game/PuzzleDebugPanel");
		Vector3 pos = prefab.transform.localPosition;
		Vector3 scale = prefab.transform.localScale;
		GameObject obj = Instantiate(prefab, _UIRoot.transform);
		obj.GetComponent<RectTransform>().localPosition = pos;
		obj.GetComponent<RectTransform>().localScale = scale;
		obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.0f, 0.5f);
		obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 0.5f);
		_debugPanel = obj.GetComponent<PuzzleDebugPanel>();
		#endif
	}

	private void InitResolutionAdaption()
	{
		if(DeviceUtility.IsIPadResolution())
		{
			ScaleSettingOnIpad news = this.MachineConfig.BasicConfig.ReelCount == 3 ? PuzzleConfig.M3OnIpad : PuzzleConfig.M4OnIpad;
				_leaderboardFrame.transform.localScale *= news._leaderboardScaleOnIpad;
			_reelFrame.transform.localScale *= news._reelFrameScaleOnIpad;
		}
	}

	private void InitSpecialMachine(){
		// zhousen 生成收集物
		if (_basicConfig.IsTriggerType (TriggerType.Collect)) {
			_nonPaylineCollectList = new List<CoreCollectData> ();
			StartSpinEventHandler += GenerateCollectOnSymbol;
			_collecting = false;
			UpdateCollectNum ();
		} else if (_basicConfig.HasFixWild) {
			_isFrozen = false;
		}

		if (PuzzleUtility.IsSpecialMachine (_coreMachine)) {
			EndRoundEventHandler += HideStrongSpecialEffect;
			EndSpinEventHandler += ShowReelSpecialEffect;
			EndSpinEventHandler += PlaySpecialAudio;
		}

		if (_basicConfig.ShowBgEffectType != ShowBackgroundEffectType.Max) {
			EndSpinEventHandler += ShowBackgroundEffect;
		}

		if (_basicConfig.CloseBgEffectType != CloseBackgroundEffectType.Max){
			StartSpinEventHandler += CloseBackgroundEffect;
		}

		if (_basicConfig.HideJackpotScoreWhenSmallGame) {
			EndSpinEventHandler += UpdateHideJackpotScoreWhenSmallGame;
		}
	}

	private void InitBGM(){
		UnityTimer.Start (this, 0.5f, () => {
			AudioManager.Instance.PlaySoundBGM (_basicConfig.MachineBGM);
		});
	}

	private void InitWheelPrefabDict()
	{
		if(_basicConfig.HasWheel)
		{
			Dictionary<int, string> wheelPrefabNameDict = _basicConfig.WheelPrefabNameDict;
			foreach(var pair in wheelPrefabNameDict)
			{
				string prefabName = pair.Value;
				GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(prefabName, _name);
				GameObject obj = UGUIUtility.CreateObj(prefab, _UIRoot);
				obj.SetActive(false);
				_wheelPrefabDict[prefabName] = obj;
			}
		}
	}

	void InitTapBox()
	{
		if(_basicConfig.IsPuzzleTapBox)
		{
			Debug.Assert(!string.IsNullOrEmpty(_basicConfig.SmallGamePrefab));
			GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(_basicConfig.SmallGamePrefab, _name);
			Debug.Assert(prefab != null);
			_tapBoxObject = UGUIUtility.CreateObj(prefab, _UIRoot);
			_tapBoxObject.transform.localPosition = PuzzleDefine.SmallGameCanvasLocalPos;
			_tapBoxObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
			_tapBoxObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
			_tapBoxObject.SetActive(false);

			PuzzleTapBoxController controller = _tapBoxObject.GetComponent<PuzzleTapBoxController>();
			controller.Init(this);
		}
	}

	void InitTapChip()
	{
		if(_basicConfig.IsPuzzleTapChip)
		{
			Debug.Assert(!string.IsNullOrEmpty(_basicConfig.SmallGamePrefab));
			GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(_basicConfig.SmallGamePrefab, _name);
			Debug.Assert(prefab != null);
			_tapChipObject = UGUIUtility.CreateObj(prefab, _UIRoot);
			_tapChipObject.transform.localPosition = PuzzleDefine.SmallGameCanvasLocalPos;
			_tapChipObject.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
			_tapChipObject.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
			_tapChipObject.SetActive(false);

			PuzzleTapChipController controller = _tapChipObject.GetComponent<PuzzleTapChipController>();
			controller.Init(this);
		}
	}

    void InitTryEndRoundHandler()
    {
        TryEndRoundHandler += PlaySpecialModeEffect;
    }

    void InitMachineRewardAdManager()
    {
        _machineRewardAdMng.Init();
    }

	void InitFreeSpinControllers()
	{
		SymbolFlyEffectController controller = _effect.SmallGameEffectParent.GetComponentInChildren<SymbolFlyEffectController>();
		if(controller != null)
			controller.Init(this);
	}

    private void OnDestroy()
	{
		SystemUtility.EnableScreenSleep();
		CitrusEventManager.instance.RemoveListener<EnterMainMapSceneEvent>(EnterMainMapSceneDelegate);
		if(MachineAssetManager.Instance && !string.IsNullOrEmpty(_name))
			MachineAssetManager.Instance.UnloadMachineAssetBundle(_name);
	}

	#endregion

	#region Misc

	private void EnterMainMapSceneDelegate(EnterMainMapSceneEvent message)
	{
		//after back button down, don't auto spin any more
		SetSpinMode(SpinMode.Normal);

		// 如果切换场景，则不需要继续start round
		if (_startRoundCoroutine != null)
		{
			this.StopCoroutine(_startRoundCoroutine);
			_startRoundCoroutine = null;
		}
	}

	#endregion

	#region Button events

	private bool TryStartRound()
	{
		if (!CanTryStartRound ())
			return false;

		if (_startRoundCoroutine != null)
		{
			this.StopCoroutine(_startRoundCoroutine);
			_startRoundCoroutine = null;
		}

		ExecuteSpecialPlayPre (null);

		bool result = _gameData.HasEnoughCredits() || _specialMode == SpecialMode.FreeMaxBet;
		if (result) {
			_curSmallGameStage = PuzzleSmallGameStage.None;
			#if false // zhousen
			_coreMachine.ClearRespinCount ();
#else
            _coreMachine.TryStartRound();
#endif
            Spin(false);

            if (_specialMode != SpecialMode.FreeMaxBet)
            {
                _gameData.SubtractCreditsFromBet();
            }

            _gameData.SetWinAmount(0);
        }
        else
        {
            // show store
            //StoreController.Instance.OpenStoreUI("Auto",StoreType.SmallBuy);
            // Xhj 改动打开smallBuy的方法
            CitrusEventManager.instance.Raise(new OutOfCreditsEvent(UserBasicData.Instance.Credits));
            SetSpinMode(SpinMode.Normal);
        }
        return result;
    }

    private bool CanForceHalt()
	{
		bool result = _curSmallGameStage == PuzzleSmallGameStage.None;
		result = IsPatch(result);
		return result;
	}

	private void ForceHalt()
	{
		DebugPrint("ForceHalt()");

		_spinData.IsUserForcedHalt = true;
//		AudioManager.Instance.PlaySound(AudioType.StopReel);

		for(int i = 0; i < _reelList.Count; i++)
		{
			PuzzleReel reel = _reelList[i];
			reel.SetShouldHalt();
		}
	}

	#endregion

	#region State

	private void ChangeState(MachineState s)
	{
		if (_state != s)
		{
			_state = s;
			//more possible logic here
		}
	}

	public MachineState GetState()
	{
		return _state;
	}

	#endregion

	#region Spin

	public void Spin(bool isRespin)
	{
		// #if UNITY_EDITOR
		// StringUtility.Test();
		// #endif
		StartPlayBackgroundSpinEffect (isRespin);

		EndLastSpinEffect();

		EndResetSwitchSymbol();

		//Note: if this function is continuously called more than once, it would cause critical bugs:
		//the symbol positions would be wrong
		Debug.Assert(_state != MachineState.Spinning, "Error: Spin() is called twice");
		ChangeState(MachineState.Spinning);

#if DEBUG_MACHINE
		++_debugSpinCount;
		DebugPrint("+++++ Spin() debugSpinCount: " + _debugSpinCount.ToString() + " isRespin:" + _shouldRespin.ToString());
#endif

		_spinData.Clear();
		ResetSmallGameProcessStates();
		_shouldRespin = false;
		_effect.StartSpinEffect();
		_effect.EndIdleEffect();

		_spinInput = new CoreSpinInput(_gameData.BetAmount, _basicConfig.ReelCount, isRespin);
		_coreMachine.Spin(_spinInput);
		UserBasicData.Instance.UpdateSpinDays();
		AnalysisManager.Instance.SendSpin(_name, _coreMachine.SpinResult, _spinMode, _coreMachine.SmallGameState, _specialMode);

		_spinManager.StartSpinReels(_coreMachine.SpinResult);
		StartSpinEventHandler(this);
		PlaySpinSound();
		RefreshDebugPanel();
	}

	private void EndResetSwitchSymbol(){
		if (_coreMachine.SpinResult != null && _coreMachine.SpinResult.SymbolListBefore.Count > 0){
			CoreSpinResult result = new CoreSpinResult(_coreMachine.SpinResult.Type, _coreMachine, _spinInput);
			result.SymbolList = new List<CoreSymbol>(_coreMachine.SpinResult.SymbolList);
			result.SymbolListBefore = new List<CoreSymbol>(_coreMachine.SpinResult.SymbolListBefore);
			UnityTimer.Start(this, 0.3f, ()=>{
				ResetSwitchSymbol(result);
			});
		}
	}

	private void EndLastSpinEffect()
	{
		_effect.EndWinEffect();
		ListUtility.ForEach(_reelList, (PuzzleReel r) => {
			r.EndLastSpinEffect();
		});
	}

	private void RefreshDebugPanel()
	{
		#if DEBUG
		if (_debugPanel != null){
			_debugPanel.Refresh(_coreMachine.SpinResult);
			_debugPanel.RefreshSpinParam(_puzzleReelSpinConfig, _puzzleConfig);
		}
		#endif
	}

	private void PlaySpinSound()
	{
		if (_coreMachine.SmallGameState == SmallGameState.FreeSpin)
		{
			_spinData.SpinSound = AudioType.FreeSpinSpinReel;
			UnityTimer.Start (this, _puzzleConfig.freeSpinReelAudioDelay
				- _puzzleConfig.tryStartRoundDelay, () => {
				AudioManager.Instance.PlaySound(_spinData.SpinSound);
			});
		}
		else if (_coreMachine.SmallGameState == SmallGameState.Rewind) 
		{
			_spinData.SpinSound = AudioType.RewindSpinReel;
			UnityTimer.Start (this, _puzzleConfig.rewindSpinReelAudioDelay
				- _puzzleConfig.tryStartRoundDelay, () => {
				AudioManager.Instance.PlaySound(_spinData.SpinSound);
			});
		}
		else {
			// 机台特殊的spin reel音效
			if (_basicConfig.SpinReelSoundNumber != 0) {
				_spinData.SpinSound = AudioManager.Instance.RollSound (_coreMachine.Name, _basicConfig.SpinReelSoundNumber);
			} else if (_basicConfig.SpinReelSoundNames.Length > 0){
				_spinData.SpinSound = RandomUtility.RollSingleElement(_coreMachine.Roller, _basicConfig.SpinReelSoundNames);
			} else {
				_spinData.SpinSound = AudioManager.Instance.RollSound ("SpinReel");
			}
			UnityTimer.Start (this, _puzzleConfig.spinReelAudioDelay
				- _puzzleConfig.tryStartRoundDelay, () => {
				AudioManager.Instance.PlaySound(_spinData.SpinSound);
			});
		}
	}

	private void StopSpinSound()
	{
		AudioManager.Instance.StopSound(_spinData.SpinSound);
	}

	private void ResetSmallGameProcessStates()
	{
		for (int i = 0; i < _smallGameHandlers.Count; i++)
			_smallGameHandlers[i].ProcessState = PuzzleSmallGameProcessState.Ready;
	}

	public void ReelHaltSpinCallback(PuzzleReel reel)
	{
		bool isAllHalt = ListUtility.IsAllElementsSatisfied(_reelList, (PuzzleReel r) =>
		{
			return !r.IsSpinning;
		});
		if (isAllHalt)
			AllReelsHaltSpin();
	}

	private void AllReelsHaltSpin()
	{
		DebugPrint("AllReelsHaltSpin()");

		_effect.EndSpinEffect();
		StopSpinSound();

		EnterSmallGameFront();
	}

	private void EnterSmallGameFront()
	{
		DebugPrint("StartEnterSmallGameFront()");

		ChangeState(MachineState.SmallGameFront);

		bool isSwitch = _coreMachine.CheckSwitchSmallGameState(_coreMachine.SpinResult, SmallGameMomentType.Front);

		TryNextSmallGameStage(isSwitch, EnterPostSpin);
	}

	private void TryNextSmallGameStage(bool isSwitch, Callback endCallback)
	{
		bool isEndAll = true;
		CoreSpinResult spinResult = _coreMachine.SpinResult;

		for (int i = 0; i < _smallGameHandlers.Count; i++)
		{
			PuzzleSmallGameHandler handler = _smallGameHandlers[i];
			if (handler.ProcessState == PuzzleSmallGameProcessState.Ready && handler.CheckFunc(spinResult, isSwitch))
			{
				handler.ProcessState = PuzzleSmallGameProcessState.Done;
				bool isEnd = handler.PerformFunc(spinResult, (bool shouldSwitch) => {
					TryNextSmallGameStage(shouldSwitch, endCallback);
				});
				_curSmallGameStage = handler.Stage;
				if (!isEnd)
				{
					isEndAll = false;
					break;
				}
			}
		}

		if (isEndAll)
		{
			_curSmallGameStage = PuzzleSmallGameStage.None;
			endCallback();
		}
	}

	private void CheckRespinEffectAudio(){
		if (_basicConfig.RespinAudio != AudioType.None){
			AudioManager.Instance.StopSound(_basicConfig.RespinAudio);
			if (_shouldRespin){
				UnityTimer.Start(this, 1.0f, ()=>{
					AudioManager.Instance.PlaySound(_basicConfig.RespinAudio);
				});
			}
		}
	}
	private void EnterPostSpin()
	{
		StartPlayBackgroundEndEffect ();

		DebugPrint("EndSpin()");

		ChangeState(MachineState.PostSpin);

		CheckAddWinAmount();
		RefreshShouldRespin();
		CheckAddJackpotPoint (_gameData.WinType);

		_effect.StartWinEffect(_gameData.WinType, _coreMachine.SpinResult.NormalWinType, _shouldRespin);
		CheckRespinEffectAudio();

		for (int i = 0; i < _reelList.Count; i++)
		{
			PuzzleReel reel = _reelList[i];
			reel.MachineEndSpin();
		}

		EndSpinEventHandler(this);

		float waitTime = GetPostSpinWaitTime();
		if (waitTime == 0.0f) {
			EndPostSpin ();
		}
		else {
			_spinData.EndPostSpinCoroutine = UnityTimer.Start (this, waitTime, EndPostSpin);
		}
	}

	private void RefreshShouldRespin()
	{
		_shouldRespin = _coreMachine.ShouldRespin();
	}

	private void CheckAddJackpotPoint (WinType winType)
	{
		_coreMachine.CheckAddJackpotPoint (winType);
	}

	private float GetPostSpinWaitTime()
	{
		float time = _spinData.GetPostSpinWaitTime();
		//when respin, the time should be short
		if (_shouldRespin) {
			if (_coreMachine.LastSpinResult == null || _coreMachine.LastSpinResult.Type == SpinResultType.Win) {
				if (_basicConfig.PostSpinWaitTimeWhenRespin != -1.0f) {
					time = _basicConfig.PostSpinWaitTimeWhenRespin;
				} else {
					time = PuzzleConfig._postSpinWaitTimeWhenRespin;
				}
			} else {
				time = 0.5f;//上一次spin没中奖，快速发起下一次spin
			}
		}

		return time;
	}

	private void EndPostSpinEarly()
	{
		_machineBar.EndPostSpinEarly();
	}

	private void EndPostSpin()
	{
		DebugPrint("EndPostSpin()");

		_spinData.EndPostSpinCoroutine = null;

		EnterSmallGameBehind();
	}

	private void EnterSmallGameBehind()
	{
		DebugPrint("StartEnterSmallGameBehind()");

		ChangeState(MachineState.SmallGameBehind);

		bool isSwitch = _coreMachine.CheckSwitchSmallGameState(_coreMachine.SpinResult, SmallGameMomentType.Behind);

		TryNextSmallGameStage(isSwitch, EndSmallGameBehind);
	}

	private void EndSmallGameBehind()
	{
		DebugPrint("EndSmallGameBehind() shouldRespin:" + _shouldRespin.ToString());

		if (_shouldRespin)
			StartRespin();
		else
			TryEndRound();
	}

	private void StartRespin()
	{
		DebugPrint("StartRespin()");

		//todo, respin prepare effect for waitTime
		ChangeState(MachineState.PreSpin);

		_effect.StartRespinPreEffect();

		UnityTimer.Start(this, GetRespinPreTime(), () => {
			ExecuteSpecialPlayPre (null);
			Spin(true);
		});
	}

    void TryEndRound()
    {
        TryEndRoundHandler(this);
        float delayTime = GetSpecialModeEffectDuringTime();

        if (delayTime > 0f)
            UnityTimer.Start(this, delayTime, EndRound);
        else
            EndRound();
    }

    private void EndRound()
	{
        DebugPrint("----- EndRound()");

        ChangeState( MachineState.Idle);

        _coreMachine.EndRound();

        EndRoundEventHandler(this);
	    RaiseRegisterWinAmountEvent();

        //don't call it here. Instead, call it when EndSpinEffect is ended
        //_effect.StartIdleEffect();

        CheckSendBankruptcyEvent();
		CheckBankruptCompensate ();
        CheckIsSpecialSpin();

		if ((_spinMode == SpinMode.Auto && PuzzleUtility.CanAutoSpinInWinType(_gameData.WinType)
            && PuzzleUtility.CanAutoSpinSmallGameState(_coreMachine.SmallGameState))
            || _spinData.IsUserTriggeredNextSpin)
        {
            _startRoundCoroutine = UnityTimer.Start (this, 
                _puzzleConfig.tryStartRoundDelay, () => {
                TryStartRound ();
            });
        }
	}

    private void RaiseRegisterWinAmountEvent()
    {
        CitrusEventManager.instance.Raise(new RegisterWinAmountEvent(_gameData.WinAmount));
    }

    private void CheckSendBankruptcyEvent()
	{
		ulong credits = _gameData.Credits;
		bool isLessThanMinBet = credits < _gameData.MinBetAmount;
		bool isLessThanCurBet = credits < _gameData.BetAmount;
		if(isLessThanMinBet || isLessThanCurBet)
			AnalysisManager.Instance.SendBankruptcy(_name, _spinMode, isLessThanMinBet, isLessThanCurBet,
				_gameData.BetAmount, credits);
	}

	private void CheckBankruptCompensate(){
		ulong credits = _gameData.Credits;
		bool isLessThanMinBet = credits < _gameData.MinBetAmount;
		// TODO: 5个if，应该重构一下。
		if (isLessThanMinBet) {
			// 是否是付费玩家
			int buyTimes = UserBasicData.Instance.BuyNumber;
			int lastBankruptBuytimes = UserBasicData.Instance.PayProtectionLastBankruptBuytimes;
			if (buyTimes > 0){
				// 是否符合补偿条件
				bool isCompensate = CoreConfig.Instance.LuckyConfig.IsBankruptCompensate (buyTimes, lastBankruptBuytimes);
				if (isCompensate) {
					float totalPayAmounts = UserBasicData.Instance.TotalPayAmount;
					int[] buyTimesRef = CoreConfig.Instance.LuckyConfig.PayProtectionBankruptIndexes;
					// 补偿credits计算
					int compensateCredits = BankruptCompensateConfig.Instance.GetCompensateCredits(totalPayAmounts, buyTimes, buyTimesRef);
					if (compensateCredits > 0) {
						LogUtility.Log ("CheckBankruptCompensate is on", Color.yellow);
						LogUtility.Log("credits = "+compensateCredits+" buytimes = "+buyTimes+" totoalPayamounts = "+totalPayAmounts, Color.yellow);
						GameObject obj = UIManager.Instance.OpenBankruptCreditsUI (compensateCredits);
						BankruptCompensateBehaviour behaviour = obj.GetComponent<BankruptCompensateBehaviour> ();
						if (behaviour != null) {
							behaviour.InitUI (compensateCredits);
							behaviour._collectionAction = () => {
								// 补偿成功
								UserBasicData.Instance.AddCredits((ulong)compensateCredits, FreeCreditsSource.PayUserBackruptCompesation, true);
								// _gameData.AddCredits ((ulong)compensateCredits);
								// 记录此次补偿时玩家购买次数
								UserBasicData.Instance.PayProtectionLastBankruptBuytimes = buyTimes;
								AnalysisManager.Instance.SendBankruptcyCredits(_name, compensateCredits);
								// 完成收集
								behaviour.CollectCoins();
							};
						}
					}
				}
			}
		}
	}

    void CheckIsSpecialSpin()
    {
        if (_specialMode != SpecialMode.Normal)
        {
            OnSpecialModeOver();
        }
    }

    void CheckAddWinAmount()
	{
		CoreSpinResult spinResult = _coreMachine.SpinResult;
		if (spinResult.IsWinWithNonZeroRatio())
			_gameData.AddWinAmount(spinResult.WinAmount);

		JackpotWinManager.Instance.RefreshJackpotWin (_gameData, spinResult);
	}

	public void AddWinAmountForIndieGame(ulong winAmount, SmallGameMomentType momentType)
	{
		if(momentType == SmallGameMomentType.Front)
		{
			float ratio = (float)winAmount / (float)_coreMachine.SpinResult.NormalizedBetAmount;
			_coreMachine.SpinResult.AddSpecialSmallGameWinRatio(ratio);
			_gameData.AddWinAmount(winAmount);
		}
		else if(momentType == SmallGameMomentType.Behind)
		{
			_gameData.AddCredits(winAmount);
			_machineBar.UpdateWin(winAmount);
		}
		else
		{
			Debug.Assert(false);
		}
	}

    void CheckAddWinAmountOnRoundEnd()
    {
        if (_gameData.WinAmount != 0)
            _gameData.AddWinAmount(_gameData.WinAmount);
    }

    #endregion

    #region PuzzleEffect

    private void ShowFrozenEffect(int index, bool show){
		if (index < _reelList.Count){
			_reelList[index].ShowFrozenEffect(show);
		}
	}

	#endregion

	#region Stage handlers
	private void ShowTriggerSmallGameSymbolEffectsDelay(CoreSpinResult spinResult){
		if (_basicConfig.EnableTriggerSmallGameEffectsDelay){
			List<CoreSymbol> triggerSymbols = _coreMachine.PlayModule.TriggerSymbols;
			LogUtility.Log("ShowTriggerSmallGameSymbolEffectsDelay count = "+triggerSymbols.Count, Color.red);

			for(int i = 0; i < triggerSymbols.Count; ++i){
				PuzzleReel reel = _reelList[triggerSymbols[i].ReelIndex];
				PuzzleSymbol puzzleSymbol = reel.GetPuzzleSymbol(triggerSymbols[i].StopIndex);
				// UnityTimer.Start(this, _basicConfig.TriggerSmallGameEffectsDelay, ()=>{
					puzzleSymbol.ShowWinEffect(WinType.Normal, false);
				// });

				UnityTimer.Start(this, _basicConfig.TriggerSmallGameWinEffectDelay, ()=>{
					puzzleSymbol.HideWinEffect();
				});

				float delayTime = _basicConfig.TriggerSmallGameEffectsDelay  + _basicConfig.TriggerSmallGameWinEffectDelay + i * _basicConfig.TriggerSmallGameEffectsDelayInterval;

				UnityTimer.Start(this, delayTime, ()=>{
					// puzzleSymbol.ShowWinEffect(WinType.Normal, false);
					reel.ShowSpecialSymbolLightEffect(true);
				});

				UnityTimer.Start(this, _basicConfig.TriggerSmallGameEffectsDuration + delayTime, ()=>{
					// puzzleSymbol.HideWinEffect();
					reel.ShowSpecialSymbolLightEffect(false);
				});
			}
		}
	}

	void ShowTriggerSmallGameSymbolEffects(CoreSpinResult spinResult, Callback endCallback)
	{
		List<CoreSymbol> triggerSymbols = _coreMachine.PlayModule.TriggerSymbols;

		ListUtility.ForEach(triggerSymbols, (CoreSymbol s) => {
			PuzzleReel reel = _reelList[s.ReelIndex];
			PuzzleSymbol puzzleSymbol = reel.GetPuzzleSymbol(s.StopIndex);
			puzzleSymbol.ShowWinEffect(WinType.Normal, false);
			reel.ShowSpecialSymbolLightEffect(true);
		});

		UnityTimer.Start(this, 2.0f, () => {
			ListUtility.ForEach(triggerSymbols, (CoreSymbol s) => {
				PuzzleReel reel = _reelList[s.ReelIndex];
				PuzzleSymbol puzzleSymbol = reel.GetPuzzleSymbol(s.StopIndex);
				puzzleSymbol.HideWinEffect();
				reel.ShowSpecialSymbolLightEffect(false);
			});

			if(endCallback != null)
				endCallback();
		});
	}

	private bool CheckCollect(CoreSpinResult result, bool isSwitch){
		bool isSwitched = false;

		UpdateCollect (result);
		isSwitched = _collecting;

		return isSwitched;
	}

	private bool PerformCollect(CoreSpinResult result, Callback<bool> endCallback){
		LogUtility.Log ("PerformCollect", Color.yellow);

		if (_collecting) {
			float duration = _basicConfig.CollectTotalDuration > 0.0f ? _basicConfig.CollectTotalDuration : 1.0f;
			UnityTimer.Start (this, duration, () => {
				if (endCallback != null)
					endCallback (false);
			});
		}

		return false;
	}

	bool CheckTapBox(CoreSpinResult spinResult, bool isSwitch)
	{
		bool isSwitched = _coreMachine.SmallGameState == SmallGameState.TapBox;
		return isSwitched;
	}

	bool PerformTapBox(CoreSpinResult spinResult, Callback<bool> endCallback)
	{
		LogUtility.Log("PerformTapBox", Color.yellow);

		UnityTimer.Start (this, 0.5f, () => {
			Debug.Assert(_tapBoxObject != null);

			AudioManager.Instance.PlaySound(AudioType.M40_TapBoxBGM, true);

			_effect.EndWinEffect();

			PuzzleTapBoxController controller = _tapBoxObject.GetComponent<PuzzleTapBoxController>();
			controller.Start(_gameData.BetAmount, (bool r) => {
				_coreMachine.CheckSwitchSmallGameState(spinResult, SmallGameMomentType.Behind);

				if(_basicConfig.IsTriggerType(TriggerType.Collect))
					_reelFrame.SetCollectSliderValue(0.0f);

				AudioManager.Instance.StopSound(AudioType.M40_TapBoxBGM);
				
				endCallback(r);
			});
		});

		return false;
	}

	bool CheckTapChip(CoreSpinResult spinResult, bool isSwitch)
	{
		bool isSwitched = _coreMachine.SmallGameState == SmallGameState.TapBox;
		return isSwitched;
	}

	bool PerformTapChip(CoreSpinResult spinResult, Callback<bool> endCallback)
	{
		LogUtility.Log("PerformTapChip", Color.yellow);

		Debug.Assert(_tapChipObject != null);

		AudioManager.Instance.PlaySound(AudioType.FreeSpinSwitch);

		ShowTriggerSmallGameSymbolEffects(spinResult, () => {
			//todo
			AudioManager.Instance.PlaySound(AudioType.M20_TapChipBGM, true);

			PuzzleTapChipController controller = _tapChipObject.GetComponent<PuzzleTapChipController>();
			controller.Start(_gameData.BetAmount, (bool r) => {
				//todo
				AudioManager.Instance.StopSound(AudioType.M20_TapChipBGM);

				endCallback(r);

				_coreMachine.CheckSwitchSmallGameState(spinResult, SmallGameMomentType.Front);
			});
		});

		return false;
	}

	private bool CheckFixWild(CoreSpinResult result, bool isSwitch)
	{
		bool isSwitched = false;

		CorePlayModuleFixWild module = _coreMachine.PlayModuleDict[SmallGameState.FixWild] as CorePlayModuleFixWild;
		isSwitched = module.LastFixReelIndexList.Count > 0;

		if (isSwitched){
			for (int i = 0; i < module.LastFixReelIndexList.Count; ++i) {
				_reelList [module.LastFixReelIndexList[i]].ShowFrozenEffect (true);
			}
			_isFrozen = true;
			AudioManager.Instance.PlaySound(_basicConfig.FrozenAudio);
			if (_basicConfig.ShowBackgroundEffectWhenFix){
				ShowBackgroundEffect (true);
			}
		}

		return isSwitched;
	}

	private bool PerformFixWild(CoreSpinResult result, Callback<bool> endCallback){
		LogUtility.Log ("PerformFixWild", Color.yellow);

		if (_isFrozen) {
			UnityTimer.Start (this, 1.0f, () => {
				_isFrozen = false;
				if (endCallback != null)
					endCallback (false);
			});
		}

		return false;
	}

	private bool CheckWheel(CoreSpinResult result, bool isSwitch){
		bool isSwitched = _coreMachine.SmallGameState == SmallGameState.Wheel;
		return isSwitched;
	}

	private bool PerformWheel(CoreSpinResult result, Callback<bool> endCallback){
		LogUtility.Log ("PerformWheel", Color.yellow);

		StartWheelProcess (result, endCallback);

		return false;
	}

	private void StartWheelProcess(CoreSpinResult result, Callback<bool> endCallback)
	{
		PayoutData triggerPayoutData = _coreMachine.PlayModule.TriggerPayoutData;
		Debug.Assert(triggerPayoutData.WheelNames.Length > 0);

		WheelConfig[] wheelConfigs = new WheelConfig[triggerPayoutData.WheelNames.Length];
		for(int i = 0; i < triggerPayoutData.WheelNames.Length; i++)
		{
			wheelConfigs[i] = _machineConfig.GetCurWheelConfig(result.LuckyMode, triggerPayoutData.WheelNames[i]);
		}

		Dictionary<int, string> wheelPrefabNameDict = _basicConfig.WheelPrefabNameDict;
		Debug.Assert(wheelPrefabNameDict.ContainsKey(triggerPayoutData.Id));
		string wheelPrefabName = wheelPrefabNameDict[triggerPayoutData.Id];

		ShowBackgroundEffect (true);

		AudioManager.Instance.PlaySound(AudioType.FreeSpinSwitch);
		AudioManager.Instance.PlaySound(AudioType.M10_WheelBGM, true);

		ShowTriggerSmallGameSymbolEffects(result, () => {
			// 转盘创建
			Debug.Assert(_wheelPrefabDict.ContainsKey(wheelPrefabName));
			GameObject obj = _wheelPrefabDict[wheelPrefabName];
			UIManager.Instance.OpenWheelUI(obj, this, wheelConfigs, endCallback);
		});
	}

	private bool CheckSlide(CoreSpinResult result, bool isSwitch)
	{
		return result.ShouldSlide;
	}

	private bool PerformSlide(CoreSpinResult result, Callback<bool> endCallback)
	{
		//Note: Unlike FreeSpin and Rewind handlers, here it should return false to indicate the spin stage doesn't end here.
		//Because slide triggers spin which would call AllReelsHaltSpin and StartEnterSmallGameFront again
		DebugPrint("PerformSlide()");
		LogUtility.Log ("PerformSlide", Color.yellow);

		UnityTimer.Start(this, 0.5f, () => {
			List<int> stopIndexes = result.StopIndexes;
			int slideCount = 0;
			for (int i = 0; i < result.SlideOffsetList.Count; i++)
			{
				int offset = result.SlideOffsetList[i];
				if (offset != 0)
				{
					SingleReel singleReel = _machineConfig.ReelConfig.GetSingleReel(i);
					PuzzleReel reel = _reelList[i];

					//Here is burning brain. When offset > 0, it should be the case like:
					//The current stopIndex is 1, while the final wild stopIndex is 2.
					//Now the wild symbol should be above the current one, so the reel should move down
					SpinDirection dir = (offset > 0) ? SpinDirection.Down : SpinDirection.Up;
					int neighborIndex = singleReel.GetNeighborStopIndex(stopIndexes[i], offset);

					// Set isSpinning flag to notify other logic the reel is spinning now
					// More specifically, it makes AllReelsHaltSpin not be called when the first slide reel stops
					reel.IsSpinning = true;

					Callback slideFunc = () => {
						reel.SpinToNeighbor(neighborIndex, dir, null);
					};

					if(slideCount == 0 || _puzzleReelSpinConfig._slideReelInterval == 0.0f)
					{
						slideFunc();
					}
					else
					{
						float interval = _puzzleReelSpinConfig._slideReelInterval * slideCount;
						UnityTimer.Start(this, interval, slideFunc);
					}

					++slideCount;
				}
			}

			AudioManager.Instance.PlaySound(AudioType.SlideSymbol);
		});

		return false;
	}

	private bool CheckFreeSpin(CoreSpinResult spinResult, bool isSwitch)
	{
		//force enter PerformFunc
		bool result = isSwitch || _coreMachine.SmallGameState == SmallGameState.FreeSpin
			|| _coreMachine.LastSmallGameState == SmallGameState.FreeSpin;
		return result;
	}

	private bool IsButterflyHitDelayValid(float delay){
		return delay < _butterflyHitDelayValidThreshold;
	}

	private float GetButterFlyHitTwinkleDelay(HitTwinkleType type, BasicConfig config){
		float delay = 1.3f;
		if (type == HitTwinkleType.Enter){
			if (config.ButterFlyHitTwinkleEnterDelay > 0.0f){
				delay = config.ButterFlyHitTwinkleEnterDelay;
			}
		}else if (type == HitTwinkleType.Leave){
			if (config.ButterFlyHitTwinkleEnterAndLeaveDelay > 0.0f){
				delay = config.ButterFlyHitTwinkleEnterAndLeaveDelay;
			}
		}
		return delay;
	}

	private float GetFreeSpinHintDelay()
	{
		float result = 0.5f;
		if (_basicConfig.FreespinHintDelay > 0.0f) {
			result = _basicConfig.FreespinHintDelay;
		}
		return result;
	}

	private AudioType GetFreeSpinHintAudio()
	{
		AudioType result = AudioType.FreeSpinSwitch;
		if (_basicConfig.FreespinHintAudio != AudioType.None){
			result = _basicConfig.FreespinHintAudio;
		}
		return result;
	}

	private bool PerformFreeSpin(CoreSpinResult spinResult, Callback<bool> endCallback)
	{
		DebugPrint("PerformFreeSpin");

		bool result = true;
		CollectionProcessType collectType = _basicConfig.CollectionProcessType;
		MachineTheme theme = CoreDefine.MachineThemeDict[_coreMachine.Name];

		if (_coreMachine.SmallGameState == SmallGameState.FreeSpin){
			if (_basicConfig.FreeSpinType == FreeSpinType.FixCount) {
				UpdateFreeSpinFixCount ();
				UpdateFreeSpinMultiplier ();

				if (_basicConfig.HitTwinkle){
					float delay = GetButterFlyHitTwinkleDelay(HitTwinkleType.Enter, _basicConfig);
					if (_coreMachine.LastSmallGameState == SmallGameState.FreeSpin) {
						delay = GetButterFlyHitTwinkleDelay(HitTwinkleType.Leave, _basicConfig);
					}
					if (IsButterflyHitDelayValid(delay)){
						UnityTimer.Start (this, delay, () => {
							_effect.StartButterflyHitTwinkle ();
						});
					}
				}
			}
		}

		if (_coreMachine.LastSmallGameState == SmallGameState.FreeSpin) {
			if (_basicConfig.HitTwinkle){
				if (spinResult.Type == SpinResultType.Win)
				{
					result = false;
					_effect.StartButterflyHit(endCallback);
				}
			}
		}

		if (_coreMachine.SmallGameState == SmallGameState.FreeSpin
			&& _coreMachine.LastSmallGameState != SmallGameState.FreeSpin)
		{
			AudioManager.Instance.PlaySoundBGM (_basicConfig.SpecialBGM);

			ShowBackgroundEffect (true);
			UpdateCanvasAlpha(true);
			if (_basicConfig.IsTriggerType (TriggerType.Collect)) {
				UnityTimer.Start (this, 1.5f, () => {
					AudioManager.Instance.PlaySound (AudioType.FreeSpinSwitch2);
					_effect.StartSmallGameEffect ();
				});

				_reelFrame.ShowSliderEffect(CollectEffectType.Complete, true);
				AudioManager.Instance.PlaySound(_basicConfig.FreespinBGM, true);

				if (collectType == CollectionProcessType.MultiLocation) {
					// 需要播放一个特殊特效动画
					if (!_basicConfig.SpecialReelBackAnim.IsNullOrEmpty ()) {
						UnityTimer.Start (this, _basicConfig.SpecialReelBackStartDelay, () => {
							_effect.StartSpecialReelBackEffectAnimation (_basicConfig.SpecialReelBackAnim);
						});
					}
				}
			}
			else {
				float delay = GetFreeSpinHintDelay();
				AudioType hintAudio = GetFreeSpinHintAudio();

				UnityTimer.Start(this, delay, () => {
					AudioManager.Instance.PlaySound (hintAudio);
					AudioManager.Instance.PlaySound(_basicConfig.FreespinBGM, true);
					ShowBackgroundEffect(_basicConfig.FreespinBackgroundEffectType);
				});

				if (_basicConfig.FreespinEffectNoDelay){
					_effect.StartSmallGameEffect ();
				}else{
					UnityTimer.Start(this, delay, ()=>{
						_effect.StartSmallGameEffect ();
					});
				}
			}

			AudioManager.Instance.StopSoundBGM (_basicConfig.MachineBGM);
		}
		else if (_coreMachine.SmallGameState == SmallGameState.None
			&& _coreMachine.LastSmallGameState == SmallGameState.FreeSpin)
		{

			AudioManager.Instance.StopSoundBGM (_basicConfig.SpecialBGM);

			ShowBackgroundEffect (false);
			UpdateCanvasAlpha(false);
			_effect.EndSmallGameEffect();

			if (_basicConfig.IsTriggerType(TriggerType.Collect)){
				// 清空收集条
				_reelFrame.SetCollectSliderValue(0.0f);
				_reelFrame.ShowSliderEffect(CollectEffectType.Complete, false);
				if (collectType == CollectionProcessType.MultiLocation) {
					// 切换状态
					if (!_basicConfig.SpecialReelBackIdleAnimTrigger.IsNullOrEmpty ()) {
						_effect.StartSpecialReelBackEffectAnimationTrigger (_basicConfig.SpecialReelBackIdleAnimTrigger);
					}
				}

				AudioManager.Instance.StopSound(_basicConfig.FreespinBGM);
			}
			else 
			// if (_basicConfig.IsTriggerType(TriggerType.Payout)
			// 	|| _basicConfig.IsTriggerType(TriggerType.UnorderCount))
			{
				AudioManager.Instance.StopSound(_basicConfig.FreespinBGM);
			}

			AudioManager.Instance.PlaySoundBGM (_basicConfig.MachineBGM);
		}

		if(_coreMachine.SmallGameState == SmallGameState.FreeSpin
		   && _coreMachine.LastSmallGameState == SmallGameState.FreeSpin)
		{
			CorePlayModuleFreeSpin module = _coreMachine.PlayModuleDict [SmallGameState.FreeSpin] as CorePlayModuleFreeSpin;
			if(module.IsTriggerAgain)
			{
				float delay = GetFreeSpinHintDelay();
				AudioType hintAudio = GetFreeSpinHintAudio();

				UnityTimer.Start(this, delay, () => {
					AudioManager.Instance.PlaySound (hintAudio);
				});
			}
		}

		return result;
	}

	private bool CheckRewind(CoreSpinResult result, bool isSwitch)
	{
		bool isSwitched = _coreMachine.SmallGameState == SmallGameState.Rewind;
		return isSwitched;
	}

	private bool PerformRewind(CoreSpinResult result, Callback<bool> endCallback)
	{
		DebugPrint("PerformSwitchRewind()");

		LogUtility.Log("PerformSwitchRewind", Color.red);

		UnityTimer.Start(this, _puzzleConfig.rewindSwitchAudioDelay, () => {
			ShowBackgroundEffect (true);

			for (int i = 0; i < _reelList.Count; ++i) {
				PuzzleSymbol symbol = _reelList [i].GetPuzzleSymbol (result.StopIndexes [i]);
				symbol.StartRewindAnimation ();
			}

			AudioManager.Instance.PlaySound(AudioType.RewindSwitch);

			_effect.StartSmallGameEffect();
			UnityTimer.Start(this, 1.5f, () => {
				_effect.EndSmallGameEffect();
			});
		});

		UnityTimer.Start(this, 0.5f, ()=>{
			if (endCallback != null){
				endCallback(false);
			}
		});

		return false;
	}

	private bool CheckSwitchSymbol(CoreSpinResult result, bool isSwitch){
		bool isSwitched = _coreMachine.SmallGameState == SmallGameState.SwitchSymbol;
		return isSwitched;
	}

	private bool PerformSwitchSymbol(CoreSpinResult result, Callback<bool> endCallback){
		LogUtility.Log("PerformSwitchSymbol()", Color.red);

		// 过场动画
		UnityTimer.Start(this, _basicConfig.SwitchSymbolEffectStartDelay, ()=>{
			_effect.StartSmallGameEffect();
		});
		UnityTimer.Start(this, _basicConfig.SwitchSymbolSound1Delay, ()=>{
			AudioManager.Instance.PlaySound(_basicConfig.SwitchSymbolSound1);
		});
		UnityTimer.Start(this, _basicConfig.SwitchSymbolSound2Delay, ()=>{
			AudioManager.Instance.PlaySound(_basicConfig.SwitchSymbolSound2);
		});

		SwitchSymbol(result, endCallback);

		// trigger时reel特效
		ShowTriggerSmallGameSymbolEffectsDelay(result);

		AnalysisManager.Instance.SendSpinLevelUp(_name, _coreMachine.SpinResult, _spinMode, _coreMachine.SmallGameState, _specialMode);

		return false;
	}

	private void SwitchSymbol(CoreSpinResult result, Callback<bool> endCallback){
		if (_basicConfig.EnableTriggerSmallGameEffectsDelay){
			// 这里的逻辑跟忍者机台相同
			List<CoreSymbol> triggerSymbols = _coreMachine.PlayModule.TriggerSymbols;
			List<CoreSymbol> symbolListBefore = result.SymbolListBefore;
			List<CoreSymbol> switchList = result.SymbolList;

			for(int i = 0; i < triggerSymbols.Count; ++i){
				int reelIndex = triggerSymbols[i].ReelIndex;
				PuzzleReel reel = _reelList[reelIndex];
				PuzzleSymbol puzzleSymbol = reel.GetPuzzleSymbol(symbolListBefore[reelIndex].StopIndex);
				CoreSymbol coreSymbol = switchList[reelIndex];

				float delayTime = _basicConfig.TriggerSmallGameWinEffectDelay + 0.3f * _basicConfig.TriggerSmallGameEffectsDuration + _basicConfig.TriggerSmallGameEffectsDelay + i * _basicConfig.TriggerSmallGameEffectsDelayInterval;
				UnityTimer.Start(this, delayTime, ()=>{
					puzzleSymbol.HideWinEffect();
					puzzleSymbol.SwitchSymbol(coreSymbol);
					puzzleSymbol.ShowWinEffect(WinType.Normal, false);
					puzzleSymbol.ChangeAnimation(_basicConfig.ChangeAnim);
					AudioManager.Instance.PlaySound(_basicConfig.SwitchSymbolSound3);
				});
			}

			float endSmallGameDelay = _basicConfig.TriggerSmallGameWinEffectDelay + _basicConfig.SwitchSymbolEffectEndDelay;
			UnityTimer.Start(this, endSmallGameDelay, ()=>{
				_effect.EndSmallGameEffect();
			});

			float endCallbackDelay = _basicConfig.TriggerSmallGameWinEffectDelay + _basicConfig.TriggerSmallGameEffectsDuration + _basicConfig.TriggerSmallGameEffectsDelay + triggerSymbols.Count * _basicConfig.TriggerSmallGameEffectsDelayInterval;
			UnityTimer.Start(this, endCallbackDelay, ()=>{
				if (endCallback != null){
					endCallback(false);
				}
			});

		}else{
			// 这里的逻辑跟圣诞节机台相同
			List<CoreSymbol> list = result.SymbolList;
			Debug.Assert(list.Count == _basicConfig.SwitchSymbolDelays.Length, "switch symbol delay length is not correct");
			
			// 播放一段时间后，根据corespinresult替换puzzlesymbol
			for(int i = 0; i < list.Count; ++i){
				PuzzleSymbol symbol = _reelList[i].GetPuzzleSymbol(list[i].StopIndex);
				CoreSymbol coreSymbol = list[i];
				if (symbol != null){
					UnityTimer.Start(this, _basicConfig.SwitchSymbolDelays[i], ()=>{
						symbol.HideWinEffect();
						symbol.SwitchSymbol(coreSymbol);
						symbol.ShowWinEffect(WinType.Normal, false);
						symbol.ChangeAnimation(_basicConfig.ChangeAnim);
						UnityTimer.Start(this, _basicConfig.SwitchSymbolSound3Delay, ()=>{
							AudioManager.Instance.PlaySound(_basicConfig.SwitchSymbolSound3);
						});
					});
				}
			}

			UnityTimer.Start(this, _basicConfig.SwitchSymbolEffectEndDelay, ()=>{
				_effect.EndSmallGameEffect();

				if (endCallback != null){
					endCallback(false);
				}
			});
		}
	}

	private void ResetSwitchSymbol(CoreSpinResult result){
		// 下次spin的时候延迟进行 根据corespinresult替换回来puzzlesymbol
		List<CoreSymbol> beforeList = result.SymbolListBefore;
		List<CoreSymbol> list = result.SymbolList;

		for(int i = 0; i < beforeList.Count; ++i){
			if (beforeList[i] != null){
				PuzzleSymbol symbol = _reelList[i].GetPuzzleSymbol(beforeList[i].StopIndex);
				
				LogUtility.Log("Reset Switch Symbol " + list[i].SymbolData.Name + " to " + symbol.CoreSymbol.SymbolData.Name + " reel = " + i, Color.red);
				if (symbol != null){
					symbol.SwitchSymbol(beforeList[i]);
				}
			}
		}
	}

	#endregion

	#region patch
	// 解决M17在freespin时候强制halt会导致symbol重叠的问题
	private bool IsPatch(bool enable){
		bool result = enable;
		// if (_coreMachine.Name == "M17"){
		// 	result = enable && _coreMachine.SmallGameState != SmallGameState.FreeSpin;
		// }
		return result;
	}

	#endregion

	#region Spin button

	public void SpinButtonDown()
	{
		if (_spinMode == SpinMode.Auto)
		{
			SetSpinMode(SpinMode.Normal);
		}
		else
		{
			DebugPrint("SpinButtonDown() _state:" + _state.ToString());

			if(_state == MachineState.Idle)
			{
				bool started = TryStartRound();
				if(!started)
				{
					//send event: pop purchase menu
				}
			}
			else if(_state == MachineState.Spinning)
			{
				if(CanForceHalt())
					ForceHalt();
			}
			else if(_state == MachineState.SmallGameFront)
			{
				_spinData.IsUserTriggeredNextSpin = true;
			}
			else if(_state == MachineState.PostSpin)
			{
				if(!_shouldRespin && _gameData.WinType == WinType.Normal && _spinData.EndPostSpinCoroutine != null)
				{
					DebugPrint("Call EndPostSpin() earlier");

					this.StopCoroutine(_spinData.EndPostSpinCoroutine);
					_spinData.EndPostSpinCoroutine = null;

					EndPostSpinEarly();
					EndPostSpin();
				} 
			}
			else if(_state == MachineState.PreSpin)
			{
				//Spin is scheduled, so don't do anything here
			}
		}
	}

	public void SpinButtonLongPressed()
	{
		if (_spinMode == SpinMode.Normal)
		{
			SetSpinMode(SpinMode.Auto);
			if (_state == MachineState.Idle)
				TryStartRound();
		}
	}

	public void SetSpinMode(SpinMode mode)
	{
		if(_spinMode != mode)
		{
			_spinMode = mode;
			if (mode == SpinMode.Normal)
			{
				_machineBar.SetSpinButtonState(SpinButtonState.Normal);
				SystemUtility.EnableScreenSleep();
			}
			else if (mode == SpinMode.Auto)
			{
				_machineBar.SetSpinButtonState(SpinButtonState.Auto);
				SystemUtility.DisableScreenSleep();
			}
		}
	}

    #endregion

	#region Debug

	void DebugPrint(string message)
	{
#if DEBUG_MACHINE
		Debug.LogError(message);
#endif
	}

	#endregion

	#region background effect
	private void ShowBackgroundEffect(BackgroundType type){
		if (_backgroundEffectBehaviour != null) {
			_backgroundEffectBehaviour.ShowEffect (type);
		}
	}

	private void StartPlayBackgroundEffect(){
		if (_basicConfig.BackgroundEffectMultiType){
			ShowBackgroundEffect(BackgroundType.Idle);
		}
	} 

	private void StartPlayBackgroundSpinEffect(bool isRespin){
		if (_basicConfig.BackgroundEffectMultiType && !isRespin) {
			ShowBackgroundEffect(BackgroundType.Spin);
		}
	}

	private void StartPlayBackgroundEndEffect(){
		if (_basicConfig.BackgroundEffectMultiType) {
			ShowBackgroundEffect(BackgroundType.Idle);
		}
	}
	
	private void ShowBackgroundEffect(bool show){
		if (_backgroundEffect != null) {
			if (show){
				UnityTimer.Start(this, _basicConfig.BackgroundEffectStartDelay, ()=>{
					_backgroundEffect.SetActive (show);	
				});
			}else{
				_backgroundEffect.SetActive (show);
			}
		}
	}

	private void ShowBackgroundEffect(PuzzleMachine machine){
		if (_basicConfig.ShowBgEffectType == ShowBackgroundEffectType.HighOrSpecialWin) {
			if (PuzzleUtility.IsSpecialWin (GameData.WinType) || machine.CoreMachine.SpinResult.NormalWinType == NormalWinType.High) {
				ShowBackgroundEffect (true);
			}
		} else if (_basicConfig.ShowBgEffectType == ShowBackgroundEffectType.Respin) {
			ShowBackgroundEffect (_shouldRespin);
		} else if (_basicConfig.ShowBgEffectType == ShowBackgroundEffectType.HighWin){
			if (machine.CoreMachine.SpinResult.NormalWinType == NormalWinType.High){
				ShowBackgroundEffect(true);
			}
		}
	}

	private void CloseBackgroundEffect(PuzzleMachine machine){
		if (_basicConfig.CloseBgEffectType == CloseBackgroundEffectType.NoRespin) {
			// 必须是这次spin不是respin的情况
			if (!machine.CoreMachine.ShouldRespin()){
				ShowBackgroundEffect (false);
			}
		} else {
			ShowBackgroundEffect (false);
		}
	}
	#endregion

	#region Small game process

	private void UpdateHideJackpotScoreWhenSmallGame(PuzzleMachine machine){
		if (_coreMachine.SmallGameState == SmallGameState.FreeSpin && _coreMachine.LastSmallGameState != SmallGameState.FreeSpin){
			_reelFrame.JackpotCheck.Hide();
		}else if (_coreMachine.SmallGameState == SmallGameState.None && _coreMachine.LastSmallGameState == SmallGameState.FreeSpin){
			_reelFrame.JackpotCheck.Show();
		}
	}

	// 关闭特殊动画
	private void HideStrongSpecialEffect(PuzzleMachine machine){
		if (machine.CoreMachine.SpinResult.Type != SpinResultType.Win || 
			machine.CoreMachine.SpinResult.IsWinWithZeroRatio()) {
			_effect.HideStrongSpecialSymbolEffect ();
		}
	}

	//卷轴的特别效果
	private void ShowReelSpecialEffect(PuzzleMachine machine){
		if(machine.CoreMachine.ShouldRespin()){
			if (_basicConfig.RespinReelSpecialEffect){
				// 中轴播放特效
				UnityTimer.Start(this, 3.0f, () => {
					_reelList[1].ShowDragonEffect(true);
					_dragonReelEffectCoroutine = UnityTimer.Start (this, 3.3f, () => {
						_reelList[1].ShowDragonEffect(false);
					});	
				});
			}
		}
	}

	// 特殊音效需求
	private void PlaySpecialAudio(PuzzleMachine machine){
		CoreSpinResult result = machine.CoreMachine.SpinResult;
		if (_basicConfig.SpecialAudio != AudioType.None){
			if (machine.CoreMachine.SpinResult.Type == SpinResultType.Win){
				PuzzleSymbol symbol = _reelList [1].GetPuzzleSymbol (result.StopIndexes [1]);
				if (machine.CoreMachine.MachineConfig.SymbolConfig.IsMatchSymbolType(symbol.CoreSymbol.SymbolData.SymbolType, SymbolType.Wild)){
					AudioManager.Instance.PlaySound(_basicConfig.SpecialAudio);
				}
			}
		}
	}

	// 生成收集物
	private void GenerateCollectOnSymbol(PuzzleMachine machine){
		if (_coreMachine.SmallGameState == SmallGameState.FreeSpin)
			return;

		List<PuzzleSymbol> puzzleSymbolList = new List<PuzzleSymbol> ();

		// 找出哪些puzzle symbol需要添加收集物
		List<CoreCollectData> collectList = _coreMachine.SpinResult.CollectDataList;
		for (int i = 0; i < collectList.Count; ++i) {
			CoreCollectData data = collectList [i];
			PuzzleSymbol symbol = _reelList [data.ReelIndex].GetPuzzleSymbol (data.StopIndex);
//			LogUtility.Log ("GenerateCollectOnSymbol reelindex and stopindex is "+data.ReelIndex+" "+data.StopIndex, Color.red);
			symbol.CollectData = data;
			puzzleSymbolList.Add (symbol);
		}

		// 需要等转起来才能生成收集物
		ListUtility.ForEach<PuzzleSymbol>(puzzleSymbolList, (PuzzleSymbol symbol)=>{
			symbol.AddHandler = ()=>{ symbol.GenerateCollect(); };
		});
	}

	// 更新收集槽状态(进机台，切换BET时调用)
	public void UpdateCollectNum(){
		CorePlayModuleNormal module = _coreMachine.PlayModuleDict [SmallGameState.None] as CorePlayModuleNormal;
		if (module != null) {
			int collectNum = UserMachineData.Instance.GetBetCollectNum (_coreMachine.Name, _gameData.BetAmount);
			module.SetCurrentCollectParam (_gameData.BetAmount, collectNum);
			
			CollectionProcessType type = _basicConfig.CollectionProcessType;
			float value = 0.0f;
			if (type == CollectionProcessType.SingleLocation) {
				value = (float)collectNum / _basicConfig.CollectNum;
			} else if (type == CollectionProcessType.MultiLocation) {
				value = (float)collectNum;
			}
			_reelFrame.SetCollectSliderValue(value);
		}
	}

	// 更新收集物
	private void UpdateCollect(CoreSpinResult result){
		// 收集物列表
		List<CoreCollectData> collectList = result.CollectDataList;
		// 支付线上的收集物
		List<CoreCollectData> flyList = result.GetPayLineCollectList();
		// 非支付线上的收集物
		List<CoreCollectData> noFlyList = ListUtility.SubtractList (collectList, flyList);
		// 更新非支付线上的物体
		_nonPaylineCollectList.AddRange(noFlyList);

		for (int i = 0; i < flyList.Count; ++i) {
			CoreCollectData data = flyList[i];
			PuzzleSymbol symbol = _reelList [data.ReelIndex].GetPuzzleSymbol (data.StopIndex);
			CollectController ctl = symbol.gameObject.GetComponentInChildren<CollectController> ();
			if (ctl == null) {
				LogUtility.Log ("collect controller is null, reelindex is "
					+ data.ReelIndex + " stopindex is " + data.StopIndex);
			} else {
				CollectionProcess (ctl, i);
				// 飞行中不能触发spin
				_collecting = true;
			}
		}
	}

	// 收集物处理
	private void CollectionProcess(CollectController ctrl, int index){
		CollectionProcessType type = _basicConfig.CollectionProcessType;

		// 收集物从symbol上转移到frame上
		ctrl.gameObject.transform.SetParent (_reelFrame._widgetEffect.transform);
		// 收集物飞行结束的回调
		ctrl.FlyEndCallback += CollectEndProcess;
		// 收集物终点设置
		UpdateFlyLocation (ctrl, type, index);
	}

	// 到收集点时的处理
	private void CollectEndProcess(GameObject obj){
		CollectionProcessType type = _basicConfig.CollectionProcessType;
		UpdateCollectionBar (type);
		CollectionEffectProcess (obj, type);
	}

	// 收集物飞行的目标位置
	private void UpdateFlyLocation(CollectController ctrl, CollectionProcessType type, int index){
		GameObject destObj = _reelFrame.GetLocationObjFromCurrentValue(index);
		ctrl.StartFly(destObj, _basicConfig.CollectFlyDuration, _basicConfig.CollectAnimatorDuration);
		AudioManager.Instance.PlaySound(_basicConfig.CollectFlyAudio);
	}

	// 收集槽更新
	private void UpdateCollectionBar(CollectionProcessType type){
		_reelFrame.UpdateCollectSliderValue();
	}

	// 收集点特效处理
	private void CollectionEffectProcess(GameObject obj, CollectionProcessType type){
		if(type == CollectionProcessType.SingleLocation)
		{
			Destroy(obj);
			_collecting = false;

			_reelFrame.ShowSliderEffect(CollectEffectType.Once, true);
			UnityTimer.Start(this, 3.0f, () => {
				_reelFrame.ShowSliderEffect(CollectEffectType.Once, false);
			});
		}
		else if(type == CollectionProcessType.MultiLocation)
		{
			//  TODO: 可能还要加
			Destroy(obj);
			_collecting = false;
		}
		else
		{
			Debug.Assert(false);
		}
	}

	private bool CanTryStartRound(){
		if (_basicConfig.IsTriggerType (TriggerType.Collect)) {
			return !_collecting;
		}

		return true;
	}

	// 固定次数的freespin特效计数
	private void UpdateFreeSpinFixCount(){
		FreeSpinController ctrl = _effect.SmallGameEffectParent.GetComponentInChildren<FreeSpinController>();
		if (ctrl != null){
			CorePlayModuleFreeSpin module = _coreMachine.PlayModuleDict[SmallGameState.FreeSpin] as CorePlayModuleFreeSpin;

			SymbolFlyEffectController flyController = _effect.SmallGameEffectParent.GetComponentInChildren<SymbolFlyEffectController>();
			if(flyController != null)
			{
				List<PuzzleSymbol> symbols = GetPuzzleSymbols(module.TriggerSymbols);
				flyController.PlayFlyEffect(symbols, () => {
					ctrl.SetTotalCount(module.FreeSpinData.TotalFixCount);
				});
			}
			else
			{
				//set TotalFixCount earlier here
				ctrl.SetTotalCount(module.FreeSpinData.TotalFixCount);
			}

			UnityTimer.Start (this, GetPostSpinWaitTime() + _puzzleConfig.tryStartRoundDelay + 
				GetRespinPreTime (),
//				_respinPreWaitTime,
				() => {
					ctrl.SetCurrentAndTotalCount(module.RespinCount, module.FreeSpinData.TotalFixCount);
				});
		}
	}

	//  freespin下不同multiplier的更新
	private void UpdateFreeSpinMultiplier(){
		MultiplierFreeSpinController ctrl = _effect.SmallGameEffectParent.GetComponentInChildren<MultiplierFreeSpinController> (true);
		if (ctrl != null) {
			CorePlayModuleFreeSpin module = _coreMachine.PlayModuleDict [SmallGameState.FreeSpin] as CorePlayModuleFreeSpin;
			CoreSpinResult result = _coreMachine.SpinResult;
			float multiplier = result.GetResultSpecialMultiplier();
			ctrl.SetMultiplier(multiplier);
		}
	}

	// 卷轴半透明化
	private void UpdateCanvasAlpha(bool state){
		if (_basicConfig.IsCanvasAlphaEnable){
			CanvasGroup group = _reelFrame.GetComponentInChildren<CanvasGroup>();
			if (group != null){
				if (state){
					group.alpha = _basicConfig.CanvasAlphaValue;
				}else{
					group.alpha = 1.0f;
				}
			}
		}
	}
	private void ExecuteSpecialPlayPre(UnityAction action){
		if (_basicConfig.HasFixWild) {
			if (_coreMachine.SmallGameState != SmallGameState.FixWild &&
			    _coreMachine.LastSmallGameState == SmallGameState.FixWild) {
				// 解冻特效
				CorePlayModuleFixWild module = _coreMachine.PlayModuleDict [SmallGameState.FixWild] as CorePlayModuleFixWild;
				for (int i = 0; i < module.FixReelIndexList.Count; ++i) {
					_reelList [module.FixReelIndexList [i]].ShowFrozenEffect (false);
				}
				AudioManager.Instance.PlaySound(_basicConfig.BreakFrozenAudio);
				ShowBackgroundEffect (false);
			}
		} else if (_basicConfig.IsTriggerType (TriggerType.Collect)) {
			if (_nonPaylineCollectList.Count > 0) {
				for (int i = 0; i < _nonPaylineCollectList.Count; ++i) {
					CoreCollectData data = _nonPaylineCollectList [i];
					PuzzleSymbol symbol = _reelList [data.ReelIndex].GetPuzzleSymbol (data.StopIndex);
					CollectController ctl = symbol.gameObject.GetComponentInChildren<CollectController> ();
					if (ctl == null) {
						LogUtility.Log ("ExecuteSpecialPlayPre collect controller is null, reelindex is "
						+ data.ReelIndex + " stopindex is " + data.StopIndex);
					} else {
						symbol.PopHandler = () => {
							Destroy (ctl.gameObject);
						};
					}
				}
				_nonPaylineCollectList.Clear ();
			}
		} else if (_basicConfig.HasWheel && _basicConfig.ShowBgEffectType == ShowBackgroundEffectType.Max) {
			ShowBackgroundEffect (false);
		}
	}

	// 获得respin的pre时间
	private float GetRespinPreTime(){
		CoreSpinResult result = _coreMachine.LastSpinResult;
		if (result == null) {
			return _respinPreWaitTime;
		}

		// 第一次进入respin的状态，不需要进行判断
		if (_coreMachine.LastSmallGameState == SmallGameState.None){
			// LogUtility.Log("First Enter Respin Pre Time = " + _respinPreWaitTime, Color.red);
			if (_basicConfig.EnableTriggerSmallGameDelay){
				return _basicConfig.TriggerSmallGameDelay;
			}
			return _respinPreWaitTime;
		}

		float preTime = result.Type == SpinResultType.Win ? _respinPreWaitTime : _respinPreWaitTimeFail;
		// LogUtility.Log("Respin Pre Time = " + preTime, Color.red);
		return preTime;
	}

	#endregion

	#region jackpot

	private void ShowJackpotTip(){
		// 获得当前betindex, 判断是小于最低jackpot可玩数， 还是大于
		int jackpotMinIndex = MathUtility.GetLocateIndex (_gameData.BetOptions, _basicConfig.JackpotMinBet);
		bool isShowJackpotCanPlay = _gameData.BetIndex >= jackpotMinIndex;
		if (_reelFrame.JackpotTip != null) {
			_reelFrame.JackpotTip.ShowTip (isShowJackpotCanPlay ? 
				JackpotTipType.JackpotCanPlay : JackpotTipType.LeastBet);
		}
		if (_reelFrame.JackpotCheck != null) {
			_reelFrame.JackpotCheck.EnableJackpot (isShowJackpotCanPlay);
		}
	}

	#endregion

	#region bet unlock

	private void ShowBetUnlockTip(){
		if (_machineBar.BetTip == null)
			return;

		BetTipBehaviour tips = _machineBar.BetTip;
		BetTipType type = BetTipType.Max;

		if (_gameData.IsBetLowest) {
			type = BetTipType.Lowest;
		}else if (_gameData.IsBetHighest){
			type = BetTipType.Highest;
		}

		if (type != BetTipType.Max) {
			tips.ShowBetTip (type);
		}
	}

    #endregion

    #region special mode
    public void EnterSpecialMode(SpecialMode mode)
    {
        if (_state == MachineState.Idle &&
            _specialMode != mode &&
            EnterSpecialModeHandler != null)
        {
            _specialMode = mode;
            EnterSpecialModeHandler(this);
            if (mode == SpecialMode.FreeMaxBet)
            {
                _gameData.MaxBet();
                TryStartRound();
            }
        }
    }

    void OnSpecialModeOver()
    {
        if (EndSpecialModeHandler != null)
        {
            EndSpecialModeHandler(this);
        }

        _specialMode = SpecialMode.Normal;
    }

    public float GetSpecialModeEffectDuringTime()
    {
        float result = 0;
        if (_specialMode == SpecialMode.DoubleWin && _gameData.WinType == WinType.Normal)
        {
            result = 1.4f;
        }

        return result;
    }

    void PlaySpecialModeEffect(PuzzleMachine machine)
    {
        if (_specialMode == SpecialMode.DoubleWin &&
             _coreMachine.SpinResult.IsWinWithNonZeroRatio() && 
            DuringSpecialModeHandler != null)
        {
            DuringSpecialModeHandler(this);
            UnityTimer.Start(this, GetSpecialModeEffectDuringTime(), CheckAddWinAmountOnRoundEnd);
        }
    }

    #endregion

	#region Utility

	List<PuzzleSymbol> GetPuzzleSymbols(List<CoreSymbol> symbols)
	{
		List<PuzzleSymbol> result = new List<PuzzleSymbol>();
		ListUtility.ForEach(symbols, (CoreSymbol coreSymbol) => {
			PuzzleSymbol puzzleSymbol = _reelList[coreSymbol.ReelIndex].GetPuzzleSymbol(coreSymbol.StopIndex);
			result.Add(puzzleSymbol);
		});
		return result;
	}

	#endregion

	#region BGM

	public void ResumeBGM(){
		AudioManager.Instance.PlaySoundBGM(_basicConfig.MachineBGM);
	}

	public void StopBGM(){
		AudioManager.Instance.StopSoundBGM(_basicConfig.MachineBGM);
	}

	#endregion

    #region Test

    private void Test(){
		#if DEBUG
		// StartCoroutine (TestCoroutine());
		string id = "4000000";
		AnalysisManager.Instance.LocalAndroidPush(id);
		#endif
	}

	private IEnumerator TestCoroutine(){
		#if DEBUG
		while (true) {
			yield return new WaitForSeconds (15);
			TestBankruptCompensate ();
		}
		yield return null;
		#else
		yield return null;
		#endif
	}

	private void TestBankruptCompensate(){
		#if DEBUG
		System.Random random = new System.Random ();
		int credits = random.Next (1000, 99999);
		GameObject obj = UIManager.Instance.OpenBankruptCreditsUI (credits);
		BankruptCompensateBehaviour behaviour = obj.GetComponent<BankruptCompensateBehaviour> ();
		if (behaviour != null) {
			behaviour.InitUI (credits);
			behaviour._collectionAction = () => {
				behaviour.CollectCoins();
			};
		}
		#endif
	}

	#endregion
}

