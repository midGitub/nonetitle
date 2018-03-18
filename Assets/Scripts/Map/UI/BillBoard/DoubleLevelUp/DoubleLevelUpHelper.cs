using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoubleLevelUpHelper :SimpleSingleton<DoubleLevelUpHelper>{
	DayOfWeek doubleday;
	int defaultdoubleday = 6;
	string path = "Game/UI/BillBoard/DoubleLevelUp";
	string iconpath = "Game/UI/BillBoard/DoubleLevelUpIcon";
	public void Tryshow()
	{
		if (ShouldShowBill())
		{
			GameObject doublelevelupgameObject = UGUIUtility.InstantiateUI (path);
			doublelevelupgameObject.SetActive(false);
			DoubleLevelUp doublelevelup = doublelevelupgameObject.GetComponent<DoubleLevelUp>();
			BillBoardManager.Instance.Add(doublelevelup);
		}
	}

	public void TryShowIcon()
	{
		if (ShouldShowIcon())
		{
			Transform transform = GameObject.Find("GiftParent").transform;
			GameObject icon = UGUIUtility.InstantiateUI (iconpath);
			icon.transform.SetParent(transform, false);
			icon.SetActive(true);
		}
	}

	public bool IsInDouble()
	{
		return ShouldShowIcon();
	}

	public bool IsDoubleLevelUpOn()
	{
		bool result = false;
		if (MapSettingConfig.Instance.MapSettingMap.ContainsKey("DoubleLevelUpOn"))
		{
			string temp = MapSettingConfig.Instance.MapSettingMap ["DoubleLevelUpOn"];
			result = string.Equals(temp, "1");
		}
		return result;
	}

	public int DaysLeft()
	{
		Instance.doubleday = DoubleDay();
		DayOfWeek nowday = NetworkTimeHelper.Instance.GetNowTime().DayOfWeek;
		return Instance.doubleday - nowday;
	}

	public DayOfWeek DoubleDay()
	{
		int dayofweek = defaultdoubleday;
		if (MapSettingConfig.Instance.MapSettingMap.ContainsKey("DoubleLevelUpDay"))
		{
			int len = Enum.GetValues(typeof(DayOfWeek)).Length;
			int temp;
			int.TryParse(MapSettingConfig.Instance.MapSettingMap ["DoubleLevelUpDay"], out temp);
			if (Enum.IsDefined(typeof(DayOfWeek), temp))
				dayofweek = temp;
		}
		return (DayOfWeek)dayofweek;
	}

	bool ShouldShowIcon()
	{
		bool result = false;
		if (IsDoubleLevelUpOn() && DaysLeft() == 0)
			result = true;
		else if (DoubleLevelUpNotify.InDoubleHours())
			result = true;
		return result;
	}

	bool ShouldShowBill()
	{
		return IsDoubleLevelUpOn()&&(DaysLeft() == 0 || DaysLeft() == 1);
	}
		
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
