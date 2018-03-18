using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class M6_SliderController : SliderControllerBase{
	public GameObject _cutoffLine;
	public GameObject _rowParent;
	public Slider _slider;
	public CollectEffectController _effectController;
	public GameObject _background;
	public GameObject _collectBottom;// 收集物飞向的点
	public float _offsetY = 136.0f;// Y起始位置的offset
	private float _sliderHeight = 0;
	private float _singleValueDelta = 0;
	public Image _slideImage;
	public Image _bottomImage;

	// Use this for initialization
	void Awake () {
		RectTransform trans = GetComponent<RectTransform> ();
		_sliderHeight = _slideImage.rectTransform.rect.height;
		LogUtility.Log("_sliderHeight = "+_sliderHeight, Color.blue);
		// _sliderHeight = trans.sizeDelta.y - _offsetY;
		_offsetY = _bottomImage.rectTransform.rect.height;
		LogUtility.Log("_offsetY = "+_offsetY, Color.blue);

		Canvas canvas = _background.GetComponent<Canvas>();
		if(canvas == null)
			canvas = _background.AddComponent<Canvas>();
		
		canvas.overrideSorting = true;
		canvas.sortingOrder = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (_effectController != null) {
			_effectController.UpdateCollectHintEffect (_slider.value, _sliderHeight, 40);
		}
	}

	private void SetCutOffLine(int count){
		float deltaHeight = _sliderHeight / count;

		if (_cutoffLine != null) {
			for (int i = 1; i < count; ++i) {
				GameObject obj = Instantiate (_cutoffLine, _rowParent.transform);
				obj.transform.localPosition = Vector3.zero + new Vector3 (0.0f, _offsetY + i * deltaHeight, 0.0f);
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
				obj.SetActive (true);
			}
		}
	}
	public override void Init(PuzzleMachine machine){
		base.Init(machine);
		BasicConfig config = machine.CoreMachine.MachineConfig.BasicConfig;
		SetCutOffLine(config.CollectNum);
		_singleValueDelta = (float) 1 / config.CollectNum;
	}

	public override void SetValue(float value){
		if (_slider != null){
			_slider.value = value;
		}
	}
	
	public override void AddValue(float value){
		if (_slider != null){
			_slider.value += value;
		}
	}

	public override void ResetValue(){
		if (_slider != null){
			_slider.value = 0.0f;
		}
	}

	public override GameObject GetLocationObjByIndex(int index){
		return _collectBottom;
	}
	
	public override GameObject GetLocationObjFromCurrentValue(int offsetIndex){
		return _collectBottom;
	}

	public override void UpdateSingleValue(){
		AddValue(_singleValueDelta);
	}
}