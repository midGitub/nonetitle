using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMachineButton : MonoBehaviour
{
	public string _machineName;
	public Text _text;

	public string MachineName { get { return _machineName; } }

	// Use this for initialization
	void Start () {

		if (_text != null)
			_text.text = _machineName;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
