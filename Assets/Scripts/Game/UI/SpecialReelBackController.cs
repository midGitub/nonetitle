using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialReelBackController : MonoBehaviour {
	public Animator _animator;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Play(string clip){
		_animator.Play (clip);
	}

	public void PlayTrigger(string trigger){
		int hash = Animator.StringToHash (trigger);
		_animator.SetTrigger (hash);
	}
}
