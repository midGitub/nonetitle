using System;
using System.Collections;
using CitrusFramework;
using UnityEngine;
using UnityEngine.UI;

public class MultiTextCountdown : MonoBehaviour
{
    public enum CountdownType
    {
        DHMS,
        HMS,
        MS,
    }

    public bool IsMultipleText;
    public string Seprator;
    public CountdownType Type;
    public Text Day;
    public Text Hour;
    public Text Minute;
    public Text Second;
    public Text Timer;

    public float ShowHoursBeforeActivityEnd;
    public GameObject TimerUi;

    private DateTime _startDate;
    private DateTime _endDate;
    private TimeSpan _leftTime;
    private TimeSpan _lastFrameLeftTime;

    public IEnumerator StartTimer(TimeSpan timeSpan, Action onTimerOver = null)
    {
        InitTimer(timeSpan);

        while (_leftTime.TotalSeconds >= 0)
        {
            UpdateTimer();
            yield return null;
        }

        if (onTimerOver != null)
        {
            onTimerOver.Invoke();
        }
    }

    void InitTimer(TimeSpan leftTime)
    {
        _startDate = NetworkTimeHelper.Instance.GetNowTime();
        _endDate = _startDate + leftTime;
        _leftTime = leftTime;
        _lastFrameLeftTime = TimeSpan.MaxValue;

        if (TimerUi != null)
        {
            bool show = leftTime.TotalHours < ShowHoursBeforeActivityEnd;
            TimerUi.SetActive(show);

            if (!show)
            {
                float delayShowTime = (float) leftTime.TotalSeconds - ShowHoursBeforeActivityEnd*60;
                UnityTimer.Start(this, delayShowTime, () => TimerUi.SetActive(true));
            }
        }
    }

    void UpdateTimer()
    {
        _leftTime = _endDate - NetworkTimeHelper.Instance.GetNowTime();

        //1 means refresh timer text per second
        if (_lastFrameLeftTime.TotalSeconds - _leftTime.TotalSeconds >= 1)
        {
            switch (Type)
            {
                case CountdownType.DHMS:
                    int day = (int) Math.Floor(_leftTime.TotalDays);
                    if (IsMultipleText)
                    {
                        Day.text = day.ToString("00") + Seprator;
                        Hour.text = _leftTime.Hours.ToString("00") + Seprator;
                        Minute.text = _leftTime.Minutes.ToString("00") + Seprator;
                        Second.text = _leftTime.Seconds.ToString("00");
                    }
                    else
                        Timer.text = string.Format("{0:00}{1}{2:00}{1}{3:00}{1}{4:00}", day, Seprator, _leftTime.Hours,
                            _leftTime.Minutes, _leftTime.Seconds);
                    break;

                case CountdownType.HMS:
                    int hour = (int) Math.Floor(_leftTime.TotalHours);
                    if (IsMultipleText)
                    {
                        Hour.text = hour.ToString("00") + Seprator;
                        Minute.text = _leftTime.Minutes.ToString("00") + Seprator;
                        Second.text = _leftTime.Seconds.ToString("00");
                    }
                    else
                        Timer.text = string.Format("{0:00}{1}{2:00}{1}{3:00}", hour, Seprator, _leftTime.Minutes,
                            _leftTime.Seconds);
                    break;

                case CountdownType.MS:
                    int minute = (int) Math.Floor(_leftTime.TotalMinutes);
                    if (IsMultipleText)
                    {
                        Minute.text = minute.ToString("00") + Seprator;
                        Second.text = _leftTime.Seconds.ToString("00");
                    }
                    else
                        Timer.text = string.Format("{0:00}{1}{2:00}", minute, Seprator, _leftTime.Seconds);
                    break;
            }

            _lastFrameLeftTime = _leftTime;
        }
    }
}
