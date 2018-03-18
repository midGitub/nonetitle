using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if Trojan_FB
using Facebook.Unity;
#endif

namespace CitrusFramework{

	public class FacebookUtility 
	{
		#if Trojan_FB
		#region Account

		/// <summary>
		/// Check if Facebook is logged in
		/// </summary>
		public static bool IsLoggedIn()
		{
			return FB.IsLoggedIn;
		}

		/// <summary>
		/// Log in Facebook with permissions: public_profile, email, user_friends.
		/// If need more permissions, please use LogInWithPermission instead
		/// </summary>
		public static void InitSDK(InitDelegate OnInitDelegate)
		{
			FB.Init (OnInitDelegate);
		}

		/// <summary>
		/// Log in Facebook with publish_actions permissions.
		/// It is generally good behavior to split asking for read and publish
		/// permissions rather than ask for them all at once.
		/// </summary>
		public static void LogInWithPublishPermissions(FacebookDelegate<ILoginResult> result)
		{
			// In your own game, consider postponing this call until the moment
			// you actually need it.
			FB.LogInWithPublishPermissions(new List<string>() { "publish_actions" }, result);
		}


		/// <summary>
		/// Log in Facebook with permissions: public_profile, email, user_friends.
		/// If need more permissions, please use LogInWithPermission instead
		/// </summary>
		public static void LogInWithDefaultPermission(FacebookDelegate<ILoginResult> result)
		{
			List<string> defaultPermissions = new List<string> () { "public_profile", "email", "user_friends" }; //, 
			LogInWithPermission (defaultPermissions, result);

		}

		/// <summary>
		/// Log in Facebook with specified permissions
		/// </summary>
		public static void LogInWithPermission(List<string> permissions, FacebookDelegate<ILoginResult> result)
		{
			FB.LogInWithReadPermissions (permissions, result);
		}

		/// <summary>
		/// Log out Facebook
		/// </summary>
		public static void LogOut()
		{
			FB.LogOut ();
		}

		public static void RefreshCurrentAccessToken(FacebookDelegate<IAccessTokenRefreshResult> result)
		{
			FB.Mobile.RefreshCurrentAccessToken(result);
		}

		static public bool FaccbookIdExsits()
		{
			#if UNITY_EDITOR
			return true;
			#else
			return AccessToken.CurrentAccessToken != null;
			#endif
		}

		static public string FacebookId()
		{
			#if UNITY_EDITOR
			return DeviceUtility.GetDeviceId();
			#else
			return AccessToken.CurrentAccessToken.UserId;
			#endif
		}

		#endregion

		#region Facebook_Information

		/// <summary>
		/// Get facebook information from query string. It makes a call to facebook graph api
		/// https://developers.facebook.com/docs/graph-api
		/// </summary>
		public static void GetFacebookInfo(string query, FacebookDelegate<IGraphResult> result)
		{
			FB.API(query, HttpMethod.GET, result);
		}

		/// <summary>
		/// Get basic information from facebook id. The information includes name, id and picture.
		/// If need more information, please use GetFacebookInfo
		/// </summary>
		public static void GetBasicFacebookInfo(string facebookID, FacebookDelegate<IGraphResult> result)
		{
			string query = "/" + facebookID + "/?fields=name,id,picture";
			GetFacebookInfo(query, result);
		}

		/// <summary>
		/// Get basic self information. The information includes name, id and picture.
		/// If need more information, please use GetFacebookInfo
		/// </summary>
		public static void GetBasicSelfInfo(FacebookDelegate<IGraphResult> result)
		{
//			string query = "/me/?fields=name,id";//,picture";
			string query = "/me/?fields=name,id,picture.type(normal),email";
			GetFacebookInfo(query, result);
		}

		public static void GetBasicSelfPortrait(FacebookDelegate<IGraphResult> result)
		{
			string query = "/me/?fields=picture.width(200).height(200)";
			GetFacebookInfo(query, result);
		}

		#endregion

		#region Friend
		/// <summary>
		/// A person's custom friend lists who have registered this game.
		/// </summary>
		public static void GetFriendsList(FacebookDelegate<IGraphResult> result)
		{
			string query = "/me?fields=friends";
			GetFacebookInfo(query, result);
		}

		/// <summary>
		/// Invite all friends with preview image.
		/// </summary>
		public static void InviteFriends(string AppLinkUrl, string PreviewImageUrl, FacebookDelegate<IAppInviteResult> result)
		{
			FB.Mobile.AppInvite(new System.Uri(AppLinkUrl), new System.Uri(PreviewImageUrl), result);
		}

		/// <summary>
		/// Get all the friends who have NOT registered this game.
		/// If your app is a game but does not have a presence on Canvas, 
		/// you can still render the Requests Dialog with no friends pre-selected. 
		/// Access to the Invitable Friends API is not required in order to let people invite their friends to use your game.
		/// </summary>
		public static void GetInvitableFriendsInfo(FacebookDelegate<IGraphResult> result)
		{
		#if UNITY_ANDROID || UNITY_EDITOR
			string query = "me?fields=invitable_friends{name,id,picture}";
		#elif UNITY_IOS
			string query = "me/invitable_friends?fields=name,id,picture";
		#endif
			GetFacebookInfo(query, result);
		}

		public static void GetAppRequests(FacebookDelegate<IGraphResult> result){
			string query = "me?fields=apprequests";
			GetFacebookInfo (query, result);
		}

		public static void DeleteAppRequests(string request_user_id, FacebookDelegate<IGraphResult> result){
			string query = "/"+request_user_id;
			FB.API (query, HttpMethod.DELETE, result);
		}

		public static void SendGift(List<string> ids, string data, FacebookDelegate<IAppRequestResult> callback){
			FB.AppRequest (LocalizationConfig.Instance.GetValue ("facebookSendGift"), ids, null, null, 0, data, "Huge Win Slots", callback);
		}

		public static void InviteFriends(List<string> ids, FacebookDelegate<IAppRequestResult> callback){
			FB.AppRequest (LocalizationConfig.Instance.GetValue ("facebookInviteFriend"), ids, null, null, 0, "", "Huge Win Slots", callback);
		}

		#endregion

		#region Share

		/// <summary>
		/// Share information to FB all users
		/// </summary>
		public static void ShareInformation(string title, string content, FacebookDelegate<IAppRequestResult> result)
		{
			FB.AppRequest(
				content,
				null,
				null,
				null,
				0,
				string.Empty,
				title,
				result);
		}

		/// <summary>
		/// Share information to FB app users who played the game or not
		/// </summary>
		public static void ShareInformationWithFilters(string title, string content, FacebookDelegate<IAppRequestResult> result, bool isAppUser = true)
		{
			FB.AppRequest(
				content,
				null,
				new List<object>() {isAppUser ? "app_users" : "app_non_users"},
				null,
				0,
				string.Empty,
				title,
				result);
		}

		/// <summary>
		/// Invite friend with the invite ids, which is acquired by calling GetFriendsList. Or call ShareInformation
		/// </summary>
		public static void ShareInformationWithIds(List<string> ids, string title, string content, FacebookDelegate<IAppRequestResult> result)
		{
			FB.AppRequest(
				content,
				ids,
				null,
				null,
				0,
				string.Empty,
				title,
				result);
		}

		public static void UploadScreenShotAndShare(byte[] screenshot, FacebookDelegate<IGraphResult> result)
		{	
			WWWForm wwwForm = new WWWForm();
			wwwForm.AddBinaryData("image", screenshot, "screenshot.png");
//			wwwForm.AddField("privacy", "EVERYONE");
			wwwForm.AddField("no_story", "false");
			FB.API("me/photos", HttpMethod.POST, result, wwwForm);
		}

		public static void ShareFeed(string title, string content, string contentUri, string photoUri, FacebookDelegate<IShareResult> result)
		{
			FB.FeedShare(
				null, 
				new System.Uri(contentUri), 
				title, 
				null, 
				content, 
				new System.Uri(photoUri), 
				null, 
				result);
		}

		/// <summary>
		/// Share app with app url
		/// </summary>
		public static void ShareApp(string title, string content, string contentUri, string photoUri, FacebookDelegate<IShareResult> result)
		{
//			GameDebug.Log("", "FB", "  photoUri: " + photoUri + "  contentUri: " + contentUri);			
			FB.ShareLink(
				new System.Uri(contentUri),
				title,
				content,
				new System.Uri(photoUri),
				result);
		}

		#endregion

		#region Result

		/// <summary>
		/// Check if the result is valid and no error
		/// </summary>
		public static bool ResultValid(IResult result)
		{
			return string.IsNullOrEmpty (result.Error) && !string.IsNullOrEmpty (result.RawResult);
		}

		public static bool ResultValid(IAppRequestResult result){
			return string.IsNullOrEmpty (result.Error) && !string.IsNullOrEmpty (result.RawResult);
		}
				
		#endregion

		#region Events

		public static void SetUserProfile(string uniqueId, Dictionary<string, object> dict, string advertiserId, FacebookDelegate<IGraphResult> result)
		{
			if (!FB.IsInitialized)
				return;

			string query = "/" + Facebook.Unity.Settings.FacebookSettings.AppId + "/user_properties";

			Dictionary<string, string> postDict = new Dictionary<string, string>();
			List<object> users = new List<object>();

			//single user info
			Dictionary<string, object> singleUser = new Dictionary<string, object>();
			singleUser["user_unique_id"] = uniqueId;
			singleUser["custom_data"] = dict;
			if(!string.IsNullOrEmpty(advertiserId))
				singleUser["advertiser_id"] = advertiserId;

			users.Add(singleUser);
			postDict["data"] = Facebook.MiniJSON.Json.Serialize(users);

			FB.API(query, HttpMethod.POST, result, postDict);

			Debug.Log("FB SetUserProfile called");
		}

		#endregion

		#endif
	}

}
