using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerAdaptor : MonoBehaviour
{
	//Note: this flag should only be checked for pure popup UI such as Setting, Store, etc.
	public bool _shouldAdaptIpad = false;

	[Tooltip("This value will be set to Match variable in Canvas Scaler on iPad")]
	[Range(0.0f, 1.0f)]
	public float _matchOnIpad = 0.3f;

	void Start()
	{
		TryAdaptWideResolution();
		TryAdaptTallResolution();
	}

	void TryAdaptWideResolution()
	{
		if(WideResolutionHelper.ShouldAdapt())
		{
			CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
			scaler.matchWidthOrHeight = 1.0f;
		}
	}

	void TryAdaptTallResolution()
	{
		if(_shouldAdaptIpad && DeviceUtility.IsIPadResolution())
		{
			CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
			scaler.matchWidthOrHeight = _matchOnIpad;
		}
	}
}
