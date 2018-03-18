using System;
using System.Collections;
using CitrusFramework;
using UnityEngine;

public class PayUserMonitor : Singleton<PayUserMonitor>
{
    private readonly float _defaultIntervalTime = 600;
    public void Init()
    {
        StartCoroutine(SendSpinState());
    }

    IEnumerator SendSpinState()
    {
        float remainder = DateTime.UtcNow.Minute%10;
        float timeLast = remainder*60;
        yield return new WaitForSeconds(timeLast);
        WaitForSeconds coolDown = new WaitForSeconds(MapSettingConfig.Instance.Read("PayUserSpinMonitorIntervalTime", _defaultIntervalTime));

        while (true)
        {
            AnalysisManager.Instance.PayUserDailySpin(GroupConfig.Instance.GetPayUserGroupId(), TimeUtility.IsSameDay(UserBasicData.Instance.LastSpinDate, DateTime.Now));
            yield return coolDown;
        }
    }
}
