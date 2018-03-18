using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CitrusFramework;

public class ButterflyHitController : MonoBehaviour {
	// 蝴蝶特效初始位置
	private Transform _startPos;
	// 蝴蝶特效终止位置
	private Transform _destPos;
	// 蝴蝶飞舞特效
	public GameObject _flyEffect;
	// 计分板闪烁特效
	public GameObject _hitEffect;
	// 特效飞行时间
	public float _flyTime = 0.5f;
	// 关闭闪烁特效延迟
	public float _closeHitEffectTime = 1.0f;
	// 闪烁特效协程
	private Coroutine _hitCoroutine = null;
	private PuzzleMachine _machine;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDisable(){
		ReseCoroutine ();
	}

	private void ReseCoroutine(){
		if (_hitCoroutine != null) {
			StopCoroutine (_hitCoroutine);
		}
	}

	public void SetFlyPathTransform(PuzzleMachine machine, Transform start, Transform dest){
		_machine = machine;
		_startPos = start;
		_destPos = dest;
		_flyEffect.transform.position = _startPos.position;
		_hitEffect.transform.position = _destPos.position;
	}

	public void StartFly(Callback<bool> endCallback = null){
		_flyEffect.SetActive (true);
		_flyEffect.transform.position = _startPos.position;
		Tweener tweener = _flyEffect.transform.DOMove (_destPos.position, _flyTime);
		tweener.SetEase (Ease.Linear);
		tweener.OnComplete (()=>{
			_flyEffect.SetActive(false);
			_hitEffect.SetActive(true);

			if(endCallback != null)
				endCallback(false);

			AudioType hitSound = _machine.CoreMachine.MachineConfig.BasicConfig.ButterflyHitAudio;
			AudioManager.Instance.PlaySound(hitSound);
			
			ReseCoroutine();
			_hitCoroutine = UnityTimer.Start (this, _closeHitEffectTime, () => {
				_hitEffect.SetActive(false);
			});	
		});
			
		AudioType flySound = _machine.CoreMachine.MachineConfig.BasicConfig.ButterflyDownAudio;
		AudioManager.Instance.PlaySound (flySound);
	}
}
