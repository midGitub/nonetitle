using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using CitrusFramework;
using UnityEngine.SceneManagement;

public class MachineBar : MonoBehaviour
{
	public float _buttonEffectDeltaTime = 0.15f;

	public PuzzleMachine _puzzleMachine;
	public GameObject _betFrame;
	public Text _betText;
	public Text _winText;
	public NumberTickHandler _winTickHandler;

	public GameObject _minusBetButtonEffect;
	public GameObject _addBetButtonEffect;
	public GameObject _buyButtonEffect;
	public GameObject _maxBetButtonEffect;
	public GameObject _spinButtonEffect;
	public GameObject _spinButton;
	public Image _betWinFrame;// 下注win栏的背景
	public TournamentManager _tournamentManager;

	private const string PayTableAddress = "Game/PayTable/BasePayTabel_";
	private const string ImageDir = "Images/Machines/";
	private GameObject _currPayTable;
    private PayTableController _payTableController;
	private ScrollRect _scrollRectOfCurrPayTable;

	public delegate void ChangeHandler();
	private ChangeHandler _changeBetAmountHandler = null;
	public ChangeHandler ChangeBetAmountHandler
	{
		get { return _changeBetAmountHandler; }
		set { _changeBetAmountHandler = value; }
	}

	private ChangeHandler _noChangeBetAmountHandler = null;
	public ChangeHandler NoChangeBetAmountHandler {
		get { return _noChangeBetAmountHandler; }
		set { _noChangeBetAmountHandler = value; }
	}

	// tip资源路径
	private static readonly string _betTipAssetPath = "Map/Tip/BetTips";
	private BetTipBehaviour _betTipBehaviour = null;

	public BetTipBehaviour BetTip{
		get { return _betTipBehaviour; }
	}
	#region Init

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void Init()
	{
		_puzzleMachine.GameData.BetAmountChangeEventHandler += UpdateBetText;
		_puzzleMachine.GameData.WinAmountChangeEventHandler += UpdateWinText;
		_puzzleMachine.GameData.BetAmountChangeEventHandler += UpdateBet;
		_puzzleMachine.GameData.WinAmountChangeEventHandler += UpdateWin;

		UpdateBetText(_puzzleMachine.GameData.BetAmount, _puzzleMachine.GameData.BetAmount);
		UpdateWinText(_puzzleMachine.GameData.WinAmount, _puzzleMachine.GameData.WinAmount);

		InitButtonEffect();
		InitSpinButton();
		InitPayTable();
		InitBetTip ();
		InitBetWinFrame();
		_tournamentManager.StartTournament((int)_puzzleMachine.GameData.BetAmount);

		if(!UserDeviceLocalData.Instance.FirstPlayMacthineDic.ContainsKey(_puzzleMachine.MachineName))
		{
//			//delay a little to avoid the halt effect when popping up
//			UnityTimer.Start(this, 1.0f, () => {
//				
//			});

			ShowPayTable(true);
			UserDeviceLocalData.Instance.FirstPlayMacthineDic.Add(_puzzleMachine.MachineName, _puzzleMachine.MachineName);
			UserDeviceLocalData.Instance.Save();
		}
	}

	private void InitBetWinFrame(){
		BasicConfig config = _puzzleMachine.CoreMachine.MachineConfig.BasicConfig;
		if (!config.BetWinFrameImage.IsNullOrEmpty()){
			Sprite spr = AssetManager.Instance.LoadMachineAsset<Sprite>(ImageDir + config.BetWinFrameImage, _puzzleMachine.MachineName);
			if (spr != null){
				_betWinFrame.sprite = spr;

				if (config.IsBetWinFrameSimpleMode){
					_betWinFrame.type = Image.Type.Simple;
				}
			}else{
				LogUtility.Log("InitBetWinFrame failed " + config.BetWinFrameImage);
			}
		}
	}

	private void InitPayTable()
	{
		var payTable = AssetManager.Instance.LoadMachineAsset<GameObject>(PayTableAddress + _puzzleMachine.MachineName, _puzzleMachine.MachineName);
		_currPayTable = Instantiate(payTable);
		_scrollRectOfCurrPayTable = _currPayTable.GetComponentInChildren<ScrollRect> ();
        _payTableController = _currPayTable.GetComponent<PayTableController>();
		_currPayTable.SetActive(false);
	}

	private void InitButtonEffect()
	{
		SetButtonEffectDelay(_minusBetButtonEffect, _buttonEffectDeltaTime);
		SetButtonEffectDelay(_addBetButtonEffect, _buttonEffectDeltaTime * 2);
		SetButtonEffectDelay(_buyButtonEffect, _buttonEffectDeltaTime * 3);
		SetButtonEffectDelay(_maxBetButtonEffect, _buttonEffectDeltaTime * 4);
		SetButtonEffectDelay(_spinButtonEffect, _buttonEffectDeltaTime * 5);
	}

	private void InitBetTip(){
		GameObject obj = AssetManager.Instance.LoadAsset<GameObject> (_betTipAssetPath);
		if (obj != null) {
			GameObject betTip = UGUIUtility.CreateObj (obj, _betFrame);
			if (betTip != null) {
				_betTipBehaviour = betTip.GetComponent<BetTipBehaviour> ();
			}
		} else {
			LogUtility.Log ("BetTip loaded failed : " + _betTipAssetPath);
		}
	}

	private void SetButtonEffectDelay(GameObject obj, float delay)
	{
		obj.SetActive(false);
		UnityTimer.Start(this, delay, () =>
		{
			obj.SetActive(true);
		});
	}

	private void InitSpinButton()
	{
		LongPressEventTrigger trigger = _spinButton.GetComponent<LongPressEventTrigger>();
		trigger.DurationThreshold = _puzzleMachine.PuzzleConfig._pressAutoSpinTime;
		trigger.PointerDownEventHandler += SpinButtonDown;
		trigger.LongPressEventHandler += SpinButtonLongPressCallback;
	}

	#endregion

	#region Spin button

	public void SpinButtonDown(LongPressEventTrigger trigger)
	{
		_puzzleMachine.SpinButtonDown();

		AudioManager.Instance.PlaySound(AudioType.Click);

	    if (UserBasicData.Instance.IsFirstSpin)
	    {
            AdjustManager.Instance.FirstSpin();
	        UserBasicData.Instance.IsFirstSpin = false;
	    }
	}

	public void SpinButtonLongPressCallback(LongPressEventTrigger trigger)
	{
		_puzzleMachine.SpinButtonLongPressed();
	}

	public void SetSpinButtonState(SpinButtonState s)
	{
		SpinButtonHandler handler = _spinButton.GetComponent<SpinButtonHandler>();
		handler.SetState(s);
	}

	#endregion

	#region Button

	public void PayTableButtonDown()
	{
		ShowPayTable(false);
	}

	private void ShowPayTable(bool isFirst)
	{
        _payTableController.Show(isFirst);
//		Animator animator = _currPayTable.GetComponent<Animator>();
//		if(animator)
//			animator.enabled = !isFirst;
//
//		_currPayTable.SetActive(true);
		StartCoroutine (_srollPayTableToTheTop ());
	}

	// (XuHuajie) Don not know why but it seems that setting scroll position has to be done at the end of frame.
	private System.Collections.IEnumerator _srollPayTableToTheTop()
	{
		yield return new WaitForEndOfFrame ();
		_scrollRectOfCurrPayTable.verticalNormalizedPosition = 1;
	}

	public void AddBetButtonDown()
	{
		if(CanChangeBetButton())
		{
			int betIndex = _puzzleMachine.GameData.BetIndex;
			ulong prevBet = _puzzleMachine.GameData.BetAmount;
			bool isChanged = _puzzleMachine.GameData.AddBet();

			PlayAddBetSound(betIndex);

			if (isChanged) {
				ulong curBet = _puzzleMachine.GameData.BetAmount;
				AnalysisManager.Instance.SendBetChange (_puzzleMachine.MachineName, (int)prevBet, (int)curBet);

				if (_changeBetAmountHandler != null)
					_changeBetAmountHandler ();
			} else {
				if (_noChangeBetAmountHandler != null) {
					_noChangeBetAmountHandler ();
				}
			}
		}
	}

	public void SetBet(ulong bet)
	{
		if(CanChangeBetButton())
		{
			int betIndex = MathUtility.GetLocateIndex(_puzzleMachine.GameData.BetOptions, bet);
			ulong prevBet = _puzzleMachine.GameData.BetAmount;
			bool isChanged = _puzzleMachine.GameData.BetIndex != betIndex;

			_puzzleMachine.GameData.SetBetIndex(betIndex);

			PlayMinusBetSound(betIndex);

			if(isChanged)
			{
				ulong curBet = _puzzleMachine.GameData.BetAmount;
				AnalysisManager.Instance.SendBetChange(_puzzleMachine.MachineName, (int)prevBet, (int)curBet);

				if(_changeBetAmountHandler != null)
					_changeBetAmountHandler();
			}else {
				if (_noChangeBetAmountHandler != null) {
					_noChangeBetAmountHandler ();
				}
			}
		}
	}

	private void PlayAddBetSound(int betIndex)
	{
		betIndex = (betIndex >= AudioDefine.MaxBetSound) ? AudioDefine.MaxBetSound - 1 : betIndex;
		AudioType type = AudioType.AddBet1 + betIndex;
		AudioManager.Instance.PlaySound(type);
	}

	public void MinusBetButtonDown()
	{
		if(CanChangeBetButton())
		{
			ulong prevBet = _puzzleMachine.GameData.BetAmount;
			bool isChanged = _puzzleMachine.GameData.SubtractBet();
			int betIndex = _puzzleMachine.GameData.BetIndex;

			PlayMinusBetSound(betIndex);

			if(isChanged)
			{
				ulong curBet = _puzzleMachine.GameData.BetAmount;
				AnalysisManager.Instance.SendBetChange(_puzzleMachine.MachineName, (int)prevBet, (int)curBet);

				if(_changeBetAmountHandler != null)
					_changeBetAmountHandler();
			}else {
				if (_noChangeBetAmountHandler != null) {
					_noChangeBetAmountHandler ();
				}
			}
		}
	}

	private void PlayMinusBetSound(int betIndex)
	{
		betIndex = (betIndex >= AudioDefine.MaxBetSound) ? AudioDefine.MaxBetSound - 1 : betIndex;
		AudioType type = AudioType.MinusBet1 + betIndex;
		AudioManager.Instance.PlaySound(type);
	}

	public void MaxBetButtonDown()
	{
		if(CanChangeBetButton())
		{
			bool isNoChanged = _puzzleMachine.GameData.IsBetHighest;// 已经是最高bet
			ulong prevBet = _puzzleMachine.GameData.BetAmount;
			_puzzleMachine.GameData.MaxBet();
			AudioType maxBetSound = AudioType.AddBet1 + AudioDefine.MaxBetSound - 1;
			AudioManager.Instance.PlaySound(maxBetSound);

			ulong curBet = _puzzleMachine.GameData.BetAmount;
			AnalysisManager.Instance.SendMaxBet(_puzzleMachine.MachineName, (int)prevBet, (int)curBet);

			if(_changeBetAmountHandler != null)
				_changeBetAmountHandler();

			if (isNoChanged && _noChangeBetAmountHandler != null){
				_noChangeBetAmountHandler ();
			}
		}
	}

	private bool CanChangeBetButton()
	{
		MachineState s = _puzzleMachine.GetState();
		return s == MachineState.Idle;
	}

	#endregion

	#region Text

	private void UpdateBetText(ulong prevAmount, ulong betAmount)
	{
		string str = StringUtility.ConvertDigitalULongToString(betAmount);
		_betText.text = str;
	}

	private void UpdateWinText(ulong prevAmount, ulong winAmount)
	{
	    bool cancelTickAnim = _puzzleMachine.GetSpecialModeEffectDuringTime() > 0;
		if(winAmount == 0 || cancelTickAnim)
		{
			string str = StringUtility.ConvertDigitalULongToString(winAmount);
			_winText.text = str;
		}
		else
		{
			float tickTime = _puzzleMachine.PuzzleConfig.GetNumberTickTime(_puzzleMachine);
			_puzzleMachine.SpinData.SetNumberTickTime(tickTime);
			_winTickHandler.StartTick(prevAmount, winAmount, tickTime);
		}
	}

	private void UpdateBet(ulong prevAmount, ulong betAmount)
	{
		_tournamentManager.ChangeBet((int)betAmount);
	}

	private void UpdateWin(ulong prevAmount, ulong winAmount)
	{
		if(winAmount != 0)
			_tournamentManager.Report(winAmount - prevAmount);
	}

	public void UpdateWin(ulong deltaAmount)
	{
		if(deltaAmount != 0)
			_tournamentManager.Report(deltaAmount);
	}

    #endregion

    #region Effect

    void PlayDoubleWinEffect(NumberTickHandler handler)
    {
        Debug.LogError("播放双倍奖励特效");
    }

    #endregion

    #region Public

    public void EndPostSpinEarly()
	{
		_winTickHandler.StopTick();
	}

	#endregion
}


