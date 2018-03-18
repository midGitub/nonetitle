using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServerConfig
{
#if DEBUG
	public static readonly string GameServerUrl = "http://inner.linux.citrusjoy.cn:49010/";
#else
	public static readonly string GameServerUrl = "https://slots-game.citrusjoy.com/";
#endif
	//public static readonly string GameServerUrl = "http://192.168.3.231:8063/";

	public static readonly string PayServerUrl = "https://slots-pay.citrusjoy.com/";
	//public static readonly string PayServerUrl = "http://citrusjoy.vicp.cc:8061/";

	public static readonly string FeedbackServerUrl = "https://kepler-feedback.citrusjoy.com:443/add_feedback";
	//public static string StoreServerUrl = "http://slots-pay.citrusjoy.com:8061/";

	public static readonly string GoogleStoreUrl = "https://play.google.com/store/apps/details?id=com.citrusjoy.trojan";

	public static readonly string GoogleStoreScorePageUrl = "market://details?id=com.citrusjoy.trojan";
	public static readonly string AppleStoreScorePageUrl = "itms-apps://itunes.apple.com/us/app/jelly-blast-mania-tap-match-2!/id1247414258";

	public static readonly string FacebookUrl = "https://www.facebook.com/hugewinslots/";
	public static readonly string FacebookUrl_Ios = "https://www.facebook.com/414745635550876";
#if false
	public static readonly string APPLINK = "https://fb.me/269323516808999";
#else
    public static readonly string APPLINK = "https://slots-game.citrusjoy.com/facebook?";
#endif
	public static readonly string SharePhotoURL = "http://us.cj.down.s3-website-us-east-1.amazonaws.com/slots-release/image/share001.jpg";
	public static readonly string SharePhotoBaseURL = "http://us.cj.down.s3-website-us-east-1.amazonaws.com/slots-release/image/";

	public static readonly string MachineAssetUrlDebug = "https://s3.amazonaws.com/us.cj.down/slots-debug/machineassets/";
	public static readonly string MachineAssetUrlRelease = "https://s3.amazonaws.com/us.cj.down/slots-release/machineassets/";
}
