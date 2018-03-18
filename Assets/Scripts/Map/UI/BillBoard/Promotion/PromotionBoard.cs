using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PromotionBoard : BillBoardBase {

	public Image StartImage;
	public Button EndButton;
	DateTime _PromotionDay;
	private Coroutine coroutine;
	public Text StartTimeText;
	public Text EndTimeText;
	// Use this for initialization
	private string _promotionName;
	public int LastDay;
	public int AdvanceDay;

	public string PromotionName
	{
		get
		{
			return _promotionName;
		}
	}

	public void SetPromotionName(string name)
	{
		_promotionName = name;
		string _promotionNameTrim = _promotionName.TrimEnd(StringUtility._numberArray);
		StartImage.sprite = Resources.Load<Sprite>("Images/UI/Promotion/" + _promotionNameTrim + "Start");
		EndButton.image.sprite = Resources.Load<Sprite>("Images/UI/Promotion/" + _promotionNameTrim + "End");
	}
		
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
		StoreController.Instance.OpenStoreUI(OpenPos.LobbyNotice.ToString(), StoreType.Buy);
	}

	void SetState()
	{
		if ( PromotionName != null)
		{
			if (PromotionHelper.Instance.IsInPromotion(PromotionName, AdvanceDay, LastDay))
			{
				ChangeToEnd();
				/*
				EndButton.gameObject.SetActive(true);
				StartImage.gameObject.SetActive(false);
				CountDown ct = EndTimeText.GetComponentInChildren<CountDown>();
				ct.CountingTime = PromotionHelper.Instance.PromotionTimeLeft(PromotionName, LastDay);
				ct.WhiteSpace = true;
				ct.TimeEvent.AddListener(ChangeToDelete);
				ct.count();
				*/
			}
			else
			{
				StartImage.gameObject.SetActive(true);
				EndButton.gameObject.SetActive(false);
				CountDown ct = StartTimeText.GetComponentInChildren<CountDown>();
				ct.CountingTime = PromotionHelper.Instance.AdvanceTimeLeft(PromotionName);
				ct.WhiteSpace = true;
				ct.TimeEvent.RemoveAllListeners();
				ct.TimeEvent.AddListener(ChangeToEnd);
				ct.count();
			}
		}
	}

	void ChangeToEnd()
	{
		EndButton.gameObject.SetActive(true);
		StartImage.gameObject.SetActive(false);
		CountDown ct = EndTimeText.GetComponentInChildren<CountDown>();
		ct.CountingTime = PromotionHelper.Instance.PromotionTimeLeft(PromotionName, LastDay);
		ct.WhiteSpace = true;
		ct.TimeEvent.RemoveAllListeners();
		ct.TimeEvent.AddListener(ChangeToDelete);
		ct.count();
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
