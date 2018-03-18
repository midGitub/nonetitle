using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CitrusFramework;
using CodeStage.AntiCheat.ObscuredTypes;

public class HourBonus : MonoBehaviour
{
	/// <summary>
	/// 奖励时间
	/// </summary>
	[SerializeField]
	private float _bonusHour = 1f;

	[SerializeField]
	private Text _timeText;

	[SerializeField]
	private Text _bonusCoinsText;

	[SerializeField]
	private BonusButton _bonusButton;

    [SerializeField]
    private GameObject _multipierSymbol;

    [SerializeField]
    private Text _multipierText;

    public UnityAction<bool> GetBonusAction;

    private static readonly ulong _basicCoinNum = 1000;
	/// <summary>
	///  奖励金币的数量
	/// </summary>
	private ObscuredULong _bonusCoinsNum = _basicCoinNum;

    /// <summary>
    /// 奖励状态
    /// </summary>
    private bool _canGetBonus = false;
	private DateTime _lastGetBonusDay;
	private bool _hasUpdateServerTime;
	private bool _updatingServerTime;
	private const float _timeToUpdateServerTime = 20.0f;
	public int testDay;
	public int testHour;
	public int testMinute;
	public int testScond;

	private static bool isShowPlayHourBouns = false;
	private static string _blockPanel = "Map/BlockPanel";

	public void Start()
	{
		CitrusEventManager.instance.AddListener<UserDataLoadEvent>(UpdateDataMessageP);
        CitrusEventManager.instance.AddListener<SetHourBonusMultiplierEvent>(SetMultiplier);
		isShowPlayHourBouns = false;
        _multipierSymbol.SetActive(false);

        _bonusButton.OnClickEvent.AddListener(TryGetBonus);
		// 没获得服务器时间，需要显示no collection
		_bonusButton.ShowOfficleLine (!NetworkTimeHelper.Instance.IsServerTimeGetted);
		CheckIphoneXAdapt();
	}

	private void CheckIphoneXAdapt(){
		if (DeviceUtility.IsIphoneXResolution()){
			Vector3 originalPos = gameObject.transform.localPosition;
			Vector3 newPos = new Vector3(originalPos.x, originalPos.y + 10.0f, originalPos.z);
			gameObject.transform.localPosition = newPos;
		}
	}

	private void TryTestErrorData()
	{
		//todo 这段代码暂时是为了测试用户添加 检测时间 如果大于当前时间 说明数据错误,让他可以重新领取,然后用新时间覆盖错误时间暂时为测试用户,
		// 可能保留为普通用户进行错误检测防止数据出错
		// 因为数据错误导致的测试用户数据错误//重新覆盖把数据变为现在的数据// 之后可能就会去掉这段代码
		var lasttime = UserBasicData.Instance.LastHourBonusDateTime;
		if(lasttime > NetworkTimeHelper.Instance.GetNowTime())
		{
			Debug.LogError("每小时奖励日期错误");
			UserBasicData.Instance.SetLastGetHourBonusDate(NetworkTimeHelper.Instance.GetNowTime().AddHours(-1));
		}
	}

	private void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<UserDataLoadEvent>(UpdateDataMessageP);
        CitrusEventManager.instance.RemoveListener<SetHourBonusMultiplierEvent>(SetMultiplier);
        _bonusButton.OnClickEvent.RemoveListener(TryGetBonus);
	}

	public void Update()
	{
		if (NetworkTimeHelper.Instance.IsServerTimeGetted && !isShowPlayHourBouns) {
			StartShowHourBouns ();
		}
	}

	public void StartShowHourBouns()
	{
		TryTestErrorData();
		NetworkTimeHelper.Instance.GetNowTime((bool arg1, DateTime arg2) =>
		{
			if(arg1)
			{
				// 得到上次领取的时间
				_lastGetBonusDay = UserBasicData.Instance.LastHourBonusDateTime;
				// Debug.Log(_lastGetBonusDay);
				GetBonusState();
				isShowPlayHourBouns = true;
				if(!_canGetBonus)
				{
					StartCoroutine(StartTimer());
				}
			}
		});
	}

	/// <summary>
	/// 测试日期输入
	/// </summary>
	public void TestInsetData()
	{
		UserBasicData.Instance.SetLastGetHourBonusDate(new DateTime(2017, 3, testDay, testHour, testMinute, testScond));
		_lastGetBonusDay = UserBasicData.Instance.LastHourBonusDateTime;
		GetBonusState();
	}

	private void TryGetBonus()
	{
		if(_canGetBonus)
		{
			GetBonusState();
			// 刷新状态
			if(_canGetBonus)
			{
				AudioManager.Instance.PlaySound(AudioType.Click);
				// 可以得到
				if(GetBonusAction != null)
				{
					GetBonusAction(_canGetBonus);
				}

				GameObject blockpanel = null;
				if (TimeLimitedStoreHelper.ShouldPopupTLStore ()) 
				{
					blockpanel = UGUIUtility.InstantiateUI (_blockPanel);
					blockpanel.SetActive (true);
				}
                _bonusButton.ShowCollectioningCoinsEffect(() => {
					 
					if(blockpanel != null)
						blockpanel.SetActive(false);
					blockpanel = null;
				});
				UnityTimer.Instance.StartTimer(this, 0.5f, ()=>{
					TLStoreController.Instance.TryOpenTLStore();
				});
				// 这里需要重新写入old时间和刷新里面的testDay等
				// 重新得到状态然后开启计时
				_lastGetBonusDay = NetworkTimeHelper.Instance.GetNowTime();
				BonusHelper.ProcessHourBonusData(_lastGetBonusDay, _bonusCoinsNum);
				GetBonusState();
				StartCoroutine(StartTimer());

//                TLStoreController.Instance.TryOpenTLStore();
			}
			else
			{
				//重新计时
				StartCoroutine(StartTimer());
			}

		}
	}

	public void UpdateDataMessageP(UserDataLoadEvent loadms)
	{
		StopAllCoroutines();
		isShowPlayHourBouns = false;
	}

    public void SetMultiplier(SetHourBonusMultiplierEvent e)
    {
        _bonusCoinsNum = e.IsReset ? _basicCoinNum : (ObscuredULong)e.Multiplier * _basicCoinNum;
        _multipierSymbol.SetActive(!e.IsReset);
        SetCoinText();
        _multipierText.text = LocalizationConfig.Instance.GetValue("doublepay_symbol");
    }

    /// <summary>
	/// 得到奖励状态
	/// </summary>
	private void GetBonusState()
	{
		//_lastGetBonusDay = new DateTime(1, 1, testDay, testHour, testMinute, testScond);
		NetworkTimeHelper.Instance.GetNowTime((bool arg1, DateTime arg2) =>
		{
			if(arg1) { 
				_canGetBonus = BonusHelper.TimeContrast(arg2, _lastGetBonusDay, _bonusHour);
			}
			else {
				_canGetBonus = false;
			}
		});
		//Debug.LogError(_lastGetBonusDay+";"+ NetworkTimeHelper.Instance.GetNowTime());
        SetCoinText();
		_bonusButton.ChangeButtonState(_canGetBonus);
		// Debug.Log(_canGetBonus);
	}

    void SetCoinText()
    {
        _bonusCoinsText.text = StringUtility.FormatNumberStringWithComma((ulong)((_bonusCoinsNum * VIPSystem.Instance.GetCurrVIPInforData.HourBonusAddition) + _bonusCoinsNum)) + " Credits";
    }

    /// <summary>
	/// 开始计时
	/// </summary>
	private IEnumerator StartTimer()
	{
		WaitForSecondsRealtime delay = new WaitForSecondsRealtime (1);
		while(true)
		{
			TryTestErrorData();
			float lastscond = GetLastSecond();
			if(lastscond<=_timeToUpdateServerTime && !_hasUpdateServerTime && !_updatingServerTime)
			{
				_updatingServerTime = true;
				NetworkTimeHelper.Instance.UpdateServerTime ((bool flag)=>
					{
						_hasUpdateServerTime = true;
						_updatingServerTime=false;
					}
					);
			}

			if(lastscond <= 0)
			{
				while(!_hasUpdateServerTime) 
					yield return delay;
				lastscond = GetLastSecond();
				_hasUpdateServerTime = false;
				if (lastscond <= 0)
					break;
			}

			float sc = lastscond % 60;
			float mi = (lastscond - sc) / 60;

			_timeText.text = mi.ToString("00") + ":" + sc.ToString("00");
			//
			yield return delay;
		}
		GetBonusState();
		yield return null;
	}

	private float GetLastSecond()
	{
		float passTime = BonusHelper.LeftTime (NetworkTimeHelper.Instance.GetNowTime (), _lastGetBonusDay);
		return _bonusHour * 3600 - passTime ;
	}
}
