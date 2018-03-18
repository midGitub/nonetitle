using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using CitrusFramework;

public class FacebookLikes : Singleton<FacebookLikes> {

	private readonly string _facebookPageId = "414745635550876";
	private readonly string _facebookAppPackageName = "com.facebook.katana";

	public void Init(){	}

	public void CheckUserLikes()
	{
		#if UNITY_EDITOR
		#else 
		//TODO Need user login first! if not the callback returns error datas
//		FB.API(
//		query: "/me/likes/" + _facebookPageId,
//		method: HttpMethod.GET,
//		callback: APICallback);

		Debug.LogError("is facebook app is installed : " + CheckPackageAppIsPresent(_facebookAppPackageName));
		//we decided do not need user logining, if user click "like us" button, give user reward directly 
		GiveUserReward();
		#endif
	}

//	private void APICallback (IResult result)
//	{
//		foreach(var key in result.ResultDictionary.Keys)
//		{
//			string value = "";
//			result.ResultDictionary.TryGetValue(key, out value);
//			Debug.LogError(string.Format ("item's key is {0} , it's value is {1}", key, value));
//		}
//
//		Debug.LogError("cancalled : " + result.Cancelled);
//		Debug.LogError("Error : " + result.Error);
//		Debug.LogError("RawResult : " + result.RawResult);
//
//		//become our facebook fan successfully
//		if(!result.Cancelled && result.Error.IsNullOrEmpty())
//		{
//			GiveUserReward();
//		}
//		//failed to be a facebook fan or got facebook query issue, do nothing
//	}

	public void JumpToFacebookPage()
	{
		var url = "";
#if UNITY_EDITOR
        GiveUserReward();
#elif UNITY_ANDROID
		if(CheckPackageAppIsPresent(_facebookAppPackageName))
		{
			//there is Facebook app installed so let's use it
			url = "fb://page/" + _facebookPageId; 
		}
		else
		{
			// no Facebook app - use built-in web browser
			url = ServerConfig.FacebookUrl;
		}
#elif UNITY_IOS
        url = PackageConfigManager.Instance.CurPackageConfig.FacebookUrl;
#else
		url = ServerConfig.FacebookUrl;
#endif

        Application.OpenURL(url);
	}

	private bool CheckPackageAppIsPresent(string package)
	{
		bool result = false;

#if UNITY_ANDROID
		AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

		//take the list of all packages on the device
		AndroidJavaObject appList = packageManager.Call<AndroidJavaObject>("getInstalledPackages",0);
		int num = appList.Call<int>("size");
		for(int i = 0; i < num; i++)
		{
			AndroidJavaObject appInfo = appList.Call<AndroidJavaObject>("get", i);
			string packageNew = appInfo.Get<string>("packageName");
			if (packageNew.CompareTo (package) == 0)
			{
				result = true;
			} 
		}
#endif

		return result;
	}

	private void GiveUserReward()
	{
		bool isOurFan = UserBasicData.Instance.LikeOurAppInFacebook;
		if(!isOurFan)
		{
			UserBasicData.Instance.LikeOurAppInFacebook = true;

			var reward = CoreConfig.Instance.MiscConfig.FacebookLikesReward;
			UserBasicData.Instance.AddCredits((ulong)reward, FreeCreditsSource.FanPageBonus, true);
			CitrusEventManager.instance.Raise(new LikeUsInFacebookEvent());
			GetCredits.Instance.PlayGetCreditEffect();
		}
	}
}
