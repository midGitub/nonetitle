using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BackgroundType{
	Idle,
	Spin,
	Freespin,
	Max,
}

public class BackgroundEffectBehaviour : MonoBehaviour {
	private GameObject _idleEffect;
	private GameObject _spinEffect;
	private GameObject _freespinEffect;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetBackgroundEffect(BackgroundType type, GameObject obj){
		if (type == BackgroundType.Idle) {
			_idleEffect = obj;
		} else if (type == BackgroundType.Spin) {
			_spinEffect = obj;
		} else if (type == BackgroundType.Freespin) {
			_freespinEffect = obj;
		} else {
			LogUtility.Log ("no background effect is set : type = " + type.ToString(), Color.red);
		}
	}

	public void ShowEffect(BackgroundType type){
		CloseEffect ();
		if (type == BackgroundType.Idle) {
			if (_idleEffect != null)
				_idleEffect.SetActive (true);
		} else if (type == BackgroundType.Spin) {
			if (_spinEffect != null)
				_spinEffect.SetActive (true);
		} else if (type == BackgroundType.Freespin) {
			if (_freespinEffect != null)
				_freespinEffect.SetActive (true);
		}
	}

	public void CloseEffect(){
		if (_idleEffect != null)
			_idleEffect.SetActive (false);
		if (_spinEffect != null)
			_spinEffect.SetActive (false);
		if (_freespinEffect != null)
			_freespinEffect.SetActive (false);
	}
}
