using System.Collections.Generic;
public abstract class BasePackageConfig
{
    #region Only used by editor scripts
    public virtual int FacebookAppIdIndex { get; set; }
	public virtual string FacebookAppId { get; }
    public virtual string ProductName { get; set; }
    public virtual string iOSBundleIdentifier { get; set; }
    public virtual string iOSProvisionDev { get; set; }
    public virtual string iOSProvisionAppstore { get; set; }
    public virtual string iOSProvisionAdhoc { get; set; }
    public virtual string DistributionCertification { get; set; }
    public virtual string CertificationId { get; set; }
    public virtual string AppLovinSdkKey { get; set; }
    public virtual string KeychainPlistDebugPath { get; set; }
    public virtual string KeychainPlistReleasePath { get; set; }
    public virtual string SplashIconsPath { get; set; }
    public virtual string AppIconsPath { get; set; }
    public virtual string LoadingTexPath { get; set; }
    public virtual string SvUdidFilePath { get; set; }
    #endregion

    public virtual string AdmobRewardId { get;}
    public virtual string AdmobInterstitialId { get;}
	public virtual string AdmobHomeInInterstitialIOSId { get; }
	public virtual string AdmobHomeInInterstitialAndroidId { get; }
    public virtual string VungleiOSAppId { get;}
    public virtual string FacebookUrl { get;}
    public virtual string AppsflyeriOSAppId { get;}
    public virtual string PrivacyPolicyUrl { get;}
    public virtual string AppScoreUrl { get;}
    public virtual ChannelType ChannelType { get;}
    public virtual string AdjustAppToken { get;}
    public virtual string AdjustFirstSpinToken { get;}
    public virtual string AdjustPurchaseToken { get;}
    public virtual string AdjustSecondDayLeftToken { get; }

    public abstract void InitFireBase();
    public abstract void FireBaseTrackEvent(Dictionary<string, object> dict);
}
