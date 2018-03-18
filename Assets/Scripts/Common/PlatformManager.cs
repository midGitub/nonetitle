using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public enum ChannelType
{
	None,
	iOS,
	GooglePlay,
    iOS_IW,  //IW meas invincible warroir company
    Count
}

public enum PlatformType
{
	None,
	iOS,
	Android,
	Web
}

public class PlatformManager : SimpleSingleton<PlatformManager>
{
	public ChannelType GetChannelType()
	{
		ChannelType type = ChannelType.None;

#if UNITY_EDITOR
		type = ChannelType.None;
#elif UNITY_IOS
        type = PackageConfigManager.Instance.CurPackageConfig.ChannelType;
#elif UNITY_ANDROID
		type = ChannelType.GooglePlay;
#else
		//#error Unknown platform
#endif

        return type;
	}

	public string GetChannelString()
	{
		string result = "";
		ChannelType type = GetChannelType();
		switch(type)
		{
			case ChannelType.iOS:
				result = "iOS";
				break;
			case ChannelType.GooglePlay:
				result = "GooglePlay";
				break;
            case ChannelType.iOS_IW:
                result = "iOS_IW";
                break;
			default:
				result = "";
				break;
		}

		return result;
	}

	public string GetPlatformString()
	{
		#if UNITY_EDITOR
		return "Editor";
		#elif UNITY_IOS
		return "iOS";
		#elif UNITY_ANDROID
		return "Android";
		#else
		Debug.Assert(false);
		return "Android";
		#endif
	}

	// 跟服务器通信时使用的channel字段，有别于我们神策上打点的channel
	// 对于服务器来说，它没有平台的概念
	public string GetServerChannelString(){
		string result = "";

		#if UNITY_IOS
		result = "iOS";
		#elif UNITY_ANDROID
		result = "Android";
		#elif USE_iOS_IW
		result = "iOS_IW";
		#else
		result = "default";
		#endif

		return result.ToLower();
	}
}
