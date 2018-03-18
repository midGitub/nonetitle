using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplierFreeSpinController : MonoBehaviour {
	public Text _multiplier;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetMultiplier(float multiplier){
		if (_multiplier != null) {
			string str = "X " + multiplier.ToString ();
			_multiplier.text = str;
		}
	}
}
