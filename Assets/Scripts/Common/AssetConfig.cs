using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AssetConfig
{
	private static bool _isUseExcelAssetBundle = true;
	private static bool _isExcelAssetBundleEncrypted = true;

	public static bool IsUseExcelAssetBundle { get { return _isUseExcelAssetBundle; } }
	public static bool IsExcelAssetBundleEncrypted { get { return _isExcelAssetBundleEncrypted; } }
}
