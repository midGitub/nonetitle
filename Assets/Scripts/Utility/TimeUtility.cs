using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TimeUtility  {
	
	// 到指定的星期几的时间
	public static TimeSpan TimeLeft(DayOfWeek day, int hour = 0, int minute = 0, int second = 0)
	{
		#if DEBUG
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		#else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		#endif
		DayOfWeek nowDayOfWeek = now.DayOfWeek;
		DateTime targetDay = now;
		TimeSpan span;
		int daySpan = 0;

		if (nowDayOfWeek <= day) {
			daySpan = day - nowDayOfWeek;
		} else {
			daySpan = 7 - (nowDayOfWeek - day);
		}
		targetDay = now.AddDays (daySpan);
		span = (new DateTime (targetDay.Year, targetDay.Month, targetDay.Day,
			hour, minute, second) - now);

		if (span.TotalSeconds < 0) {
			span = span.Add (new TimeSpan(7, 0, 0, 0));
		}

		return span;
	}

	// 到指定日期的时间差，可能为负数
	public static TimeSpan TimeLeft(int year, int month, int day, int hour, int minute, int second){
		#if DEBUG
		DateTime now = System.DateTime.Now;
		#else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		#endif
		DateTime targetDay = new DateTime (year, month, day, hour, minute, second);
		TimeSpan span = targetDay - now;
		return span;
	}

	// 到指定时间的时间差，若超过则补spanOffsetHour小时
	public static TimeSpan TimeLeft(int hour, int minute, int second, int spanOffsetHour = 24){
		#if DEBUG
		DateTime now = System.DateTime.Now;
		#else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		#endif
		DateTime targetTime = new DateTime (now.Year, now.Month, now.Day, hour, minute, second);
		TimeSpan span = targetTime - now;
		if (span.TotalSeconds < 0) {
			span = span.Add (new TimeSpan (spanOffsetHour, 0, 0));
		}
		return span;
	}

	// 计算两个日期之间的天数，2017.7.30.23:00到2017.7.31.01:20为一天这种算法
	public static int DaysLeft(DateTime date1,DateTime date2)
	{
		TimeSpan span = date1 - date2;
		DateTime date = date2.AddHours(span.Hours);
		date = date.AddMinutes (span.Minutes);
		date = date.AddSeconds (span.Seconds);
		if(date.Day>date2.Day)
			return (int)span.TotalDays + 1;
		return (int)span.TotalDays;
	}

	public static bool IsInPeriodDays(DateTime date,int days)
	{
		return DaysLeft (NetworkTimeHelper.Instance.GetNowTime (), date) < days;
	}

	public static DateTime ConvertString2DateTime(string date){
		// string must be like "yyyy-MM-dd HH:mm:ss"
		return Convert.ToDateTime (date);
	}

	public static string ConvertDateTime2String(DateTime date){
		return date.ToString("yyyy-MM-dd HH:mm:ss");
	}

	public static bool IsSameDay(DateTime day1, DateTime day2){
		bool result = (day1.Date == day2.Date);
		return result;
	}

    public static IEnumerator StartTimerMMSS(DateTime startTime, Text timeText, float seconds, Action callback)
    {
        DateTime endTime = startTime + new TimeSpan(0, 0, (int)seconds);

        while (!IsDatePast(endTime))
        {
            TimeSpan restTime = CountdownOfDateFromNowOn(endTime);
            timeText.text = string.Format("{0:00}:{1:00}", (int)Math.Floor(restTime.TotalMinutes), restTime.Seconds);
            yield return new WaitForSeconds(1);
        }

        callback();
    }


    public static bool IsBetweenRange(DateTime startDate, DateTime endDate)
    {
        bool result = false;
        TimeSpan span = NetworkTimeHelper.Instance.GetNowTime() - startDate;
        TimeSpan timeOfDuration = endDate - startDate;
        if (span.TotalDays > 0 && span.TotalDays < timeOfDuration.TotalDays)
            result = true;
        return result;
    }

    public static TimeSpan CountdownOfDateFromNowOn(DateTime dateTime)
    {
        return dateTime - NetworkTimeHelper.Instance.GetNowTime();
    }

    public static bool IsDatePast(DateTime endDate)
    {
        bool result = false;
        TimeSpan span = endDate - NetworkTimeHelper.Instance.GetNowTime();
        if (span.TotalDays < 0)
            result = true;
        return result;
    }
}
