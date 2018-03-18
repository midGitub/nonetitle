using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using CitrusFramework;

public class DelayEffectTest : MonoBehaviour {
	// 播放延迟
	public float _startDelay = 1.0f;
	// 关闭延迟
	public float _closeDelay = -1.0f;

	private ParticleSystem[] _particleSystems;

	void Start(){
		_particleSystems = gameObject.GetComponentsInChildren<ParticleSystem> (true);
	}

	// Use this for initialization
	void OnEnable () {
		if (_startDelay > 0.0f) {
			ParticlePlay (false);
			Invoke ("Show", _startDelay);
		}

		if (_closeDelay >= 0.0f) {
			Invoke ("Close", _closeDelay);
		}
	}

	void Show(){
		ParticlePlay (true);
	}

	void Close(){
		ParticlePlay (false);
	}

	void ParticlePlay(bool play){
		for (int i = 0; i < _particleSystems.Length; ++i) {
			if (play) {
				_particleSystems [i].Play ();
			} else {
				_particleSystems [i].Stop ();
			}
		}
	}

	// Update is called once per frame
	void Update () {

	}
}
