using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class WheelController : RotaryTableController {

	// zhousen
	// 当前使用配置表
	private WheelConfig _wheelConfig;
	private int _wheelIndex;

	private bool _rotating;
	public Vector3 _defaultRotation;

	public bool IsRotating{
		get { return _rotating; }
	}

	public void Init(WheelConfig config, int index){
		_wheelConfig = config;
		_wheelIndex = index;
		_rotating = false;
	}

	public void StartRotate(WheelData data, Callback endCallback)
	{
		StartCoroutine(ControllerAnimation(data, endCallback));
	}

	private float CalculateRotateAngle(WheelData data)
	{
		#if false
		float angle = 0;
		for(int i = 0; i < data.ID; i++)
		{
			angle += _wheelConfig.DataArray[i].Angle[_wheelIndex];
		}
		angle += _wheelConfig.DataArray[data.ID].Angle[_wheelIndex] / 2;
		Debug.Log(angle);

		// 在最高速度的时候已经包含转到转到想要的位置
		return angle += MaxSpeeRotateAngle;
		#endif
		float angle = _wheelConfig.DataArray [data.ID - 1].Angle [_wheelIndex] - 45.0f;

		if (_rotaryType == RotaryType.AntiClockwise)
			angle = 360.0f - angle;
		
		LogUtility.Log ("calculate rotate angle is "+angle + " id is" + data.ID + " index is "+_wheelIndex, Color.red);
		return angle;
	}

	private IEnumerator ControllerAnimation(WheelData data, Callback endCallback)
	{
		_rotating = true;
		yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, StartSpeed, MaxSpeed, ToMaxSpeedAngle, _Rotatedir, MinSpeed, _rotaryType));
		yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, MaxSpeed, MaxSpeed, CalculateRotateAngle(data),_Rotatedir,  MinSpeed, _rotaryType));
		yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, MaxSpeed, 0, RotateDownAnagle, _Rotatedir, MinSpeed, _rotaryType));
		yield return new WaitForSeconds(StopWaitTime);
		RotateFinisedEvent.Invoke();
		_rotating = false;

		if(endCallback != null)
			endCallback();
	}

	public void ResetRotation(){
		LogUtility.Log ("_defaultRotation is " + _defaultRotation);
		transform.localRotation = Quaternion.Euler( _defaultRotation );
//		transform.localRotation = Quaternion.identity;
	}
}
