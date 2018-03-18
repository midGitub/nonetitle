using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;

public class ReloadGameManager : Singleton<ReloadGameManager>
{
	static int _defaultPauseMinutes = 2 * 60;
	static int _defaultPauseMinutesOnNewDay = 30;

	DateTime _pauseTime;
	bool _isRecordPauseTime;

	public void Init()
	{
	}

	void OnApplicationPause(bool isPause)
	{
//		Debug.Log("ReloadGameManager OnApplicationPause: " + isPause);
//
//		if(isPause)
//		{
//			RecordTime();
//		}
//		else
//		{
//			if(ShouldReload())
//				PerformReload();
//		}
	}

	void RecordTime()
	{
		_pauseTime = DateTime.Now;
		_isRecordPauseTime = true;
	}

	bool ShouldReload()
	{
		bool result = false;
		if(_isRecordPauseTime)
		{
			DateTime now = DateTime.Now;
			TimeSpan span = now.Subtract(_pauseTime);

			int pauseMinutes = _defaultPauseMinutes;
			if(MapSettingConfig.Instance.MapSettingMap.ContainsKey("PauseMinutesOfReloadGame"))
				int.TryParse(MapSettingConfig.Instance.MapSettingMap["PauseMinutesOfReloadGame"], out pauseMinutes);

			int pauseMinutesOnNewDay = _defaultPauseMinutesOnNewDay;
			if(MapSettingConfig.Instance.MapSettingMap.ContainsKey("PauseMinutesOnNewDayOfReloadGame"))
				int.TryParse(MapSettingConfig.Instance.MapSettingMap["PauseMinutesOnNewDayOfReloadGame"], out pauseMinutesOnNewDay);

			result = (span.TotalMinutes >= pauseMinutes)
				|| (!TimeUtility.IsSameDay(_pauseTime, now) && span.TotalMinutes >= pauseMinutesOnNewDay);
		}

		return result;
	}

	void PerformReload()
	{
		_isRecordPauseTime = false;

		Application.Quit();
		UnityEngine.SceneManagement.SceneManager.LoadScene(ScenesController.StartLoadingSceneName);
	}

	#region Test

	public void TestApplicationPause(bool isPause)
	{
		OnApplicationPause(isPause);
	}

	#endregion
}
