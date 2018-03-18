/*  
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

namespace CitrusFramework{

	public class FacebookUtility 
	{
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
		/// Log in Facebook with permissions: public_profile, email, user_friends.
		/// If need more permissions, please use LogInWithPermission instead
		/// </summary>
		public static void LogInWithDefaultPermission(FacebookDelegate<ILoginResult> result)
		{
			List<string> defaultPermissions = new List<string> () { "public_profile", "email", "user_friends" };
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
			string query = "/me/?fields=name,id,picture";
			GetFacebookInfo(query, result);
		}

		#endregion

		#region Friend
		/// <summary>
		/// Get all the friends who have registered this game
		/// </summary>
		public static void GetFriendsInfo(FacebookDelegate<IGraphResult> result)
		{
			string query = "/me/friends";
			GetFacebookInfo(query, result);
		}

		/// <summary>
		/// Get all the friends who have NOT registered this game
		/// </summary>
		public static void GetInvitableFriendsInfo(FacebookDelegate<IGraphResult> result)
		{
			string query = "/me/invitable_friends";
			GetFacebookInfo(query, result);
		}

		/// <summary>
		/// Invite friend with the invite token, which is acquired by calling GetInvitableFriendsInfo
		/// </summary>
		public static void InviteFriends(List<string> tokens, string title, string content, FacebookDelegate<IAppRequestResult> result)
		{
			FB.AppRequest(
				content,
				tokens,
				null,
				null,
				0,
				string.Empty,
				title,
				result);
		}
		#endregion

		#region Share

		/// <summary>
		/// Share information to app users
		/// </summary>
		public static void ShareInformation(string title, string content, FacebookDelegate<IAppRequestResult> result)
		{
			FB.AppRequest(
				content,
				null,
				new List<object>() {"app_users"},
				null,
				0,
				string.Empty,
				title,
				result);
		}

		/// <summary>
		/// Share app with app url
		/// </summary>
		public static void ShareApp(string title, string content, string contentUri, string photoUri, FacebookDelegate<IShareResult> result)
		{
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
				
		#endregion
	}

}
*/