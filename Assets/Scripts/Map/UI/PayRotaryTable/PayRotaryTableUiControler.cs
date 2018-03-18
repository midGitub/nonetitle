using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.UI;

public class PayRotaryTableUiControler : PopUpControler
{
    public Button SpinButton;
    public Button ExitButton;

    public PayRotaryTableControler PRtControler;

    [SerializeField] private Text _priceText;
    [SerializeField] private Text _smallerPriceText;
    [SerializeField] private GameObject _spinStartButton;

    private Vector3 _initRotaryRotation;
    private Dictionary<GameObject, bool> _activeThingsDic = new Dictionary<GameObject, bool>();

    void Start()
    {
        _initRotaryRotation = PRtControler.RotaryTransform.eulerAngles;

        SpinButton.onClick.AddListener(OnSpinButtonClick);
        ExitButton.onClick.AddListener(OnExitButtonClick);
        _spinStartButton.SetActive(false);

        InitActiveThings();
        CitrusEventManager.instance.AddListener<OnStorePurchaseSucceed>(CheckPurchaseInfo);
    }

    void OnDestroy()
    {
        SpinButton.onClick.RemoveListener(OnSpinButtonClick);
        ExitButton.onClick.RemoveListener(OnExitButtonClick);
        UnRegisterCloseButton(ExitButton);

        CitrusEventManager.instance.RemoveListener<OnStorePurchaseSucceed>(CheckPurchaseInfo);
    }

    //付费转盘复用免费转盘的结算界面，本身付费转盘应该属于daybonus prefab的一部分，不应该单独做一个prefab(因为弹窗机制要保证只显示一个ui弹窗)
    //所以付费转盘不受弹窗机制的管理，不应该是这样子，但是设计有问题，只好这样做了
    public override void Open()
    {
        CanvasComponent.worldCamera = Camera.main;
        StoreManager.Instance.InitStore();
        SetVisibleObjs();

        InitText();
        InitButtons();
        InitRotary();
        InitLightBar();
        CanvasComponent.gameObject.SetActive(true);
    }

    public override void Close()
    {
        CanvasComponent.gameObject.SetActive(false);
    }

    private void InitActiveThings()
    {
        _activeThingsDic.Clear();
        for (int i = 0; i < PRtControler.gameObject.transform.childCount; i++)
        {
            GameObject child = PRtControler.gameObject.transform.GetChild(i).gameObject;
            _activeThingsDic.Add(child, child.activeSelf);
        }
    }

    private void SetVisibleObjs()
    {
        foreach (var item in _activeThingsDic)
        {
            item.Key.SetActive(item.Value);
        }
    }

    protected override void InitText()
    {
        _priceText.text =
            StoreManager.Instance.GetPriceString(IAPCatalogConfig.Instance.FindIAPItemByID(PayRotaryTableSystem.Instance.PRTPurchaseItemId));
        _smallerPriceText.text =
            StoreManager.Instance.GetPriceString(IAPCatalogConfig.Instance.FindIAPItemByID(PayRotaryTableSystem.Instance.PRTPurchaseItemId));
    }

    private void InitButtons()
    {
        SpinButton.enabled = true;
        ExitButton.enabled = true;
        _spinStartButton.SetActive(false);
    }

    private void InitRotary()
    {
        PRtControler.RotaryTransform.eulerAngles =  _initRotaryRotation;
    }

    private void InitLightBar()
    {
        PRtControler.LightBar.OpenGameObject.SetActive(false);
        PRtControler.LightBar.CloseGameObject.SetActive(false);
    }

    public void OnSpinButtonClick()
    {
       PayRotaryTableSystem.Instance.PayForPlayingGame();
    }

    public void CheckPurchaseInfo(OnStorePurchaseSucceed e)
    {
        IAPCatalogData item = IAPCatalogConfig.Instance.FindIAPItemByID(e.Data.LocalItemId);
		if (item != null && item.Title.Contains("WheelOfLuck"))
        {
            StoreController.Instance.CloseShowTab();
            PayRotaryTableData result = PayRotaryTableSystem.Instance.GameResultData(UserBasicData.Instance.BonusDaysType, e.Data);
            PlayRotateAnim(result);
        }
    }

    public void PlayRotateAnim(PayRotaryTableData data)
    {
        PRtControler.StartRotate(data);

        SpinButton.enabled = false;
        ExitButton.enabled = false;
        _spinStartButton.SetActive(true);
    }

    private void OnExitButtonClick()
    {
        Close();
        CitrusEventManager.instance.Raise(new OnPayRotaryTableOver(null));
    }
}
