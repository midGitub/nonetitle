using System;
using UnityEngine;
using UnityEngine.UI;

public class DoubleHourBonusBillboard : BillBoardBase
{
    public MultiTextCountdown Countdown;
    public Button ResponseButton;

    void Start()
    {
        ResponseButton.onClick.AddListener(ShowDoubleHourBonusPopup);
        StartCounter();
    }

    void ShowDoubleHourBonusPopup()
    {
        UIManager.Instance.ShowPopup<DoubleHourBonusUiController>(UIManager.DoubleHourBonusPopupPath);
    }

    void StartCounter()
    {
        TimeSpan leftTime = TimeUtility.CountdownOfDateFromNowOn(DoubleHourBonusActivity.Instance.EndDate);
        StartCoroutine(Countdown.StartTimer(leftTime, RemoveSelfWhenActivityOver));
    }

    void RemoveSelfWhenActivityOver()
    {
        BillBoardManager.Instance.Delete(this);
    }
}
