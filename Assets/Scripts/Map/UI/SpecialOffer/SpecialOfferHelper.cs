using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialOfferHelper : SimpleSingleton <SpecialOfferHelper> {
	private static SpecialOfferHelper _instance;
	private GameObject _specialOfferIcon = null;
	private StoreAnalysisData _storeAnalysisData;
	string iconpath = "Game/SpecialOfferIcon";
	string windowpath = "Game/SpecialOffer";
	string parentname = "GiftParent";
				
	public void TryShowIcon()
	{
		if(!HasBoughtSpecialOffer() && ShouldShow())
			ShowIcon();
	}
		
	public void TryShowSpecialWindow()
	{
		if(!HasBoughtSpecialOffer() && IsFirstEnterToday() && NetworkTimeHelper.Instance.IsServerTimeGetted && ShouldShow())
			ShowSpecialWindow(false,OpenPos.EnterLobby);
	}

	public void BuySuccess()
	{
		UserBasicData.Instance.HasBoughtSpecialOffer = true;
		HideIcon ();
	}

	public void SetStoreAnalysisData()
	{
		StoreController.Instance.CurrStoreAnalysisData = _storeAnalysisData;
		AnalysisManager.Instance.OpenShop();
	}
		
	bool HasBoughtSpecialOffer()
	{
		return UserBasicData.Instance.HasBoughtSpecialOffer;
	}

	bool IsFirstEnterToday()
	{
		return TimeUtility.DaysLeft (UserBasicData.Instance.LastShowSpecialOffer, NetworkTimeHelper.Instance.GetNowTime ()) != 0;
	}

	void ShowIcon()
	{
		if (_specialOfferIcon == null)
		{
			Transform transform = GameObject.Find(parentname).transform;
			_specialOfferIcon = UGUIUtility.InstantiateUI(iconpath);
			_specialOfferIcon.transform.SetParent(transform, false);
		}
		if(_specialOfferIcon.activeSelf == false)
			_specialOfferIcon.SetActive (true);
	}

	public void ShowSpecialWindow(bool IsShowByClickButton,OpenPos pos = OpenPos.Auto)
	{
		GameObject specialoffer = UGUIUtility.InstantiateUI (windowpath);
		specialoffer.SetActive (true);
		if (IsShowByClickButton) 
		{
			if (ScenesController.Instance.GetCurrSceneName () == ScenesController.MainMapSceneName)
				pos = OpenPos.Lobby;
			else if (ScenesController.Instance.GetCurrSceneName () == ScenesController.GameSceneName)
				pos = OpenPos.GameUp;
		}
		SetAnalysisData (pos);
	}

	private void SetAnalysisData(OpenPos pos = OpenPos.EnterLobby)
	{/*
		StoreController.Instance.CurrStoreAnalysisData = new StoreAnalysisData();
		StoreController.Instance.CurrStoreAnalysisData.OpenPosition = pos.ToString();
		StoreController.Instance.CurrStoreAnalysisData.StoreEntrance = StoreType.SpecialOffer.ToString();
		AnalysisManager.Instance.OpenShop();*/
		_storeAnalysisData = new StoreAnalysisData ();
		_storeAnalysisData.OpenPosition = pos.ToString ();
		_storeAnalysisData.StoreEntrance = StoreType.SpecialOffer.ToString ();
	}

	void HideIcon()
	{
		if (_specialOfferIcon) 
		{
			if(_specialOfferIcon.activeSelf)
				_specialOfferIcon.SetActive (false);
			_specialOfferIcon = null;
		}
	}

	bool ShouldShow()
	{
		return GroupConfig.Instance.IsProductExist (StoreType.SpecialOffer);
	}
}
