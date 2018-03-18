using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SliderControllerBase : MonoBehaviour {
	protected PuzzleMachine _machine;

	public virtual void Init(PuzzleMachine machine){
		_machine = machine;
	}
	// 收集槽设置
	public virtual void SetValue(float value){}
	// 收集槽增长
	public virtual void AddValue(float value){}
	// 更新一单位的收集槽
	public virtual void UpdateSingleValue(){}
	public virtual void ResetValue(){}
	// 根据索引值来判断飞行的最终位置
	public virtual GameObject GetLocationObjByIndex(int index){
		return null;
	}
	// 根据相对索引值来判断飞行最终位置
	public virtual GameObject GetLocationObjFromCurrentValue(int offsetIndex){
		return null;
	}
}



