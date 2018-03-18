using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MapMachineAnimatorController : MonoBehaviour {

	[SerializeField]
	private Animator _machineAnimator;

	private MachineButton _button;

	/// <summary>
	/// 开始播放的时间间隔 
	/// </summary>
	[SerializeField]
	private float _startIntervalPlayTime;

	[SerializeField]
	private float _playAgainInterval;

	// 将名称转换为哈希值
	private int _playKey = Animator.StringToHash("Play");
	// 解锁动画
	private int _unlockKey = Animator.StringToHash("Unlock");

	// 缩小
	private bool _bShrink = false;
	private bool _init = false;

	private void Init(){
		if (!_init){
			_init = true;

			#if false
			CitrusFramework.UnityTimer.Instance.StartTimer(this,_startIntervalPlayTime, Play);
			#endif
			_machineAnimator.GetBehaviour<AnimatorStateEvent>().OnStateExitEvent.AddListener(PlayAgain);

			_button = gameObject.GetComponentInChildren<MachineButton> ();
			_button._downHandler = OnPointDown;
			_button._upHandler = OnPointUp;
			
			string name = _button._machineController.MachineName;
			_startIntervalPlayTime = MachineUnlockSettingConfig.Instance.GetMachineStartDelay(name);
			_playAgainInterval = MachineUnlockSettingConfig.Instance.GetMachinePlayAgainDelay(name);
		}
	}

	// Use this for initialization
	void Start () 
	{
		Init();
	}

	private void PlayAgain()
	{
		_machineAnimator.SetBool(_playKey, false);
		CitrusFramework.UnityTimer.Instance.StartTimer(this,_playAgainInterval, Play);
	}

	public void StartPlay(){
		Init();
		if (_machineAnimator != null)
			_machineAnimator.SetBool(_playKey, false);
		CitrusFramework.UnityTimer.Instance.StartTimer(this,_startIntervalPlayTime, Play);
	}

	public void Play()
	{
		_machineAnimator.SetBool(_playKey, true);
	}

	public void Unlock(){
		_machineAnimator.SetBool (_unlockKey, true);
		UnityTimer.Start (this, 3.0f, () => {
			_machineAnimator.SetBool(_unlockKey, false);
		});
	}

	private void OnDisable()
	{
		// CitrusFramework.UnityTimer.Instance.Sto
		// StopAllCoroutines();
	}

	private void OnPointDown(PointerEventData eventData){
//		LogUtility.Log ("point down "+gameObject.name, Color.red);
		Vector3 scale = new Vector3(0.95f, 0.95f, 0.95f);
		Tweener tweener = transform.DOScale (scale, 0.2f);
		_bShrink = true;
	}

	private void OnPointUp(PointerEventData eventData){
//		LogUtility.Log ("point up "+gameObject.name, Color.red);
		if (_bShrink) {
			Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);
			if (DeviceUtility.IsIPadResolution()){
				scale = new Vector3(1.1f, 1.1f, 1.0f);
			}
			Tweener tweener = transform.DOScale (scale, 0.2f);
			_bShrink = false;
		}
	}
}
