using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BackFlowReward : MonoBehaviour {
	public GameObject _gameObject;
	public Canvas _canvas;
	public Button _collectButton;
	public Text _coinsNumText;
	public Text _tagText;

	private DateTime _lastLoginDate;
	private int _leftDay;
	private WindowInfo _windowInfoReceipt = null;
	private int _rewardCoins;
	private int _LTLucky;

	public static void TryShow ()
	{
		if (ShouldShow()) 
		{
			GameObject _backFlowReward = UGUIUtility.InstantiateUI ("Map/BackFlowReward");
			_backFlowReward.SetActive (true);
			_backFlowReward.GetComponent<BackFlowReward> ().Init ();
		}
	}

	public static bool ShouldShow()
	{
		if (!NetworkTimeHelper.Instance.IsServerTimeGetted)
			return false;
		int limitDay =int.Parse( MapSettingConfig.Instance.MapSettingMap ["BackFlowRewardLimitedDay"]);
		DateTime dateTime = NetworkTimeHelper.Instance.GetNowTime ();
		DateTime lastLoginDate=UserBasicData.Instance.LastLoginDateTime;
		int leftDay= TimeUtility.DaysLeft(dateTime,lastLoginDate);
		if (leftDay >= limitDay)
			return true;
		else 
		{
			UserBasicData.Instance.SetLastLoginDateTime (NetworkTimeHelper.Instance.GetNowTime());
			return false;
		}
	}

	public void Init () {
		_canvas.gameObject.SetActive (false);
		CheckDate ();
	}
		
	void Start () {
		_collectButton.onClick.AddListener (CollectButtonClick);
	}

	// Update is called once per frame
	void Update () {
	}

	void OnDestroy()
	{
		_collectButton.onClick.RemoveListener (CollectButtonClick);
	}

	private void CheckDate()
	{
		int _limitDay =int.Parse( MapSettingConfig.Instance.MapSettingMap ["BackFlowRewardLimitedDay"]);
		int _coinsBaseNumber =int.Parse( MapSettingConfig.Instance.MapSettingMap ["BackFlowRewardCoinsBaseNumber"]);
		DateTime dateTime = NetworkTimeHelper.Instance.GetNowTime ();
		_lastLoginDate=UserBasicData.Instance.LastLoginDateTime;
		_leftDay= TimeUtility.DaysLeft(dateTime,_lastLoginDate);

		_rewardCoins = _coinsBaseNumber * _leftDay;
		int linerx = BackFlowRewardLTLuckyConfig.Instance.GetLinerX (_leftDay);
		int linery = BackFlowRewardLTLuckyConfig.Instance.GetLinerY (_leftDay);
		_LTLucky = linerx * _leftDay + linery;
		if (_leftDay >= _limitDay) 
		{
			_coinsNumText.text = _rewardCoins.ToString();
			string front = LocalizationConfig.Instance.GetValue ("notify_backFlowMessageFront");
			string end = LocalizationConfig.Instance.GetValue ("notify_backFlowMessageEnd");
			_tagText.text = front + string.Format (" <color=#f9ff00>{0}</color> ", _leftDay) + end;
			ShowBackFlowRewardWindow ();
		} 
	}
		
	private void ShowBackFlowRewardWindow()
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
		_canvas.gameObject.SetActive (false);
		_gameObject.SetActive (false);
		Destroy (_gameObject);
	}

	public  void ProcessBackFlowRewardData()
	{
		UserBasicData.Instance.AddCredits((ulong)_rewardCoins, FreeCreditsSource.BackFlowBonus, false);
		UserBasicData.Instance.AddLongLucky (_LTLucky, false);
		UserBasicData.Instance.Save();
		AnalysisManager.Instance.GetBackFlowReward (_rewardCoins,_LTLucky,_leftDay);
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

	private void CollectButtonClick()
	{
		AudioManager.Instance.PlaySound(AudioType.HourlyBonusCreditsRollUp);
		UserBasicData.Instance.SetLastLoginDateTime (NetworkTimeHelper.Instance.GetNowTime());
		ProcessBackFlowRewardData ();
		ExitButtonClick ();
	}
		
}
