using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using System;
using System.Linq;
//#if Trojan_FB
using Facebook.Unity;
//#endif

public class FacebookHelper
{
	//#if Trojan_FB
	//	const string AppLinkUrl = "https://fb.me/810530068992919";

	public static bool IsSimpleInitAndLogin = false;
	public static bool IsInitialized { get { return FB.IsInitialized; } }
	public static bool IsLoggedIn { get { return FacebookUtility.IsLoggedIn(); } }

	private static string _socialName = "";
	private static string _socialId = "";
	private static bool _hadLoginEnd = false;
	private static string _userEmail = "";

	#region facebook login callback 
	private static FacebookDelegate<ILoginResult> _handleFBCallback = null;

	public static void AddLoginFBCallback(FacebookDelegate<ILoginResult>  callback){
		_handleFBCallback += callback;
	}

	public static void RemoveLoginFBCallback(FacebookDelegate<ILoginResult>  callback){
		_handleFBCallback -= callback;
	}
	#endregion

	public static string SocialId { get { return _socialId; } }
	public static string UserEmail { get{ return _userEmail;} }

    public static string UserName { get { return _socialName; } }

    public static bool HavePublishActions
	{
		get
		{
			Debug.Log("TOKEN HAVE" + AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions"));
			return AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions");
		}
	}

	public static bool HaveUserFriends
	{
		get
		{
			return AccessToken.CurrentAccessToken != null
				&& AccessToken.CurrentAccessToken.Permissions.Contains("user_friends");
		}
	}

    #region Init

	public static void InitSDK()
	{
		// zhousen
		_handleFBCallback += HandleFBLoginResult;

        if (!FB.IsInitialized)
		{
			FacebookUtility.InitSDK(() => 
			{
				GameDebug.Log("", "FB", "Facebook inited");
				FB.ActivateApp();

				//Huangfu said the reason of logout:
				//因为在手机上登录授权后，FB好像是保留你登录状态。 关闭游戏重进后会登不上去。。
				//显示回调就是登录失败，必须手动登出
				//也可以刷新token，不过那种麻烦
				//GameDebug.Log("", "FB", "Force Logout");

				FetchDeepLink();

				if(FB.IsLoggedIn)
				{
					RefreshCurrentAccessToken();
					Debug.Log("刷新token");
					foreach(var item in AccessToken.CurrentAccessToken.Permissions)
					{
						Debug.Log(item);
					}
				}
				//FacebookUtility.LogOut();
			});
		}
		else
		{
			FB.ActivateApp();
		}
	}

	static void HandleFBLoginResult(ILoginResult result)
	{
		if(result != null && FacebookUtility.ResultValid(result) && result.AccessToken != null)
		{
			GameDebug.Log("", "FB", "Login Success Response:\n" + result.RawResult);
			LogUtility.Log("获取到FB读权限");
			if(FB.IsInitialized)
			{
				if(!IsSimpleInitAndLogin)
				{
					//PlatFormManager.Instance.SocialSDKLoginCallBack(SocialType.FACEBOOK, result.AccessToken.UserId);
					GetSelfInformation();
				}

				if(UserBasicData.Instance.IsFirstLoadingFaceBook)
				{
					UserBasicData.Instance.IsFirstLoadingFaceBook = false;
					UserBasicData.Instance.AddCredits((ulong)CoreConfig.Instance.LuckyConfig.FaceBookLoginAddCoins, FreeCreditsSource.ConnectFacebookBonus, false);
					UserBasicData.Instance.AddLongLucky(CoreConfig.Instance.LuckyConfig.FaceBookLoginLongLuck, true);
					Debug.Log("FackeBook 20000金币加入成功");
					AnalysisManager.Instance.GetFacebookBonus(result.AccessToken.UserId, CoreConfig.Instance.LuckyConfig.FaceBookLoginAddCoins, CoreConfig.Instance.LuckyConfig.FaceBookLoginLongLuck);
					UserDeviceLocalData.Instance.FirstBindFacebookTime = NetworkTimeHelper.Instance.GetNowTime();
				}

				FB.LogAppEvent("LoginFaceBook");

				AnalysisManager.Instance.SendLoginFacebookSuccessCallback();
			}
			else
			{
				GameDebug.Log("", "FB", "Failed to Initialize the Facebook SDK");

				Debug.Assert(false);
			}

			CitrusEventManager.instance.Raise(new UserLoginFBSuccessEvent());

			RefreshCurrentAccessToken();
		}
		else
		{
			if(result == null)
				GameDebug.Log("", "FB", "Login Fail Response result is null\n");
			else
				GameDebug.Log("", "FB", "Login Fail Response:\n" + result.RawResult);

			CitrusEventManager.instance.Raise(new UserLoginFBFailEvent());

			AnalysisManager.Instance.SendLoginFacebookFailCallback();
		}
	}

    //static void HandleFBLoginPublishResult(ILoginResult result)
    //{
    //	if(FacebookUtility.ResultValid(result))
    //	{
    //		GameDebug.Log("", "FB", "Login publish Success Response:\n" + result.RawResult);
    //	}
    //	else
    //	{
    //		GameDebug.Log("", "FB", "Login publish Fail Response:\n" + result.RawResult);
    //	}
    //	// IsInLogging = false;
    //}

    public static void LoginWithFB()
	{
		// 首先默认权限登录 登录成功后 立马申请publish权限登录
		GameDebug.Log("", "FB", "Login");
		// IsInLogging = true;
		//FacebookUtility.LogInWithDefaultPermission(HandleFBLoginResult);
		//FacebookUtility.LogInWithPublishPermissions(HandleFBLoginResult);
		FacebookUtility.LogInWithDefaultPermission(HandleFBLoginResult);
		AnalysisManager.Instance.SendStartLoginFacebook();
	}

	public static void LoginWithFB(FacebookDelegate<ILoginResult> result)
	{
		GameDebug.Log("", "FB", "Login with func");
		_handleFBCallback += result;
		FacebookUtility.LogInWithDefaultPermission(_handleFBCallback);
		AnalysisManager.Instance.SendStartLoginFacebook();
	}

	static void HandleFBLoginWriteResult(ILoginResult result)
	{
		if(FacebookUtility.ResultValid(result) && result.AccessToken != null)
		{
			GameDebug.Log("", "FB", "Login Success Response:\n" + result.RawResult);
			LogUtility.Log("获取到FB写权限");

			// 检查是否有权限
			if(AccessToken.CurrentAccessToken.Permissions.Contains("publish_actions"))
			{
				Debug.Log("have publish actions");
			}
			else
			{
				Debug.Log("no publish actions");
			}

			// 刷新访问令牌
			RefreshCurrentAccessToken();
		}
		else
		{
			GameDebug.Log("", "FB", "Login Fail Response:\n" + result.RawResult);
		}
	}

	public static void LoginPublishWithFB(FacebookDelegate<ILoginResult> result)
	{
		result += HandleFBLoginWriteResult;
		GameDebug.Log("", "FB", "Login publish");
		// IsInLogging = true;
		FacebookUtility.LogInWithPublishPermissions(result);
	}

	public static void LogoutFB()
	{
		GameDebug.Log("Logout FB");
		CitrusEventManager.instance.Raise(new UserLogoutEvent());
		FacebookUtility.LogOut();
	}

	public static void RefreshCurrentAccessToken()
	{
		FacebookUtility.RefreshCurrentAccessToken(null);
	}

	#endregion

	#region Log

	public static void LogPurchase(string title, string price)
	{
		//don't log in DEBUG version
#if RELEASE
		var iapParameters = new Dictionary<string, object>();
		iapParameters["ItemTitle"] = title;

		FB.LogPurchase(Convert.ToSingle(price), "USD", iapParameters);
#endif
	}

	#endregion

	static void HandleFBGetSelfInformation(IGraphResult result)
	{
		if(FacebookUtility.ResultValid(result))
		{
			GameDebug.Log("", "FB", "Self facebook information is:\n" + result.RawResult);

			Dictionary<string, object> picture = result.ResultDictionary["picture"] as Dictionary<string, object>;
			Dictionary<string, object> picData = picture["data"] as Dictionary<string, object>;

			_socialId = result.ResultDictionary["id"] as string;
			GameDebug.Log("FB id:" + _socialId);

			if(result.ResultDictionary.ContainsKey("name"))
				_socialName = result.ResultDictionary["name"] as string;
			
			if (result.ResultDictionary.ContainsKey ("email")) 
			{
				_userEmail = result.ResultDictionary ["email"] as string;
				UserBasicData.Instance.FacebookBindEmail = _userEmail;
			}

			UserDataHelper.Instance.UserSocialLogin(UserSocialState.Facebook);
		}
		else
		{
			//Can't get any information, then logout.
			LogoutFB();
		}
	}

	public static string GetProfilePictureURLWithID(string userID)
	{
		if(userID == "")
		{
			return "";
		}
		return "http://graph.facebook.com/" + userID + "/picture?type=normal";
	}

	static void HandleFBGetSelfPortrait(IGraphResult result)
	{
		if(FacebookUtility.ResultValid(result))
		{
			GameDebug.Log("", "FB", "Self facebook protrait is:\n" + result.RawResult);
			Dictionary<string, object> picture = result.ResultDictionary["picture"] as Dictionary<string, object>;
			Dictionary<string, object> picData = picture["data"] as Dictionary<string, object>;
			//			PlatFormManager.Instance.StartGetPortrait(picData["url"] as string, UserDataManager.Instance.UserInfoData.GetSocialUserID());
		}
	}

	public static void GetSelfInformation()
	{
		FacebookUtility.GetBasicSelfInfo(HandleFBGetSelfInformation);
		//		FacebookUtility.GetBasicSelfPortrait (HandleFBGetSelfPortrait);
	}

	public static void EasyShareInformation(Uri contentURL, string contentDescription, string contentTitle = "", Uri photoURL = null, FacebookDelegate<IShareResult> callback = null)
	{
		FB.ShareLink(
			contentURL,
			contentTitle,
			contentDescription,
			photoURL,
			callback
		  );
	}

	public static void PublishEasyShare(string applink, string contentDescription, string contentTitle = "", string photoURL = null, FacebookDelegate<IGraphResult> callback = null)
	{
		Dictionary<string, string> data = new Dictionary<string, string>();
		string link = "";
		link = applink;

	#if false // old method
		{
			data.Add("description", contentDescription);
			data.Add("name", contentTitle);
			data.Add("picture", photoURL);
		}
	#else
		link = applink + "title=" + contentTitle + "&desc=" + contentDescription + "&image=" + photoURL;
	#endif
		GameDebug.Log ("easy share link = "+link);
		data.Add("link", link);

		FB.API("/me/feed", HttpMethod.POST, callback, data);
	}

	private static void DeepLinkCallback(IAppLinkResult result)
	{
		Debug.Log("DeepLinkCallback call ");
		if(result != null && !string.IsNullOrEmpty(result.RawResult))
		{
			Debug.Log("RawResult:" + result.RawResult + ", Url:" + result.Url);
			if(!string.IsNullOrEmpty(result.Url))
				AnalysisManager.Instance.DeepLink(result.Url);
		}
	}

	public static void FetchDeepLink()
	{
		FB.Mobile.FetchDeferredAppLinkData(DeepLinkCallback);
	}

	//#endif
}
