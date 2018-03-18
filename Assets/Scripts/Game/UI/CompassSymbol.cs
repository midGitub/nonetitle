using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassSymbol : MonoBehaviour {
	public Image _compass;
	public GameObject _effect;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Show(bool show){
		_compass.enabled = show;
		_effect.SetActive (show);
	}

	public void Init(string path, string machineName){
		GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject> (path, machineName);
		_effect = UGUIUtility.CreateObj (prefab, gameObject);
	}
}
