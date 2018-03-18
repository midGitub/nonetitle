using System;
using UnityEngine;
using UnityEngine.UI;

public class DoubleHourBonusUiController : PopUpControler
{
    public Button CloseButton;
    public MultiTextCountdown Countdown;

    void OnEnable()
    {
        StartTimer();
    }

    public override void Init()
    {
        base.Init();
        RegisterCloseButton(CloseButton);
    }

    void StartTimer()
    {
        TimeSpan leftTime = TimeUtility.CountdownOfDateFromNowOn(DoubleHourBonusActivity.Instance.EndDate);
        StartCoroutine(Countdown.StartTimer(leftTime, Close));
    }
}
