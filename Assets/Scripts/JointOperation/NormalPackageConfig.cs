using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPackageConfig :  BasePackageConfig{

    #region useInEditor
    public override int FacebookAppIdIndex {get { return 0; }}
	public override string FacebookAppId
	{
		get { return "236472750094076"; }
	}
    public override string ProductName { get { return "Huge Win Slots"; } }
    public override string iOSBundleIdentifier { get { return "com.cjfafafa.trojan.ios"; } }
    public override string iOSProvisionDev { get { return "20171015_trojan_dev"; } }
    public override string iOSProvisionAppstore { get { return "20171015_trojan_appstore"; } }
    public override string iOSProvisionAdhoc { get { return "20171015_trojan_adhoc"; } }
    public override string DistributionCertification { get { return "iPhone Distribution: Shanghai Joy Mania Network Technology Co., Ltd (RMEJ5W9E7D)"; } }
    public override string CertificationId { get { return "RMEJ5W9E7D"; } }
    public override string AppLovinSdkKey { get { return "IyAtVha0hCZ61LPgF_VHCoievLXTtr0AoB1ItcITptfrC4WR31kFpQvhnQAdGAp2LqLCQemEVQ4hVa8TbspOHi"; } }
    public override string KeychainPlistDebugPath { get { return "/../Assets/Plugins/iOS/projmods/iOS/Debug/KeychainAccessGroups.plist"; } }
    public override string KeychainPlistReleasePath { get { return "/../Assets/Plugins/iOS/projmods/iOS/Release/KeychainAccessGroups.plist"; } }
    public override string SplashIconsPath { get { return "/../Assets/Images/Splash/"; } }
    public override string AppIconsPath { get { return "/../Assets/Images/Icon/"; } }
    public override string LoadingTexPath { get { return "/Images/PayTable/"; } }
    public override string SvUdidFilePath { get { return "/Plugins/iOS/"; } }

    #endregion
    public override string AdmobRewardId { get { return "ca-app-pub-1221767908077728/7650436692"; } }
    public override string AdmobInterstitialId { get { return "ca-app-pub-1221767908077728/1743503895"; } }
	public override string AdmobHomeInInterstitialIOSId { get { return "ca-app-pub-1221767908077728/4779380780";}}
	public override string AdmobHomeInInterstitialAndroidId { get { return "ca-app-pub-1221767908077728/4139595844"; }}
    public override string VungleiOSAppId { get { return "595c836814e742f404001598"; } }
    public override string FacebookUrl { get { return ServerConfig.FacebookUrl_Ios; } }
    public override string AppsflyeriOSAppId { get { return "1247414258"; } }
    public override string PrivacyPolicyUrl { get { return "http://citrusjoy.com/pp.html"; } }
    public override string AppScoreUrl { get { return ServerConfig.AppleStoreScorePageUrl; } }
    public override ChannelType ChannelType { get { return ChannelType.iOS; } }
    public override string AdjustAppToken { get { return "raqapanqxqf4"; }}
    public override string AdjustFirstSpinToken { get { return "x3vh7s"; } }
    public override string AdjustPurchaseToken { get { return "8s633c"; } }
    public override string AdjustSecondDayLeftToken { get { return "o7mtsi"; } }

    public override void InitFireBase()
    {
        FireBaseCloudMessage.Instance.Init();
        FireBaseAnalytics.Instance.Init();
    }

    public override void FireBaseTrackEvent(Dictionary<string, object> dict)
    {
        FireBaseAnalytics.Instance.TrackEvent(dict);
    }
}
