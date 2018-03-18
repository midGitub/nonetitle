using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System.Text;
using System;
using MiniJSON;
#if Trojan_FB
using Facebook.Unity;
#endif

public class FriendDefine{
	public static readonly int GiftNumTHreshold = 20;
	public static readonly int CollectGiftLuckyFactor = 10;
}

public class FriendManager : Singleton<FriendManager> {
	private Dictionary<string, FriendData> _sendFriends = new Dictionary<string, FriendData>();
	private Dictionary<string, FriendData> _collectFriends = new Dictionary<string, FriendData> ();
	private Dictionary<string, FriendData> _inviteFriends = new Dictionary<string, FriendData> ();

	private List<FriendData> _sendList = new List<FriendData> ();
	private List<FriendData> _collectList = new List<FriendData> ();
	private List<FriendData> _inviteList = new List<FriendData> ();

	public Dictionary<string, FriendData> SendFriends{
		get { return _sendFriends; }
	}

	public Dictionary<string, FriendData> CollectFriends{
		get { return _collectFriends; }
	}

	public Dictionary<string, FriendData> InviteFriends{
		get { return _inviteFriends; }
	}

	public List<FriendData> SendList{
		get { return _sendList; }
	}

	public List<FriendData> CollectList{
		get { return _collectList; }
	}

	public List<FriendData> InviteList{
		get { return _inviteList; }
	}

	// 初始化标记位
	private bool _initSendFriends = false;
	private bool _initSendFriendsSuccess = false;
	private bool _initCollectFriends = false;
	private bool _initCollectFriendsSuccess = false;
	private bool _initInviteFriends = false;
	private bool _initInviteFriendsSuccess = false;

	private bool _initAll = false;
	// 服务器数据获取协议
	private static readonly string _getUserData = "user_data/get_by_social_id";
	// 服务器通信重试次数
	private static int _serverErrorRetryCount = 0;
	// 服务器通信最大尝试次数
	private static readonly int _serverErrorRetryMax = 3;
	// 初始化协程
	private Coroutine _initCoroutine = null;

	public Callback SendScrollViewHandler = null;
	public Callback CollectScrollViewHandler = null;
	public Callback InviteScrollViewHandler = null;

	public bool HasAllFriends{
		get { return _initAll; }
	}

	public bool HasSendFriends {
		get { return _initSendFriends && _sendFriends.Count > 0; }
	}

	public bool HasCollectFriends {
		get { return _initCollectFriends && _collectFriends.Count > 0; }
	}

	public bool HasInviteFriends {
		get { return _initInviteFriends && _inviteFriends.Count > 0; }
	}

	public void Init(){
		if (_initCoroutine != null) {
			StopCoroutine (_initCoroutine);
		}
		_serverErrorRetryCount = 0;
		_initCoroutine = StartCoroutine (InitFriends());
	}

	public IEnumerator InitFriends(){

		LogUtility.Log ("InitFriends", Color.red);

		// 和facebook的申请
		yield return StartCoroutine (InitSendFriends());
		yield return StartCoroutine (InitCollectFriends());
		yield return StartCoroutine (InitInviteFriends());

		// 和服务器数据申请
		yield return StartCoroutine(GetInfoFromServer());

		// 头像加载
		yield return StartCoroutine(GetFriendsPicture());

		// 更新本地好友信息给收到的facebook好友列表
		UpdateUserDeviceLocalDataToFriendData();

		// 保存信息到本地
		SaveFriendList();

		_initAll = true;

		_initCoroutine = null;

		InitScrollView ();
	}

	private IEnumerator ReInitFriends(){
		yield return new WaitForSeconds (1.0f);
		yield return StartCoroutine (InitFriends ());
	}

	public void InitScrollView(){
		LogUtility.Log ("InitScrollView", Color.red);

		if (_initSendFriendsSuccess) {
			if (SendScrollViewHandler != null)
				SendScrollViewHandler ();
		}

		if (_initCollectFriendsSuccess) {
			if (CollectScrollViewHandler != null)
				CollectScrollViewHandler ();
		}

		if (_initInviteFriendsSuccess) {
			if (InviteScrollViewHandler != null)
				InviteScrollViewHandler ();
		}

		bool allSuccess = _initSendFriendsSuccess && _initCollectFriendsSuccess && _initInviteFriendsSuccess;
		if (!allSuccess) {
			StartCoroutine (ReInitFriends());
		}
	}

	private void UpdateUserDeviceLocalDataToFriendData(){
		if (_initSendFriendsSuccess) {
			ListUtility.ForEach (_sendList, (FriendData data) => {
				FriendData localData = UserDeviceLocalData.Instance.GetFriendData (data.ID);
				if (localData != null && localData.LastSendTime != DateTime.MinValue){
					TimeSpan span = localData.LastSendTime - data.LastSendTime;
					if (span.Seconds > 0){
						data.LastSendTime = localData.LastSendTime;
					}
				}
			});
		}
	}

	public IEnumerator GetFriendsPicture(){
		if (_initSendFriendsSuccess) {
			foreach (var pair in _sendFriends) {
				SpriteStoreManager.Instance.DownloadSprite (pair.Key, pair.Value.ICON);
			}
		}

		if (_initInviteFriendsSuccess) {
			foreach (var pair in _inviteFriends) {
				SpriteStoreManager.Instance.DownloadSprite (pair.Key, pair.Value.ICON);
			}
		}

		yield return null;
	}

	// 向服务器申请数据
	public IEnumerator GetInfoFromServer(){
		if (_initSendFriendsSuccess) {
			List<string> ids = new List<string> ();
			foreach (var pair in _sendFriends) {
				string id = UserIDWithPrefix.GetFBIDWithPrefix (pair.Key);
				ids.Add (id);
			}

			if (ids.Count != 0) {
				List<string> fields = new List<string> () { 
					ServerFieldName.UserLevel.ToString ()
				};

				Dictionary<string, object> dict = new Dictionary<string, object> ();
				dict.Add ("ProjectName", BuildUtility.GetProjectName ());
				dict.Add ("SocialIdArray", ids);
				dict.Add ("FieldsArray", fields);

				yield return StartCoroutine (NetWorkHelper.UniversalNetCall (this, 10, ServerConfig.GameServerUrl,
					_getUserData, dict, HandleSuccessResult, HandleFailedResult));
			}
		}
	}

	// 服务器数据成功
	private void HandleSuccessResult(JSONObject json){
		if (json.HasField ("DataArray")) {
			List<JSONObject> jsonList = json.GetField ("DataArray").list;

			ListUtility.ForEach (jsonList, (JSONObject jsonObj) => {
				if (jsonObj.HasField("SocialId")){
					string id = UserIDWithPrefix.GetFBIDNoPrefix(jsonObj.GetField("SocialId").str);
					int level = (int)jsonObj.GetField(ServerFieldName.UserLevel.ToString()).n;
					UpdateSendFriendData(id, level);	
				}
			});

			_serverErrorRetryCount = 0;
		}
	}

	// 服务器数据失败
	private void HandleFailedResult(JSONObject json){
		// 解析服务器数据出错
		LogUtility.Log("handle friends data failed", Color.red);

		++_serverErrorRetryCount;
		if (_serverErrorRetryCount > _serverErrorRetryMax) {
			_serverErrorRetryCount = 0;
			// TODO: 提示好友服务器信息错误
			LogUtility.Log("handle friend server info failded", Color.red);
		} else {
			StartCoroutine(GetInfoFromServer());
		}
	}

	// 更新发送列表数据
	public void UpdateSendFriendData(string id, int level){
		if (_sendFriends.ContainsKey (id)) {
			_sendFriends [id].Level = level;
		}
	}

	public IEnumerator InitCollectFriends(){
		LogUtility.Log ("InitCollectFriends", Color.red);
		if (!_initCollectFriendsSuccess) {
			_initCollectFriends = false;
			FacebookUtility.GetAppRequests (HandleCollectFriends);
			while (!_initCollectFriends) {
				yield return null;
			}
		}
	}

	public IEnumerator InitSendFriends(){
		LogUtility.Log ("InitSendFriends", Color.red);
		if (!_initSendFriendsSuccess) {
			_initSendFriends = false;
			FacebookUtility.GetFriendsList (HandleSendFriends);
			while (!_initSendFriends) {
				yield return null;
			}
		}
	}
		
	public IEnumerator InitInviteFriends(){
		LogUtility.Log ("InitInviteFriends", Color.red);
		if (!_initInviteFriendsSuccess) {
			_initInviteFriends = false;
			FacebookUtility.GetInvitableFriendsInfo (HandleInviteFriends);
			while (!_initInviteFriends) {
				yield return null;
			}
		}
	}

	private void HandleSendFriends(IGraphResult result){
		if (FacebookUtility.ResultValid (result)) {
			LogUtility.Log ("HandleSendFriends rawresult is " + result.RawResult, Color.red);

			JSONObject json = new JSONObject (result.RawResult);
			if (json.HasField ("friends")) {
				LogUtility.Log ("HasField friends", Color.red);

				List<JSONObject> jsonlist = json.GetField ("friends").GetField ("data").list;

				_sendFriends.Clear ();
				_sendList.Clear ();

				ListUtility.ForEach (jsonlist, (JSONObject obj) => {
					string name = obj.GetField ("name").str;
					string id = obj.GetField ("id").str;
					string icon_path = FacebookHelper.GetProfilePictureURLWithID (id);
					FriendData friend = new FriendData (name, id, icon_path);
					_sendFriends.Add (id, friend);
					_sendList.Add(friend);
				});
			}
			_initSendFriendsSuccess = true;
		} else {
			LogUtility.Log ("handle send friends failed error = "+result.Error, Color.red);
		}

		_initSendFriends = true;
		LogUtility.Log ("_initSendFriends = "+_initSendFriends+" _success = "+_initSendFriendsSuccess, Color.red); 
	}

	private void HandleCollectFriends(IGraphResult result){
		if (FacebookUtility.ResultValid (result)) {
			LogUtility.Log ("HandleCollectFriends rawresult is " + result.RawResult, Color.red);

			JSONObject json = new JSONObject (result.RawResult);
			if (json.HasField ("apprequests")) {
				LogUtility.Log ("HasField apprequests", Color.red);

				List<JSONObject> jsonlist = json.GetField("apprequests").GetField ("data").list;

				_collectFriends.Clear ();
				_collectList.Clear ();
				ListUtility.ForEach (jsonlist, (JSONObject obj) => {
					string name = obj.GetField("from").GetField("name").str;;
					string id = obj.GetField("from").GetField("id").str;;
					string icon_path = FacebookHelper.GetProfilePictureURLWithID (id);
					string request_user_id = obj.GetField("id").str;
					FriendData friend = new FriendData (name, id, icon_path);
					if (obj.HasField("data")){// 这里需要判断是否有data，只有有data的才是领取礼物的request，如果没data，就是发送邀请的request
						FriendGift gift = FriendGift.Deserialize(obj.GetField("data").str);
						friend.Gift = gift;
						_collectFriends.Add (request_user_id, friend);
						_collectList.Add(friend);	
					}
				});
			}
			_initCollectFriendsSuccess = true;
		} else {
			LogUtility.Log ("handle collect friends failed error = "+result.Error, Color.red); 
		}
		
		_initCollectFriends = true;
		LogUtility.Log ("_initCollectFriends = "+_initCollectFriends+" _success = "+_initCollectFriendsSuccess, Color.red); 
	}

	private void HandleInviteFriends(IGraphResult result){
		if (FacebookUtility.ResultValid (result)) {
			LogUtility.Log ("HandleInviteFriends rawresult is " + result.RawResult, Color.red);

			JSONObject json = new JSONObject (result.RawResult);
			#if UNITY_ANDROID || UNITY_EDITOR
			if (json.HasField ("invitable_friends")) {
				LogUtility.Log ("HasField invitable_friends", Color.red);
				List<JSONObject> jsonlist = json.GetField("invitable_friends").GetField ("data").list;
			#elif UNITY_IOS
			if (json.HasField ("data")) {
				LogUtility.Log ("HasField invitable_friends", Color.red);
				List<JSONObject> jsonlist = json.GetField ("data").list;
			#endif

				_inviteFriends.Clear ();
				_inviteList.Clear ();
				ListUtility.ForEach (jsonlist, (JSONObject obj) => {
					string name = obj.GetField("name").str;
					string token = obj.GetField("id").str;
					string icon_path  = obj.GetField("picture").GetField("data").GetField("url").str;
					FriendData friend = new FriendData(name, token, icon_path);
					_inviteFriends.Add(token, friend);
					_inviteList.Add(friend);
				});
			}
			_initInviteFriendsSuccess = true;
		}else {
			LogUtility.Log ("handle invite friends failed error = "+result.Error, Color.red); 
		}
		
		_initInviteFriends = true;
		LogUtility.Log ("_initInviteFriends = "+_initInviteFriends+" _success = "+_initInviteFriendsSuccess, Color.red); 
	}

	public void GetGift(List<string> ids, FacebookDelegate<IGraphResult> callback){
		if (ids.Count == 0)
			return;
		
		ListUtility.ForEach (ids, (string id) => {
			LogUtility.Log("DeleteAppRequests id = "+id);
			FacebookUtility.DeleteAppRequests(id, callback);
		});
	}

	public void SendGift(List<string> ids, FacebookDelegate<IAppRequestResult> callback){
		if (ids.Count == 0)
			return;
		
		ListUtility.ForEach (ids, (string id) => {
			if (_sendFriends.ContainsKey(id)){
				string giftdata = FriendGift.Serialize(_sendFriends[id].Gift);
				FacebookUtility.SendGift(new List<string>(){id}, giftdata, callback);
			}
		});
	}

	public void SendGift(List<string> ids, FriendGift gift, FacebookDelegate<IAppRequestResult> callback){
		if (ids.Count == 0)
			return;
		
		FacebookUtility.SendGift(ids, FriendGift.Serialize(gift), callback);
	}

	public void InviteFriend(List<string> ids, FacebookDelegate<IAppRequestResult> callback){
		if (ids.Count == 0)
			return;

		FacebookUtility.InviteFriends (ids, callback);
	}

	public string FindCollectRequestId(FriendData data){
		foreach (var pair in _collectFriends) {
			if (data == pair.Value) {
				string[] strs = pair.Key.Split ('_');
				string request_id = strs [0];
				return request_id;
			}
		}

		return "";
	}

	// 获得request user id
	public string FindCollectRequestUserId(FriendData data){
		foreach (var pair in _collectFriends) {
			if (data == pair.Value) {
				return pair.Key;
			}
		}

		return "";
	}

	// 更新本地好友信息
	public void SaveFriendList(){
		UserDeviceLocalData.Instance.SetFriendDataList(_sendFriends);
	}
}
