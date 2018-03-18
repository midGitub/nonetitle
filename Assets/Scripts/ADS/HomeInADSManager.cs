using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;
using GoogleMobileAds.Api;

public class HomeInADSManager : Singleton <HomeInADSManager> {

	private enum ADState {
		HasntWatched = 0,
		AlreadyWatched = 1,
	};

	private InterstitialAd interstitial;
	private ADState _adState = ADState.HasntWatched;
	private float _delayTime = 20f;

	public void Init()
	{
		RequestInterstitial();
	}

	private void RequestInterstitial()
	{
		string adUnitId;


		#if UNITY_EDITOR
		adUnitId = "unused";
		#elif UNITY_ANDROID
		adUnitId = PackageConfigManager.Instance.CurPackageConfig.AdmobHomeInInterstitialAndroidId;
		#elif UNITY_IPHONE || UNITY_IOS
		adUnitId = PackageConfigManager.Instance.CurPackageConfig.AdmobHomeInInterstitialIOSId;
		#else
		adUnitId = "unexpected_platform";
		#endif


		interstitial = new InterstitialAd(adUnitId);

		interstitial.OnAdFailedToLoad += HandleInterstitialFailedToLoad;

		interstitial.OnAdOpening += HandleInterstitialOpen;

		interstitial.OnAdClosed += HandleInterstitialClosed;

		interstitial.OnAdLeavingApplication += HandleInterstitialLeave;

		interstitial.OnAdLoaded += HandleInterstitialLoad;

			
		AdRequest request = new AdRequest.Builder().Build();

		interstitial.LoadAd(request);
	}

	void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		GameDebug.Log("Admob: Interstitial fails to load" + args.Message);
		UnityTimer.Start(NetworkTimeHelper.Instance, _delayTime, () =>	{
			RequestInterstitial();
		});
	}

	void HandleInterstitialOpen(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Interstitial successfully Open");
	}


		
	void HandleInterstitialClosed(object sender, System.EventArgs args)
	{
		_adState = ADState.AlreadyWatched;
		GameDebug.Log("Admob: Interstitial successfully closed");
		RequestInterstitial();
		AnalysisManager.Instance.WatchAd((int)_adState,ADSPos.HomeIn);
	}

	void HandleInterstitialLeave(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Interstitial successfully left");
	}

	void HandleInterstitialLoad(object sender, System.EventArgs args)
	{
		GameDebug.Log("Admob: Interstitial successfully loaded");
	}
		
	public void ShowPictureADS()
	{
		if (interstitial != null)
		{
			if (interstitial.IsLoaded())
			{
				Debug.Log("Show HomeIn interstitial ADS");
				interstitial.Show();
			}
			else
			{
				Debug.Log("There is no interstitial video available");
				RequestInterstitial();
			}
		}
		else
		{
			RequestInterstitial();
		}
	}
}
