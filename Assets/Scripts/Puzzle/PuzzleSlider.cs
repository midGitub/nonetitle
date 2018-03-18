using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSlider {
	private static readonly string _effectPrefabPath = "Effect/Prefab/";

	private PuzzleMachine _machine;
	private GameObject _parent;
	private GameObject _collectBottom;// 收集槽按钮
	private SliderControllerBase _sliderController;
	private CollectEffectController _effectController;


	public PuzzleSlider(PuzzleMachine machine, GameObject parent){
		_machine = machine;
		_parent = parent;
		InitCollectSlider(_machine);
	}

	public void SetCollectSliderValue(float value){
		if (_sliderController != null){
			_sliderController.SetValue(value);
		}
	}

	public void UpdateCollectSliderValue(float value){
		if (_sliderController != null){
			_sliderController.AddValue(value);
		}
	}

	public void UpdateCollectSliderValue(){
		if (_sliderController != null){
			_sliderController.UpdateSingleValue();
		}
	}

	public GameObject GetLocationObjFromCurrentValue(int offsetIndex){
		if (_sliderController != null){
			return _sliderController.GetLocationObjFromCurrentValue(offsetIndex);
		}
		return null;
	}

	public void ShowSliderEffect(CollectEffectType type, bool isShow){
		if (_effectController != null){
			_effectController.ShowEffect(type, isShow);
		}
	}

	private void InitCollectSlider(PuzzleMachine machine){
		string collectSlider = machine.CoreMachine.MachineConfig.BasicConfig.CollectSlider;
		CreateSlider(machine, !collectSlider.IsNullOrEmpty(), collectSlider);
	}

	private void CreateSlider(PuzzleMachine machine, bool hasSlider, string path){
		if (hasSlider){
			GameObject obj = UGUIUtility.CreateMachineAsset(path, machine.MachineName, _parent);
			InitSlider(machine, obj);
			InitSliderEffect(machine, obj);
		}
	}

	private void InitSlider(PuzzleMachine machine, GameObject obj){
		_sliderController = obj.GetComponent<SliderControllerBase>();
		if (_sliderController != null){
			_sliderController.Init(machine);
		}
	}

	private void InitSliderEffect(PuzzleMachine machine, GameObject obj){
		_effectController = obj.GetComponentInChildren<CollectEffectController> ();
		if (_effectController != null) {
			_collectBottom = _effectController.gameObject;
			CreateSliderEffect(machine, _effectController);
		}
	}
	
	private void CreateSliderEffect(PuzzleMachine machine, CollectEffectController control){
		BasicConfig config = machine.CoreMachine.MachineConfig.BasicConfig;

		// 收集特效
		List<string> effectNames = new List<string>{
			config.CollectEffect, config.CollectCompleteEffect, config.CollectHintEffect, config.CollectAlwaysEffect
		};

		for (int i = 0; i < (int)CollectEffectType.Max; ++i) {
			if (!string.IsNullOrEmpty (effectNames[i])) {
				string path = _effectPrefabPath + effectNames[i];
				control.CreateCollectEffect (path, control.gameObject, (CollectEffectType)i, machine.MachineName);
			}
		}
	}
}
