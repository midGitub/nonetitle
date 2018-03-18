//TODO abandoned at 12/5/2017, maybe someday we need AF sdk again so i did't delete this script from our project
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using CitrusFramework;
//
//public class AppsFlyerManager : Singleton<AppsFlyerManager> {
//
//	private bool tokenSent;
//
//	public void Init()
//	{
//	}
//
//	// Use this for initialization
//	void Start () 
//	{
//        AppsFlyer.setAppsFlyerKey("TJFTQrkPUsqkEKVpqnx3EM");
//        //set currency 
//        AppsFlyer.setCurrencyCode("USD");
//		//set user id (use sensor id)
//		AppsFlyer.setCustomerUserID(DeviceUtility.GetDeviceId());
//		//create callback gameobject and do not destoryOnLoad
//		GameObject callbackGo = CreateCallGameobject();
//		DontDestroyOnLoad(callbackGo);
//
//#if UNITY_IOS
//        AppsFlyer.setAppID(PackageConfigManager.Instance.CurPackageConfig.AppsflyeriOSAppId);
//        AppsFlyer.getConversionData();
//		AppsFlyer.trackAppLaunch();
//		// register to push notifications for iOS uninstall
//		UnityEngine.iOS.NotificationServices.RegisterForNotifications (UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
//
//#elif UNITY_ANDROID
//		AppsFlyer.setAppID ("com.citrusjoy.trojan");
//        // for getting the conversion data
//        AppsFlyer.init(DeviceUtility.GetDeviceId(), "AppsFlyerTrackerCallbacks");
//		//For Android Uninstall
//		//AppsFlyer.setGCMProjectNumber ("YOUR_GCM_PROJECT_NUMBER");
//#endif
//    }
//
//
//    // Update is called once per frame
//    void Update () 
//	{
//		#if UNITY_IOS 
//		if (!tokenSent) 
//		{ 
//			byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;           
//			if (token != null)
//			{     
//				//For iOS uninstall
//				AppsFlyer.registerUninstall (token);
//				tokenSent = true;
//			}
//		}    
//		#endif
//	}
//
//	//create callback gameobject which attach appsflyercallbacks.cs
//	private GameObject CreateCallGameobject()
//	{
//		GameObject callback = new GameObject("AppsFlyerTrackerCallbacks");
//		callback.AddComponent<AppsFlyerTrackerCallbacks>();
//
//		return callback;
//	}
//
//	//purcase event tracking
//	public void Purchase(IAPData iapData)
//	{
//		#if DEBUG
//		#else
//		Dictionary<string, string> eventValue = new Dictionary<string,string> ();
//
//		if (iapData == null) 
//		{
//			Debug.LogError ("AppsflyerManager got purchase data with empty iapData, please check");
//		}
//		else 
//		{
//            string localItemId = iapData.LocalItemId;
//            var iapItem = IAPCatalogConfig.Instance.FindIAPItemByID(localItemId);
//
//		    if (iapItem != null )
//		    {
//				float ourRev = iapItem.Price * 0.7f;
//				eventValue.Add("af_revenue", ourRev.ToString());
//                eventValue.Add("localItemId", localItemId);
//                eventValue.Add("storeSpecificId", iapData.StoreSpecificId);
//                eventValue.Add("transactionId", iapData.TransactionId);
//
//                AppsFlyer.trackRichEvent("af_purchase", eventValue);
//            }
//		}
//		#endif
//	}
//
//	//track first time spin event
//	public void FirstSpin()
//	{
//		//check if first time spin
//		if(UserBasicData.Instance.IsFirstSpin)
//		{
//			UserBasicData.Instance.IsFirstSpin = false;
//			string spinTime = NetworkTimeHelper.Instance.GetNowTime().ToString();
//			AppsFlyer.trackRichEvent("af_trojan_firstspin", new Dictionary<string, string>{{"firstSpinTime", spinTime}});
//		}
//	}
//}
