#define NEW_NOTIFY// 推送内容需要区分更多ID
#define DEBUG_NOTIFY_TEST// Send all notifications within 5 minutes

using UnityEngine;
using System.Collections;
using CitrusFramework;
using System.Text.RegularExpressions;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Collections.Generic;

#if UNITY_IOS
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public class NotificationUtility : Singleton<NotificationUtility> {
	// ios下带表情符的推送内容

	private static readonly int doubleLevelHour = 9;
	private static readonly Color32 _defaultFontColor = new Color32(0x00, 0x00, 0x00, 0xff);
# if DEBUG && DEBUG_NOTIFY_TEST
    private readonly float _minTestDuration = 5f;
    private readonly float _maxTestDuration = 300f;
#endif


    public enum NotifyType
	{
		HourBonus = 1,
		DayBonus,
		Weekly,
		BackFlow,
		DoubleExp,
		DloubleExp1h,
		DoubleSaleForenotice,
		DoubleSale,
		BlackFriday,
        MaxWinBonus,
        ChristmasBonus,
        DoublePayStart,
        DoublePayEnd,
		Max,
	}

	#if UNITY_IOS && !UNITY_EDITOR
	[DllImport ("__Internal")] 
	private static extern void resetNotification();
	#endif

	public void Init(){
		
	}

	void Start()
	{
		#if !UNITY_EDITOR
		DontDestroyOnLoad (this);
		#if UNITY_IOS
		UnityEngine.iOS.NotificationServices.RegisterForNotifications(NotificationType.Badge
			| NotificationType.Alert | NotificationType.Sound);
		#endif
		CleanNotification();
		#endif
	}

	void OnApplicationPause(bool isPause)
	{
		if (isPause) {
//			TestNotification ();
//			TestTimeLeft ();
			#if DEBUG
			DateTime now = System.DateTime.Now;
			#else
			DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
			#endif

			NotificationHourBonus(now);
			NotificationDayBonus(now);
			NotificationWeekly();
			NotificationBackflow();
			NotificationDoubleLevelUp();
			NotificationDoubleLevelUpHours();
			    
			NotificationFestival();
//            ChristmasActivityManager.Instance.HandleLocalNotification();
//            RegisterMaxWinActivity.Instance.HandleLocalNotification();
//            DoubleHourBonusActivity.Instance.HandleLocalNotification();
			LocalLog();

		} else {
			LocalLog();
			CleanNotification ();
		}
	}

	void LocalLog()
	{
		#if UNITY_IOS
		Debug.Log("IOS local push" + UnityEngine.iOS.NotificationServices.localNotificationCount);
		for(int i= 0 ;i <UnityEngine.iOS.NotificationServices.localNotificationCount;i++)
		{
			if (UnityEngine.iOS.NotificationServices.localNotifications [i].userInfo != null){
				if (UnityEngine.iOS.NotificationServices.localNotifications [i].userInfo.Contains("title"))
				{
					Debug.Log(UnityEngine.iOS.NotificationServices.localNotifications [i].userInfo ["title"]);
					AnalysisManager.Instance.LocalIOSPush(UnityEngine.iOS.NotificationServices.localNotifications [i].userInfo ["title"].ToString());
					UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
				}
			}
		}
		#endif
	}

	void NotificationWithID(NotifyType notifytype,LocalNotificationData data,double delayTime,int repeat = 0, int index = 0)
	{
		List<int> notificationID = GroupConfig.Instance.GetNotificationID();
		string notifyStr = "";
		if (data != null && notificationID.Contains(data.ID))
		{
			#if UNITY_IOS
			notifyStr = data.ContentIOS;
			#else
			notifyStr = data.Content;
			#endif

			#if NEW_NOTIFY
			int id = NotifyIDFactory.CreateLocalID(data.ID, index);
			if (repeat != 0)
				NotificationMessageRepeat(id, notifyStr, data.Title, delayTime, repeat);
			else
				NotificationMessage (id, notifyStr,data.Title,delayTime);
			#else
			if (repeat != 0)
				NotificationMessageRepeat(notifytype, notifyStr, data.Title, delayTime, repeat);
			else
				NotificationMessage (notifytype, notifyStr,data.Title,delayTime);
			#endif
		}
	}
		
	void NotificationHourBonus(DateTime now)
	{
		double delayTime = 0;

		bool canGetBonus = BonusHelper.TimeContrast(now, UserBasicData.Instance.LastHourBonusDateTime, 1);
		// 没领取每小时奖励&奖励为可领取	
		if (canGetBonus) {
			delayTime = 0.5f * 60 * 60;
		} else {
			// 领奖励冷却中
			int timePass = BonusHelper.LeftTime(now, UserBasicData.Instance.LastHourBonusDateTime);
			delayTime = Mathf.Clamp(60 * 60 - timePass, 0, float.MaxValue);
		}
		LocalNotificationData data = LocalNotificationConfig.Instance.GetData("hourBonus");
		NotificationWithID(NotifyType.HourBonus,data, delayTime);
	}

	void NotificationDayBonus(DateTime now)
	{
		double delayTime = 0;
		TimeSpan span;
		bool canGetDayBonus = BonusHelper.TimeContrast(now, UserBasicData.Instance.LastDayBonusDateTime, 24);
		LocalNotificationData data = LocalNotificationConfig.Instance.GetData("dayBonus");
		if (data != null && TimeSpan.TryParse(data.Time, out span))
			delayTime = TimeUtility.TimeLeft(span.Hours, span.Minutes, span.Seconds).TotalSeconds;
		else
		{
			Debug.Assert(false,"error in localnotification excel,timespan is not in timespan format");
			delayTime = TimeUtility.TimeLeft(20, 30, 0).TotalSeconds;
		}
		// 当前就可以领每日奖励
		if (canGetDayBonus) {
			NotificationWithID(NotifyType.DayBonus,data, delayTime, 1);
		} else {
			// 若当前还不能领，则要计算到晚上8点是否能领
			bool canGetBonusAtEight = BonusHelper.TimeContrast(now.AddSeconds(delayTime)
				, UserBasicData.Instance.LastDayBonusDateTime, 24);
			NotificationWithID(NotifyType.DayBonus, data, canGetBonusAtEight ? delayTime : delayTime + 24 * 60 * 60, 1);
		}	

	}

	void NotificationWeekly()
	{
		LocalNotificationData data = LocalNotificationConfig.Instance.GetData("week");
		double delayTime = TimeUtility.TimeLeft (DayOfWeek.Monday, 19, 30, 0).TotalSeconds;
		TimeSpan span;
		if (data != null && TimeSpan.TryParse(data.Time, out span))
			delayTime = TimeUtility.TimeLeft (DayOfWeek.Monday, span.Hours,span.Minutes,span.Seconds).TotalSeconds;
		else
		{
			Debug.Assert(false,"error in localnotification excel,timespan is not in timespan format");
			delayTime = TimeUtility.TimeLeft (DayOfWeek.Monday, 19, 30, 0).TotalSeconds;
		}
		NotificationWithID(NotifyType.Weekly, data, delayTime,7);
	}

	void NotificationBackflow()
	{
		// backflow也需要以lostday来区别判断
		int baseday = int.Parse( MapSettingConfig.Instance.MapSettingMap ["BackFlowRewardLimitedDay"]);
		int basehour = int.Parse(  MapSettingConfig.Instance.MapSettingMap ["BackFlowRewardPushHour"]);
		DateTime nowDate = NetworkTimeHelper.Instance.GetNowTime();
		TimeSpan leftTime = TimeUtility.TimeLeft (nowDate.DayOfWeek + baseday, basehour);
		double delayTime = leftTime.TotalSeconds;

		LocalNotificationData data = LocalNotificationConfig.Instance.GetData("backFlow");
		if (data.LostDays.Length > 0){
			TimeSpan time;
			DateTime target;
			if (!TimeSpan.TryParse(data.Time,out time)) {
				Debug.Assert(false, "Error In LocalNotification Excel,TimeSpan is not in TimeSpan format");
			} else {
				for(int i = 0; i < data.LostDays.Length; ++i){
					target = nowDate.Date;
					target = target.AddDays(data.LostDays [i]);
					target = target.Add(time);
					#if NEW_NOTIFY
					NotificationWithID(NotifyType.BackFlow, data,(target - nowDate).TotalSeconds, 0, i);
					#else
					NotificationWithID(NotifyType.BackFlow, data,(target - nowDate).TotalSeconds);
					#endif
				}
			}
		}else {
			NotificationWithID(NotifyType.BackFlow, data, delayTime, 7);
		}
	}

	void NotificationDoubleLevelUp()
	{
		LocalNotificationData data = LocalNotificationConfig.Instance.GetData("doubleLevelUp");
		DayOfWeek dayofweek = DoubleLevelUpHelper.Instance.DoubleDay();
		double delayTime = TimeUtility.TimeLeft(dayofweek,doubleLevelHour).TotalSeconds;
		NotificationWithID(NotifyType.DoubleExp, data, delayTime, 7);
	}

	void NotificationDoubleLevelUpHours()
	{
		List<LocalNotificationData> list = LocalNotificationConfig.Instance.GetDataList("doubleLevelUp_");
		DateTime now = NetworkTimeHelper.Instance.GetNowTime();
		foreach (LocalNotificationData data in list)
		{
			TimeSpan time;
			DateTime target;
			if (!TimeSpan.TryParse(data.Time,out time))
			{
				Debug.Assert(false, "Error In LocalNotificationfestival Excel,TimeSpan is not in TimeSpan format");
				continue;
			}
			for (int i = 0; i < data.LostDays.Length; i++)
			{
				target = now.Date;
				target = target.AddDays(data.LostDays [i]);
				target = target.Add(time);
				#if NEW_NOTIFY
				NotificationWithID(NotifyType.DloubleExp1h, data,(target - now).TotalSeconds, 0, i);
				#else
				NotificationWithID(NotifyType.DloubleExp1h, data,(target - now).TotalSeconds);
				#endif
			}
		}
	}

	void NotificationFestival()
	{
		LocalNotificationfestivalSheet sheet = LocalNotificationfestivalConfig.Instance.Sheet;

		foreach (LocalNotificationfestivalData data in sheet.DataArray)
		{
			DateTime startDate, endDate;
			TimeSpan time;
			if (!ParseToDate(data.StartDate, out startDate))
				continue;
			if (!ParseToDate(data.EndDate, out endDate))
				continue;
			if (!TimeSpan.TryParse(data.Time,out time))
			{
				Debug.Assert(false, "Error In LocalNotificationfestival Excel,TimeSpan is not in TimeSpan format");
				continue;
			}
				
			#if UNITY_IOS
			string message = data.ContentIOS;
			#else
			string message = data.Content;
			#endif
			string title = data.Title;
			#if NEW_NOTIFY
			int index = 0;
			int id;
			for (DateTime date = startDate; date <= endDate; date = date.AddDays(1), ++index)
			{
				id = NotifyIDFactory.CreateFestivalID(data.ID, index);
				NotificationMessage(id, message, title, (date - NetworkTimeHelper.Instance.GetNowTime() + time).TotalSeconds);
			}
			#else
			for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
			{
				NotificationMessage(NotifyType.DoubleSale, message, title, (date - NetworkTimeHelper.Instance.GetNowTime() + time).TotalSeconds);
			}
			#endif
		}
	}

	bool ParseToDate(string str,out DateTime date)
	{
		date = System.DateTime.MaxValue;
		bool result = false;
		result = DateTime.TryParse(str, out date);
		return result;
	}
		
	string DeleteEmoji(string message)
	{
		return Regex.Replace(message,@"\p{Cs}","");
	}

	void CleanNotification()
	{
//		LogUtility.Log ("Clear notification", Color.green);
		#if UNITY_ANDROID
		#if NEW_NOTIFY
		ListUtility.ForEach(UserDeviceLocalData.Instance.NotifyIDs, (int i)=>{
			LocalNotification.CancelNotification (i);
		});
		UserDeviceLocalData.Instance.ClearNotifyID();
		#endif
		for(int i = (int)NotifyType.HourBonus; i < (int)NotifyType.Max; ++i){
			LocalNotification.CancelNotification (i);	
		}
		#elif UNITY_IOS && !UNITY_EDITOR
//		UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification (); 
//		l.applicationIconBadgeNumber = -1; 
//		UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow (l); 
//		UnityTimer.Instance.WaitForFrame(1, () => {
//			UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications (); 
//			UnityEngine.iOS.NotificationServices.ClearLocalNotifications (); 
//		});

		resetNotification();
		#endif
	}

	public void NotificationMessage(int id, string message, string title, double DelaySec)
	{
		if (DelaySec < 0)
			return;
        #if DEBUG && DEBUG_NOTIFY_TEST
        DelaySec = SetDelayTimeInTestMode(DelaySec);
        #endif
        //		LogUtility.Log ("NotificationMessage id = " + id 
        //			+ " message : " + message + " delay : "+ DelaySec, Color.green);

		// #if DEBUG
		// DelaySec = UnityEngine.Random.Range(10, 100);
		// #endif

        #if UNITY_ANDROID
        message = DeleteEmoji(message);
		UserDeviceLocalData.Instance.AddNotifyID(id, false);
        LocalNotification.SendNotification(id, (long)DelaySec, title, message, _defaultFontColor);
		#elif UNITY_IOS && !UNITY_EDITOR
		#if DEBUG
		DateTime now = System.DateTime.Now;
		#else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		#endif

		DateTime newDate = now.AddSeconds(DelaySec);
		UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
		localNotification.fireDate = newDate;   
		localNotification.alertBody = message;
		localNotification.applicationIconBadgeNumber = NotificationServices.scheduledLocalNotifications.Length + 1;
		localNotification.hasAction = true;
		localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		localNotification.userInfo = new Dictionary<string,string>(){
			{
				"title",
				id.ToString()
			}
		};
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
		#endif
	}

	public void NotificationMessage(NotifyType type, string message, string title, double DelaySec)
	{
		if (DelaySec < 0)
			return;
        #if DEBUG && DEBUG_NOTIFY_TEST
        DelaySec = SetDelayTimeInTestMode(DelaySec);
        #endif
        //		LogUtility.Log ("NotificationMessage type = " + type 
        //			+ " message : " + message + " delay : "+ DelaySec, Color.green);

		// #if DEBUG
		// DelaySec = UnityEngine.Random.Range(10, 100);
		// #endif

        #if UNITY_ANDROID
        message = DeleteEmoji(message);
        LocalNotification.SendNotification((int)type, (long)DelaySec, title, message, _defaultFontColor);
		#elif UNITY_IOS && !UNITY_EDITOR
		#if DEBUG
		DateTime now = System.DateTime.Now;
		#else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
		#endif

		DateTime newDate = now.AddSeconds(DelaySec);
		UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
		localNotification.fireDate = newDate;   
		localNotification.alertBody = message;
		localNotification.applicationIconBadgeNumber = NotificationServices.scheduledLocalNotifications.Length + 1;
		localNotification.hasAction = true;
		localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		localNotification.userInfo = new Dictionary<string,string>(){
			{
				"title",
				type.ToString()
			}
		};
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
		#endif
	}

	public void NotificationMessageRepeat(int id, String message, string title, double DelaySec, int day = 1, int hour = 0, int minute = 0, int second = 0)
	{
		#if DEBUG
		DateTime now = System.DateTime.Now;
        #else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
        #endif
        #if DEBUG && DEBUG_NOTIFY_TEST
        DelaySec = SetDelayTimeInTestMode(DelaySec);
        #endif

		// #if DEBUG
		// DelaySec = UnityEngine.Random.Range(10, 100);
		// #endif

        //		LogUtility.Log ("NotificationMessageRepeat id = " + id 
        //			+ " message : " + message + " delay : " + DelaySec, Color.green);

        long timeOut = day * 24 * 60 * 60 + hour * 60 * 60 + minute * 60 + second;

		#if UNITY_ANDROID
		UserDeviceLocalData.Instance.AddNotifyID(id, false);
		message = DeleteEmoji(message);
		LocalNotification.SendRepeatingNotification(id, (long)DelaySec, timeOut, title, message, _defaultFontColor);
		#elif UNITY_IOS && !UNITY_EDITOR
		UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
		DateTime newDate = now.AddSeconds (DelaySec);
		localNotification.fireDate = newDate;   
		localNotification.alertBody = message;
		localNotification.applicationIconBadgeNumber = NotificationServices.scheduledLocalNotifications.Length + 1;
		localNotification.hasAction = true;
		localNotification.repeatCalendar = UnityEngine.iOS.CalendarIdentifier.ChineseCalendar;
		localNotification.userInfo = new Dictionary<string,string>(){
			{
				"title",
				id.ToString()
			}
			};
		if (day == 7) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Week;
		} else if (day == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		} else if (hour == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Hour;
		} else if (minute == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Minute;
		} else if (second == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Second;
		} else {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		}
		localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
		#endif
	}

	public void NotificationMessageRepeat(NotifyType type, String message, string title, double DelaySec, int day = 1, int hour = 0, int minute = 0, int second = 0)
	{
		#if DEBUG
		DateTime now = System.DateTime.Now;
        #else
		DateTime now = NetworkTimeHelper.Instance.GetNowTime ();
        #endif
        #if DEBUG && DEBUG_NOTIFY_TEST
        DelaySec = SetDelayTimeInTestMode(DelaySec);
        #endif

		// #if DEBUG
		// DelaySec = UnityEngine.Random.Range(10, 100);
		// #endif
        //		LogUtility.Log ("NotificationMessageRepeat type = " + type 
        //			+ " message : " + message + " delay : " + DelaySec, Color.green);

        long timeOut = day * 24 * 60 * 60 + hour * 60 * 60 + minute * 60 + second;

		#if UNITY_ANDROID
		message = DeleteEmoji(message);
		LocalNotification.SendRepeatingNotification((int)type, (long)DelaySec, timeOut, title, message, _defaultFontColor);
		#elif UNITY_IOS && !UNITY_EDITOR
		UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
		DateTime newDate = now.AddSeconds (DelaySec);
		localNotification.fireDate = newDate;   
		localNotification.alertBody = message;
		localNotification.applicationIconBadgeNumber = NotificationServices.scheduledLocalNotifications.Length + 1;
		localNotification.hasAction = true;
		localNotification.repeatCalendar = UnityEngine.iOS.CalendarIdentifier.ChineseCalendar;
		localNotification.userInfo = new Dictionary<string,string>(){
			{
				"title",
				type.ToString()
			}
			};
		if (day == 7) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Week;
		} else if (day == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		} else if (hour == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Hour;
		} else if (minute == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Minute;
		} else if (second == 1) {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Second;
		} else {
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		}
		localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
		#endif
	}
#if DEBUG && DEBUG_NOTIFY_TEST
    private double SetDelayTimeInTestMode(double delaySeconds)
    {
        return UnityEngine.Random.Range(_minTestDuration, _maxTestDuration);
    }
#endif

    private void TestNotification(){
//		NotificationMessageRepeat (NotifyType.HourBonus, _testNotificationEmoji, "Huge Slot", 5, 0, 0, 0, 30);
//		NotificationMessage (NotifyType.DayBonus, _testNotificationEmoji2, "Huge Slot", 15);
//		NotificationMessage (NotifyType.Weekly, _testNotificationEmoji3, "Huge Slot", 25);
	}

	private void TestTimeLeft(){
		TimeSpan span;
		span = TimeUtility.TimeLeft (DayOfWeek.Thursday);
		LogUtility.Log ("now until Thursday hours : "+span.TotalHours, Color.green);
		span = TimeUtility.TimeLeft (DayOfWeek.Friday);
		LogUtility.Log ("now until Friday hours : "+span.TotalHours, Color.green);
		span = TimeUtility.TimeLeft (DayOfWeek.Saturday);
		LogUtility.Log ("now until Saturday hours : "+span.TotalHours, Color.green);

		span = TimeUtility.TimeLeft (DayOfWeek.Wednesday, 14, 15, 00);
		LogUtility.Log ("now until Wednesday 14:15 hours : "+span.TotalHours, Color.green);
		span = TimeUtility.TimeLeft (DayOfWeek.Wednesday, 19, 30, 00);
		LogUtility.Log ("now until Wednesday 19:30 hours : "+span.TotalHours, Color.green);
		span = TimeUtility.TimeLeft (DayOfWeek.Wednesday, 13, 05, 00);
		LogUtility.Log ("now until Wednesday 13:05 hours : "+span.TotalHours, Color.green);

		span = TimeUtility.TimeLeft (12, 0, 0, 24);
		LogUtility.Log ("now until 12, 0, 0, : "+span.TotalHours, Color.green);
		span = TimeUtility.TimeLeft (16, 0, 0, 24);
		LogUtility.Log ("now until 16, 0, 0, : "+span.TotalHours, Color.green);
		span = TimeUtility.TimeLeft (23, 0, 0, 24);
		LogUtility.Log ("now until 23, 0, 0, : "+span.TotalHours, Color.green);
	}

	public void AndroidPushCallBack(string msg)
	{
		Debug.Log ("Android Local Push Call Back: " + msg);
	}
}

