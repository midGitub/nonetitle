using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendCoinController : MonoBehaviour {

	public ParticleSystem _particle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Play(){
		if (_particle != null)
			_particle.Play ();
	}

	public void Stop(){
		if (_particle != null)
			_particle.Stop ();
	}
}
