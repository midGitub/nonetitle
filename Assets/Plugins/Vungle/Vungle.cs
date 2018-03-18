using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Threading;

#pragma warning disable 618
#pragma warning disable 162
#pragma warning disable 67

public class Vungle
{
	//Change this constant fields when a new version of the plugin or sdk was released
	private const string PLUGIN_VERSION = "3.1.35";
	private const string IOS_SDK_VERSION = "4.1.0";
	private const string WIN_SDK_VERSION = "1.3.16";
	private const string ANDROID_SDK_VERSION = "4.1.0";

	#region Events

	// Fired when a Vungle ad starts
	public static event Action onAdStartedEvent;

	// Fired when a Vungle ad finishes.
	[Obsolete("Please use onAdFinishedEvent event instead this and onAdViewedEvent event.")]
	public static event Action onAdEndedEvent;
	
	// Fired when a Vungle ad is ready to be displayed
	public static event Action<bool> adPlayableEvent;

	// Fired when a Vungle ad is cached and ready to be displayed
	[Obsolete("Please use adPlayableEvent event instead this and onCachedAdAvailableEvent event.")]
	public static event Action onCachedAdAvailableEvent;

	// Fired when a Vungle video is dismissed and provides the time watched and total duration in that order.
	[Obsolete("Please use onAdFinishedEvent event instead this and onAdEndedEvent event.")]
	public static event Action<double,double> onAdViewedEvent;
	
	// Fired log event from sdk.
	public static event Action<string> onLogEvent;

	//Fired when a Vungle ad finished and provides the entire information about this event.
	public static event Action<AdFinishedEventArgs> onAdFinishedEvent; 

#if (!VUNGLE_AD_OFF)
	static void adStarted()
	{
		if( onAdStartedEvent != null )
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					onAdStartedEvent();
				});
			#else
			onAdStartedEvent();
			#endif
	}

	static void adEnded()
	{
		if (onAdEndedEvent != null)
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					onAdEndedEvent();
				});
			#else
			onAdEndedEvent();
			#endif
    }

	static void videoViewed(double timeWatched, double totalDuration)
	{
		if( onAdViewedEvent != null )
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					onAdViewedEvent(timeWatched, totalDuration);
				});
			#else
			onAdViewedEvent(timeWatched, totalDuration);
			#endif
	}

	static void cachedAdAvailable()
	{
		if( onCachedAdAvailableEvent != null )
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					onCachedAdAvailableEvent();
				});
			#else
			onCachedAdAvailableEvent();
			#endif
	}

	static void adPlayable(bool playable)
	{
		if( adPlayableEvent != null )
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					adPlayableEvent(playable);
				});
			#else
			adPlayableEvent(playable);
			#endif
	}
	
	static void onLog(string log)
	{
		if( onLogEvent != null )
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					onLogEvent(log);
				});
			#else
			onLogEvent(log);
			#endif
	}

    static void adFinished(AdFinishedEventArgs args)
	{
		if (onAdFinishedEvent != null)
			#if UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			VungleSceneLoom.Loom.QueueOnMainThread(() =>
				{
					onAdFinishedEvent(args);
				});
			#else
			onAdFinishedEvent(args);
			#endif
	}
#endif

	#endregion

	public static string VersionInfo
	{
		get
		{
#if (!VUNGLE_AD_OFF)
			StringBuilder stringBuilder = new StringBuilder("unity-");
			#if UNITY_IPHONE
			return stringBuilder.Append(PLUGIN_VERSION).Append("/iOS-").Append(IOS_SDK_VERSION).ToString();
			#elif UNITY_ANDROID
			return stringBuilder.Append(PLUGIN_VERSION).Append("/android-").Append(ANDROID_SDK_VERSION).ToString();
			#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
			return stringBuilder.Append(PLUGIN_VERSION).Append("/android-").Append(WIN_SDK_VERSION).ToString();
			#else
			return stringBuilder.Append(PLUGIN_VERSION).ToString();
			#endif
#else
			return "OFF";
#endif
		}
	}

	static Vungle()
	{
#if (!VUNGLE_AD_OFF)
		VungleManager.OnAdStartEvent += adStarted;
		VungleManager.OnAdEndEvent += adEnded;
		VungleManager.OnCachedAdAvailableEvent += cachedAdAvailable;
		VungleManager.OnAdPlayableEvent += adPlayable;
		VungleManager.OnVideoViewEvent += videoViewed;
		VungleManager.OnSDKLogEvent += onLog;
		VungleManager.OnAdFinishedEvent += adFinished;
#endif
	}

	// Initializes the Vungle SDK. Pass in your Android and iOS app ID's from the Vungle web portal.
	public static void init( string androidAppId, string iosAppId, string winAppId = "" )
	{
#if (!VUNGLE_AD_OFF)
#if UNITY_EDITOR
		return;
#endif
#if UNITY_IPHONE
		VungleBinding.startWithAppId( iosAppId , PLUGIN_VERSION);
#elif UNITY_ANDROID
		VungleAndroid.init( androidAppId , PLUGIN_VERSION);
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
        VungleWin.init(winAppId  , PLUGIN_VERSION);
		VungleSceneLoom.Initialize();
#endif
#endif
    }


    // Sets if sound should be enabled or not
    public static void setSoundEnabled( bool isEnabled )
	{
#if (!VUNGLE_AD_OFF)
#if UNITY_EDITOR
		return;
#endif
#if UNITY_IPHONE
		VungleBinding.setSoundEnabled( isEnabled );
#elif UNITY_ANDROID
		VungleAndroid.setSoundEnabled( isEnabled );
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
        VungleWin.setSoundEnabled( isEnabled );
#endif
#endif
	}


	// Checks to see if a video is available
	public static bool isAdvertAvailable()
	{
#if (!VUNGLE_AD_OFF)
#if UNITY_EDITOR
		return false;
#endif
#if UNITY_IPHONE
		return VungleBinding.isAdAvailable();
#elif UNITY_ANDROID
		return VungleAndroid.isVideoAvailable();
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
        return VungleWin.isVideoAvailable();
#else
		return false;
#endif
#else
		return false;
#endif
	}


	// Displays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd( bool incentivized = false, string user = "", int orientation = 6)
	{
#if (!VUNGLE_AD_OFF)
#if UNITY_EDITOR
		return;
#endif
#if UNITY_IPHONE
		VungleBinding.playAd( incentivized, user, (VungleAdOrientation)orientation);
#elif UNITY_ANDROID
		VungleAndroid.playAd( incentivized, user );
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
        VungleWin.playAd( incentivized, user );
#endif
#endif
    }

    // Displays an ad with the given options. The user option is only supported for incentivized ads.
    public static void playAdWithOptions( Dictionary<string,object> options )
	{
#if (!VUNGLE_AD_OFF)
#if UNITY_EDITOR
		return;
#endif
		if(options == null)
		{
			throw new ArgumentException("You can not call this method with null parameter");
		}
#if UNITY_IPHONE
		VungleBinding.playAdEx( options );
#elif UNITY_ANDROID
		VungleAndroid.playAdEx( options );
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
        VungleWin.playAdEx( options );
#endif
#endif
	}
	
	// Clear cache
	public static void clearCache()
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return;
		#endif
		#if UNITY_IPHONE
		VungleBinding.clearCache();
		#elif UNITY_ANDROID
		//VungleAndroid.clearCache();
		#else
		return;
		#endif
#endif
	}

	// Clear sleep
	public static void clearSleep()
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return;
		#endif
		#if UNITY_IPHONE
		VungleBinding.clearSleep();
		#elif UNITY_ANDROID
		#else
		#endif
#endif
	}
	
	public static void setEndPoint(string endPoint)
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return;
		#endif
		#if UNITY_IPHONE
		VungleBinding.setEndPoint(endPoint);
		#elif UNITY_ANDROID
		#else
		return;
		#endif
#endif
	}

	public static void setLogEnable(bool enable)
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return;
		#endif
		#if UNITY_IPHONE
		VungleBinding.enableLogging(enable);
		#elif UNITY_ANDROID
		#else
		return;
		#endif
#endif
	}
	
	public static string getEndPoint()
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return "";
		#endif
		#if UNITY_IPHONE
		return VungleBinding.getEndPoint();
		#elif UNITY_ANDROID
		return "";
		#else
		return "";
		#endif
#else
		return "";
#endif
	}
	
	public static void onResume()
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return;
		#endif
		#if UNITY_ANDROID
		VungleAndroid.onResume();
		#endif
#endif
	}

	public static void onPause()
	{
#if (!VUNGLE_AD_OFF)
		#if UNITY_EDITOR
		return;
		#endif
		#if UNITY_ANDROID
		VungleAndroid.onPause();
		#endif
#endif
	}
}
