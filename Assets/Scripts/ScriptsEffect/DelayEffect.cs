using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class DelayEffect : MonoBehaviour {
	// 播放延迟
	public float _startDelay = 1.0f;
	// 关闭延迟
	public float _closeDelay = -1.0f;

	// Use this for initialization
	void Start () {
		if (_startDelay > 0.0f) {
			gameObject.SetActive (false);
			Invoke ("Show", _startDelay);
		}

		if (_closeDelay >= 0.0f) {
			Invoke ("Close", _closeDelay);
		}
	}

	void Show(){
		gameObject.SetActive (true);
	}

	void Close(){
		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
