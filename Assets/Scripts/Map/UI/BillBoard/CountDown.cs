using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
public class CountDown: MonoBehaviour {
	public TimeSpan CountingTime;
	public Text TimeText;
	public UnityEvent TimeEvent = new UnityEvent();
	public bool WhiteSpace = false;

	private DateTime _lasTtime;

	// Update is called once per frame
	void Update () {
		
	}

	public void count()
	{
		_lasTtime = NetworkTimeHelper.Instance.GetNowTime();
		StartCoroutine(CountDownTimer());
	}

	IEnumerator CountDownTimer()
	{
		TimeText.text = CountingTime.ToString();
		WaitForSeconds delay = new WaitForSeconds (1.0f);
		while (true)
		{
			DateTime now = NetworkTimeHelper.Instance.GetNowTime();
			if(now >= _lasTtime)
				CountingTime = CountingTime.Add(_lasTtime - now);
			if (CountingTime.TotalSeconds < 0)
				break;
			int hour = (int) Math.Floor(CountingTime.TotalHours);
			string str = ":";
			if (WhiteSpace)
				str = " ";
			TimeText.text = hour.ToString ("00") + str + CountingTime.Minutes.ToString ("00") + str + CountingTime.Seconds.ToString ("00");
			_lasTtime = now;
			yield return delay;
		}
		TimeEvent.Invoke();
	}
}
