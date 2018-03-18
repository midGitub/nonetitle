using UnityEngine;
using System.Collections;
using System.IO;
using System;

public static class BuildUtility
{
	//Increase version number when upgrade version:
	//(1) 代码里: BuildUtility的_androidVersion和_iOSVersion
	//(2) Jenkins: 配置 -> 参数化构建过程 -> version
	//另外，工程设置里的这个设置由于是iOS和Android共享的，所以不用: BuildSetting -> PlayerSetting -> Version

	private static readonly string _androidVersion = "2.2.0";
	private static readonly string _iOSVersion = "2.2.0";
	private static string _versionCode = "-1";

	static BuildUtility()
	{
		ReadVersionCode();
	}

	public static string GetBundleVersion()
	{
		//int code = PlayerSettings.Android.bundleVersionCode;
		#if UNITY_ANDROID || UNITY_EDITOR
		return _androidVersion;
		#elif UNITY_IOS
		return _iOSVersion;
		#else
		return _androidVersion;
		#endif
	}

	public static string GetBundleMajorMinorVersion()
	{
		string s = GetBundleVersion();
		string[] array = s.Split('.');
		string result = string.Format("{0}.{1}", array[0], array[1]);
		return result;
	}

	public static string GetProjectName()
	{
		return "Slots";
	}

	public static void ReadVersionCode()
	{
		string str = "0";
#if UNITY_EDITOR
		if(UnityEditor.EditorUserBuildSettings.activeBuildTarget != UnityEditor.BuildTarget.iOS)
		str = UnityEditor.PlayerSettings.Android.bundleVersionCode.ToString();
		else 
		str = UnityEditor.PlayerSettings.iOS.buildNumber.ToString();
#else  
		TextAsset txtfile = Resources.Load ("versionCode") as TextAsset;
		if (txtfile != null) 
		{
			str = txtfile.text;
		}
		else 
		{
			str = "0";
			Debug.Assert (false, "VersionCode.txt Not Found");
		}
#endif
		_versionCode = str;
	}

	public static string GetVersionCode()
	{ 
		return _versionCode;
	}

}

