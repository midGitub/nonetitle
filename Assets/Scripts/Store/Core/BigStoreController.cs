using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;
using UnityEngine.UI;

public class BigStoreController : MonoBehaviour {

	public Image TitleImage;
	public GameObject ADS;
	public GameObject ADSglow;
	public GameObject ADS1;
	public GameObject BuyCredits;
    [SerializeField]
    private Button _exitButton;

    private WindowInfo _windowInfoReceipt = null;
	private GameObject PromotionGameObject;
	private string _promotionPath = "Game/UI/BillBoard/";
	private bool _openWithoutWindowManager;

    public void Show(OpenPos openPos)
    {
        if (_windowInfoReceipt == null)
        {
            switch (openPos)
            {
                case OpenPos.Lobby:
                    _windowInfoReceipt = new WindowInfo(LobbyOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
                    break;
                case OpenPos.GameUp:
                    _windowInfoReceipt = new WindowInfo(GameUpOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
                    break;
                case OpenPos.GameBelow:
                    _windowInfoReceipt = new WindowInfo(GameBelowOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
                    break;
            }

            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt); 
        }
    }

    public void HideBigStore()
    {
        SelfClose(() => {
			if(!_openWithoutWindowManager)
            	WindowManager.Instance.TellClosed(_windowInfoReceipt);
			else 
				_openWithoutWindowManager = false;
            _windowInfoReceipt = null;
        });
    }

    public void LobbyOpen()
    {
        StoreController.Instance.OpenStoreUI(OpenPos.Lobby.ToString(), StoreType.Buy);
    }

    public void GameUpOpen()
    {
        StoreController.Instance.OpenStoreUI(OpenPos.GameUp.ToString(), StoreType.Buy);
    }

    public void GameBelowOpen()
    {
        StoreController.Instance.OpenStoreUI(OpenPos.GameBelow.ToString(), StoreType.Buy);
    }

	public void AutoOpen()
	{
		_openWithoutWindowManager = true;
		StoreController.Instance.OpenStoreUI(OpenPos.Auto.ToString(), StoreType.Buy);
	}

    private void SelfClose(Action callBack)
    {
        StoreController.Instance.CloseAllStoreUI(callBack);
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(StoreController.Instance.StoreCanvas, _exitButton.gameObject))
        {
            StoreController.Instance.CloseAllStoreUI(() => {
                _windowInfoReceipt = null; 
                callBack(true);
            });
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        StoreController.Instance.CloseAllStoreUI(() =>
            {
            }, true);
        _windowInfoReceipt = null;
    }

	void OnEnable()
	{
		bool flag = false;
		List<int> advance = PromotionHelper.Instance.PromotionAdvanceDayList;
		List<int> last = PromotionHelper.Instance.PromotionLastDayList;
		List<string> name = PromotionHelper.Instance.PromotionNameList;
		for(int i=0;i<PromotionHelper.Instance.PromotionLen;i++)
		{
			if (PromotionHelper.Instance.IsInPromotion(name [i], advance [i], last [i]))
			{
				ShowPromotion(name[i],last[i]);
				flag = true;
			}
		}
		if(!flag)	
			HidePromotion();
	}

	void ShowPromotion(string name,int lastday)
	{
		ADS.SetActive(false);
		ADSglow.SetActive(false);
		ADS1.SetActive(false);
		BuyCredits.SetActive(false);
		if (PromotionGameObject == null)
		{
			string nameTrim = name.TrimEnd(StringUtility._numberArray);
			PromotionGameObject = UGUIUtility.InstantiateUI (_promotionPath+nameTrim+"Title");
			PromotionGameObject.transform.SetParent(GameObject.Find("BigBGPanel").transform, false);
		}
		CountDown ct = PromotionGameObject.GetComponentInChildren<CountDown>();
		ct.CountingTime = PromotionHelper.Instance.PromotionTimeLeft(name,lastday);
		ct.TimeEvent.AddListener(HidePromotion);
		ct.count();
		PromotionGameObject.SetActive(true);
	}

	void HidePromotion()
	{
		ADS.SetActive(true);
		ADSglow.SetActive(true);
		ADS1.SetActive(true);
		BuyCredits.SetActive(true);
		if (PromotionGameObject != null)
		{
			PromotionGameObject.SetActive(false);
			PromotionGameObject = null;
		}
	}
}
