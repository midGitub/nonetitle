//#define SANDBOX_TEST

using System;
using System.Collections.Generic;
using com.adjust.sdk;
using CitrusFramework;
using UnityEngine;

public class AdjustManager : Singleton<AdjustManager>
{  
    private static readonly string _androidAppToken = "ye3oqy6p8y68";
    public string AdjustId {get{ return Adjust.getAdid() ?? ""; }}
    public string UserSource {get{return Adjust.getAttribution() == null ? "" : Adjust.getAttribution().trackerName;}}
    private string _purchaseToken = "";
    private string _firstSpinToken = "";
    private string _secondDayLeftToken = "";
    private readonly string _purchaseTokenAndroid = "sy8ugl";
    private readonly string _firstSpinTokenAndroid = "oeavzu";
    private string _secondDayLeftTokenAndroid = "rixdn9";
    private readonly string _prefabPath = "Map/Adjust/Adjust";
    private readonly string _currencySymbol = "USD";
    private Adjust _adjustScript;

    public void Init()
    {
        LoadPrefab();
        SetAdjustArgs();
    }

    void LoadPrefab()
    {
        GameObject go = AssetManager.Instance.LoadAsset<GameObject>(_prefabPath);
        Debug.Assert(go != null, "AdjustManager : Can't find adjust prefab at path : " + _prefabPath);
        if (go != null)
        {
            GameObject obj = UGUIUtility.CreateObj(go, null);
            DontDestroyOnLoad(obj);
            _adjustScript = obj.GetComponent<Adjust>();
            Debug.Assert(_adjustScript != null, "AdjustManager : Can't find adjust script on adjust gameobject! : ");
        }
    }

    void SetAdjustArgs()
    {
#if UNITY_ANDROID
        _adjustScript.appToken = _androidAppToken;
        _firstSpinToken = _firstSpinTokenAndroid;
        _purchaseToken = _purchaseTokenAndroid;
        _secondDayLeftToken = _secondDayLeftTokenAndroid;

#elif UNITY_IPHONE || UNITY_IOS
        _adjustScript.appToken = PackageConfigManager.Instance.CurPackageConfig.AdjustAppToken;
        _firstSpinToken = PackageConfigManager.Instance.CurPackageConfig.AdjustFirstSpinToken;
        _purchaseToken = PackageConfigManager.Instance.CurPackageConfig.AdjustPurchaseToken;
        _secondDayLeftToken = PackageConfigManager.Instance.CurPackageConfig.AdjustSecondDayLeftToken;
#else
        _adjustScript.appToken = "";
#endif

#if DEBUG
        _adjustScript.environment = AdjustEnvironment.Sandbox;
        _adjustScript.logLevel = AdjustLogLevel.Verbose;

#elif RELEASE
        _adjustScript.environment = AdjustEnvironment.Production;
         _adjustScript.logLevel = AdjustLogLevel.Suppress;
#endif

        _adjustScript.startManually = true;
        Adjust.setOfflineMode(!DeviceUtility.IsConnectInternet());
        Adjust.addSessionCallbackParameter("deviceId", DeviceUtility.GetDeviceId());
        AdjustConfig config = new AdjustConfig(_adjustScript.appToken, _adjustScript.environment);
        config.setSendInBackground(true);
        config.setLogLevel(_adjustScript.logLevel);
        config.setAttributionChangedDelegate( x => Debug.Log("adjust Attribution Changed!"));
        Adjust.start(config);
    }

#region InAppEvent
    /* Notice : the event can also be send to url we assigned, need config callback url in dashboard, maybe we will need this function someday
       use envent.addCallbackParameter to add parameters, then a GET request will be send to callback url
       event addPartnerParameter is used to send event data to special partners */
    public void Purchase(IAPData iapData)
    {
#if RELEASE || SANDBOX_TEST
        if (iapData == null)
        {
            Debug.LogError("AppsflyerManager got purchase data with empty iapData, please check");
        }
        else
        {
            string localItemId = iapData.LocalItemId;
            var iapItem = IAPCatalogConfig.Instance.FindIAPItemByID(localItemId);

            if (iapItem != null)
            {
                float ourRev = iapItem.Price * 0.7f;

                AdjustEvent e = new AdjustEvent(_purchaseToken);
                e.setRevenue(ourRev, _currencySymbol);
                e.setTransactionId(iapData.TransactionId);
                e.addCallbackParameter("revenue", ourRev.ToString());
                e.addCallbackParameter("localItemId", localItemId);
                e.addCallbackParameter("storeSpecificId", iapData.StoreSpecificId);
                e.addCallbackParameter("transactionId", iapData.TransactionId);

                Adjust.trackEvent(e);
            }
        }
#endif
    }

    public void FirstSpin()
    {
        string spinTime = NetworkTimeHelper.Instance.GetNowTime().ToString();
        AdjustEvent e = new AdjustEvent(_firstSpinToken);
        e.addCallbackParameter("spinTime", spinTime);

        Adjust.trackEvent(e);
    }

    public void SecondDayLeftEvent()
    {
        AdjustEvent e = new AdjustEvent(_secondDayLeftToken);
        e.addCallbackParameter("second_day_left",  UserBasicData.Instance.UDID);

        Adjust.trackEvent(e);
    }

    #endregion

#region misc fuctions

    public int GetAdStrategyId()
    {
        int result = 0;
        AdjustAttribution attribution = Adjust.getAttribution();
        if (attribution != null)
        {
            result = UserSourceConfig.Instance.GetAdStrategyId(attribution.trackerName);
#if DEBUG
            if (NeedTestFunction.AttributionId != NeedTestFunction.DefaultAttributionId)
                result = UserSourceConfig.Instance.GetAdStrategyId(NeedTestFunction.AttributionId);
#endif
        }
        else
            Debug.LogError("Adjust have not get attributtin data yet!");

        return result;
    }

#endregion
}
