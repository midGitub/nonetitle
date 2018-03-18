using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PromotionHelper : SimpleSingleton<PromotionHelper> {

	public readonly float PromotedCreditFactor = 2.0f;

	private const int _defautPromotionDay = 3;
	private const int _defautPromotionAdvanceDay = 1;

	private List<string> _promotionNameList;
	private List<int> _promotionLastDayList;
	private List<int> _promotionAdvanceDayList;

	public int PromotionLen
	{
		get
		{
			return Math.Min(Math.Min(PromotionLastDayList.Count, PromotionNameList.Count), PromotionAdvanceDayList.Count);
		}
	}

	public List<string> PromotionNameList
	{
		get
		{
			if (_promotionNameList == null)
			{
				_promotionNameList = ReadList<string>("ActivePromotion",null);
			}
			return _promotionNameList;
		}
	}

	public List<int> PromotionLastDayList
	{
		get
		{
			if (_promotionLastDayList == null)
			{
				_promotionLastDayList = ReadList<int>("ActivePromotionLastDay", 1);
			}
			return _promotionLastDayList;
		}
	}

	public List<int> PromotionAdvanceDayList
	{
		get
		{
			if (_promotionAdvanceDayList == null)
			{
				_promotionAdvanceDayList = ReadList<int>("ActivePromotionAdvanceDay", 1);
			}
			return _promotionAdvanceDayList;
		}
	}
		
	List<T> ReadList<T>(string name,T defaultvalue)
	{
		List<T> result = new List<T>();
		string str = MapSettingConfig.Instance.Read<string>(name, null);
		if (str != null)
		{
			string[] strs = str.Split(new char[]{ ',' });
			foreach (string ss in strs)
			{
				T value = defaultvalue;
				try
				{
					value = (T)Convert.ChangeType((object)ss, typeof(T));
				}
				catch(Exception e)
				{
					
				}
				result.Add(value);
			}
		}
		return result;
	}

	public bool IsInPromotion()
	{
		bool result = false;
		for (int i = 0; i < PromotionLen; i++)
		{
			if (IsInPromotion(PromotionNameList [i], PromotionAdvanceDayList [i], PromotionLastDayList [i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}
		
	public bool IsInPromotion(string promotionName,int advance  = _defautPromotionAdvanceDay,int lastday = _defautPromotionDay)
	{
		bool result = false;
		DateTime promotionDay = PromotionDay(promotionName);
		TimeSpan span = NetworkTimeHelper.Instance.GetNowTime() - promotionDay;
		if (span.TotalDays > 0 && span.TotalDays < lastday)
			result = true;
		return result;
	}

	public TimeSpan PromotionTimeLeft(string promotionName , int _promotionLastDay = _defautPromotionDay)
	{
		DateTime promotionDay = PromotionDay(promotionName);
		TimeSpan span =   promotionDay + new TimeSpan(_promotionLastDay,0,0,0) - NetworkTimeHelper.Instance.GetNowTime();
		return span;
	}

	public TimeSpan AdvanceTimeLeft(string promotionName)
	{
		DateTime promotionDay = PromotionDay(promotionName);
		TimeSpan result = promotionDay - NetworkTimeHelper.Instance.GetNowTime()  ;
		return result;
	}

	public bool IsPromotionOn(string promotionName)
	{
		string value = MapSettingConfig.Instance.Read(promotionName+"On", "false");
		return string.Equals(value, "1");
	}
		
	public void Tryshow(string promotionName,int defaultday,int advancedDay)
	{
		if (ShouldShowBill(promotionName,defaultday,advancedDay))
		{
			GameObject promotion = UGUIUtility.InstantiateUI ("Game/UI/BillBoard/PromotionBillBoard");
			promotion.SetActive(false);
			PromotionBoard board = promotion.GetComponent<PromotionBoard>();
			board.SetPromotionName(promotionName);
			board.AdvanceDay = advancedDay;
			board.LastDay = defaultday;
			BillBoardManager.Instance.Add(board);
		}
	}

	bool ShouldShowBill(string promotionName,int defaultPromotionDay = _defautPromotionDay,int defaultPromotionAdvanceDay = _defautPromotionAdvanceDay)
	{
		bool result = false;
		TimeSpan span = TimeLeft(promotionName);
		if (span.TotalDays > -defaultPromotionDay  && span.TotalDays < defaultPromotionAdvanceDay)
			result = true;
		return result;
	}

	DateTime PromotionDay(string promotionName,string defaultDate = "2017-1-1")
	{
		string value = MapSettingConfig.Instance.Read(promotionName+"Date", defaultDate);
		return DateTime.Parse(value);
	}

	TimeSpan TimeLeft(string promotionName)
	{
		DateTime promotionDay = PromotionDay(promotionName);
		TimeSpan span = promotionDay - NetworkTimeHelper.Instance.GetNowTime();
		return span;
	}
		
}
