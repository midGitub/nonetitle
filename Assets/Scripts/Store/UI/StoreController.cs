using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using UnityEngine.Purchasing;
using System;

public enum StoreType
{
    Buy,
    Deal,
    SmallBuy,
    RemoveAd,
    PiggyBank,
    Deal_TL,
    CrazyDice,
    WheelOfLuck,
    SpecialOffer,
}

public class StoreController : Singleton<StoreController>
{
    public Canvas StoreCanvas;
    public Canvas PopUpCanvas;

    public GameObject Tab;
    public GameObject SuccesText;
    public GameObject FaildText;
    public GameObject WaitingText;
    public GameObject TryAgainButton;

    public GameObject BigStore;
    public GameObject ThreeStore;
    public GameObject TwoStore;
    public GameObject TimeLimitedStore;

    public BuySuccessUI BuySuccessUIScript;

    public GameObject NoADS;

    public WidgetJumpController _jumpController;

    [HideInInspector]
    public BigStoreController bigStoreController;
    [HideInInspector]
    public ThreeStoreController threeStoreController;
    [HideInInspector]
    public TwoStoreController twoStoreController;
    [HideInInspector]
    public NoADsController noADsController;

    // Use this for initialization
    private bool _isrevalidationing = false;

    public StoreAnalysisData CurrStoreAnalysisData = new StoreAnalysisData();

    private bool _isAtLeastOneStoreOpening = false;

    public bool IsAtLeastOneStoreOpening { get { return _isAtLeastOneStoreOpening; } }

    // 检测间隔（判断当前是否在startloading的场景）
    public float _purchaseSuccessCheckInterval = 0.3f;

    private void OnEnable()
    {
        StoreManager.Instance.StartPrePurchaseEvent.AddListener(OnStartPrePurchase);
        StoreManager.Instance.StartPurchaseEvent.AddListener(OnStartPurchase);
        StoreManager.Instance.OnPurchaseCompletedEvent.AddListener(OnPurchaseSuccess);
        StoreManager.Instance.OnPurchaseFailedEvent.AddListener(OnPurchaseFailed);
    }

    private void OnDisable()
    {
        StoreManager.Instance.StartPrePurchaseEvent.RemoveListener(OnStartPrePurchase);
        StoreManager.Instance.StartPurchaseEvent.RemoveListener(OnStartPurchase);
        StoreManager.Instance.OnPurchaseCompletedEvent.RemoveListener(OnPurchaseSuccess);
        StoreManager.Instance.OnPurchaseFailedEvent.RemoveListener(OnPurchaseFailed);
    }

    private void Start()
    {
        StoreNetWorkServer.Instance.TryStartRevalidation();

        bigStoreController = BigStore.GetComponent<BigStoreController>();
        threeStoreController = ThreeStore.GetComponent<ThreeStoreController>();
        twoStoreController = TwoStore.GetComponent<TwoStoreController>();
        noADsController = NoADS.GetComponent<NoADsController>();
    }

    public void Test()
    {
        //IAPData ip = new IAPData("1", "1");
        //UserBasicData.Instance.SetIAPData(ip);
    }

    private void CloseAllStoreFunc()
    {
        if (StoreCanvas != null)
            StoreCanvas.gameObject.SetActive(false);
        if (BigStore != null)
            BigStore.SetActive(false);
        if (ThreeStore != null)
            ThreeStore.SetActive(false);
        if (TwoStore != null)
            TwoStore.SetActive(false);
        if (NoADS != null)
            NoADS.SetActive(false);
        if (TimeLimitedStore != null)
            TimeLimitedStore.SetActive(false);

        _isAtLeastOneStoreOpening = false;
    }

    private void CloseAllStore(Action callBack, bool doImmediately = false)
    {
        if (!doImmediately)
        {
            if (_jumpController != null)
            {
                _jumpController.Open(false, () =>
                    {
                        CloseAllStoreFunc();
                        callBack();
                    });
            }
            else
            {
                CloseAllStoreFunc();
                callBack();
            }
        }
        else
        {
            CloseAllStoreFunc();
            callBack();
        }

    }

    private void ShowStore(GameObject go)
    {
        StoreCanvas.gameObject.SetActive(true);

        BigStore.SetActive(false);
        ThreeStore.SetActive(false);
        TwoStore.SetActive(false);
        NoADS.SetActive(false);
        TimeLimitedStore.SetActive(false);
        go.SetActive(true);

        _isAtLeastOneStoreOpening = true;
    }

    private void ShowStore(StoreType st)
    {
        switch (st)
        {
            case StoreType.Buy:
                ShowStore(BigStore);
                break;
            case StoreType.Deal:
                ShowStore(ThreeStore);
                break;
            case StoreType.Deal_TL:
                ShowStore(TimeLimitedStore);
                break;
            case StoreType.RemoveAd:
                ShowStore(NoADS);
                break;
            case StoreType.SmallBuy:
                ShowStore(TwoStore);
                break;
            default:
                break;
        }
    }

    public bool IsStoreOpening(StoreType st)
    {
        bool result;
        switch (st)
        {
            case StoreType.Buy:
                result = BigStore.activeInHierarchy;
                break;
            case StoreType.Deal:
                result = ThreeStore.activeInHierarchy;
                break;
            case StoreType.Deal_TL:
                result = TimeLimitedStore.activeInHierarchy;
                break;
            case StoreType.RemoveAd:
                result = NoADS.activeInHierarchy;
                break;
            case StoreType.SmallBuy:
                result = TwoStore.activeInHierarchy;
                break;
            default:
                result = false;
                break;
        }
        return result;
    }

    public void OpenBigStoreUIForSelf()
    {
        OpenStoreUI();
    }

    public void OpenStoreUI(string openpostion = "Auto", StoreType st = StoreType.Buy)
    {
        StoreCanvas.worldCamera = Camera.main;
        ShowStore(st);
        CurrStoreAnalysisData = new StoreAnalysisData();
        CurrStoreAnalysisData.OpenPosition = openpostion;
        CurrStoreAnalysisData.StoreEntrance = st.ToString();

        StoreManager.Instance.InitStore();
        AnalysisManager.Instance.OpenShop();
    }

    public void CloseAllStoreUI(Action callBack, bool doImmediately = false)
    {
        if (StoreCanvas.gameObject.activeSelf)
        {
            AnalysisManager.Instance.CloseShop();
            CloseAllStore(callBack, doImmediately);
        }
        else
        {
            callBack();
        }
    }

    void OnStartPrePurchase(string id)
    {
        UpdateShowTab(WaitingText);
    }

    void OnStartPurchase(string id)
    {
        UpdateShowTab(WaitingText);

        var iapItemD = IAPCatalogConfig.Instance.FindIAPItemByID(id);
        var ipa = StoreManager.Instance.GetProductById(id);
        CurrStoreAnalysisData.LocalItemId = ipa.definition.id;
        CurrStoreAnalysisData.StoreSpecificId = ipa.definition.storeSpecificId;
        CurrStoreAnalysisData.RealMoney = iapItemD.Price;
        CurrStoreAnalysisData.OrderID = DeviceUtility.GetDeviceId() + "_" + System.DateTime.Now;
        AnalysisManager.Instance.StartPurchase();
    }

    void OnPurchaseSuccess(IAPData data)
    {
        if (StoreManager.Instance.Controller == null)
        {
            Debug.LogError("OnPurchaseSuccess: Controller is null");
            Debug.Assert(false);
        }
        else
        {
            Product item = StoreManager.Instance.Controller.products.WithID(data.LocalItemId);
            Debug.Log("Purchase success: itemId: " + item.definition.id + ", " + item.metadata.localizedTitle);
        }

        string itemId = data.LocalItemId;
        var iapT = IAPCatalogConfig.Instance.FindIAPItemByID(itemId);

        float credits = IAPCatalogConfig.Instance.GetCreditsWithPromotion(iapT);
        float creditsWithoutPromotion = IAPCatalogConfig.Instance.GetCreditsWithoutPromotion(iapT);
        int vippoint = IAPCatalogConfig.Instance.GetVIPPoint(iapT);

        StopAllCoroutines();

        SaveBuyItem.SaveBuyItemToData(itemId, credits, creditsWithoutPromotion, vippoint);
        AnalysisManager.Instance.SuccessReceiveIAP(itemId, (long)credits, vippoint);

        UserBasicData.Instance.RecordPayAmount(iapT.Price, true);

        string currentSceneName = ScenesController.Instance.GetCurrSceneName();
        if (currentSceneName.Equals(ScenesController.StartLoadingSceneName))
        {
            // 延迟到场景为非StartLoading
            StartCoroutine(DelayBuySuccessUIShow(data, (ulong)credits, StoreNetWorkServer.Instance.IsRevalidationing));
        }
        else
        {
            BuySucces(data, (ulong)credits, StoreNetWorkServer.Instance.IsRevalidationing);
        }

        UserDeviceLocalData.Instance.SetIAPState(StoreManager.Instance.currentReceiptID, IAPData.IAPState.Verified);
        UserDeviceLocalData.Instance.RemoveIAPData(data.Receipt);
    }

    void OnPurchaseFailed(bool showTryAgain, IAPData data, PurchaseFailureReason reason)
    {
        LogUtility.Log("Purchase failed: itemId: " + data.LocalItemId + ", reason: " + reason.ToString(), Color.red);
        FaildBuy(showTryAgain);

        //not call UserDeviceLocalData.Instance.RemoveIAPData, since I'm not sure if it would cause any problem
    }

    IEnumerator DelayBuySuccessUIShow(IAPData data, ulong rewardCredis, bool isRevalidPurchase = false)
    {
        while (ScenesController.Instance.GetCurrSceneName().Equals(ScenesController.StartLoadingSceneName))
        {
            yield return new WaitForSeconds(_purchaseSuccessCheckInterval);
        }
        BuySucces(data, rewardCredis, isRevalidPurchase);
    }

    void BuySucces(IAPData data, ulong rewardCredis, bool isRevalidPurchase = false)
    {
        //because we can't canculate iap's credits amount(after purchase, user vip lv may upgrade, 
        //IAPCatalogConfig.Instance.GetCreditsWithPromotion(item) return a unreliable result,),so we pass
        //iap's credits amount var rewardCredis
        string localItemId = data.LocalItemId;
        IAPCatalogData item = IAPCatalogConfig.Instance.FindIAPItemByID(localItemId);
        int vippoint = IAPCatalogConfig.Instance.GetVIPPoint(item);

        if (isRevalidPurchase)
        {
            CloseShowTab();
            GameObject compensation = UIManager.Instance.LoadPopupAtPath(UIManager.CompensationUiPath);
            CompensationUiController controller = compensation.GetComponent<CompensationUiController>();
            controller.HandleRevalidPurchase(data);
        }
        else
        {
            CitrusEventManager.instance.Raise(new OnStorePurchaseSucceed(data));

            // 小猪存钱罐不展示
            if (item.Title.Contains("PiggyBank"))
            {
                // 关闭所有商城UI
                // ??Xhj 没弄清楚这里的关闭商城意图
                //CloseAllStoreUI();
                CloseShowTab();
            }
            else if (item.Title.Contains("Dice"))
            {
                CloseShowTab();
                DiceManager.Instance.HavePaidForPlaying = true;
                DiceManager.Instance.OnPaymentValid(data);
            }
            else if (item.Title.Contains("WheelOfLuck"))
            {
            }
            else if (item.Title.Contains("TimeLimited"))
            {
                Debug.Log(rewardCredis + " " + item.CREDITS);
                UpdateShowTab(SuccesText);
                BuySuccessUIScript.Show(IAPCatalogConfig.Instance.GetItemImageByPrice(item.Price), rewardCredis,
                    StringUtility.FormatNumberStringWithComma((ulong)vippoint));
                TLStoreController.Instance.BuySuccess();
            }
            else if (item.Title.Contains("BecomeAVIP"))
            {
                UpdateShowTab(SuccesText);
                BuySuccessUIScript.Show(IAPCatalogConfig.Instance.GetItemImageByPrice(item.Price), rewardCredis,
                    StringUtility.FormatNumberStringWithComma((ulong)vippoint));
                SpecialOfferHelper.Instance.BuySuccess();
            }
            else if (item.Title.Contains("MORE"))
            {
                UpdateShowTab(SuccesText);
                BuySuccessUIScript.Show(IAPCatalogConfig.Instance.GetItemImageByID(localItemId), rewardCredis,
                    StringUtility.FormatNumberStringWithComma((ulong)vippoint));
            }
            else
            {
                // 通用展示
                // 然而...仅限前7个商品
                UpdateShowTab(SuccesText);
                BuySuccessUIScript.Show(IAPCatalogConfig.Instance.GetItemImageByID(localItemId), rewardCredis,
                    StringUtility.FormatNumberStringWithComma((ulong)vippoint));
            }
        }
    }

    void FaildBuy(bool showTryAgainButton)
    {
        UpdateShowTab(FaildText);
        TryAgainButton.SetActive(showTryAgainButton);
    }

    void UpdateShowTab(GameObject showGameObject)
    {
        PopUpCanvas.worldCamera = Camera.main;
        Tab.SetActive(true);
        SuccesText.SetActive(false);
        FaildText.SetActive(false);
        WaitingText.SetActive(false);
        showGameObject.SetActive(true);
    }

    public void CloseShowTab()
    {
        Tab.SetActive(false);
        SuccesText.SetActive(false);
        FaildText.SetActive(false);
        WaitingText.SetActive(false);
    }
}
