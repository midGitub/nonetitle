using System.Collections.Generic;
using UnityEngine;

public static class WideResolutionHelper
{
	// This number is a litter greater than DeviceUtility.DesignRatio
	static float aspectRatioThreshold = 2.0f;

	static float _blackBorderWidth = 0.0f;

	static WideResolutionHelper()
	{
		InitBlackBorderWidth();
	}

	static void InitBlackBorderWidth()
	{
		int width = Screen.width;
		int height = Screen.height;

		float ratio = (float)width / (float)height;
		if(ratio > aspectRatioThreshold)
		{
			float curWidth = DeviceUtility.DesignHeight * ratio;
			_blackBorderWidth = 0.5f * (curWidth - DeviceUtility.DesignWidth) / curWidth;
		}
	}

	public static bool ShouldAdapt()
	{
		return _blackBorderWidth > 0.0f;
	}

	public static float GetBlackBorderWidth()
	{
		return _blackBorderWidth;
	}
}
