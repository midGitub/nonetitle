using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RotateUtility  {
	public static IEnumerator  ARotateT(Transform trans, float startSpeed, float LastSpeed, float Dictance, Vector3 dir, float minSpeed = 20.0f, RotaryType type = RotaryType.Clockwise)
	{
		var startAng = trans.localRotation.eulerAngles;
		float rotaryDirFactor = (type == RotaryType.Clockwise) ? 1.0f : -1.0f;
		float a = 0.0f;
		if (Dictance != 0.0f) {
			a = (LastSpeed * LastSpeed - startSpeed * startSpeed) / (2 * Dictance) * rotaryDirFactor;
		}
		float currDic = 0;

		Predicate<float> checkFunc = null;

		if (type == RotaryType.Clockwise) {
			checkFunc = (float cur) => {
				return cur <= Dictance;
			};
		} else {
			checkFunc = (float cur) => {
				return cur >= -Dictance;
			};
		}

		startSpeed = startSpeed * rotaryDirFactor;
		minSpeed = minSpeed * rotaryDirFactor;

		while(checkFunc(currDic))
		{
			startSpeed += a * Time.deltaTime;
			if (type == RotaryType.Clockwise) {
				if (a < 0) {
					if (startSpeed < minSpeed) {
						startSpeed = minSpeed;
					}
				}
			} else {
				if (a > 0) {
					if (startSpeed > minSpeed) {
						startSpeed = minSpeed;
					}
				}
			}
			currDic += startSpeed * Time.deltaTime;
			//			LogUtility.Log ("rotate utility startspeed is "+startSpeed+" curDic = "+currDic+" rotate = "+startSpeed * dir * Time.deltaTime, Color.red);
			trans.Rotate(startSpeed * dir * Time.deltaTime, Space.Self);
			yield return null;
		}

		trans.localRotation = Quaternion.Euler(Dictance * rotaryDirFactor * dir + startAng);
		yield return null;
	}
}
