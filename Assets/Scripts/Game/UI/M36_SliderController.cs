using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class M36_SliderController : SliderControllerBase{
	private static readonly string _effectPrefabPath = "Effect/Prefab/";
	public bool _dynamicCreateCompass = false;

	// 克隆的罗盘实体
	public GameObject _compassItem;
	// 背景宽度
	public RectTransform _compassBackgroundRect;
	// 第一个罗盘的起始位置
	public Transform _startPos;
	// 罗盘排列方式
	public CompassSliderDir _dir = CompassSliderDir.Horizontal;
	// 缩放系数
	public float _compassScaleFactor = 1.0f;
	// x坐标的偏移量(左起）
	public float _compassOffset_x = 0.0f;
	// y坐标的偏移量(上起）
	public float _compassOffset_y = 0.0f;
	// 罗盘数量
	private int _compassNumMax = 9;
	// 当前收集数量
	private float _currentCollectNum = 0;
	// 面板尺寸
	private Vector2 _sizeDelta = Vector2.zero;
	// 集齐后的回调函数
	private Action _func;
	// 罗盘数组
	private List<CompassSymbol> _compassSymbolList = new List<CompassSymbol>();
	public CompassSymbol[] _compassSymbols;

	// 特效父节点
	public CollectEffectController _effectController;

	void Awake(){
		if (_dynamicCreateCompass) {
			_sizeDelta = _compassBackgroundRect.sizeDelta;
			LogUtility.Log ("CompassSliderController sizedelta x = " + _sizeDelta.x + " y = " + _sizeDelta.y, Color.yellow);

			CreateItems ();
		}
	}

	public float CurrentIndex {
		get { return _currentCollectNum; }
	}

	// Use this for initialization
	void Start () {
	}

	public void InitSymbols(string path, string machineName){
		if (_dynamicCreateCompass) {
			ListUtility.ForEach (_compassSymbolList, (CompassSymbol symbol) => {
				symbol.Init(path, machineName);
			});
		} else {
			ListUtility.ForEach (_compassSymbols, (CompassSymbol symbol) => {
				symbol.Init(path, machineName);
			});
		}
	}
	
	// Update is called once per frame
	void Update () {
	}

	private bool CheckInvoke(){
		return _currentCollectNum != 0 && _compassNumMax != 0 
			&& _currentCollectNum >= _compassNumMax;
	}

	public void Init(int num, CompassSliderDir dir = CompassSliderDir.Horizontal, Action func = null){
		_compassNumMax = num;
		_dir = dir;
		_func = func;
		if (_dynamicCreateCompass) {
			CreateItems ();
		}
	}

	// 创建罗盘对象
	private void CreateItems(){
		Vector3 deltaOffset;
		Vector3 startPos;
		CreateDeltaOffset(out deltaOffset, out startPos);

		_compassSymbolList.Clear ();
		for (int i = 0; i < _compassNumMax; ++i) {
			GameObject obj = Instantiate (_compassItem, _compassItem.transform.parent);
			obj.name = obj.name + i;
			obj.transform.localPosition = startPos + i * deltaOffset;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one * _compassScaleFactor;
			obj.SetActive (true);
			_compassSymbolList.Add (obj.GetComponent<CompassSymbol>());
		}
	}

	// 创建偏移值等参数
	private void CreateDeltaOffset(out Vector3 deltaOffset , out Vector3 pos){
		deltaOffset = Vector3.zero;
		pos = Vector3.zero;
		if (_dir == CompassSliderDir.Horizontal) {
			float x = _compassOffset_x -  _sizeDelta.x / 2;
			pos = new Vector3 (x, _startPos.localPosition.y, _startPos.localPosition.z);
			float delta = Mathf.Abs (x) * 2;
			float offset_x = delta / (_compassNumMax - 1);
			deltaOffset = new Vector3 (offset_x, 0.0f, 0.0f);

		} else if (_dir == CompassSliderDir.Vertical) {
			float y = _compassOffset_y -  _sizeDelta.y / 2;
			pos = new Vector3 (_startPos.localPosition.x, y, _startPos.localPosition.z);
			float delta = Mathf.Abs (y) * 2;
			float offset_y = delta / (_compassNumMax - 1);
			deltaOffset = new Vector3 (0.0f, offset_y, 0.0f);
		}
	}

	// 获得罗盘对象
	public GameObject GetIndexCompass(int index){
		if (_dynamicCreateCompass) {
			Debug.Assert (index < _compassSymbolList.Count);
			if (index < _compassSymbolList.Count) {
				return _compassSymbolList [index].gameObject;
			}
			int last = _compassSymbolList.Count - 1;
			return _compassSymbolList[last].gameObject;
		} else {
			Debug.Assert (index < _compassSymbols.Length);
			if (index < _compassSymbols.Length) {
				return _compassSymbols [index].gameObject;
			}
			int last = _compassSymbols.Length - 1;
			return _compassSymbols[last].gameObject;
		}
	}

	// 更新罗盘收集槽
	public void SetCompassSlider(float num){
		ShowCollectCompass (0, num);
		_currentCollectNum = num;
		if (CheckInvoke ()) {
			if (_func != null)
				_func ();
		}
	}

	// 更新罗盘收集槽
	public void UpdateCompassSlider(float num){
		ShowCollectCompass (_currentCollectNum, num);
		_currentCollectNum += num;
		if (CheckInvoke ()) {
			if (_func != null)
				_func ();
		}
	}

	// 显示罗盘收集槽进度
	private void ShowCollectCompass(float startIndex, float count){
		if (count == 0)
			return;

		int max = GetSymbolsLength();
		int showMax = (int)startIndex + (int)count;

		for (int i = (int)startIndex; i < max; ++i) {
			if (i < showMax) {
				Show (i, true);
			} else {
				Show (i, false);
			}
		}
	}

	private void Show(int index, bool state){
		if (_dynamicCreateCompass) {
			_compassSymbolList [index].Show (state);
		} else {
			_compassSymbols [index].Show (state);
		}
	}

	private int GetSymbolsLength(){
		int result = 0;
		if (_dynamicCreateCompass) {
			result = _compassSymbolList.Count;
		} else {
			result = _compassSymbols.Length;
		}
		return result;
	}

	// 重置罗盘收集槽
	public void Reset(){
		_currentCollectNum = 0;

		if (_dynamicCreateCompass) {
			ListUtility.ForEach (_compassSymbolList, (CompassSymbol symbol) => {
				symbol.Show (false);
			});
		} else {
			ListUtility.ForEach (_compassSymbols, (CompassSymbol symbol) => {
				symbol.Show (false);
			});
		}
	}

	public override void Init(PuzzleMachine machine){
		base.Init(machine);
		string path = _effectPrefabPath + machine.CoreMachine.MachineConfig.BasicConfig.CollectSymbolEffect;
		InitSymbols (path, machine.MachineName);
	}

	public override void SetValue(float value){
		if (value == 0.0f){
			Reset();
		}else{
			SetCompassSlider(value);
		}
	}

	public override void AddValue(float value){
		UpdateCompassSlider(value);
	}
	
	public override void ResetValue(){
		Reset();
	}
	
	public override GameObject GetLocationObjByIndex(int index){
		int length = GetSymbolsLength();
		Debug.Assert(index < length);
		return GetIndexCompass(index);
	}
	
	public override GameObject GetLocationObjFromCurrentValue(int offsetIndex){
		int currentIndex = (int)_currentCollectNum;
		int length = GetSymbolsLength();
		int index = currentIndex + offsetIndex;
		Debug.Assert(index < length);
		return GetIndexCompass(index);
	}
	public override void UpdateSingleValue(){
		AddValue(1);
	}
}
