using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Halloween : BillBoardBase {

	public Image StartImage;
	public Button EndButton;
	DateTime _holleweenDay;
	private Coroutine coroutine;
	public Text TimeText1;
	public Text TimeText2;
	// Use this for initialization

	void OnEnable()
	{
		EndButton.onClick.AddListener(ButtonClick);
		SetState();
	}

	void OnDisable()
	{
		EndButton.onClick.RemoveAllListeners();
	}

	void ButtonClick()
	{
		StoreController.Instance.OpenStoreUI(OpenPos.Auto.ToString(), StoreType.Buy);
	}

	void SetState()
	{
		TimeSpan timeleft = HalloweenHelper.Instance.TimeLeft();
		int days = (int)Math.Ceiling(timeleft.TotalDays);
		StartImage.gameObject.SetActive(days == 1);
		EndButton.gameObject.SetActive(days != 1);
		if (days != 1)
		{
			CountDown ct = TimeText2.GetComponentInChildren<CountDown>();
			ct.CountingTime = HalloweenHelper.Instance.PromotionTimeLeft();
			ct.WhiteSpace = true;
			ct.TimeEvent.AddListener(ChangeToEnd);
			ct.count();
		}
		else
		{
			float second = (float)timeleft.TotalSeconds;
			CountDown ct = TimeText1.GetComponentInChildren<CountDown>();
			ct.CountingTime = new TimeSpan (0, 0, (int)second);
			ct.WhiteSpace = true;
			ct.TimeEvent.AddListener(ChangeToDelete);
			ct.count();
		}
	}

	void ChangeToEnd()
	{
		SetState();
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
