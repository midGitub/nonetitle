using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleReelSpinConfig : MonoBehaviour
{
	public int _spinSpeed = 34;

	public float _startSpinOffsetFactor = 0.15f;
	public float _startSpinTime = 0.2f;
	public float _spinNeighorTime = 0.4f;
	public float _slideReelInterval = 0.4f;

	public List<float> _endSpinOffsetFactors = new List<float> { 0.2f, -0.07f };
	public List<float> _endSpinTimes = new List<float> { 0.1f, 0.05f };

	public List<float> _fasterSpeedFactors = new List<float>{ 0.5f, 0.6f, 0.7f, 0.8f, 0.9f };
	public List<float> _slowerSpeedFactors = new List<float>{ 0.9f, 0.6f, 0.3f };


	#if true // 金手指用

	public void SetSpinSpeed(string speed){
		StringUtility.SetValue(ref _spinSpeed, speed);
	}

	public void SetStartSpinOffsetFactor(string factor){
		StringUtility.SetValue(ref _startSpinOffsetFactor, factor);
	}

	public void SetStartSpinTime(string factor){
		StringUtility.SetValue(ref _startSpinTime, factor);
	}

	public void SetSpinNeighorTime(string factor){
		StringUtility.SetValue(ref _spinNeighorTime, factor);
	}

	public void SetSlideReelInterval(string factor){
		StringUtility.SetValue(ref _slideReelInterval, factor);
	}

	public void SetEndSpinOffsetFactors(string str){
		StringUtility.SetValueList(ref _endSpinOffsetFactors, str);
	}

	public void SetEndSpinTImes(string str){
		StringUtility.SetValueList(ref _endSpinTimes, str);
	}

	public void SetFasterSpeedFactors(string str){
		StringUtility.SetValueList(ref _fasterSpeedFactors, str);
	}

	public void SetSlowerSpeedFactors(string str){
		StringUtility.SetValueList(ref _slowerSpeedFactors, str);
	}

	#endif
}
