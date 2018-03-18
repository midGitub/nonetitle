using System;
using CitrusFramework;
using UnityEngine;

public class DoubleHourBonusActivity : SimpleSingleton<DoubleHourBonusActivity>
{
    private DateTime _notifyForStartDate;
    private DateTime _notifyForEndDate;
    private DateTime _startDate;
    private DateTime _endDate;
    private float _popCooldownTime;
    private bool _isActivityStart;
    private bool _isInited;
    private Coroutine _stopHourbonusActivity;

    public DateTime StartDate {
        get { return _startDate;}
    }

    public DateTime EndDate
    {
        get { return _endDate; }
    }

    public DateTime NotifyForStartDate
    {
        get { return _notifyForStartDate; }
    }

    public void Init()
    {
        LoadDateConfig();
    }

    void LoadDateConfig()
    {
        _notifyForStartDate = MapSettingConfig.Instance.Read("DoubleHourBonusNotifyDate", DateTime.MinValue);
        _notifyForEndDate = MapSettingConfig.Instance.Read("DoubleHourBonusEndNotifyDate", DateTime.MinValue);
        _startDate = MapSettingConfig.Instance.Read("DoubleHourBonusStartDate", DateTime.MinValue);
        _endDate = MapSettingConfig.Instance.Read("DoubleHourBonusEndDate", DateTime.MinValue);
        _popCooldownTime = MapSettingConfig.Instance.Read("DoubleHourBonusCooldownHour", 0);
    }

    bool IsDuringActivity()
    {
        return TimeUtility.IsBetweenRange(_startDate, _endDate);
    }

    public void TryStartActivity()
    {
        if (!_isInited)
        {
            Init();
            _isInited = true;
        }

        if (IsDuringActivity())
        {
            ShowPopup();
            ShowBillboard();
            DoubleHourBonusReward();
        }
    }

    void ShowPopup()
    {
        if (!IsPopupInCooldownTime())
        {
            UIManager.Instance.ShowPopup<DoubleHourBonusUiController>(UIManager.DoubleHourBonusPopupPath);
            UserDeviceLocalData.Instance.LastOpenDoubleHourBonusUiDate = NetworkTimeHelper.Instance.GetNowTime();
        }
    }

    bool IsPopupInCooldownTime()
    {
        DateTime lastOpenDoubleHourBonusUiDate = UserDeviceLocalData.Instance.LastOpenDoubleHourBonusUiDate;
        bool isInCoolDownTime = (NetworkTimeHelper.Instance.GetNowTime() - lastOpenDoubleHourBonusUiDate).TotalHours < _popCooldownTime;
        return isInCoolDownTime;
    }

    void ShowBillboard()
    {
        GameObject go = UGUIUtility.InstantiateUI(UIManager.DoubleHourBonusBillboardPath);
        go.SetActive(false);
        DoubleHourBonusBillboard billboard = go.GetComponent<DoubleHourBonusBillboard>();
        BillBoardManager.Instance.Add(billboard);
    }

    void DoubleHourBonusReward()
    {
        CitrusEventManager.instance.Raise(new SetHourBonusMultiplierEvent(2));
        if (_stopHourbonusActivity != null)
        {
            UnityTimer.Instance.StopCoroutine(_stopHourbonusActivity);
        }
        _stopHourbonusActivity = UnityTimer.Start(UnityTimer.Instance,
        (float)TimeUtility.CountdownOfDateFromNowOn(_endDate).TotalSeconds,
        () => CitrusEventManager.instance.Raise(new SetHourBonusMultiplierEvent(1, true)));
    }

    public void HandleLocalNotification()
    {
        SendLocalNotification(_notifyForStartDate, "doublepay_title", "notify_double_hour_bonus", NotificationUtility.NotifyType.DoublePayStart);

        bool canGetDayBonus = BonusHelper.TimeContrast(_notifyForEndDate, UserBasicData.Instance.LastHourBonusDateTime, 24);
        DateTime sendDate = canGetDayBonus ? _notifyForEndDate : UserBasicData.Instance.LastHourBonusDateTime + new TimeSpan(1,0,0);
        
        SendLocalNotification(sendDate, "doublepay_title", "notify_double_hour_bonus_end", NotificationUtility.NotifyType.DoublePayEnd);
    }

    void SendLocalNotification(DateTime sendDate, string titleKey, string contentKey, NotificationUtility.NotifyType type)
    {
        if (!TimeUtility.IsDatePast(sendDate))
        {
            string title = LocalizationConfig.Instance.GetValue(titleKey);
            string notifyContent = LocalizationConfig.Instance.GetValue(contentKey);
            double delayTime = TimeUtility.CountdownOfDateFromNowOn(sendDate).TotalSeconds;
            NotificationUtility.Instance.NotificationMessage(type, notifyContent, title, delayTime);
        }
    }
}
