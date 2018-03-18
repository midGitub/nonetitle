using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using System;

public class ButterflyController : MonoBehaviour {
	// 对应三卷轴上的蝴蝶飞行效果
	public GameObject[] _butterflyUpEffects;
	// 蝴蝶飞舞时间
	public float _flyingTime = 1.5f;
	// 播放延迟
	public float _startDelayMin = 0.0f;
	public float _startDelayMax = 0.15f;
	// 元素实例
	private CoreSymbol _symbol;
	// 蝴蝶飞舞协程
	private Coroutine _coroutine = null;
	private System.Random _random;
	private PuzzleMachine _machine;
	private AudioType _flySound;
	private AudioType _hitSound;

	// Use this for initialization
	void Awake () {
		_random = new System.Random();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnEnable(){
		StartFly ();
	}

	void OnDisable(){
		ResetCoroutine ();
	}

	public void Init(PuzzleMachine machine, CoreSymbol symbol) {
		_symbol = symbol;
		_machine = machine;

		_flySound = _machine.MachineConfig.BasicConfig.ButterflyUpAudio;
		_hitSound = _machine.MachineConfig.BasicConfig.ButterflyHitAudio;
	}

	public void StartFly(){
		if (_symbol == null) return;

		CloseAllEffect ();

		int reelIndex = _symbol.ReelIndex;
		if (reelIndex < _butterflyUpEffects.Length) {
			float delay = _startDelayMin + ((float) _random.Next() / int.MaxValue) * _startDelayMax;
			UnityTimer.Start(this, delay, ()=>{
				_butterflyUpEffects [reelIndex].SetActive (true);
				AudioManager.Instance.PlaySound(_flySound);
			});

			UnityTimer.Start(this, delay + 1.0f, ()=>{
				AudioManager.Instance.PlaySound(_hitSound);
			});

			ResetCoroutine ();

			_coroutine = UnityTimer.Start (this, _flyingTime, () => {
				LogUtility.Log("end in StartFly", Color.red);
				_butterflyUpEffects[reelIndex].SetActive(false);
			});

		}
	}

	private void CloseAllEffect(){
		for (int i = 0; i < _butterflyUpEffects.Length; i++) {
			_butterflyUpEffects [i].SetActive (false);
		}
	}

	private void ResetCoroutine(){
		if (_coroutine != null) {
			StopCoroutine (_coroutine);
		}
	}
}
