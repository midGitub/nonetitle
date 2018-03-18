using System;
using CitrusFramework;
using UnityEngine;

public class ChristmasActivityManager : SimpleSingleton<ChristmasActivityManager>
{
    private DateTime _bonusNotifyDate;

    public void HandleLocalNotification()
    {
        _bonusNotifyDate = MapSettingConfig.Instance.Read("ChristmasBonusNotifyDate", DateTime.MinValue);
        SendLocalNotification(_bonusNotifyDate, "notify_christmas_title", "notify_christmas_bonus", NotificationUtility.NotifyType.ChristmasBonus);
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
