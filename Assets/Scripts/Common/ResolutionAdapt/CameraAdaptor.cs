using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraAdaptor : MonoBehaviour
{
	void Start()
	{
		ChangeViewPort();
	}

	void ChangeViewPort()
	{
		if(WideResolutionHelper.ShouldAdapt())
		{
			float borderWidth = WideResolutionHelper.GetBlackBorderWidth();
			Camera camera = gameObject.GetComponent<Camera>();
			camera.rect = new Rect(borderWidth, 0.0f, 1.0f - 2 * borderWidth, 1.0f);
		}
	}
}
