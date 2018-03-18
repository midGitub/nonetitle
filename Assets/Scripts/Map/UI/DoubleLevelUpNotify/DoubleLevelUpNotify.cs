using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class DoubleLevelUpNotify : MonoBehaviour {
	public GameObject _gameObject;
	public Canvas _canvas;
	public Button _exitButton;
	public Text _dayText;
	private WindowInfo _windowInfoReceipt = null;
	private static int _defaultDays = 5;
	private static int hours = 0;
	private static DateTime ShowTime;

	public static bool TryShow ()
	{
		bool result = shouldShow();
		if (result)
		{
			GameObject _backFlowReward = UGUIUtility.InstantiateUI("Map/DoubleLevelUp/DoubleLevelUpNotify");
			_backFlowReward.SetActive(true);
			_backFlowReward.GetComponent<DoubleLevelUpNotify>().Init();
		}
		return result;
	}

	public static bool InDoubleHours()
	{
		return (hours != 0 && (NetworkTimeHelper.Instance.GetNowTime() - ShowTime).TotalHours < hours);
	}

	static bool shouldShow()
	{
		if (!NetworkTimeHelper.Instance.IsServerTimeGetted)
			return false;

		bool flag = false;
		List<LocalNotificationData> list = LocalNotificationConfig.Instance.GetDataList("doubleLevelUp_");
		DateTime now = NetworkTimeHelper.Instance.GetNowTime();
		DateTime before = UserDeviceLocalData.Instance.DoubleLevelUpTime;
		DateTime _defaultShowTime;
		TimeSpan time;
		int days = Mathf.FloorToInt((float)(now - before).TotalDays);
		foreach (LocalNotificationData data in list)
		{
			if (!TimeSpan.TryParse(data.Time,out time))
			{
				Debug.Assert(false, "Error In LocalNotificationfestival Excel,TimeSpan is not in TimeSpan format");
				continue;
			}
			_defaultShowTime = now.Date + time;
			for (int i = 0; i < data.LostDays.Length; i++)
			{
				if (days >= data.LostDays [i] && days < data.LostDays [i] + _defaultDays)
				{
					Regex r = new Regex("\\d+\\.?\\d*");
					MatchCollection mc = r.Matches(data.Key);
					int temp = 0;
					string str = string.Empty;
					for (int j = 0; j < mc.Count; j++)
					{
						str += mc [j];
					}
						
					if (int.TryParse(str, out temp))
					{
						if (_defaultShowTime < now && (now - _defaultShowTime).TotalHours < temp)
						{
							flag = true;
							hours = temp;
							ShowTime = _defaultShowTime;
							break;
						}
					}
				}
			}
		}
		if (!flag)
		{
			UserDeviceLocalData.Instance.DoubleLevelUpTime = NetworkTimeHelper.Instance.GetNowTime();
		}
		return flag;
	}

	public void Init () {
		_canvas.gameObject.SetActive (false);
		_dayText.text = hours.ToString();
		Show();
	}

	void Start () {
		_exitButton.onClick.AddListener(ExitButtonClick);
	}
		
	void OnDestroy()
	{
		_exitButton.onClick.RemoveListener(ExitButtonClick);
	}

	private void Show()
	{
		if (_windowInfoReceipt == null)
		{
			_windowInfoReceipt = new WindowInfo(Open, null, _canvas, ForceToCloseImmediately);
			WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
		}
	}

	private void Open()
	{
		if (_canvas == null) 
			HadFinished ();
		else 
		{
			_canvas.worldCamera = Camera.main;
			_canvas.gameObject.SetActive (true);
		}
	}

	private void ForceToCloseImmediately()
	{
		UserDeviceLocalData.Instance.DoubleLevelUpTime = NetworkTimeHelper.Instance.GetNowTime();
		_canvas.gameObject.SetActive (false);
		_gameObject.SetActive (false);
		Destroy (_gameObject);
	}
		
	private void HadFinished()
	{
		WindowManager.Instance.TellClosed (_windowInfoReceipt);
	}


	private void ExitButtonClick()
	{
		HadFinished ();
		ForceToCloseImmediately ();
	}
		
}
