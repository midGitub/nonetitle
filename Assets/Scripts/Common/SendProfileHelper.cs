using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;

public class SendProfileHelper {

	private  static System.DateTime _sendProfileTime = System.DateTime.Now;
	private  static bool _firstSendProfile = true;
	private const float SEND_PROFILE_TIME_THRESHOLD = 5 * 60;
	private const float SEND_PROFILE_TIME_THRESHOLD_TEST = 10;

	public static void SendProfile()
	{
		#if true
		System.DateTime now = System.DateTime.Now;
		TimeSpan span = now - _sendProfileTime;
		if (span.Seconds < SEND_PROFILE_TIME_THRESHOLD 
			&& !_firstSendProfile) {
			return;
		}
		AnalysisManager.Instance.SendProfile ();
		Debug.Log ("Send profiler");
		_sendProfileTime = System.DateTime.Now;
		_firstSendProfile = false;
		#else
		SendProfileTest();
		#endif
	}

	public static void ForceSendProfile(){
		LogUtility.Log ("ForceSendProfile", Color.yellow);
		AnalysisManager.Instance.SendProfile ();
	}

	public static void SendProfileTest(){
		System.DateTime now = System.DateTime.Now;
		TimeSpan span = now - _sendProfileTime;
		if (span.Seconds < SEND_PROFILE_TIME_THRESHOLD_TEST 
			&& !_firstSendProfile) {
			return;
		}
		LogUtility.Log ("send profiler test", Color.yellow);
		_sendProfileTime = System.DateTime.Now;
		_firstSendProfile = false;

		LogUtility.Log ("test date string is "+ _sendProfileTime.ToString("yyyy-MM-dd HH:mm:ss"));
	}

}

