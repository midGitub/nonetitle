using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAdapterBehaviour : MonoBehaviour {
	public Vector3 _ipadScale = new Vector3(1.1f, 1.1f, 1.0f);

	private bool _autoAdapt = true;

	// Use this for initialization
	void Start () {
		if (_autoAdapt)
			DoIpadAdapt();
	}

	private void DoIpadAdapt(){
		if (DeviceUtility.IsIPadResolution())
		{
			Vector3 localScale = gameObject.transform.localScale;
			Vector3 newScale =  new Vector3(localScale.x * _ipadScale.x, localScale.y * _ipadScale.y, localScale.z * _ipadScale.z);
			gameObject.transform.localScale = newScale;
			// LogUtility.Log("DoIpaddapt scale scale = " + newScale, Color.red);
		}
	}

	public void UpdateScale(Vector3 scale){
		if (DeviceUtility.IsIPadResolution()){
			gameObject.transform.localScale = scale;
			// LogUtility.Log("update scale scale = " + scale, Color.red);
		}	
	}

	public void EnableAutoAdapt(){
		_autoAdapt = true;
	}

	public void DisableAutoAdapt(){
		_autoAdapt = false;
	}
}
