using System.Collections.Generic;
using UnityEngine;

//iw means chengdu invincible warrior company
public class IWiOSPackageConfig : BasePackageConfig
{
    #region useInEditor
    public override int FacebookAppIdIndex
    {
        get { return 1; }
    }
	public override string FacebookAppId
	{
		get { return "fb169308743640502"; }
	}
    public override string ProductName
    {
        get { return "Lucky Classic Casino"; }
    }
    public override string iOSBundleIdentifier
    {
        get { return "com.luckyvegascasino.trojan.ios"; }
    }
    public override string iOSProvisionDev
    {
        get { return "20170907_trojan_iw_dev"; }
    }
    public override string iOSProvisionAppstore
    {
        get { return "201709014_trojan_iw_appstore"; }
    }
    public override string iOSProvisionAdhoc
    {
        get { return "20170907_trojan_iw_adhoc"; }
    }
    public override string DistributionCertification
    {
        get { return "iPhone Distribution: Fang Lu (867XKS3SUH)"; }
    }
    public override string CertificationId
    {
        get { return "867XKS3SUH"; }
    }
    public override string AppLovinSdkKey
    {
        get { return "3VQ4TIs0L-iEa-cZIosrUoRFAouBezHiTinx_dyLbeaBcBlw-blTOBkZ-blcT0J1i5zikv2JTdUAqE4prqGD0-"; }
    }
    public override string KeychainPlistDebugPath
    {
        get { return "/../Assets/Plugins/iOS/projmods/iOS/IW/Debug/KeychainAccessGroups.plist"; }
    }
    public override string KeychainPlistReleasePath
    {
        get { return "/../Assets/Plugins/iOS/projmods/iOS/IW/Release/KeychainAccessGroups.plist"; }
    }
    public override string SplashIconsPath
    {
        get { return "/../Assets/Images/Splash_IW/"; }
    }
    public override string AppIconsPath
    {
        get { return "/../Assets/Images/Icon_IW/"; }
    }
    public override string LoadingTexPath
    {
        get { return "/Images/Loading/"; }
    }
    public override string SvUdidFilePath
    {
        get { return "/../IW_Assets/"; }
    }

    #endregion

	public override string AdmobHomeInInterstitialIOSId { get { return "ca-app-pub-1221767908077728/4779380780";}}
	public override string AdmobHomeInInterstitialAndroidId { get { return "ca-app-pub-1221767908077728/4139595844"; }}

    public override string AdmobRewardId
    {
        get { return "ca-app-pub-6399179990784550/6365996131"; }
    }
    public override string AdmobInterstitialId
    {
        get { return "ca-app-pub-6399179990784550/5384086950"; }
    }
    public override string VungleiOSAppId
    {
        get { return "599d4f82370d4a3669008a56"; }
    }
    public override string FacebookUrl
    {
        get { return "https://www.facebook.com/luckyclassiccasino"; }
    }
    public override string AppsflyeriOSAppId
    {
        get { return "1278845364"; }
    }
    public override string PrivacyPolicyUrl
    {
        get { return "http://joymania.co/pp.html"; }
    }
    public override string AppScoreUrl
    {
        get { return "itms-apps://itunes.apple.com/us/app/jelly-blast-mania-tap-match-2!/id1278845364"; }
    }
    public override ChannelType ChannelType
    {
        get { return ChannelType.iOS_IW; }
    }
    public override string AdjustAppToken { get { return "raqapanqxqf4"; } }
    public override string AdjustFirstSpinToken { get { return "x3vh7s"; } }
    public override string AdjustPurchaseToken { get { return "8s633c"; } }
    public override string AdjustSecondDayLeftToken { get { return "o7mtsi"; }}

    public override void InitFireBase()
    {
        LogUtility.Log("PackageConfig : IW package config works for init firebase");
    }
		
    public override void FireBaseTrackEvent(Dictionary<string, object> dict)
    {
        //LogUtility.Log("PackageConfig : IW package config works for track firebase events");
    }
}
