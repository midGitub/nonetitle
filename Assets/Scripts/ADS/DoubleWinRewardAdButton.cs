using UnityEngine;
using UnityEngine.UI;

public class DoubleWinRewardAdButton : BaseRewardAdButton
{

    public PopUpControler PopupController;
    [SerializeField] private Text _content;
    [SerializeField] private Text _nextSpinText;
    [SerializeField] private Text _timer;
    void Start()
    {
        SetButtonClickListener();
    }

    void OnEnable()
    {
        Init();
        InitTexts();

        ShowAdButton(false);
        RewardAdController.TryShowAdButton();
    }

    protected override void Init()
    {
        AdTypeName = "doubleWin";
        base.Init();
    }

    void InitTexts()
    {
        _content.text = LocalizationConfig.Instance.GetValue("bonusAd_win");
        _nextSpinText.text = LocalizationConfig.Instance.GetValue("bonusAd_nextSpin");
    }

    public override void OnButtonClick()
    {
        AudioManager.Instance.PlaySound(AudioType.Click);
        PopupController.Init();
        ShowPopUp(true);
    }

    public override void OnAdShow()
    {
        base.OnAdShow();

        DoubleWinAdUiController popupCtrl = PopupController as DoubleWinAdUiController;
        Debug.Assert(popupCtrl != null, "bonusAdModule : fail to parse PopupController as DoubleWinAdUiController");

        if (popupCtrl != null)
        {
            StartCoroutine(popupCtrl.SetTimer(RewardAdController.AdDurationTime));
        }

        StartCoroutine(TimeUtility.StartTimerMMSS(NetworkTimeHelper.Instance.GetNowTime(), _timer, RewardAdController.AdDurationTime, () => { ShowAdButton(false); }));  
    }

    public override void OnAdClose()
    {
        base.OnAdClose();
        ShowPopUp(false);
    }

    public void ShowPopUp(bool show)
    {
        if (show)
        {
            PopupController.Open();
        }
        else
        {
            PopupController.Close();
        }
    }
}
