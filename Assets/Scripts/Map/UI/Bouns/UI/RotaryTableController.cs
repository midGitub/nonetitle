using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum RotaryType{
	Clockwise,
	AntiClockwise,
	Max,
}

public class RotaryTableController : MonoBehaviour
{
	public Transform RotaryTransform;

	public RotaryType _rotaryType = RotaryType.Clockwise;

	public float StartSpeed = 0;
	public float MaxSpeed = 180;
	/// <summary>
	/// 到最高速的需要转的角度
	/// </summary>
	public float ToMaxSpeedAngle = 720;
	/// <summary>
	/// 保持最高速度需要转的角度,Base  和Stop 合起来要为 360的倍数
	/// </summary>
	public float MaxSpeeRotateAngle = 640;

	public float RotateDownAnagle = 720;

	public float StopWaitTime = 1;

	public float MinSpeed = 20;

	public float ToMaxSpeedTime;
	public float ToMinSpeedTime;

	protected Vector3 _Rotatedir = Vector3.forward;

	public UnityEvent RotateFinisedEvent = new UnityEvent();

	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		ToMaxSpeedTime = TextTime(StartSpeed, MaxSpeed, ToMaxSpeedAngle);
		ToMinSpeedTime = TextTime(MaxSpeed, 0, RotateDownAnagle);
	}
#endif

	private float TextTime(float startSpeed, float LastSpeed, float Dictance)
	{
		var startAng = RotaryTransform.localRotation.eulerAngles;
		float a = (LastSpeed * LastSpeed - startSpeed * startSpeed) / (2 * Dictance);
		return (LastSpeed - startSpeed) / a;
	}

	public void StartRotate(DailyBonusData dbd)
	{
		StartCoroutine(ControllerAnimation(dbd));
	}

	private float CalculateRotateAngle(DailyBonusData dbd)
	{
		if (dbd == null) {
			LogUtility.Log ("daily bonus data is null", Color.red);
			return 0.0f;
		}

		float angle = 0;
		for(int i = 0; i < dbd.BonusID; i++)
		{
			angle += DailyBonusConfig.Instance.DailyBonusList[i].Angle;
		}
		angle += DailyBonusConfig.Instance[dbd.BonusID].Angle / 2;

		angle += MaxSpeeRotateAngle;

		if (_rotaryType == RotaryType.AntiClockwise) {
			float rotateRoundNum = Mathf.Floor (angle / 360.0f);
			angle = angle - 360.0f * (2 * rotateRoundNum + 1);
		}

		// 在最高速度的时候已经包含转到转到想要的位置
		return angle += MaxSpeeRotateAngle;
	}

	private IEnumerator ControllerAnimation(DailyBonusData dbd)
	{
		yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, StartSpeed, MaxSpeed, ToMaxSpeedAngle, _Rotatedir, MinSpeed, _rotaryType));
		yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, MaxSpeed, MaxSpeed, CalculateRotateAngle(dbd),_Rotatedir,  MinSpeed, _rotaryType));
		yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, MaxSpeed, 0, RotateDownAnagle, _Rotatedir, MinSpeed, _rotaryType));
		yield return new WaitForSeconds(StopWaitTime);
		RotateFinisedEvent.Invoke();
	}

}
