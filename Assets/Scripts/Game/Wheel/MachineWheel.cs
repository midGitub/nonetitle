using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using System;

public enum WheelStructureType
{
	Separate,
	Overlap,
}

[Serializable]
public class WheelRatioNumber
{
	public Text _text;
	public Animator _animator;
}

public class MachineWheel : WheelBase {
	private enum WheelEffectType
	{
		Before = 0,
		Spin,
		Post,
		Max
	};

	static readonly float _ratioDelayTime = 0.3f;
	static readonly float _resultDelayTime = 0.7f;
	static string TextHintScaleAnimationClip = "TextHintScale";

	public WheelRatioNumber[] _wheelRatioNumbers;
	private int _wheelRatioNumberCount;
	public WheelStructureType _wheelStructureType;

	public bool _popBoardAfterSpin;
	public GameObject[] _beforePopBoardShowObjects;
	public GameObject[] _afterPopBoardShowObjects;

	public GameObject _board;
	public Animator _animator;
	public Text _textAmount;
	public Text _textResult;
	public Animator _resultAnimator;

	public GameObject[] _beforeEffects;
	public GameObject[] _spinEffects;
	public GameObject[] _postEffects;
	public GameObject[] _chooseEffects;

	private int _exitHash = 0;

	private AnimatorStateEvent _animatorStateEvent;

	private int _wheelCount;
	private WheelConfig[] _wheelConfigs;
	private WheelData[] _wheelDatas;
	private bool[] _isWheelStarted;

	private Dictionary<int, WheelController[]> _wheelControllerDict = new Dictionary<int, WheelController[]>();

	private bool _isEnded;

	private Callback<bool> _finishEvent;
	public Callback<bool> FinishEvent{
		get { return _finishEvent; }
		set { _finishEvent = value; }
	}

	#region Init

	public override void Start () {
		base.Start ();

		_exitHash = Animator.StringToHash ("ExitWheel");

		_wheelRatioNumberCount = _wheelRatioNumbers.Length;

		_animator = gameObject.GetComponent<Animator> ();
		Debug.Assert (_animator != null, "MachineWheel animator is null");
		_animatorStateEvent = _animator.GetBehaviour<AnimatorStateEvent> ();
	}

	public void InitWheel (PuzzleMachine puzzleMachine, WheelConfig[] wheelConfigs)
	{
		base.Init (puzzleMachine);

		_isEnded = false;

		for(int i = 0; i < _wheelCount; i++)
		{
			_isWheelStarted[i] = false;
			setWheelEffect (WheelEffectType.Before, i);
		}

		_animator.SetBool(_exitHash, false);

		_wheelConfigs = wheelConfigs;
		_wheelCount = _wheelConfigs.Length;
		_wheelDatas = new WheelData[_wheelCount];
		_wheelControllerDict.Clear();

		if(_wheelStructureType == WheelStructureType.Separate)
		{
			Debug.Assert(_wheelConfigs.Length == _rotaryControllers.Length);
			for(int i = 0; i < _wheelConfigs.Length; i++)
			{
				_rotaryControllers[i].ResetRotation ();
				_rotaryControllers[i].Init(_wheelConfigs[i], 0);
				_wheelControllerDict.Add(i, new WheelController[]{ _rotaryControllers[i] });
			}
		}
		else if(_wheelStructureType == WheelStructureType.Overlap)
		{
			//Note by nichos: only support 1 for now
			Debug.Assert(_wheelConfigs.Length == 1);
			for (int i = 0; i < _rotaryControllers.Length; ++i)
			{
				_rotaryControllers[i].ResetRotation ();
				_rotaryControllers[i].Init(_wheelConfigs[0], i);
			}
			_wheelControllerDict.Add(0, _rotaryControllers);
		}

		_isWheelStarted = new bool[_wheelCount];
		for(int i = 0; i < _wheelCount; i++)
			_isWheelStarted[i] = false;

		for(int i = 0; i < _chooseEffects.Length; i++)
			_chooseEffects[i].SetActive(false);

		_textAmount.text = _puzzleMachine.CoreMachine.SpinResult.NormalizedBetAmount.ToString();

		ListUtility.ForEach(_wheelRatioNumbers, (WheelRatioNumber ratioNumber) => {
			ratioNumber._text.text = "?";
		});
		_textResult.text = "?";

		InitBoard();

		HandlePopBoardObjects(false);

		SetWheelRootVisible(true);

		//force to not trigger next spin
		_puzzleMachine.SpinData.IsUserTriggeredNextSpin = false;
	}

	public override void OnEnable(){
		base.OnEnable ();
		CitrusFramework.CitrusEventManager.instance.Raise(new TournamentChangeSoundEvent (false));
	}

	void OnDisable(){
		CitrusFramework.CitrusEventManager.instance.Raise(new TournamentChangeSoundEvent (true));
	}

	void InitBoard()
	{
		if(_popBoardAfterSpin)
			_board.SetActive(false);
	}

	#endregion
	
	// Update is called once per frame
	public override void Update () {
		base.Update ();

		bool isAllWheelStarted = ListUtility.IsAllElementsSatisfied(_isWheelStarted, (bool b) => {
			return b;
		});

		if(isAllWheelStarted && !base.IsRotating() && !_isEnded) {
			TriggerEnd();
		}
	}

	void TriggerEnd()
	{
		_isEnded = true;

		if(_wheelStructureType == WheelStructureType.Overlap)
		{
			for(int i = 0; i < _chooseEffects.Length; i++)
			{
				_chooseEffects[i].SetActive(true);
				setWheelEffect (WheelEffectType.Post, i);
			}
		}

		StartCoroutine(EndCoroutine());
	}

	public override void ButtonPress(GameObject obj)
	{
		Button curButton = obj.GetComponent<Button>();
		int index = _clickButtons.IndexOf(curButton);

		if(!_isWheelStarted[index])
		{
			_isWheelStarted[index] = true;

			base.ButtonPress (obj);

			setWheelEffect (WheelEffectType.Spin, index);

			_wheelDatas[index] = WheelHelper.RandomCreateWheelData (_wheelConfigs[index], _puzzleMachine.CoreMachine.Roller);

			AudioManager.Instance.PlaySound(AudioType.WheelSpin);
			AudioManager.Instance.PlaySound(AudioType.M10_SpinWheel);

			WheelController[] controllers = _wheelControllerDict[index];
			ListUtility.ForEach<WheelController> (controllers, (WheelController c) => {
				c.StartRotate(_wheelDatas[index], () => {
					if(_wheelStructureType == WheelStructureType.Separate)
					{
						_chooseEffects[index].SetActive(true);
						setWheelEffect (WheelEffectType.Post, index);
					}
					AudioManager.Instance.PlaySound(AudioType.WheelSpin);
				});
			});
		}
	}

	public override void OnDestroy ()
	{
		base.OnDestroy ();
	}

	public override void StartAnimation ()
	{
		base.StartAnimation ();
	}

	public override void EndAnimation ()
	{
		base.EndAnimation ();
	}

	public override bool IsRotating ()
	{
		return base.IsRotating ();
	}

	private void showCoinsEffect(float winAmount){
//		float tickTime = 0.0f;
//
//		NumberTickHandler handler = gameObject.GetComponent<NumberTickHandler> ();
//		handler.StartTick (0, (ulong)winAmount, tickTime);
	}

	private void ProcessWinAmount()
	{
		if(_popBoardAfterSpin)
			_board.SetActive(true);

		HandlePopBoardObjects(true);
		
		float totalRatio = WheelHelper.GetTotalRatio(_wheelDatas);
		ulong winAmount = (ulong)(_puzzleMachine.CoreMachine.SpinResult.NormalizedBetAmount * totalRatio);

		_textAmount.text = _puzzleMachine.CoreMachine.SpinResult.NormalizedBetAmount.ToString ();

		if(_wheelStructureType == WheelStructureType.Separate)
		{
			for(int i = 0; i < _wheelCount; i++)
			{
				float ratio = WheelHelper.GetSingleRatio(_wheelDatas[i]);
				_wheelRatioNumbers[i]._text.text = ratio.ToString();
			}
		}
		else if(_wheelStructureType == WheelStructureType.Overlap)
		{
			Debug.Assert(_wheelDatas.Length == 1);
			int ratioCount = WheelHelper.GetRatioCount(_wheelDatas[0]);
			for(int i = 0; i < ratioCount; i++)
			{
				float ratio = _wheelDatas[0].Ratio[i];
				_wheelRatioNumbers[i]._text.text = ratio.ToString();
			}
		}

		_textResult.text = winAmount.ToString ();

		_puzzleMachine.AddWinAmountForIndieGame(winAmount, SmallGameMomentType.Front);

		int substractLuckyAmount = (int)winAmount;
		_puzzleMachine.CoreMachine.LuckyManager.LongLuckyManager.SubtractLongLucky(substractLuckyAmount);
        AnalysisManager.Instance.SendWheelSpin(0, _puzzleMachine, totalRatio, _puzzleMachine.CoreMachine.SmallGameState, _puzzleMachine._specialMode);

//		showCoinsEffect (winAmount);
	}

	private IEnumerator EndCoroutine()
	{
		yield return new WaitForSeconds(1.0f);

		ProcessWinAmount ();

		AudioManager.Instance.PlaySound(AudioType.M10_WheelResult);

		for(int i = 0; i < _wheelRatioNumberCount; i++)
		{
			float delay = (i == 0) ? 0.0f : _ratioDelayTime;
			yield return new WaitForSeconds(delay);
			_wheelRatioNumbers[i]._animator.Play (TextHintScaleAnimationClip);
			yield return new WaitForSeconds(0.2f);
			AudioManager.Instance.PlaySound(AudioType.DailyBonus);
		}

		yield return new WaitForSeconds(_resultDelayTime);
		_resultAnimator.Play (TextHintScaleAnimationClip);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlaySound(AudioType.DailyBonus);

		yield return new WaitForSeconds(2.0f);

		_animator.SetBool(_exitHash, true);

		yield return new WaitForSeconds(2.0f);

		disableAllEffect();
		gameObject.SetActive (false);
		if (_finishEvent != null){
			_finishEvent(false);
		}
		AudioManager.Instance.StopSound(AudioType.M10_WheelBGM);

		SetWheelRootVisible(false);
	}

	void HandlePopBoardObjects(bool isPopBoard)
	{
		if(_beforePopBoardShowObjects != null && _beforePopBoardShowObjects.Length > 0)
		{
			ListUtility.ForEach(_beforePopBoardShowObjects, (GameObject o) => {
				o.SetActive(!isPopBoard);
			});
		}

		if(_afterPopBoardShowObjects != null && _afterPopBoardShowObjects.Length > 0)
		{
			ListUtility.ForEach(_afterPopBoardShowObjects, (GameObject o) => {
				o.SetActive(isPopBoard);
			});
		}
	}

	//patch for blink issue when there is canvas on WheelRoot
	void SetWheelRootVisible(bool flag)
	{
		ListUtility.ForEach(_rotaryControllers, (WheelController c) => {
			GameObject obj = c.gameObject.transform.parent.gameObject;
			if(obj != null && obj.GetComponent<Canvas>() != null)
				obj.SetActive(flag);
		});
	}

	private void setWheelEffect(WheelEffectType type, int wheelIndex){
		if (type == WheelEffectType.Before) {
			_beforeEffects[wheelIndex].SetActive (true);
			_spinEffects[wheelIndex].SetActive (false);
			_postEffects[wheelIndex].SetActive (false);
		} else if (type == WheelEffectType.Spin) {
			_beforeEffects[wheelIndex].SetActive (false);
			_spinEffects[wheelIndex].SetActive (true);
			_postEffects[wheelIndex].SetActive (false);
		} else if (type == WheelEffectType.Post) {
			_beforeEffects[wheelIndex].SetActive (false);
			_spinEffects[wheelIndex].SetActive (false);
			_postEffects[wheelIndex].SetActive (true);
		}
	}

	private void disableAllEffect(){
		for(int i = 0; i < _wheelCount; i++)
		{
			_beforeEffects[i].SetActive (false);
			_spinEffects[i].SetActive (false);
			_postEffects[i].SetActive (false);
			_chooseEffects[i].SetActive (false);
		}
	}
}
