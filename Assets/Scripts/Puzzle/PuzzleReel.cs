using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using CitrusFramework;
using DG.Tweening;
using System;
using UnityEngine.Events;

public delegate void ReelHaltCallback(PuzzleReel reel);

internal enum ReelSpinStage
{
//	Pre,
	Faster,
	Constant,
	Slower,
	Neighbor,
//	Post
}

internal enum ReelSpinMode
{
	Normal,
	SlideToNeighbor
}

internal class ReelSpinParam
{
	private int _haltIndex;
	private int _replaceIndex; //should be _haltIndex - 1
	private bool _isReplaced;
	private bool _shouldHalt;
	private float _unitTime;
	private ReelHaltCallback _haltCallback;
	private ReelSpinStage _stage;
	private int _totalSpinnedStops;
	private int _slowerSpinnedStops;
	private float _curSpeed;
	private ReelSpinMode _mode;
	private bool _isHyping;
	private List<int> _avoidStopIndexes;

	public int HaltIndex { get { return _haltIndex; } }
	public int ReplaceIndex { get { return _replaceIndex; } }
	public bool IsReplaced { get { return _isReplaced; } set { _isReplaced = value; } }
	public bool ShouldHalt { get { return _shouldHalt; } set { _shouldHalt = value; } }
	public float UnitTime { get { return _unitTime; } set { _unitTime = value; } }
	public ReelHaltCallback HaltCallback { get { return _haltCallback; } set { _haltCallback = value; } }
	public ReelSpinStage Stage { get { return _stage; } set { _stage = value; } }
	public int TotalSpinnedStops { get { return _totalSpinnedStops; } }
	public int SlowerSpinnedStops { get { return _slowerSpinnedStops; } }
	public float CurSpeed { get { return _curSpeed; } set { _curSpeed = value; } }
	public ReelSpinMode Mode { get { return _mode; } set { _mode = value; } }
	public bool IsHyping { get { return _isHyping; } set { _isHyping = value; } }
	public List<int> AvoidStopIndexes { get { return _avoidStopIndexes; } }

	public ReelSpinParam(int haltIndex, int replaceIndex, bool shouldHalt, float unitTime, ReelSpinMode mode, 
		ReelHaltCallback callback, ReelSpinStage initStage, List<int> avoidStopIndexes)
	{
		_haltIndex = haltIndex;
		_replaceIndex = replaceIndex;
		_isReplaced = false;
		_shouldHalt = shouldHalt;
		_unitTime = unitTime;
		_mode = mode;
		_haltCallback = callback;
		_stage = initStage;
		_totalSpinnedStops = 0;
		_slowerSpinnedStops = 0;
		_isHyping = false;
		_avoidStopIndexes = avoidStopIndexes;
	}

	public void IncreaseTotalSpinnedStops()
	{
		++_totalSpinnedStops;
	}

	public void IncreaseSlowerSpinnedStops()
	{
		++_slowerSpinnedStops;
	}
}

[System.Serializable]
public class DequeSymbol : Deque<PuzzleSymbol>
{
}

public class PuzzleReel : MonoBehaviour
{
	static readonly int _kAreaSymbolCountOfBasicReel = 7;
	static readonly int _kAreaSymbolCountOfSubordinateReel4 = 3;
	static readonly float _hypeSpeedUpFactor = 0.75f;
	static readonly string _spinTweenIdPrefix = "ReelSpin";

	public List<RectTransform> _pivotList = new List<RectTransform>();
	public bool _isSpinning = false;

	public GameObject _idleReelLightParent;
	public GameObject _spinReelLightParent;
	public GameObject _spinReelLightEndParent;
	public GameObject _normalWinFrameLightParent;
	public GameObject _bigWinFrameLightParent;
	public GameObject _hypeFrameLightParent;
	public GameObject _symbolDirectionParent;
	public GameObject _specialSymbolLightParent;

	private PuzzleReelSpinConfig _spinConfig;

	private PuzzleMachine _machine;
	private CoreReel _coreReel;
	private int _id;

	private int _areaSymbolCount;
	private int _areaMaxIndexOffset;

	private List<PuzzleSymbol> _allSymbolList = new List<PuzzleSymbol>();
	public List<Vector3> _positionList = new List<Vector3>();
	public DequeSymbol _areaSymbolList = new DequeSymbol();
	public List<int> _stopIndexOffsetList;
	private int _symbolCount;
	private Vector3 _unitDistance;
	private float _normalUnitTime;
	private SpinDirection _spinDirection = SpinDirection.None;
	private Transform _symbolParentTransform;

	private ReelSpinParam _spinParam;

	//todo: this dict can't be placed in _spinParam since _spinParam is null after halt
	//maybe place it in another new class like ReelSpinParam?
	private Dictionary<int, Coroutine> _hideReelSymbolEffectCoroutineDict = new Dictionary<int, Coroutine>();
	private Coroutine _SpecialSymbolLightEffectCoroutine = null;
	private Coroutine _SuperSymbolLightEffectCoroutine = null;

	private GameObject _SymbolUpReelEffect;
	private GameObject _SymbolDownReelEffect;
	private GameObject _frozenFadeInEffect;
	private GameObject _frozenFadeOutEffect;
	private GameObject _specialSymbolLightEffect;
	private GameObject _superSymbolLightEffect;

	private GameObject _dragonReelEffect;

	private UnityAction _haltSpecialSymbolEffectHandler = null;

	private SimpleMessageQueue _messageQueue = new SimpleMessageQueue();
	private string _spinTweenId;

	//getters
	public int ReelId { get { return _id; } }
	public int ReelIndex { get { return _id - 1; } }
	public bool IsSpinning { get { return _isSpinning; } set { _isSpinning = value; } }
	public PuzzleMachine Machine { get { return _machine; } }

	// Use this for initialization
	void Start () {
		#if DEBUG
		CitrusEventManager.instance.AddListener<PuzzleConfigUpdateEvent>(UpdatePuzzleConfig);
		#endif
	}

	private void UpdatePuzzleConfig(PuzzleConfigUpdateEvent message){
		UpdateNormalUnitTime();

		if(PuzzleUtility.IsSubordinateReel4(_machine.MachineConfig, _id))
		{
			_normalUnitTime *= 2.0f;
		}

		float symbolUnitDistanceFactor = _machine.MachineConfig.BasicConfig.SymbolUnitDistanceFactor;
		if(symbolUnitDistanceFactor != 1.0f && symbolUnitDistanceFactor != 0.0f)
		{
			_normalUnitTime *= symbolUnitDistanceFactor;
		}
	}

	#region Init

	public void Init(PuzzleMachine machine, CoreReel coreReel)
	{
		_machine = machine;
		_spinConfig = machine.PuzzleReelSpinConfig;
		_coreReel = coreReel;
		_id = _coreReel.Id;

		if(PuzzleUtility.IsSubordinateReel4(_machine.MachineConfig, _id))
			_areaSymbolCount = _kAreaSymbolCountOfSubordinateReel4;
		else
			_areaSymbolCount = _kAreaSymbolCountOfBasicReel;

		_areaMaxIndexOffset = (_areaSymbolCount - 1) / 2;

		_symbolCount = _coreReel.SymbolCount();

		UpdateNormalUnitTime();

		_symbolParentTransform = _pivotList[0].transform.parent;

		_spinTweenId = _spinTweenIdPrefix + _id.ToString();

		InitPositionList();
		RemovePivotSymbols();
		InitAllSymbolList();

		InitStopIndexOffsetList();
		InitActiveSymbolList();

		InitReelHeight();
		InitEffects();
	}

	private void UpdateNormalUnitTime(){
		float spinSpeed = _machine.MachineConfig.BasicConfig.SpinSpeed;
		if (spinSpeed > 0.0f){
			_normalUnitTime = 1.0f / spinSpeed;
		}else{
			_normalUnitTime = 1.0f / (float)_spinConfig._spinSpeed;
		}
	}

	private void InitPositionList()
	{
		Debug.Assert(_pivotList.Count == 3);

		Vector3 delta = _pivotList[1].localPosition - _pivotList[0].localPosition;
		_unitDistance = new Vector3(delta.x, delta.y * 0.5f, delta.z);

		//for reel4, the distance should be twice since there is no blank symbol
		if(PuzzleUtility.IsSubordinateReel4(_machine.MachineConfig, _id))
		{
			_unitDistance.y *= 2.0f;
			_normalUnitTime *= 2.0f;
		}

		//for multiLine or jackpot, the distance should be shorter
		float symbolUnitDistanceFactor = _machine.MachineConfig.BasicConfig.SymbolUnitDistanceFactor;
		if(symbolUnitDistanceFactor != 1.0f && symbolUnitDistanceFactor != 0.0f)
		{
			_unitDistance.y *= symbolUnitDistanceFactor;
			_normalUnitTime *= symbolUnitDistanceFactor;
		}

//		if (_machine.MachineConfig.BasicConfig.HasJackpot) {
//			_unitDistance.y *= 0.8f;
//		}

		Vector3 initPos = _pivotList[1].localPosition - _unitDistance * (_areaSymbolCount - 1) / 2;
		for(int i = 0; i < _areaSymbolCount; i++)
		{
			Vector3 p = initPos + i * _unitDistance;
			_positionList.Add(p);
		}
	}

	private void RemovePivotSymbols()
	{
		for(int i = 0; i < _pivotList.Count; i++)
		{
			RectTransform t = _pivotList[i];
			GameObject.Destroy(t.gameObject);
		}
	}

	private void InitAllSymbolList()
	{
		GameObject symbolPrefab = AssetManager.Instance.LoadAsset<GameObject>("Game/PuzzleSymbol");

		int count = _coreReel.SymbolCount();
		for(int i = 0; i < count; i++)
		{
			CoreSymbol coreSymbol = _coreReel.SymbolList[i];

			GameObject obj = GameObject.Instantiate(symbolPrefab) as GameObject;
			PuzzleSymbol puzzleSymbol = obj.GetComponent<PuzzleSymbol>();
			InitSingleSymbol(obj);
			_allSymbolList.Add(puzzleSymbol);
			puzzleSymbol.Init(coreSymbol, this);
		}
	}

	private void InitSingleSymbol(GameObject obj)
	{
		obj.transform.SetParent(_symbolParentTransform);
		#if true // 调换在父节点下的位置，这样能保证先初始化的symbol会盖住后初始化的symbol
		obj.transform.SetAsFirstSibling();
		#endif
		obj.transform.localPosition = _positionList[2];
		obj.transform.localScale = Vector3.one;
		if(PuzzleUtility.IsSubordinateReel4(_machine.MachineConfig, _id))
			obj.transform.localScale = new Vector3(PuzzleDefine.Reel4Scale, PuzzleDefine.Reel4Scale, 1.0f);
		obj.SetActive(false);
	}

	private void InitStopIndexOffsetList()
	{
		_stopIndexOffsetList = ListUtility.CreateIntList(-_areaMaxIndexOffset, _areaMaxIndexOffset + 1);
	}

	private void InitActiveSymbolList()
	{
		int initStopIndex = _machine.CoreMachine.MachineConfig.BasicConfig.ReelStartPos[_coreReel.Id - 1] - 1;
		for(int i = 0; i < _stopIndexOffsetList.Count; i++)
		{
			int offset = _stopIndexOffsetList[i];
			int index = initStopIndex + offset;
			PuzzleSymbol symbol = GetPuzzleSymbol(index);
			AddAreaSymbolToTail(symbol);
		}
	}

	private void InitReelHeight()
	{
		float factor = _machine._reelFrame._reelBackHeightFactor;
		//Debug.Log("f:" + factor.ToString());
		RectTransform transform = gameObject.GetComponent<RectTransform>();
		Vector2 sizeDelta = transform.sizeDelta;
		//Debug.Log("sizeDelta:" + sizeDelta.ToString());
		transform.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y * factor);
	}

	private void InitEffects()
	{
		string machineName = _machine.CoreMachine.Name;
		string path;

		//idleReelLight
		string idleReelLightName = PuzzleEffectConfig.Get_FX_Machine_Idle_ReelLight(machineName);
		path = GetEffectPath(idleReelLightName);
		GameObject idleReelLightPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		//it might be null
		if(idleReelLightPrefab != null)
		{
			GameObject idleReelLightObj = Instantiate(idleReelLightPrefab, _idleReelLightParent.transform);
			idleReelLightObj.transform.localPosition = Vector3.zero;
			idleReelLightObj.transform.localScale = Vector3.one;
		}

		//spinReelLight
		string spinReelLightName = PuzzleEffectConfig.Get_FX_Machine_Spin_ReelLight(machineName);
		path = GetEffectPath(spinReelLightName);
		GameObject spinReelLightPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		//it might be null
		if(spinReelLightPrefab != null)
		{
			GameObject spinReelLightObj = Instantiate(spinReelLightPrefab, _spinReelLightParent.transform);
			spinReelLightObj.transform.localPosition = Vector3.zero;
			spinReelLightObj.transform.localScale = Vector3.one;
		}

		//spinReelLightEnd
		string spinReelLightEndName = PuzzleEffectConfig.Get_FX_Machine_Spin_ReelLightEnd(machineName);
		path = GetEffectPath(spinReelLightEndName);
		GameObject spinReelLightEndPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		//it might be null
		if(spinReelLightEndPrefab != null)
		{
			GameObject spinReelLightEndObj = Instantiate(spinReelLightEndPrefab, _spinReelLightEndParent.transform);
			spinReelLightEndObj.transform.localPosition = Vector3.zero;
			spinReelLightEndObj.transform.localScale = Vector3.one;
		}

		//normalWinFrameLight
		_normalWinFrameLightParent.SetActive(false);
		string normalWinFrameLightName = PuzzleEffectConfig.Get_FX_Machine_LowWin_FrameLight(machineName);
		path = GetEffectPath(normalWinFrameLightName);
		GameObject normalWinFrameLightPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		if(normalWinFrameLightPrefab != null && CheckHighLowWinEnable())
		{
			GameObject normalWinFrameLightObj = Instantiate(normalWinFrameLightPrefab, _normalWinFrameLightParent.transform);
			normalWinFrameLightObj.transform.localPosition = Vector3.zero;
			normalWinFrameLightObj.transform.localScale = Vector3.one;
		}

		//bigWinFrameLight
		_bigWinFrameLightParent.SetActive(false);
		string bigWinFrameLightName = PuzzleEffectConfig.Get_FX_Machine_HighWin_FrameLight(machineName);
		path = GetEffectPath(bigWinFrameLightName);
		GameObject bigWinFrameLightPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
		if(bigWinFrameLightPrefab != null && CheckHighLowWinEnable())
		{
			GameObject bigWinFrameLightObj = Instantiate(bigWinFrameLightPrefab, _bigWinFrameLightParent.transform);
			bigWinFrameLightObj.transform.localPosition = Vector3.zero;
			bigWinFrameLightObj.transform.localScale = Vector3.one;
		}

		//hypeFrameLight
		_hypeFrameLightParent.SetActive(false);
		//only one reel needs hype effect
		if(IsHypeReel())
		{
			string hypeFrameLightName = PuzzleEffectConfig.Get_FX_Machine_Hype_FrameLight(machineName);
			path = GetEffectPath(hypeFrameLightName);
			GameObject hypeFrameLightPrefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, _machine.MachineName);
			if(hypeFrameLightPrefab != null)
			{
				GameObject hypeFrameLightObj = Instantiate(hypeFrameLightPrefab, _hypeFrameLightParent.transform);
				hypeFrameLightObj.transform.localPosition = Vector3.zero;
				hypeFrameLightObj.transform.localScale = Vector3.one;
			}
		}

		StartIdleReelLightEffect();

		_spinReelLightEndParent.SetActive(false);

		// symbol direction effect
		string symbolUp = PuzzleEffectConfig.Get_FX_Machine_Symbol_Up_Reel(machineName);
		path = GetEffectPath (symbolUp);
		GameObject symbolUpPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
		if (symbolUpPrefab != null) {
			_SymbolUpReelEffect = Instantiate (symbolUpPrefab, _symbolDirectionParent.transform);
			_SymbolUpReelEffect.transform.localPosition = Vector3.zero;
			_SymbolUpReelEffect.transform.localScale = Vector3.one;
			_SymbolUpReelEffect.SetActive (false);
		}

		string symbolDown = PuzzleEffectConfig.Get_FX_Machine_Symbol_Down_Reel(machineName);
		path = GetEffectPath (symbolDown);
		GameObject symbolDownPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
		if (symbolDownPrefab != null) {
			_SymbolDownReelEffect = Instantiate (symbolDownPrefab, _symbolDirectionParent.transform);
			_SymbolDownReelEffect.transform.localPosition = Vector3.zero;
			_SymbolDownReelEffect.transform.localScale = Vector3.one;
			_SymbolDownReelEffect.SetActive (false);
		}

		// symbol frozen effect
		string frozenEffect = _machine.MachineConfig.BasicConfig.FrozenFadeInEffect;
		if (!string.IsNullOrEmpty (frozenEffect)) {
			path = GetEffectPath (frozenEffect);
			GameObject frozenFadeInPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
			if (frozenFadeInPrefab != null) {
				_frozenFadeInEffect = Instantiate (frozenFadeInPrefab, _symbolDirectionParent.transform);
				_frozenFadeInEffect.transform.localPosition = Vector3.zero;
				_frozenFadeInEffect.transform.localScale = Vector3.one;
				_frozenFadeInEffect.SetActive (false);
			}
		}

		frozenEffect = _machine.MachineConfig.BasicConfig.FrozenFadeOutEffect;
		if (!string.IsNullOrEmpty (frozenEffect)) {
			path = GetEffectPath (frozenEffect);
			GameObject frozenFadeOutPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
			if (frozenFadeOutPrefab != null) {
				_frozenFadeOutEffect = Instantiate (frozenFadeOutPrefab, _symbolDirectionParent.transform);
				_frozenFadeOutEffect.transform.localPosition = Vector3.zero;
				_frozenFadeOutEffect.transform.localScale = Vector3.one;
				_frozenFadeOutEffect.SetActive (false);
			}
		}

		string specialSymbolLightEffectPath = _machine.MachineConfig.BasicConfig.SpecialSymbolLightEffect;
		if (!string.IsNullOrEmpty (specialSymbolLightEffectPath)) {
			if (_machine.MachineConfig.BasicConfig.EnableSpecialSymbolEffectIndex){
				path = PuzzleEffectConfig.MachineEffectPath + specialSymbolLightEffectPath + _id.ToString();
			}else{
				path = PuzzleEffectConfig.MachineEffectPath + specialSymbolLightEffectPath;
			}
			GameObject effectPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
			if (effectPrefab != null) {
				_specialSymbolLightEffect = UGUIUtility.CreateObj (effectPrefab, _specialSymbolLightParent);
				_specialSymbolLightEffect.SetActive (false);
			}
		}

		string superSymbolLightEffectPath = _machine.MachineConfig.BasicConfig.SuperSymbolLightEffect;
		if (!string.IsNullOrEmpty (superSymbolLightEffectPath)) {
			path = PuzzleEffectConfig.MachineEffectPath + superSymbolLightEffectPath;
			GameObject effectPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
			if(effectPrefab != null)
			{
				_superSymbolLightEffect = UGUIUtility.CreateObj(effectPrefab, _specialSymbolLightParent);
				_superSymbolLightEffect.SetActive(false);
			}
			else
			{
				Debug.LogError("Fail to load SuperSymbolLightEffect: " + superSymbolLightEffectPath);
			}
		}

		string dragonReelEffect = _machine.MachineConfig.BasicConfig.DragonMachineReelEffect;
		if (!dragonReelEffect.IsNullOrEmpty ()) {
			path = PuzzleEffectConfig.MachineEffectPath + dragonReelEffect;
			GameObject effectPrefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, _machine.MachineName);
			if (effectPrefab != null) {
				_dragonReelEffect = UGUIUtility.CreateObj (effectPrefab, _specialSymbolLightParent);
				_dragonReelEffect.SetActive (false);
			}
		}
	}

	private string GetEffectPath(string name)
	{
		string machineName = _machine.CoreMachine.Name;
		string path = PuzzleEffectConfig.MachineEffectPath + machineName + "/" + name;
		if(PuzzleUtility.IsSubordinateReel4(_machine.MachineConfig, _id))
			path = PuzzleUtility.GetReel4EffectName(path);
		return path;
	}

	private bool CheckHighLowWinEnable(){
		BasicConfig config = _machine.MachineConfig.BasicConfig;
		// 判断是否只需要中间卷轴加载 highwin（因为特效是做成全屏的）
		if (config.IsOnlyMiddleReelHighLowWinEffect){
			return _id == (int) Math.Ceiling((float) config.ReelCount/ 2 );
		}
		return true;
	}

	void OnDestroy()
	{
		DOTween.Kill(_spinTweenId);
		#if DEBUG
		CitrusEventManager.instance.RemoveListener<PuzzleConfigUpdateEvent>(UpdatePuzzleConfig);
		#endif
	}

	#endregion

	#region Public

	public void Spin(int haltIndex, SpinDirection dir, float expectTime, ReelHaltCallback haltCallback)
	{
		_messageQueue.Clear();

		_isSpinning = true;
		_spinDirection = dir;

		int replaceIndex = GetStartReplaceHaltIndex(haltIndex);
		List<int> avoidStopIndexes = GetAvoidStopIndexes(replaceIndex, dir);
		_spinParam = new ReelSpinParam(haltIndex, replaceIndex, false, _normalUnitTime, ReelSpinMode.Normal, 
			haltCallback, ReelSpinStage.Faster, avoidStopIndexes);
		
		SpinOneUnit();

		UnityTimer.Start(this, expectTime, SetShouldHalt);

		StartSpinEffect(expectTime);
		HaltSpecialSymbolEffect ();
	}

	public void SpinToNeighbor(int haltIndex, SpinDirection dir, ReelHaltCallback haltCallback)
	{
		_messageQueue.Clear();

		_isSpinning = true;
		_spinDirection = dir;

		float spinNeighborTime = _machine.PuzzleReelSpinConfig._spinNeighorTime;
		_spinParam = new ReelSpinParam(haltIndex, CoreDefine.InvalidIndex, true, spinNeighborTime, ReelSpinMode.SlideToNeighbor, 
			haltCallback, ReelSpinStage.Neighbor, new List<int>());

		SpinOneUnit();

		ShowSymbolSlideEffect (dir);
	}

	List<int> GetAvoidStopIndexes(int replaceIndex, SpinDirection dir)
	{
		List<int> result = new List<int>();
		SpinDirection revDir = CoreUtility.ReverseDirection(dir);
		int curIndex = replaceIndex;
		for(int i = 0; i < _areaSymbolCount; i++)
		{
			result.Add(curIndex);
			curIndex = _coreReel.ReelConfig.GetNeighborStopIndex(curIndex, revDir);
		}
		return result;
	}

	public void EndLastSpinEffect()
	{
		//nothing
	}

	public void SetShouldHalt()
	{
		if(_spinParam != null)
			_spinParam.ShouldHalt = true;
	}

	public void MachineEndSpin()
	{
		HideSpinReelLightEffect();
		ShowReelLightEndEffect();
	}

	public void FixReel()
	{
		//hide symbol win effect?
		//HideSymbolWinEffect();
	}

	public PuzzleSymbol GetOffsetMidAreaSymbol(int offset)
	{
		//input SpinDirection.Up to avoid negating offset inside
		PuzzleSymbol symbol = GetOffsetMidAreaSymbolWithDirection(offset, SpinDirection.Up);
		return symbol;
	}

	public bool IsHypeReel()
	{
		bool result = _machine.MachineConfig.BasicConfig.HasHype
		              && _machine.MachineConfig.BasicConfig.HypeReelIds.Contains(_coreReel.Id);
		return result;
	}

	public bool IsHyping()
	{
		bool result = false;
		if(_spinParam != null)
			result = _spinParam.IsHyping;
		return result;
	}

	public int GetMaxVisibleSymbolOffset()
	{
		int result = 0;
		if(PuzzleUtility.IsSubordinateReel4(_machine.MachineConfig, _id))
			result = PuzzleDefine.MaxVisibleSymbolOffsetOfSubordinateReel4;
		else
			result = PuzzleDefine.MaxVisibleSymbolOffsetOfBasicReels;
		return result;
	}

	#endregion

	#region Spin and halt

	void Update()
	{
		_messageQueue.Run();
	}

	private void SpinOneUnit()
	{
		Vector3 unitDistance = (_spinDirection == SpinDirection.Down) ? (-_unitDistance) : _unitDistance;

		float baseTime = _spinParam.UnitTime;
		float preTime = 0.0f;
		float postTime = 0.0f;
		float speedFactor = 1.0f;

		for(int i = 0; i < _areaSymbolCount; i++)
		{
			bool isLastSymbol = (i == _areaSymbolCount - 1);
			PuzzleSymbol symbol = _areaSymbolList[i];
			Transform transform = symbol.gameObject.transform;
			Sequence seq = DOTween.Sequence().SetId(_spinTweenId);

			if(_spinParam.Stage == ReelSpinStage.Faster)
			{
				if(_spinParam.TotalSpinnedStops == 0)
				{
					//Critical fix by nichos:
					//Without this if check, the bug might happen: the LocalMove might fail and two non-blank
					//symbols are next to each other
					if(preTime > 0.0f)
					{
						Vector3 prePos = symbol.transform.localPosition - unitDistance * _spinConfig._startSpinOffsetFactor;
						Tweener preMove = transform.DOLocalMove(prePos, preTime).SetEase(Ease.OutBack);
						seq.Append(preMove);
					}

					//only call once for the symbols in this for loop
					if(isLastSymbol)
					{
						seq.PrependCallback(() => {
							_messageQueue.Add(new SimpleMessage(StartSpinBlurGlassEffect));
						});
					}
				}

				Debug.Assert(_spinParam.TotalSpinnedStops < _spinConfig._fasterSpeedFactors.Count);
				speedFactor = _spinConfig._fasterSpeedFactors[_spinParam.TotalSpinnedStops];
				baseTime = _spinParam.UnitTime / speedFactor;

				Vector3 endPos = symbol.transform.localPosition + unitDistance;
				Tweener move = transform.DOLocalMove(endPos, baseTime).SetEase(Ease.Linear);
				seq.Append(move);
			}
			else if(_spinParam.Stage == ReelSpinStage.Constant)
			{
				Vector3 endPos = symbol.transform.localPosition + unitDistance;
				Tweener move = transform.DOLocalMove(endPos, baseTime).SetEase(Ease.Linear);
				seq.Append(move);
			}
			else if(_spinParam.Stage == ReelSpinStage.Slower)
			{
				Debug.Assert(_spinParam.SlowerSpinnedStops <= _areaMaxIndexOffset);
				speedFactor = _spinConfig._slowerSpeedFactors[_spinParam.SlowerSpinnedStops];
				baseTime = _spinParam.UnitTime / speedFactor;

				if(_spinParam.SlowerSpinnedStops < _areaMaxIndexOffset - 1)
				{
					Vector3 postPos = symbol.transform.localPosition + unitDistance;
					Tweener move = transform.DOLocalMove(postPos, baseTime).SetEase(Ease.Linear);
					seq.Append(move);
				}
				else
				{
					float offsetFactor = _spinConfig._endSpinOffsetFactors[0];
					Vector3 postPos = symbol.transform.localPosition + unitDistance * (1 + offsetFactor);
					Tweener move = transform.DOLocalMove(postPos, baseTime).SetEase(Ease.Linear);
					seq.Append(move);

					//only call once for the symbols in this for loop
					if(isLastSymbol)
					{
						seq.AppendCallback(() => {
							_messageQueue.Add(new SimpleMessage(SpinToLastSymbolCallback));
						});
					}

					//post action
					for(int k = 1; k < _spinConfig._endSpinOffsetFactors.Count; k++)
					{
						float time = _spinConfig._endSpinTimes[k - 1];
						offsetFactor = _spinConfig._endSpinOffsetFactors[k];
						postPos = symbol.transform.localPosition + unitDistance * (1 + offsetFactor);
						move = transform.DOLocalMove(postPos, time).SetEase(Ease.Linear);
						seq.Append(move);
					}

					float lastTime = _spinConfig._endSpinTimes[_spinConfig._endSpinTimes.Count - 1];
					Vector3 endPos = symbol.transform.localPosition + unitDistance;
					Tweener postMove = transform.DOLocalMove(endPos, lastTime).SetEase(Ease.Linear);
					seq.Append(postMove);

					postTime = ListUtility.FoldList(_spinConfig._endSpinTimes, MathUtility.Add);
				}
			}
			else if(_spinParam.Stage == ReelSpinStage.Neighbor)
			{
				Vector3 endPos = symbol.transform.localPosition + unitDistance;
				Tweener move = transform.DOLocalMove(endPos, baseTime).SetEase(Ease.InOutBack);
				seq.Append(move);
			}

			//only call once for the symbols in this for loop
			if(isLastSymbol)
			{
				seq.AppendCallback(() => {
					_messageQueue.Add(new SimpleMessage(SpinUnitEndCallback));
				});
			}
		}

//		float totalTime = baseTime + preTime + postTime;
//		UnityTimer.Start(this, totalTime, SpinUnitEndCallback);
	}

	private void SpinToLastSymbolCallback()
	{
		//Now the symbol blur effects are stopped much earlier. Check StepSingleAreaSymbol()
		//EndSpinBlurGlassEffect();

		//play the sound here, a little earlier before DoHalt
		PlayStopReelSymbolSoundAndSuperSymbolLight();
	}

	private void SpinUnitEndCallback()
	{
		// Note by nichos:
		// This null check is a patch just trying to avoid the crash GetNeighborStopIndexOutsideAvoidList
		// for unknown reason
		if(_spinParam != null)
		{
			StepSingleAreaSymbol();

			IncreaseSpinnedStops();

			CheckSetSpinStage();

			CheckHaltSpin();
		}
	}

	private void IncreaseSpinnedStops()
	{
		if(_spinParam != null)
		{
			_spinParam.IncreaseTotalSpinnedStops();

			if(_spinParam.Stage == ReelSpinStage.Slower)
				_spinParam.IncreaseSlowerSpinnedStops();
		}
		else
		{
			Debug.Assert(false);
		}
	}

	private void CheckSetSpinStage()
	{
		if(_spinParam.Stage == ReelSpinStage.Faster && _spinParam.TotalSpinnedStops >= _spinConfig._fasterSpeedFactors.Count)
		{
			_spinParam.Stage = ReelSpinStage.Constant;
		}
		else if(_spinParam.Stage == ReelSpinStage.Constant && IsReadyToHalt())
		{
			SpinDirection reverseDir = CoreUtility.ReverseDirection(_spinDirection);
			PuzzleSymbol offsetSymbol = GetOffsetMidAreaSymbolWithDirection(_areaMaxIndexOffset, reverseDir);
			bool shouldSlower = offsetSymbol.CoreSymbol.StopIndex == _spinParam.HaltIndex;
			if(shouldSlower)
			{
				_spinParam.Stage = ReelSpinStage.Slower;
			}
		}
	}

	private void CheckHaltSpin()
	{
		bool shouldHalt = false;
		if(IsReadyToHalt())
		{
			int curStopIndex = GetCurrentStopIndex();
			ReelSpinMode mode = _spinParam.Mode;
			if(mode == ReelSpinMode.Normal)
				shouldHalt = (curStopIndex == _spinParam.HaltIndex && _spinParam.Stage == ReelSpinStage.Slower);
			else if(mode == ReelSpinMode.SlideToNeighbor)
				shouldHalt = curStopIndex == _spinParam.HaltIndex;
			else
				Debug.Assert(false);
		}

		if(shouldHalt)
			DoHalt();
		else
			SpinOneUnit();
	}

	private void DoHalt()
	{
		_isSpinning = false;

		EndSpinBlurGlassEffect();
		PlayStopReelSymbolEffect();

		if(_spinParam.HaltCallback != null)
			_spinParam.HaltCallback(this);

		_spinParam = null;

		//this method should be the last called one. It would start a new spin in auto-mode and assign _spinParam
		_machine.ReelHaltSpinCallback(this);
	}

	//Important note: this function is called a little before SpinUnitEndCallback. This means some data
	//such as _areaSymbolList is not prepared at this point. So be careful with this.
	private void PlayStopReelSymbolSoundAndSuperSymbolLight()
	{
		AudioType audioType = AudioType.StopReel;

		if(_id == 1 || _id == 2)
		{
			CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
			SymbolConfig symbolConfig = _machine.CoreMachine.MachineConfig.SymbolConfig;
			BasicConfig basicConfig = _machine.CoreMachine.MachineConfig.BasicConfig;
			bool isAllSpecial = true;

			bool isSpecialMachine = PuzzleUtility.IsSpecialMachine(_machine.CoreMachine);
			bool[] isSpecialStrongSymbols = new bool[_id];// 强特别元素(如Bonus， jackpot)
			bool[] isSpecialNormalSymbols = new bool[_id];// 普通特别元素（如hight7， wild）
			bool isMultiLine = _machine.MachineConfig.BasicConfig.IsMultiLine;

			Predicate<PuzzleSymbol> symbolPred = null;
			if(isMultiLine)
			{
				symbolPred = (PuzzleSymbol s) => {
					bool r = PuzzleUtility.CanPlaySpecialSymbolSound(symbolConfig, s.CoreSymbol.SymbolData);
					return r;
				};
			}

			for(int i = 0; i < _id; i++)
			{
				bool isSpecial = false;
				SingleReel singleReel = _machine.MachineConfig.ReelConfig.GetSingleReel(i);

				if (isMultiLine) 
				{
					Debug.Assert (symbolPred != null);
					List<int> visibleIndexes = singleReel.GetVisibleStopIndexes (spinResult.StopIndexes [i]);
					List<PuzzleSymbol> visibleSymbols = ListUtility.MapList (visibleIndexes, (int index) => {
						return _machine.ReelList [i]._allSymbolList [index];
					});

					isSpecial = ListUtility.IsAnyElementSatisfied (visibleSymbols, symbolPred);
				} 
				else if (isSpecialMachine) 
				{
					List<int> visibleStopIndexes = new List<int>();

					if (basicConfig.IsPaylineStrongSpecialEffect) {
						visibleStopIndexes.Add (spinResult.StopIndexes[i]);
					} else {
						visibleStopIndexes.AddRange (singleReel.GetVisibleStopIndexes (spinResult.StopIndexes [i]));
					}

					// 是否存在强元素
					bool strong = ListUtility.IsAnyElementSatisfied (visibleStopIndexes, (int stopindex) => {
						return PuzzleUtility.CanPlaySpecialStrongSymbolSound(symbolConfig, _machine.ReelList[i]._allSymbolList[stopindex].CoreSymbol.SymbolData);
					});
					isSpecialStrongSymbols[i] = strong;

					// 支付线上是否存在普通特殊元素
					CoreSymbol symbol = spinResult.SymbolList[i];
					bool normal = PuzzleUtility.CanPlaySpecialNormalSymbolSound(symbolConfig, symbol.SymbolData);
					isSpecialNormalSymbols[i] = normal;

					isSpecial = strong || normal;
				}
				else
				{
					CoreSymbol symbol = spinResult.SymbolList[i];
//					Debug.Log ("normal symbol name = "+ symbol.SymbolData.Name + " type = " + symbol.SymbolData.SymbolType);
					isSpecial = PuzzleUtility.CanPlaySpecialSymbolSound(symbolConfig, symbol.SymbolData);
				}

				if (isSpecialMachine) {
					// 第二次触发需要判断是否都是强特别元素或者普通特别元素
					if (i == _id - 1) {
						bool allStrong = ListUtility.IsAllElementsSatisfied (isSpecialStrongSymbols, (bool b) => {
							return b == true;
						});

						bool allNormal = ListUtility.IsAllElementsSatisfied (isSpecialNormalSymbols, (bool b) => {
							return b == true;
						});

						isAllSpecial &= (allStrong || allNormal);
					} else {
						isAllSpecial &= isSpecial;
					}
				} else {
					isAllSpecial &= isSpecial;
				}
			}

			bool[] matchMasks = new bool[_machine.MachineConfig.BasicConfig.BasicReelCount];
			for(int i = 0; i < _id; i++)
				matchMasks[i] = true;
			
			bool shouldPlay = false;
			if(isSpecialMachine)
			{
				shouldPlay = isAllSpecial;
			}
			else if(isMultiLine)
			{
				//don't play sound for multiLine for now
				shouldPlay = false;
			}
			else
			{
				if(isAllSpecial && !isMultiLine)
				{
					PayoutData[] dataArray = _machine.CoreMachine.MachineConfig.PayoutConfig.Sheet.dataArray;
					List<int> stopIndexes = spinResult.StopIndexes;
					PayoutData matchData = _machine.CoreMachine.Checker.TryMatchPartJoyData(dataArray, stopIndexes, matchMasks, spinResult.JoyData);
					shouldPlay = matchData != null;
//					Debug.Log ("isAllSpecial shouoldPlay = "+shouldPlay);
				}
			}

			if(shouldPlay)
			{
				if(_id == 1)
					audioType = AudioType.StopReelSpecial1;
				else if(_id == 2)
					audioType = AudioType.StopReelSpecial2;
//				Debug.Log ("Show superSymbol Light Effect reelID = " + _id);
				ShowSuperSymbolLightEffect (true);
				_SuperSymbolLightEffectCoroutine = UnityTimer.Start (this, 1.0f, () => {
					ShowSuperSymbolLightEffect(false);
				});
			}
		}

		AudioManager.Instance.PlaySound(audioType);
	}

	private void PlayStopReelSymbolEffect()
	{
		if(_id == 1 || _id == 2)
		{
			CoreSpinResult spinResult = _machine.CoreMachine.SpinResult;
			SymbolConfig symbolConfig = _machine.CoreMachine.MachineConfig.SymbolConfig;
			BasicConfig basicConfig = _machine.CoreMachine.MachineConfig.BasicConfig;

			bool isSpecialMachine = PuzzleUtility.IsSpecialMachine(_machine.CoreMachine);
			bool isAllSpecial = true;
			if (isSpecialMachine)
			{
				// 需要闪烁的INDEX
				List<int> allIndexes = null;
				// 所有可见元素列表
				List<PuzzleSymbol> allVisibleSymbols = null;

				for (int i = 0; i < _id; ++i) {
					SingleReel singleReel = _machine.MachineConfig.ReelConfig.GetSingleReel(i);

					// 这里需要判断2种情况， 在支持unorder的玩法下，需要拿到可见范围内的所有symbol，但是普通支付线的玩法下，只要拿到支付线上的元素就够了
					// basicconfig里的isPaylineStrongSpecial就是用来判断这2种情况
					List<int> visibleIndexes = new List<int>();

					if (basicConfig.IsPaylineStrongSpecialEffect) {
						visibleIndexes.Add (spinResult.StopIndexes[i]);
					} else {
						visibleIndexes.AddRange (singleReel.GetVisibleStopIndexes(spinResult.StopIndexes[i]));
					}

					List<PuzzleSymbol> visibleSymbols = ListUtility.MapList(visibleIndexes, (int index) => {
						return _machine.ReelList[i]._allSymbolList[index];
					});

					Predicate<PuzzleSymbol> symbolPred = (PuzzleSymbol s) => {
						return PuzzleUtility.CanPlaySpecialSymbolEffect(symbolConfig, s.CoreSymbol.SymbolData, _machine.CoreMachine) && 
							PuzzleUtility.CheckSpecialSymbol(symbolConfig, s.CoreSymbol.SymbolData, _machine.CoreMachine, spinResult.StopIndexes[i] == s.CoreSymbol.StopIndex);
					};
					List<int> symbolIndexes = ListUtility.FindAllIndexes(visibleSymbols, symbolPred);

					isAllSpecial &= symbolIndexes.Count > 0;

					if (i == _id - 1){
						allVisibleSymbols = visibleSymbols;
						allIndexes = symbolIndexes;
					}
				}

				if (isAllSpecial) {
					List<int> symbolIndexes = allIndexes;
					List<PuzzleSymbol> visibleSymbols = allVisibleSymbols;
					List<PuzzleSymbol> specialSymbols = new List<PuzzleSymbol> ();

					for(int i = 0; i < symbolIndexes.Count; i++)
					{
						int stopIndex = visibleSymbols[symbolIndexes[i]].CoreSymbol.StopIndex;
						if (stopIndex != CoreDefine.InvalidIndex)
						{
							#if false
							_allSymbolList[stopIndex].ShowWinEffect(WinType.Normal, false);
							#else
							_allSymbolList [stopIndex].ShowStrongSpecialHintEffect ();
							#endif
							specialSymbols.Add (_allSymbolList[stopIndex]);

							float delay = _allSymbolList [stopIndex].GetHideStrongSpecialEffectDelayTime ();
							Coroutine coroutine = UnityTimer.Start(this, delay, ()=>{
								#if false
								_allSymbolList[stopIndex].HideWin3DEffect();
								#else
								_allSymbolList[stopIndex].HideStrongSpecialHintEffect();
								#endif

								//remove coroutine
								if(_hideReelSymbolEffectCoroutineDict.ContainsKey(stopIndex))
									_hideReelSymbolEffectCoroutineDict.Remove(stopIndex);
							});
							_hideReelSymbolEffectCoroutineDict[stopIndex] = coroutine;
						}
					}

					_haltSpecialSymbolEffectHandler = () => {
						ListUtility.ForEach<PuzzleSymbol>(specialSymbols, (PuzzleSymbol symbol)=>{
							#if false
							symbol.HideWin3DEffect();
							#else
							symbol.HideStrongSpecialHintEffect();
							#endif
						});
						specialSymbols.Clear();
					};
					#if false
					// 强元素的reel特效
					ShowSpecialLightEffect (true);
					_SpecialSymbolLightEffectCoroutine = UnityTimer.Start (this, 0.5f, () => {
						ShowSpecialLightEffect (false);
					});
					#endif
				}
			}
		}
	}

	public void HaltSpecialSymbolEffect(){
		if (_haltSpecialSymbolEffectHandler != null) {
			_haltSpecialSymbolEffectHandler ();
			_haltSpecialSymbolEffectHandler = null;
		}
	}

	public void StopHideReelSymbolEffectCoroutines()
	{
		if(_hideReelSymbolEffectCoroutineDict.Count > 0)
		{
			foreach(var pair in _hideReelSymbolEffectCoroutineDict)
			{
				this.StopCoroutine(pair.Value);
			}
			_hideReelSymbolEffectCoroutineDict.Clear();
		}

		if (_SpecialSymbolLightEffectCoroutine != null) {
			this.StopCoroutine (_SpecialSymbolLightEffectCoroutine);
			_SpecialSymbolLightEffectCoroutine = null;
		}
		#if false
		if (_SuperSymbolLightEffectCoroutine != null) {
			this.StopCoroutine (_SuperSymbolLightEffectCoroutine);
			_SuperSymbolLightEffectCoroutine = null;
		}
		#endif
	}

	#endregion

	#region Utility

	private int RoundStopIndex(int index)
	{
		int result = index;
		//it assumes that index will never offset too much outside the range
		if(result < 0)
			result += _symbolCount;
		else if(result >= _symbolCount)
			result -= _symbolCount;
		return result;
	}

	public PuzzleSymbol GetPuzzleSymbol(int stopIndex)
	{
		stopIndex = RoundStopIndex(stopIndex);
		return _allSymbolList[stopIndex];
	}

	private PuzzleSymbol GetMidAreaSymbol()
	{
		int midIndex = _areaSymbolList.Count / 2;
		return _areaSymbolList[midIndex];
	}

	private PuzzleSymbol GetOffsetMidAreaSymbolWithDirection(int offset, SpinDirection dir)
	{
		int midIndex = _areaSymbolList.Count / 2;
		int dirOffset = (dir == SpinDirection.Down) ? -offset : offset;
		int index = midIndex + dirOffset;
		Debug.Assert(index >= 0 && index < _areaSymbolCount);
		return _areaSymbolList[index];
	}

//	private List<PuzzleSymbol> GetVisibleAreaSymbols()
//	{
//		PuzzleSymbol down = GetOffsetMidAreaSymbol(-1);
//		PuzzleSymbol mid = GetMidAreaSymbol();
//		PuzzleSymbol up = GetOffsetMidAreaSymbol(1);
//		List<PuzzleSymbol> result = new List<PuzzleSymbol>(){down, mid, up};
//		return result;
//	}

	private int GetCurrentStopIndex()
	{
		PuzzleSymbol midSymbol = GetMidAreaSymbol();
		return midSymbol.CoreSymbol.StopIndex;
	}

	private void AddAreaSymbolToTail(PuzzleSymbol symbol)
	{
		_areaSymbolList.AddLast(symbol);
		RefreshSingleAreaSymbolState(symbol);

		//Note: call this before calling StartBlurEffect().
		//This function will add some collect elements. And then blur the elements
		symbol.AddAreaSymbolHandler();
	}

	private void AddAreaSymbolToHead(PuzzleSymbol symbol)
	{
		_areaSymbolList.AddFirst(symbol);
		RefreshSingleAreaSymbolState(symbol);

		//Note: call this before calling StartBlurEffect().
		//This function will add some collect elements. And then blur the elements
		symbol.AddAreaSymbolHandler();
	}

	private void PopAreaSymbolFromTail()
	{
		PuzzleSymbol symbol = _areaSymbolList.PopLast();
		RefreshSingleAreaSymbolState(symbol);
		symbol.EndBlurEffect();

		symbol.PopAreaSymbolHandler ();
	}

	private void PopAreaSymbolFromHead()
	{
		PuzzleSymbol symbol = _areaSymbolList.PopFirst();
		RefreshSingleAreaSymbolState(symbol);
		symbol.EndBlurEffect();

		symbol.PopAreaSymbolHandler ();
	}

	PuzzleSymbol GetAreaSymbolFromTail(int offset)
	{
		Debug.Assert(offset >= 0 && offset < _areaSymbolList.Count);
		PuzzleSymbol symbol = _areaSymbolList[_areaSymbolList.Count - 1 - offset];
		return symbol;
	}

	PuzzleSymbol GetAreaSymbolFromHead(int offset)
	{
		Debug.Assert(offset >= 0 && offset < _areaSymbolList.Count);
		PuzzleSymbol symbol = _areaSymbolList[offset];
		return symbol;
	}

	private void RefreshSingleAreaSymbolState(PuzzleSymbol symbol)
	{
		int index = _areaSymbolList.IndexOf(symbol);
		if(index >= 0 && index < _areaSymbolList.Count)
		{
			symbol.gameObject.SetActive(true);
			symbol.gameObject.transform.localPosition = _positionList[index];
		}
		else
		{
			symbol.gameObject.SetActive(false);
		}
	}

	private void RefreshAllAreaSymbolState()
	{
		for(int i = 0; i < _areaSymbolList.Count; i++)
			RefreshSingleAreaSymbolState(_areaSymbolList[i]);
	}

	private int GetNextStopIndexForArea()
	{
		int nextIndex = CoreDefine.InvalidIndex;
		if(_spinDirection == SpinDirection.Down)
		{
			PuzzleSymbol symbol = _areaSymbolList.GetLast();
			int lastIndex = symbol.CoreSymbol.StopIndex;
			//Note### input SpinDirection.Up, Note another Note###
			nextIndex = GetNextStopIndex(lastIndex, SpinDirection.Up);
		}
		else if(_spinDirection == SpinDirection.Up)
		{
			PuzzleSymbol symbol = _areaSymbolList.GetFirst();
			int firstIndex = symbol.CoreSymbol.StopIndex;
			//Note### input SpinDirection.Down, Note another Note###
			nextIndex = GetNextStopIndex(firstIndex, SpinDirection.Down);
		}
		else
		{
			Debug.Assert(false);
		}

		nextIndex = RoundStopIndex(nextIndex);
		return nextIndex;
	}

	private bool CanReplaceSymbolIndex(int curIndex, int replaceIndex)
	{
		bool result = false;
		PuzzleSymbol curSymbol = GetPuzzleSymbol(curIndex);
		PuzzleSymbol replaceSymbol = GetPuzzleSymbol(replaceIndex);
		if(!_areaSymbolList.Contains(replaceSymbol))
		{
			//either they are all blank or they are all non-blank
			result = (curSymbol.CoreSymbol.SymbolData.SymbolType != SymbolType.Blank && replaceSymbol.CoreSymbol.SymbolData.SymbolType != SymbolType.Blank)
				|| (curSymbol.CoreSymbol.SymbolData.SymbolType == SymbolType.Blank && replaceSymbol.CoreSymbol.SymbolData.SymbolType == SymbolType.Blank);
		}
		return result;
	}

	private int GetNextStopIndex(int lastIndex, SpinDirection dir)
	{
		int result = CoreDefine.InvalidIndex;
		if(IsReadyToHalt())
		{
			if(_spinParam.IsReplaced)
			{
				result = _coreReel.ReelConfig.GetNeighborStopIndex(lastIndex, dir);
			}
			else
			{
				//try replace now
				int nextIndex = _coreReel.ReelConfig.GetNeighborStopIndex(lastIndex, dir);
				if(CanReplaceSymbolIndex(nextIndex, _spinParam.ReplaceIndex))
				{
					result = _spinParam.ReplaceIndex;
					_spinParam.IsReplaced = true;
				}
				else
				{
					result = nextIndex;
				}
			}
		}
		else
		{
			result = GetNeighborStopIndexOutsideAvoidList(lastIndex, dir);
		}
		return result;
	}

	private int GetNeighborStopIndexOutsideAvoidList(int lastIndex, SpinDirection dir)
	{
		int result = lastIndex;
		int nextIndex = _coreReel.ReelConfig.GetNeighborStopIndex(lastIndex, dir);
		
		int guardCount = 0;
		do
		{
			result = _coreReel.ReelConfig.GetNeighborStopIndex(result, dir);
			++guardCount;

			if(guardCount == _allSymbolList.Count)
			{
				Debug.Assert(false);
				break;
			}
		} while(_spinParam.AvoidStopIndexes.Contains(result)
			|| !CanReplaceSymbolIndex(nextIndex, result));

		return result;
	}

	#endregion

	#region Spin unit end

	private void StepSingleAreaSymbol()
	{
		int nextIndex = GetNextStopIndexForArea();
		PuzzleSymbol symbol = null;
		PuzzleSymbol neighborSymbol = null;
		if(_spinDirection == SpinDirection.Down)
		{
			PopAreaSymbolFromHead();
			symbol = GetPuzzleSymbol(nextIndex);
			AddAreaSymbolToTail(symbol);

			neighborSymbol = GetAreaSymbolFromTail(1);
		}
		else if(_spinDirection == SpinDirection.Up)
		{
			PopAreaSymbolFromTail();
			symbol = GetPuzzleSymbol(nextIndex);
			AddAreaSymbolToHead(symbol);

			neighborSymbol = GetAreaSymbolFromHead(1);
		}
		else
		{
			Debug.Assert(false);
		}

		//When replace happens:
		//1 don't start blur effect for new symbol
		//2 stop blur effect for neighbor symbol
		if(_spinParam.IsReplaced)
		{
			neighborSymbol.EndBlurEffect();
		}
		else
		{
			symbol.StartBlurEffect();
		}
	}

	private int GetStartReplaceHaltIndex(int haltIndex)
	{
		//Note: we should replace symbol from the previous symbol of HaltIndex. Otherwise, the player
		//could see the replace trick and know we are playing them

		//Note### Fix, Note another Note###
//		SpinDirection reverseDir = CoreUtility.ReverseDirection(_spinDirection);
//		int result = _coreReel.ReelConfig.GetNeighborStopIndex(haltIndex, reverseDir);
		int result = _coreReel.ReelConfig.GetNeighborStopIndex(haltIndex, _spinDirection);
		return result;
	}

	private bool IsReadyToHalt()
	{
		bool result = (_spinParam != null && _spinParam.ShouldHalt && IsPreviousReelHaltOrReadyToHalt());
		return result;
	}

	private bool IsPreviousReelHaltOrReadyToHalt()
	{
		bool result = true;
		for(int i = 0; i < ReelIndex; i++)
		{
			PuzzleReel reel = _machine.ReelList[i];
			if(!reel.IsHaltOrReadyToHalt())
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private bool IsHaltOrReadyToHalt()
	{
		bool result = false;
		if(!_isSpinning && _spinParam == null)
			result = true;
		else
			result = (_spinParam.Stage == ReelSpinStage.Slower || _spinParam.Stage == ReelSpinStage.Neighbor);

		return result;
	}

	#endregion

	#region Effect

	public void StartHypeDarkEffect()
	{
		PuzzleSymbol symbol = GetMidAreaSymbol();
		symbol.ShowHypeDarkEffect();
	}

	public void EndHypeDarkEffect()
	{
		PuzzleSymbol symbol = GetMidAreaSymbol();
		symbol.HideHypeDarkEffect();
	}

	public void StartHypeHighlightEffect()
	{
		if(_spinParam != null)
		{
			_spinParam.UnitTime = _normalUnitTime * _hypeSpeedUpFactor;
			_spinParam.IsHyping = true;

			_hypeFrameLightParent.SetActive(true);
		}
		else
		{
			Debug.Assert(false);
		}
	}

	public void EndHypeHighlightEffect()
	{
		if(_spinParam != null)
		{
			_spinParam.UnitTime = _normalUnitTime;
			_spinParam.IsHyping = false;
		}

		_hypeFrameLightParent.SetActive(false);
	}

	public void ShowNormalWinFrameLightEffect()
	{
		_normalWinFrameLightParent.SetActive(true);
	}

	public void HideNormalWinFrameLightEffect()
	{
		_normalWinFrameLightParent.SetActive(false);
	}

	public void ShowBigWinFrameLightEffect()
	{
		_bigWinFrameLightParent.SetActive(true);
	}

	public void HideBigWinFrameLightEffect()
	{
		_bigWinFrameLightParent.SetActive(false);
	}

	private void StartSpinEffect(float expectTime)
	{
		StartSpinReelLightEffect();
	}

	private void StartSpinReelLightEffect()
	{
		_idleReelLightParent.SetActive(false);
		_spinReelLightParent.SetActive(true);
	}

	private void HideSpinReelLightEffect()
	{
		_spinReelLightParent.SetActive(false);
	}

	private void StartIdleReelLightEffect()
	{
		_idleReelLightParent.SetActive(true);
		_spinReelLightParent.SetActive(false);
	}

	private void ShowReelLightEndEffect()
	{
		_spinReelLightEndParent.SetActive(true);
		UnityTimer.Start(this, 0.5f, () => {
			_spinReelLightEndParent.SetActive(false);

			if(_machine.GetState() == MachineState.Idle)
				StartIdleReelLightEffect();
		});
	}

	private void StartSpinBlurGlassEffect()
	{
		foreach(PuzzleSymbol s in _allSymbolList)
		{
			if(_areaSymbolList.Contains(s))
				s.StartBlurEffect();
			else
				s.EndBlurEffect();
		}
	}

	private void EndSpinBlurGlassEffect()
	{
		foreach(PuzzleSymbol s in _allSymbolList)
		{
			s.EndBlurEffect();
		}
	}

	private void ShowSymbolSlideEffect(SpinDirection dir){
		// open slide symbol effect
		if (dir == SpinDirection.Up) {
			if (_SymbolUpReelEffect != null){
				_SymbolUpReelEffect.SetActive (true);
			}
		}
		else if (dir == SpinDirection.Down) {
			if (_SymbolDownReelEffect != null){
				_SymbolDownReelEffect.SetActive (true);
			}
		}

		UnityTimer.Start(this, 2.0f, () => {
			HideSymbolSlideEffect();
		});
	}

	private void HideSymbolSlideEffect(){
		if (_SymbolUpReelEffect != null){
			_SymbolUpReelEffect.SetActive (false);
		}
		if (_SymbolDownReelEffect != null){
			_SymbolDownReelEffect.SetActive (false);
		}
	}

	public void ShowFrozenEffect(bool show){
		if (_frozenFadeInEffect != null) {
			_frozenFadeInEffect.SetActive (show);
		}
		if (_frozenFadeOutEffect != null) {
			_frozenFadeOutEffect.SetActive (!show);
			UnityTimer.Start (this, 1.0f, () => {
				_frozenFadeOutEffect.SetActive(false);
			});
		}
	}

	public void ShowSpecialSymbolLightEffect(bool show){
		if (_specialSymbolLightEffect != null) {
			_specialSymbolLightEffect.SetActive (show);
		}
	}

	public void ShowSuperSymbolLightEffect(bool show){
		if (_superSymbolLightEffect != null) {
			_superSymbolLightEffect.SetActive (show);
		}
	}

	public void ShowDragonEffect(bool show){
		if (_dragonReelEffect != null) {
			_dragonReelEffect.SetActive (show);
		}
	}

	#endregion

	#region adjust parne offset

	public void UpdateEffectParentOffset(Vector2 vec, PuzzleDefine.ReelEffectType type){
		GameObject obj = null;
		if (type == PuzzleDefine.ReelEffectType.BigWin) {
			obj = _bigWinFrameLightParent;
		} else if (type == PuzzleDefine.ReelEffectType.NormalWin) {
			obj = _normalWinFrameLightParent;
		} else if (type == PuzzleDefine.ReelEffectType.Special) {
			obj = _specialSymbolLightParent;
		}

		if (obj != null) {
			UpdateEffectParentOffset (vec, obj);
		}
	}

	private void UpdateEffectParentOffset(Vector2 vec, GameObject obj){
		Vector3 originalPos = obj.transform.localPosition;
		Vector3 newPos = originalPos + new Vector3 (vec.x, vec.y);
		obj.transform.localPosition = newPos;
	}

	#endregion
}
