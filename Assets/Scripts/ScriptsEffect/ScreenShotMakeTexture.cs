using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenShotMakeTexture
{
	public static void TakeScreenshot(MonoBehaviour HelperMon,Action<Texture2D> callBack)
	{
		HelperMon.StartCoroutine(CreatScreenShotTexture(callBack));
	}

	public static IEnumerator CreatScreenShotTexture(Action<Texture2D> callBack)
	{
		yield return new WaitForEndOfFrame();
		var width = Screen.width;
		var height = Screen.height;
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		// Read screen contents into the texture
		// 读取屏幕像素信息并存储为纹理数据，  
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();
		yield return new WaitForEndOfFrame();
		callBack(tex);
	}

}
