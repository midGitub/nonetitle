using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PiggyBankUIController : Singleton<PiggyBankUIController>
{
	public Text NowHavePiggyBanksCoins;
	public GameObject CoinsImage;
	//public Text PriceText;
	public Text OldPriceText;
	public GameObject OldPriceGameObject;
	public Animator PiggyBankAnimator;
	public Canvas PiggyBankCanvas;
	public GameObject BuyButton;
	public GameObject CloseBuyButton;
	public GameObject CloseButton;
	public Text BoxText;
    public GeneralStoreItem storeItem;
    [SerializeField]
    private Button _exitButton;

	private int currCoins;
    private ulong _creditsNumWhenBackrupt;

	// 检测间隔（判断当前是否在startloading的场景）
	public float _purchaseSuccessCheckInterval = 0.3f;

    public WindowInfo _windowInfoReceipt = null;

    private void Awake()
	{
		CitrusEventManager.instance.AddListener<BuyPiggyBankSuccessEvent>(BuyPiggyBankSuccess);
	}

	private void BuyPiggyBankSuccess(BuyPiggyBankSuccessEvent bpms)
	{
		// zhousen Patch : 在购买完小猪产品成功后，不点击完成，直接退游戏，再次进来，会重新接受到苹果商店发来的成功消息
		// 触发小猪动画界面，但是这个时候可能是在startloading界面，然后切换了scene，导致小猪的界面被切掉，进入大厅后引起卡死。
		// 这里需要判断当前scene是哪个，如果是startloading，需要做个延迟操作。
		string currentSceneName = ScenesController.Instance.GetCurrSceneName ();
		if (currentSceneName.Equals (ScenesController.StartLoadingSceneName)) {
			// 延迟到场景为非StartLoading
			StartCoroutine(DelayPiggyBankSuccessProcess());
		} else {
			PiggyBankSuccessProcess ();
		}
	}

	private IEnumerator DelayPiggyBankSuccessProcess(){
		while (ScenesController.Instance.GetCurrSceneName ().Equals (ScenesController.StartLoadingSceneName)) {
			yield return new WaitForSeconds (_purchaseSuccessCheckInterval);
		}
		PiggyBankSuccessProcess ();
	}

	private void PiggyBankSuccessProcess(){
		// 购买成功信息

		// 打开UI
        // Xhj 如果Canvas没有被激活则是刚进大厅时弹出的情况，反之则是正常打开小猪银行后购买成功的情况
        if (!PiggyBankCanvas.gameObject.activeInHierarchy)
        {
            AbnormalShow();
        }
        else
        {
            //可能需要动画
            BuyButton.SetActive(false);
            CloseButton.SetActive(false);
            CloseBuyButton.SetActive(true);
            StartCoroutine(AnimationEffect());
        }
		
	}

	private IEnumerator AnimationEffect()
	{
		PiggyBankAnimator.SetTrigger("buy");
		while(true)
		{
			if(PiggyBankAnimator.GetCurrentAnimatorStateInfo(0).IsName("piganimbreak"))
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		CitrusFramework.UnityTimer.Instance.StartTimer
					   (this, 0.7f, () =>
					   {
						   AudioManager.Instance.PlaySound(AudioType.PiggyBankOpen);
						   
					   });
		float waitTime = 1;
		yield return new WaitForSeconds(waitTime);
		float allTime = PiggyBankAnimator.GetCurrentAnimatorStateInfo(0).length;
		float startTime = 0;
		while(startTime <= allTime - waitTime)
		{
			startTime += Time.deltaTime;
			int coins = (int)Mathf.Lerp(currCoins, 0, startTime / (allTime - waitTime));
			Debug.Log(startTime + ":" + PiggyBankAnimator.GetCurrentAnimatorStateInfo(0).length +
				PiggyBankAnimator.GetCurrentAnimatorStateInfo(0).IsName("piganimbreak"));
			NowHavePiggyBanksCoins.text = StringUtility.FormatNumberStringWithComma((ulong)coins);
			yield return null;
		}

		BoxText.text = StringUtility.FormatNumberStringWithComma((ulong)currCoins);
		NowHavePiggyBanksCoins.gameObject.SetActive(false);
		CoinsImage.SetActive(false);
		CloseButton.SetActive(true);
		//yield return new WaitForSeconds(3);
		//NumberFrameGameObject.SetActive(false);
	}

	public void OpenUI()
	{
		if (PiggyBankCanvas != null) {
			PiggyBankCanvas.worldCamera = Camera.main;
			PiggyBankCanvas.gameObject.SetActive(true);
		    PiggyBankCanvas.sortingLayerName = StringDefine.SorintLayerUi;
		}
		if (BuyButton != null)
			BuyButton.SetActive(true);
		if (CloseBuyButton != null)
			CloseBuyButton.SetActive(false);
		if (CloseButton != null)
			CloseButton.SetActive(true);
		if (NowHavePiggyBanksCoins != null) 
			NowHavePiggyBanksCoins.gameObject.SetActive(true);
		if (CoinsImage != null)
			CoinsImage.SetActive(true);
		UpdateShowText();

		StoreManager.Instance.InitStore();
		AudioManager.Instance.PlaySound(AudioType.PiggyBankAppear);
	}

	private void UpdateShowText()
	{
		currCoins = UserBasicData.Instance.PiggyBankCoins;

        string strProductID = PiggyBankHelper.GetProductID();
        storeItem.ProductID = strProductID;
        storeItem.UpdateVal();

		NowHavePiggyBanksCoins.text = StringUtility.FormatNumberStringWithComma((ulong)UserBasicData.Instance.PiggyBankCoins);
	}

    public void Show(OpenPos openPosition)
    {
        if (_windowInfoReceipt == null)
        {
            switch (openPosition)
            {
                case OpenPos.Auto:
                    _windowInfoReceipt = new WindowInfo(BankruptcyOpen, ManagerClose, PiggyBankCanvas, ForceToCloseImmediately);
                    break;
                case OpenPos.GameUp:
                    _windowInfoReceipt = new WindowInfo(GameUpOpen, ManagerClose, PiggyBankCanvas, ForceToCloseImmediately);
                    break;
                case OpenPos.Lobby:
                    _windowInfoReceipt = new WindowInfo(LobbyOpen, ManagerClose, PiggyBankCanvas, ForceToCloseImmediately);
                    break;
            }

            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

    public void AbnormalShow()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(AbnormalOpen, ManagerClose, PiggyBankCanvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt); 
        }
    }

    public void Hide()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    private void StoreAnalysisInfo(string openPositon)
    {
        StoreController.Instance.CurrStoreAnalysisData = new StoreAnalysisData();
        StoreController.Instance.CurrStoreAnalysisData.OpenPosition = openPositon;
        StoreController.Instance.CurrStoreAnalysisData.StoreEntrance = StoreType.PiggyBank.ToString();
        AnalysisManager.Instance.OpenShop(); 
    }

    public void BankruptcyOpen()
    {
        //player may buy credits after backrupt, do not need open piggybank under this condition
        if (!IsPurchasedAfterBankrupt())
        {
            OpenUI();
            StoreAnalysisInfo(OpenPos.Auto.ToString());
        }
        else
        {
            if (_windowInfoReceipt != null)
                Hide();
        }
    }

    public void RecordCriditsWhenBckrupt(ulong credits)
    {
        _creditsNumWhenBackrupt = credits;
    }

    bool IsPurchasedAfterBankrupt()
    {
        return UserBasicData.Instance.Credits > _creditsNumWhenBackrupt;
    }

    public void GameUpOpen()
    {
        OpenUI();   
        StoreAnalysisInfo(OpenPos.GameUp.ToString());
    }

    public void LobbyOpen()
    {
        OpenUI();
        StoreAnalysisInfo(OpenPos.Lobby.ToString());
    }

    public void AbnormalOpen()
    {
        OpenUI();
        //可能需要动画
        BuyButton.SetActive(false);
        CloseButton.SetActive(false);
        CloseBuyButton.SetActive(true);
        StartCoroutine(AnimationEffect());
    }

    private void SelfClose(Action callBack)
    {
        if (PiggyBankCanvas.gameObject.activeSelf){
            AudioManager.Instance.PlaySound(AudioType.Click); 
            PiggyBankCanvas.gameObject.SetActive(false);
        }
        callBack(); 
    }
   
    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(PiggyBankCanvas, _exitButton.gameObject))
        {
            PiggyBankCanvas.gameObject.SetActive(false);
            _windowInfoReceipt = null;
            callBack(true);
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        PiggyBankCanvas.gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }
//    private float GetPrice(string strProductID)
//	{
//        var item = IAPCatalogConfig.Instance.FindIAPItemByID(strProductID);
//		//float credits;
//
//#if UNITY_EDITOR
//		return item.Price;
//#endif
//
//		if(StoreManager.Instance.StoreContoller != null)
//		{
//            var onlineProduct = StoreManager.Instance.GetProductById(strProductID);
//			return (float)onlineProduct.metadata.localizedPrice;
//		}
//		else
//		{
//			return item.Price;
//		}
//	}
}
