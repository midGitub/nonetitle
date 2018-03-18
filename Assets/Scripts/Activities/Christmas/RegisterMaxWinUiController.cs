using System;
using System.Threading;
using UnityEngine.UI;

public class RegisterMaxWinUiController : PopUpControler
{
    public Button CloseButton;
    public MultiTextCountdown Countdown;
    public Text NotifyRewardText;
    public int DelayNotifyHours;
    void OnEnable()
    {
        StartTimer();
        SetRewardText();
    }

    public override void Init()
    {
        base.Init();
        RegisterCloseButton(CloseButton);
    }

    void SetRewardText()
    {
        DateTime notifyDate = RegisterMaxWinActivity.Instance.CurActivityDateInfo.EndDate + new TimeSpan(DelayNotifyHours, 0, 0);
        NotifyRewardText.text = string.Format(LocalizationConfig.Instance.GetValue("chrismas_maxwin_popup_remark"), notifyDate.ToString("dd/MM/yyyy hh:mm t\\M"));
    }

    void StartTimer()
    {
        TimeSpan leftTime = TimeUtility.CountdownOfDateFromNowOn(RegisterMaxWinActivity.Instance.CurActivityDateInfo.EndDate);
        StartCoroutine(Countdown.StartTimer(leftTime, Close));
    }
}
