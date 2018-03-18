using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;

public class HomeInADS : Singleton<HomeInADS>  {
	private DateTime _lastHomeOut = System.DateTime.Now;
	private int _defaultDays = 2 ;
	private int _defaultMinute = 5;
	public void Init()
	{
		HomeInADSManager.Instance.Init();
//		#if DEBUG
//		_defaultDays = -1;
//		_defaultMinute = 2;
//		#endif
	}
		
	void OnApplicationPause(bool isPause)
	{
		if (!isPause)
		{
			string name = ScenesController.Instance.GetCurrSceneName();
			if (name == ScenesController.MainMapSceneName || name == ScenesController.GameSceneName)
			{
				if (ShouldShow())
				{
					HomeInADSManager.Instance.ShowPictureADS();
				}
			}
		}
		else
		{
			_lastHomeOut = NetworkTimeHelper.Instance.GetNowTime();
		}
	}

	bool ShouldShow()
	{
		bool flag = false;
		if (!UserBasicData.Instance.IsPayUser && AdStrategyConfig.Instance.IsHomeInInterstitialActive(GroupConfig.Instance.GetAdStrategyId()))
		{
			DateTime now = NetworkTimeHelper.Instance.GetNowTime();
			if (TimeUtility.DaysLeft(now, UserBasicData.Instance.FirstEnterGameTime) > _defaultDays)
			{
				if (TimeUtility.IsSameDay(_lastHomeOut,now))
				{
					if ((now - _lastHomeOut).TotalMinutes > _defaultMinute)
					{
						flag = true;
					}
				}
			}
		}
		return flag;
	}

}
