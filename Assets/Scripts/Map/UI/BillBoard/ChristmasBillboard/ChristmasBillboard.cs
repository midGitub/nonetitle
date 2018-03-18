using System;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasBillboard : BillBoardBase
{
    public MultiTextCountdown Countdown;
    public Button ResponseButton;

    void Start()
    {
        ResponseButton.onClick.AddListener(ShowChrismasMaxWinPopup);
        StartCounter();
    }

    void ShowChrismasMaxWinPopup()
    {
        UIManager.Instance.ShowPopup<RegisterMaxWinUiController>(UIManager.ChristmasActivityUiPath);
    }

    void StartCounter()
    {
        TimeSpan leftTime = TimeUtility.CountdownOfDateFromNowOn(RegisterMaxWinActivity.Instance.CurActivityDateInfo.EndDate);
        StartCoroutine(Countdown.StartTimer(leftTime, RemoveSelfWhenActivityOver));
    }

    void RemoveSelfWhenActivityOver()
    {
        BillBoardManager.Instance.Delete(this);
    }
}
