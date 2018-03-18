using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DoubleLevelUp : BillBoardBase 
{
	public Image StartImage;
	public Image EndImage;
	public Text TimeText;
	DayOfWeek _doubleDay;

	void OnEnable()
	{
		_doubleDay = DoubleLevelUpHelper.Instance.DoubleDay();
		int timeleft = DoubleLevelUpHelper.Instance.DaysLeft();
		SetState(timeleft);
		Debug.Assert(timeleft <= 1, "Error:Double Level Up Should Not Show");
	}

	void SetState(int timeleft)
	{
		StartImage.gameObject.SetActive(timeleft != 0);
		EndImage.gameObject.SetActive(timeleft == 0);

		CountDown ct = TimeText.GetComponentInChildren<CountDown>();
		ct.WhiteSpace = true;
	
		if (timeleft == 1)
		{
			float second = (float)TimeUtility.TimeLeft(_doubleDay).TotalSeconds;
			ct.CountingTime = new TimeSpan (0, 0, (int)second);
			ct.TimeEvent.AddListener(ChangeToEnd);
			ct.count();
		}
		else
		{
			DayOfWeek day = (DayOfWeek)(((int)_doubleDay + 1) % Enum.GetValues(typeof(DayOfWeek)).Length);
			float second = (float)TimeUtility.TimeLeft(day).TotalSeconds;
			ct.CountingTime = new TimeSpan (0, 0, (int)second);
			ct.TimeEvent.AddListener(ChangeToDelete);
			ct.count();
		}
	}
			
	void ChangeToEnd()
	{
		SetState(0);
	}

	void ChangeToDelete()
	{
		TellDelete();
	}

	void TellDelete()
	{
		BillBoardManager.Instance.Delete(this);
	}
}
