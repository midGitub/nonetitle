using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HalloweenHelper : SimpleSingleton<HalloweenHelper> {

	private int _defautPromotionDay = 3;
	private int _defautPromotionAdvanceDay = 1;
	private string _defauthalloweenday = "2017-10-31";
	public void Tryshow()
	{
		if (ShouldShowBill())
		{
			GameObject halloweengameObject = UGUIUtility.InstantiateUI ("Game/UI/BillBoard/Halloween");
			halloweengameObject.SetActive(false);
			Halloween halloween = halloweengameObject.GetComponent<Halloween>();
			BillBoardManager.Instance.Add(halloween);
		}
	}

	public bool IsInPromotion()
	{
		bool result = false;
		DateTime halloweenday = HalloweenDay();
		TimeSpan span = NetworkTimeHelper.Instance.GetNowTime() - halloweenday;
		if (span.TotalDays > 0 && span.TotalDays < _defautPromotionDay)
			result = true;
		return result;
	}

	public TimeSpan PromotionTimeLeft()
	{
		DateTime halloweenday = HalloweenDay();
		TimeSpan span =   halloweenday + new TimeSpan(_defautPromotionDay,0,0,0) - NetworkTimeHelper.Instance.GetNowTime();
		return span;
	}

	public bool IsHalloweenOn()
	{
		string value = MapSettingConfig.Instance.Read("HalloweenOn", "false");
		return string.Equals(value, "1");
	}

	public DateTime HalloweenDay()
	{
		string value = MapSettingConfig.Instance.Read("HalloweenDay", _defauthalloweenday);
		return DateTime.Parse(value);
	}

	public TimeSpan TimeLeft()
	{
		DateTime halloweenday = HalloweenDay();
		TimeSpan span = halloweenday - NetworkTimeHelper.Instance.GetNowTime();
		return span;
	}

	bool ShouldShowBill()
	{
		bool result = false;
		TimeSpan span = TimeLeft();
		if (span.TotalDays > - _defautPromotionDay  && span.TotalDays < _defautPromotionAdvanceDay)
			result = true;
		return result;
	}


		
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
