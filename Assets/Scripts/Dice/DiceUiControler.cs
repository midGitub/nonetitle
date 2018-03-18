using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using CitrusFramework;

public class DiceUiControler : MonoBehaviour {

	private enum DiceCount
	{
		OneDice = 1,
		TwoDice = 2
	}

    [SerializeField] private Canvas _cavas;

    [SerializeField] private Text _diceInfoline2;
	[SerializeField] private Text _diceInfoline5;
	[SerializeField] private Text _diceInfoline6;
	[SerializeField] private Text _extraCredits;
	[SerializeField] private Text _extraVip2;
	[SerializeField] private Text _price2;
	[SerializeField] private Text _rollText;	
	[SerializeField] private Text _resultRatio;
    [SerializeField] private Text _switchToDiceCount;
    [SerializeField] private Text _switchToDiceCount1;
    [SerializeField] private Text _confirmLeaveText;
    [SerializeField] private Text _sureLeaveText;
    [SerializeField] private Text _cancelLeaveText;
    [SerializeField] private Text _maxCreditsCanGetText;
    [SerializeField] private Text _leavePagePriceText;
    [SerializeField] private Text _winUpToText;
    [SerializeField] private Text _rollNowText;
    [SerializeField] private Text _notNowText;

	[SerializeField] private Button _rollButton;
	[SerializeField] private Button _quitButton;
    [SerializeField] private ScriptButton _switchDiceCountButton;
    [SerializeField] private Button _sureLeaveButton;
    [SerializeField] private Button _cancelLeaveButton;

    [SerializeField] private GameObject _arrowEffect;
    [SerializeField] private GameObject _switchDiceCountObj;
    [SerializeField] private GameObject _exitButton;
	[SerializeField] private GameObject _xRatioEffect;
    [SerializeField] private GameObject _confirmLeavePage;
    [SerializeField] private GameObject _refreshTextEffect;

    public CloseGameObject CloseGameObject;
	public DiceControler DiceControler;
	public DicePaymentUi PaymentPage;
    public Animator UiAnimator;

	private readonly float _effectTime = 2f;
	private WindowInfo _windowInfoReceipt;

	void Start()
	{
		CitrusEventManager.instance.AddListener<DiceGameStartEvent>(OnGameStart);
        CitrusEventManager.instance.AddListener<PayForMoreDicesEvent>(OnPayForMoreDices);
        CitrusEventManager.instance.AddListener<ResetDiceCountEvent>(OnResetDicesCount);
		CitrusEventManager.instance.AddListener<DiceGameEndEvent>(OnGameEnd);

        ButtonRegisterCallback();
	}

	void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<DiceGameStartEvent>(OnGameStart);
        CitrusEventManager.instance.RemoveListener<PayForMoreDicesEvent>(OnPayForMoreDices);
        CitrusEventManager.instance.RemoveListener<ResetDiceCountEvent>(OnResetDicesCount);
        CitrusEventManager.instance.RemoveListener<DiceGameEndEvent>(OnGameEnd);

        ButtonUnRegisterCallback();
    }

	public void Show()
	{
        if (_windowInfoReceipt == null)
		{
			_windowInfoReceipt = new WindowInfo(() =>
			    {
                    gameObject.SetActive(true);
			        _cavas.gameObject.SetActive(true);
			    }
				, ManagerClose, _cavas, ForceToCloseImmediately);
			WindowManager.Instance.ApplyToOpen(_windowInfoReceipt); 
		}

		_cavas.worldCamera = Camera.main;
		
		Init();
	}

    public void SelfClose(Action callback)
	{
		if(CloseGameObject != null)
		{
			CloseGameObject.Close(_cavas.gameObject, callback);
		}
	}

	public void Hide()
	{
		AudioManager.Instance.StopSound(AudioType.M10_WheelBGM);

		SelfClose(() => {
			WindowManager.Instance.TellClosed(_windowInfoReceipt);
			_windowInfoReceipt = null;
		});
			
		PaymentPage.Hide();
	}

	public void ManagerClose(Action<bool> callBack)
	{
		if (UGUIUtility.CanObjectBeClickedNow(_cavas, _exitButton.gameObject))
		{
			if(CloseGameObject != null)
			{
				CloseGameObject.Close(_cavas.gameObject, () => {
					_windowInfoReceipt = null;
					callBack(true);
				});
				ScoreAppSystem.Instance.SetClosePageTime();
			}
			callBack(true);
		}
		else
		{
			callBack(false);
		}
	}

	public void ForceToCloseImmediately()
	{
		_cavas.gameObject.SetActive(false); 
		_windowInfoReceipt = null;
	}
		

	public void Init()
	{
		StoreManager.Instance.InitStore();
		PlayerDiceData playerDiceData = DiceManager.Instance.PlayerDiceData;
	    InitUiElemsShowState();
        InitTexts(playerDiceData);
	    EnableButtons(true);
	}

    void InitUiElemsShowState()
    {
        DiceData data = DiceManager.Instance.GetPayForMoreDicesData();
        bool showSwitchButton = data.DiceNum != DiceManager.Instance.PlayerDiceData.DiceData.DiceNum;

        _switchDiceCountObj.SetActive(showSwitchButton);
        _arrowEffect.SetActive(true);
    }

    private void InitTexts(PlayerDiceData playerDiceData)
	{
		DiceData info = playerDiceData.DiceData;
		DiceCount countType = (DiceCount)info.DiceNum;
	    int IAPId = DiceConfig.Instance.GetIAPIdByDifferentUser(info);

		IAPCatalogData iapData = IAPCatalogConfig.Instance.FindIAPItemByID(IAPId.ToString());
		int vipPoints = iapData.OneDLAddVIPPoint;

		switch(countType)
		{
			case DiceCount.OneDice:
				ShowTexts(true);
				_diceInfoline2.text = string.Format(LocalizationConfig.Instance.GetValue("dice_infoTextLine2_oneDice"), StringUtility.FormatNumberString(playerDiceData.WinCredits, true, true));
				_diceInfoline5.text = string.Format(LocalizationConfig.Instance.GetValue ("dice_infoTextLine5"), vipPoints);
				_diceInfoline6.text = string.Format(LocalizationConfig.Instance.GetValue ("dice_infoTextLine6"), StoreManager.Instance.GetPriceString(iapData));
				break;

			case DiceCount.TwoDice:
				ShowTexts (false);
				_diceInfoline2.text = string.Format (LocalizationConfig.Instance.GetValue ("dice_infoTextLine2_twoDice"), StringUtility.FormatNumberString (playerDiceData.WinCredits, true, true));
				_extraCredits.text = string.Format (LocalizationConfig.Instance.GetValue ("dice_extraCredits"), StringUtility.FormatNumberString ((ulong)iapData.CREDITS, true, true));
				_extraVip2.text = string.Format(LocalizationConfig.Instance.GetValue ("dice_infoTextLine5"), vipPoints);
				_price2.text = string.Format(LocalizationConfig.Instance.GetValue ("dice_infoTextLine6"), StoreManager.Instance.GetPriceString(iapData));
				break;
		}

		_rollText.text = LocalizationConfig.Instance.GetValue("dice_rollText");
		_resultRatio.text = "?";

        int curDiceCount = DiceManager.Instance.PlayerDiceData.DiceData.DiceNum;
	    int origDiceCount = DiceManager.Instance.GetOrigDiceData().DiceNum;
        int switchDiceCount = DiceManager.Instance.GetPayForMoreDicesData().DiceNum;
        int curSwitchShowNum = curDiceCount == origDiceCount ? switchDiceCount: origDiceCount;
        _switchToDiceCount.text = curSwitchShowNum.ToString();
        _switchToDiceCount1.text = curSwitchShowNum.ToString();
    }

	private void ShowTexts(bool onlyOneDice)
	{
		_diceInfoline5.gameObject.SetActive(onlyOneDice);
		_diceInfoline6.gameObject.SetActive(onlyOneDice);
		_extraCredits.gameObject.SetActive(!onlyOneDice);
		_extraVip2.gameObject.SetActive(!onlyOneDice);
		_price2.gameObject.SetActive(!onlyOneDice);
	}

    void EnableButtons(bool enable)
    {
        EnableButton(_rollButton, enable);
        EnableButton(_quitButton, enable);
        _switchDiceCountButton.enabled = enable;
    }

    #region button callbacks

    void ButtonRegisterCallback()
    {
        _switchDiceCountButton.onclick.AddListener(OnSwitchDiceCountButtonClick);
        _sureLeaveButton.onClick.AddListener(OnConfirmLeaveButtonClick);
        _cancelLeaveButton.onClick.AddListener(OnCancelLeaveButtonClick);
    }

    void ButtonUnRegisterCallback()
    {
        _switchDiceCountButton.onclick.RemoveListener(OnSwitchDiceCountButtonClick);
        _sureLeaveButton.onClick.RemoveListener(OnConfirmLeaveButtonClick);
        _cancelLeaveButton.onClick.RemoveListener(OnCancelLeaveButtonClick);
    }

    void OnSwitchDiceCountButtonClick()
    {
        AudioManager.Instance.PlaySound(AudioType.Click);

        int origDiceNum = DiceManager.Instance.GetOrigDiceData().DiceNum;
        bool reset = DiceManager.Instance.PlayerDiceData.DiceData.DiceNum != origDiceNum;

        if (reset)
        {
            DiceManager.Instance.ResetDiceCount();
        }
        else
        {
            DiceManager.Instance.PlayerWantMoreDices();
        }
    }

    public void OnRollButtonClick()
    {
        DiceManager.Instance.SaveDiceDataToDevice();
        if (!DiceManager.Instance.HavePaidForPlaying)
        {
            DiceManager.Instance.PayForPlayingGame();
        }
    }

    public void OnCloseButtonClick()
    {
        AudioManager.Instance.PlaySound(AudioType.Click);
        _confirmLeavePage.SetActive(true);
        InitConfirmLeavePage();
    }

    void InitConfirmLeavePage()
    {
        PlayerDiceData data = DiceManager.Instance.PlayerDiceData;

        _maxCreditsCanGetText.text = StringUtility.FormatNumberString((ulong)data.DiceData.DiceNum 
            * (ulong)DiceHelper.Instance.SingleDiceMaxPoint * data.WinCredits, true, true);
        _leavePagePriceText.text = string.Format(LocalizationConfig.Instance.GetValue("dice_infoTextLine6"),
            StoreManager.Instance.GetPriceString(IAPCatalogConfig.Instance.FindIAPItemByID(DiceConfig.Instance.GetIAPIdByDifferentUser(data.DiceData).ToString())));
        _winUpToText.text = LocalizationConfig.Instance.GetValue("dice_winUpTo");
        _rollNowText.text = LocalizationConfig.Instance.GetValue("dice_rollNow");
        _notNowText.text = LocalizationConfig.Instance.GetValue("dice_notNow");
    }

    void OnConfirmLeaveButtonClick()
    {
        UserDeviceLocalData.Instance.CloseDicePageCount += 1;

        //if player close page more than 3 times, clean close page state
        if (UserDeviceLocalData.Instance.CloseDicePageCount >= 3)
        {
            UserDeviceLocalData.Instance.LastCloseDicePageDate = DateTime.Now;
            UserDeviceLocalData.Instance.CloseDicePageCount = 0;
        }

        _confirmLeavePage.SetActive(false);
        Hide();
    }

    void OnCancelLeaveButtonClick()
    {
        AudioManager.Instance.PlaySound(AudioType.Click);
        _confirmLeavePage.SetActive(false);
    }

    #endregion

    void OnGameStart(DiceGameStartEvent e)
	{
        EnableButtons(false);
        _arrowEffect.SetActive(false);
    }

    void RefreshTexts()
    {
        PlayEffect(_refreshTextEffect);
        InitTexts(DiceManager.Instance.PlayerDiceData);
    }

    void PlayEffect(GameObject effect)
    {
        ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Play();
        }
    }

    void OnPayForMoreDices(PayForMoreDicesEvent e)
    {
        RefreshTexts();
    }

    void OnResetDicesCount(ResetDiceCountEvent e)
    {
        RefreshTexts();
    }

    void OnGameEnd(DiceGameEndEvent e)
	{
		_resultRatio.text = DiceManager.Instance.ResultRatio.ToString();

		Animator ratioAnimator = _resultRatio.GetComponent<Animator>();
		if (ratioAnimator != null) 
		{
			ratioAnimator.SetTrigger("play");
		}

		ListUtility.ForEach(_xRatioEffect.GetComponentsInChildren<ParticleSystem>(), (x) => {x.Play();});

		//TODO fix int*int issue
		StartCoroutine(ShowPaymentPage((int)DiceManager.Instance.PlayerDiceData.WinCredits * DiceManager.Instance.ResultRatio));
	}

	private IEnumerator ShowPaymentPage(long winCredits)
	{
		yield return new WaitForSeconds(_effectTime);

		PaymentPage.Show((ulong)winCredits);
	}

	private void EnableButton(Button button, bool enable)
	{
		if (button != null) 
		{
			button.enabled = enable;
		}
	}
}
