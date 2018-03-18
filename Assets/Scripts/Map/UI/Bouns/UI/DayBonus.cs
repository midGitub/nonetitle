using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DayBonus : MonoBehaviour
{
	private static DayBonus _instance;
	public static DayBonus Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<DayBonus>();
			}

			return _instance;
		}
	}

	public List<DayBonusTypeInfor> DayBonusType = new List<DayBonusTypeInfor>();

	[HideInInspector]
	public UnityEvent DayBonusFinishedEvent = new UnityEvent();

	public List<GameObject> SpinOpenEffectObject = new List<GameObject>();
	public List<GameObject> SpinCloseEffectObject = new List<GameObject>();

	public GameObject Canvas;
	[SerializeField]
	private RotaryTableController _rotaryTableController;

    public Text DailyBonusText;
	public Text DailyBonusNum;
    public Text MultiplierNum;
    public Text VIPBonusText;
	public Text VIPBonusNum;
	public Image VIPIcon;
    public Text PayrotaryTableText;
    public Text PayrotaryTableBonusNum;

	public UnityAction GetBonusAction;


	private static bool _canShowDayBonusUI = false;
	private bool _showingDailyBonus = false;
	public bool GetCanShowDayBonusUI { get { return _canShowDayBonusUI; } }
	private int daysType;
	private DailyBonusData _nowDailyBonusData;
	private DayBonusTypeInfor _nowDayBonusTypeInfor;

	public DailyBonusData NowDailyBonusData { get { return _nowDailyBonusData; } }
	public int BonusCoins { get; private set; }
	private bool _isStartRotate = false;

    private PayRotaryTableData _payRotaryTableData;
    public PayRotaryTableData PayRotaryTableData
    {
        get { return _payRotaryTableData;}
        set { _payRotaryTableData = value; }
    }
    // 转盘默认位置
    private Vector3 _rotaryTableDefaultLocalPosition;
	// 转盘默认旋转
	private Quaternion _rotaryTableDefaultLocalRotation;

	private RotaryAnimatorContoller _rotaryAnimator;

    private WindowInfo _windowInfoReceipt = null;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		_rotaryAnimator = _rotaryTableController.GetComponent<RotaryAnimatorContoller> ();
		_rotaryTableDefaultLocalPosition = _rotaryAnimator.RotartAnimator.transform.localPosition;
		_rotaryTableDefaultLocalRotation = _rotaryTableController.RotaryTransform.localRotation;

		AudioManager.Instance.CrossFadeBgMusic(AudioManager.Instance.LoadAudio(AudioType.LobbyBGM), 0f);

		CloseDayBonus();

		CitrusEventManager.instance.AddListener<UserDataLoadEvent> (HandleShowBonusUserDataLoad);

		#if UNITY_EDITOR // zhousen 编辑器模式下，从大厅scene启动，需要这步操作，用来获得服务器时间，并执行TryShow，正常流程不需要
		NetworkTimeHelper.Instance.CheckServerSystemTime(TryShowEditor);
		#endif
	}

	private void TryShowEditor(bool updateSucess){
		if (updateSucess) {
			TryShow ();
		}
	}

	public void ShowDayBonus()
	{
		daysType = UserBasicData.Instance.BonusDaysType;
		Debug.Log ("ShowDayBonus daysType = " + daysType);
		AnalysisManager.Instance.OpenDailyBonus();
		_showingDailyBonus = true;
		AudioManager.Instance.FadeBackGroundMusicVolumeToTarget(0.4f, 1f);
		DayBonusFinishedEvent.AddListener(() => { AudioManager.Instance.FadeBackGroundMusicVolumeToTarget(1f, 1f); });
		if (Canvas != null)
			Canvas.SetActive(true);
		//show this gameobject
		Debug.Log("当前天数" + UserBasicData.Instance.BonusDaysType);
    }

	/// <summary>
	/// 开始旋转
	/// </summary>
	public void StartRotate()
	{
		if(_isStartRotate)
		{
			return;
		}
		_isStartRotate = true;

		AnalysisManager.Instance.SpinDailyBonus();
		foreach(var item in SpinOpenEffectObject)
		{
			item.gameObject.SetActive(true);
		}
		foreach(var item in SpinCloseEffectObject)
		{
			item.gameObject.SetActive(false);
		}

		SetDailyBonusData();
		Debug.Log(AudioType.WheelTick);
		AudioManager.Instance.PlaySound(AudioType.WheelTick);
		AudioManager.Instance.PlaySound(AudioType.WheelSpin);
		_rotaryTableController
			.StartRotate(_nowDailyBonusData);
	}

	public void CloseDayBonus()
	{
        LogUtility.Log("DailyBonus RealClose", Color.magenta);
		// zhousen 所有效果关闭，转盘复原
		ListUtility.ForEach<DayBonusTypeInfor>(DayBonusType, (DayBonusTypeInfor info)=>{
			info.DayType.UpdateButtonState(false);
		});
	    DailyBonusText.text = LocalizationConfig.Instance.GetValue("daybonus_dailybonus");
		DailyBonusNum.text = "";
        MultiplierNum.text = "";
	    VIPBonusText.text = LocalizationConfig.Instance.GetValue("daybonus_vipbonus");
        VIPBonusNum.text = "";
	    PayrotaryTableText.text = LocalizationConfig.Instance.GetValue("daybonus_wheel_of_luck");
        PayrotaryTableBonusNum.text = "";
		foreach(var item in SpinOpenEffectObject)
		{
			item.gameObject.SetActive(false);
		}
		foreach(var item in SpinCloseEffectObject)
		{
			item.gameObject.SetActive(true);
		}
        VIPIcon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(VIPSystem.Instance.GetCurrVIPInforData.VIPLevelName);
        _isStartRotate = false;
		// spin按钮复原
		ScriptButton button = gameObject.GetComponentInChildren<ScriptButton> (true);
		button.ButtonShowGameObject.UpdateGameObject (false);
		// 复原转盘位置
		_rotaryAnimator.RotartAnimator.transform.localPosition = _rotaryTableDefaultLocalPosition;
		// 复原转盘旋转
		_rotaryTableController.RotaryTransform.localRotation = _rotaryTableDefaultLocalRotation;

		_rotaryAnimator.CoinsText.text = "";
		_rotaryAnimator.LightBar.OpenGameObject.SetActive(false);
		_rotaryAnimator.LightBar.CloseGameObject.SetActive(false);
		///

		DayBonusFinishedEvent.Invoke();

		Canvas.SetActive(false);
		_showingDailyBonus = false;
		// 检查锦标赛奖励
		BonusManager.Instance.CheckReward();
		CitrusEventManager.instance.Raise(new DailyBonusFinishEvent());
	}

	public static bool CheckRefreshBonusState()
	{
		NetworkTimeHelper.Instance.GetNowTime((bool arg1, DateTime arg2) => {
			if(arg1)
				_canShowDayBonusUI = BonusHelper.CanGetDayBonus();
			else
				_canShowDayBonusUI = false;
		});
		return _canShowDayBonusUI;
	}

	public void UpdateDaysTypeState(Action<DayBonusTypeInfor> currDBAction)
	{
		for(int i = 0; i < DayBonusType.Count; i++)
		{
			if(i == daysType)
			{
				DayBonusType[i].DayType.UpdateButtonState(true);
				currDBAction(DayBonusType[i]);
				continue;
			}

			DayBonusType[i].DayType.UpdateButtonState(false);
		}
	}

    private void SetDailyBonusData()
    {
        _nowDailyBonusData = RandomDayBonusType.GetRandomDailyBonusData(daysType);
    }

    public void GetBonus()
	{
		daysType = UserBasicData.Instance.BonusDaysType;// zhousen 可能初始化的时候因为断网daysType这个变量没有初始化
		LogUtility.Log("GetBonus daysType = "+daysType, Color.red);
		_nowDayBonusTypeInfor = DayBonusType[daysType];

	    long payRotaryTableBonus = _payRotaryTableData != null ? _payRotaryTableData.Bonus : 0;
        LogUtility.Log("PayRotaryTable Module :　coin num is " + payRotaryTableBonus  + "   _payRotaryTableData is null : " + (_payRotaryTableData == null));
		BonusHelper.ProcessDayBonusData(_nowDayBonusTypeInfor, _nowDailyBonusData.Bonus ,(int)payRotaryTableBonus,
            (coinDatas) => {
                //5 means five coins data used by ui text
                Debug.Assert(coinDatas != null && coinDatas.Length >= 5, "Daybonus : coindatas Error");
                if (coinDatas != null && coinDatas.Length >= 5)
                {
                    DailyBonusNum.text = coinDatas[0].ToString();
                    MultiplierNum.text = "x" + coinDatas[1].ToString();
                    VIPBonusNum.text = coinDatas[2].ToString();
                    PayrotaryTableBonusNum.text = coinDatas[3].ToString();
                    BonusCoins = coinDatas[4];
                }
            }
		);
		VIPIcon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(VIPSystem.Instance.GetCurrVIPInforData.VIPLevelName);

		if(GetBonusAction != null)
			GetBonusAction();
		
		CheckRefreshBonusState();
	}

    static int tempCount = 1;

	public void TryShow()
	{
	    if (CheckRefreshBonusState())
	        Show();
	    else
	        StartCoroutine(TryShowNextBonus());
	}

    IEnumerator TryShowNextBonus()
    {
        double delayTime = TimeUtility.CountdownOfDateFromNowOn(NetworkTimeHelper.Instance.GetNowTime().Date + new TimeSpan(1, 0, 0, 0)).TotalSeconds;
        yield return new WaitForSeconds((float)delayTime);
        TryShow();
    }

    public void Show()
    {
        //TODO 临时修复，需要确认动画机制后才能保证转盘的位置在整个流程结束后复位成功
        // 复原转盘位置
        _rotaryAnimator.RotartAnimator.transform.localPosition = _rotaryTableDefaultLocalPosition;

        if (_windowInfoReceipt == null)
        {
            LogUtility.Log("DailyBonus ApplyToOpen" + (tempCount++), Color.magenta);
            _windowInfoReceipt = new WindowInfo(Open, null, Canvas.GetComponent<Canvas>(), ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

    void OnDestroy()
    {
		if (_windowInfoReceipt != null && WindowManager.Instance != null)
        {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
		}
		CitrusEventManager.instance.RemoveListener<UserDataLoadEvent> (HandleShowBonusUserDataLoad);
    }

    public void ForceToCloseImmediately()
    {
        Canvas.SetActive(false); 
        _windowInfoReceipt = null;
    }

    public void Hide()
    {
        LogUtility.Log("DailyBonus Hide", Color.magenta);
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    public void Open()
    {
        ShowDayBonus();
    }

    private void SelfClose(Action callBack)
    {
        CloseDayBonus();
        StartCoroutine(TryShowNextBonus());
        callBack(); 
    }

//    public void ManagerClose(Action callBack)
//    {
//        CloseDayBonus();
//        _windowInfoReceipt = null;
//        callBack();
//    }

	public void Test()
	{
		UserBasicData.Instance.SetLastGetDayBonusDate(NetworkTimeHelper.Instance.GetNowTime().AddDays(-1));
        Show();
	}

	private void HandleShowBonusUserDataLoad(UserDataLoadEvent e){
		TryShow ();
	}
}
