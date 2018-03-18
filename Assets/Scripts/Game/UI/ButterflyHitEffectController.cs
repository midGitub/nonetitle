using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class ButterflyHitEffectController : MonoBehaviour {
	public GameObject _hitEffect;
	public float _hitDelayTime = 1.5f;
	private Coroutine _coroutine = null;

	// Use this for initialization
	void Start () {
		
	}

	void OnDisable(){
		_hitEffect.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void ResetCoroutine(){
		if (_coroutine != null) {
			StopCoroutine (_coroutine);
		}
	}

	public void Show(){
		_hitEffect.SetActive (true);
		ResetCoroutine ();
		_coroutine = UnityTimer.Start (this, _hitDelayTime, () => {
			_hitEffect.SetActive(false);
		});
	}
}
