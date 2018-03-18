using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonChange : MonoBehaviour {
	public Toggle _toggle;
	public GameObject _chooseButton;
	public GameObject _noChooseButton;

	// Use this for initialization
	void Start () {
		_toggle.onValueChanged.AddListener(OnValueChange);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnValueChange(bool value){
		if (value) {
			_chooseButton.SetActive (true);
			_noChooseButton.SetActive (false);
		} else {
			_chooseButton.SetActive (false);
			_noChooseButton.SetActive (true);
		}
	}
}
