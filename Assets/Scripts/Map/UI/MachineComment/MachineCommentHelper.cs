using System.Collections;
using System.Collections.Generic;
using System;

public static class MachineCommentHelper {

    /// <summary>
    /// Xhj 新玩家有一段时间之内不用弹出机台评价
    /// </summary>
    /// <returns><c>true</c> if is in no comment period; otherwise, <c>false</c>.</returns>
    public static bool IsInNewNoCommentPeriod()
    {
        return NetworkTimeHelper.Instance.GetNowTime() - UserDeviceLocalData.Instance.FirstEnterGameTime // 头几天不弹出机台评价
            <= new TimeSpan(Convert.ToInt32(MapSettingConfig.Instance.MapSettingMap[SettingKeyMap.mCfirstNoCDayPeriod]), 0, 0, 0);
    }

    /// <summary>
    /// Xhj 每次机台评价间有个XX分钟的间隔
    /// </summary>
    /// <returns><c>true</c> if is in minimum interval; otherwise, <c>false</c>.</returns>
    public static bool IsInMinInterval()
    {
        return NetworkTimeHelper.Instance.GetNowTime() - UserDeviceLocalData.Instance.LastMachineCommentTime // 头几天不弹出机台评价
            <= new TimeSpan(0, Convert.ToInt32(MapSettingConfig.Instance.MapSettingMap[SettingKeyMap.mCMinInterval]), 0);
    }

    /// <summary>
    /// Xhj 玩家一天内弹出的机台评价是有上限的
    /// </summary>
    /// <returns><c>true</c> if is exceed times limit ber day; otherwise, <c>false</c>.</returns>
    public static bool DoesExceedTimesLimitBerDay()
    {
        string lastTime = UserDeviceLocalData.Instance.LastMachineCommentTime.ToString("d");
        string nowTime = NetworkTimeHelper.Instance.GetNowTime().ToString("d");

        if (lastTime != nowTime)
            UserDeviceLocalData.Instance.MachineCommentTimesToday = 0;

        return UserDeviceLocalData.Instance.MachineCommentTimesToday > Convert.ToInt32(MapSettingConfig.Instance.MapSettingMap[SettingKeyMap.mCTimesTopLimitPerDay]);
    }

    /// <summary>
    /// Xhj 玩家拒绝评价后有一个dayInterval，之后才会继续弹出评价
    /// </summary>
    /// <returns><c>true</c> if is in day interval; otherwise, <c>false</c>.</returns>
    public static bool IsInDayInterval()
    {
        return NetworkTimeHelper.Instance.GetNowTime() - UserDeviceLocalData.Instance.LastRefuseMachineCommentTime // 头几天不弹出机台评价
            <= new TimeSpan(Convert.ToInt32(MapSettingConfig.Instance.MapSettingMap[SettingKeyMap.mCDayInterval]), 0, 0, 0);
    }
}
