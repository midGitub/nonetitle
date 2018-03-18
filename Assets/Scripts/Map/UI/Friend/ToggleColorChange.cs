using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleColorChange : MonoBehaviour {
	public Toggle _toggle;
	public Color _defaultColor = Color.white;
	public Color _changeColor = Color.gray;
	public Image _img;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (_toggle != null && _img != null) {
			_img.color = _toggle.interactable ? _defaultColor : _changeColor;
		}
	}
}
