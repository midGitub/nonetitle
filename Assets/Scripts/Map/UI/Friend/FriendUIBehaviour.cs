using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using System;
#if Trojan_FB
using Facebook.Unity;
#endif

public enum FriendToggleType{
	Send = 0,
	Collect,
	Invite,
	Max,
}

public class FriendUIBehaviour : MonoBehaviour{
	// 面板选项卡
	public Toggle[] _toggles;
	// 退出按钮
	public GameObject _exitButton;
	// 输入框清除按钮
	public GameObject _inputClearButton;
	// 输入框
	public InputField _inputField;
	// loading图
	public GameObject _loading;
	// 送礼界面
	public FriendScrollviewController _sendControl;
	// 收礼界面
	public FriendScrollviewController _collectControl;
	// 邀请界面
	public FriendScrollviewController _inviteControl;
    [SerializeField]
    private Canvas _canvas;

	// 当前界面
	private FriendToggleType _toggleType;
	// 上一个界面
	private FriendToggleType _lastToggleType;
	// 当前搜索字段
	private string _findName = "";
	// 上一个搜索字段
	private string _lastFindName = "";

    private WindowInfo _windowInfoReceipt = null;
	// Use this for initialization
	void Start () {
		EventTriggerListener.Get (_exitButton).onClick += OnClick;
		EventTriggerListener.Get (_inputClearButton).onClick += OnClick;

		_toggleType = FriendToggleType.Invite;
		_lastToggleType = _toggleType;

		EventTriggerListener.Get( _toggles[(int)FriendToggleType.Send].gameObject).onClick += OnClick;
		EventTriggerListener.Get( _toggles[(int)FriendToggleType.Collect].gameObject).onClick += OnClick;
		EventTriggerListener.Get( _toggles[(int)FriendToggleType.Invite].gameObject).onClick += OnClick;

		InitFriendManagerHandler ();

		if (DeviceUtility.IsConnectInternet ()) {
			HandleUserFriendsPermission ();
		}
	}

	void OnDestroy(){
		ClearFriendManagerHandler ();
        WindowManager.Instance.TellClosed(_windowInfoReceipt);
        _windowInfoReceipt = null;
	}

	private void InitFriendManagerHandler(){
		if (FriendManager.Instance != null) {
			FriendManager.Instance.SendScrollViewHandler = () => {
				_sendControl.InitScrollView (FriendManager.Instance.SendList);
			};
			FriendManager.Instance.CollectScrollViewHandler = () => {
				_collectControl.InitScrollView (FriendManager.Instance.CollectList);
			};
			FriendManager.Instance.InviteScrollViewHandler = () => {
				_inviteControl.InitScrollView (FriendManager.Instance.InviteList);
			};
		}
	}

	private void ClearFriendManagerHandler(){
		if (FriendManager.Instance != null) {
			FriendManager.Instance.SendScrollViewHandler = null;
			FriendManager.Instance.CollectScrollViewHandler = null;
			FriendManager.Instance.InviteScrollViewHandler = null;
		}
	}

	private void HandleUserFriendsPermission(){
		#if false
		InitFriendScrollView();
		#else

		#if Trojan_FB
		if (FacebookHelper.IsInitialized && FacebookHelper.IsLoggedIn)
		{
			LogUtility.Log("HandleUserFriendsPermission FacebookHelper.IsInitialized && FacebookHelper.IsLoggedIn");
			// 有好友权限
			if (FacebookHelper.HaveUserFriends) {
				LogUtility.Log("FacebookHelper.HaveUserFriends");
				if (FriendManager.Instance.HasAllFriends) {
					FriendManager.Instance.InitScrollView();
				} else {
					// 向facebook要数据
					FriendManager.Instance.Init();
				}
			} else {
				// 申请好友权限
				FacebookHelper.LoginWithFB (HandleUserFriendsPermission);
			}
		}
		#endif

		#endif
	}

	#if Trojan_FB
	private void HandleUserFriendsPermission(ILoginResult result){
		if(FacebookUtility.ResultValid(result) && result.AccessToken != null){
			GameDebug.Log("", "FB", "HandleUserFriendsPermission Success Response:\n" + result.RawResult);
			if(FB.IsInitialized){
				if (FacebookHelper.HaveUserFriends) {
					FriendManager.Instance.Init();
				}
				AnalysisManager.Instance.SendLoginFacebookSuccessCallback();
			}
			else {
				GameDebug.Log("", "FB", "HandleUserFriendsPermission Failed to Initialize the Facebook SDK");
				AnalysisManager.Instance.SendLoginFacebookFailCallback();
			}
		}
		else {
			GameDebug.Log("", "FB", "HandleUserFriendsPermission Fail Response:\n" + result.RawResult);
			AnalysisManager.Instance.SendLoginFacebookFailCallback();
		}
		FacebookHelper.RemoveLoginFBCallback (HandleUserFriendsPermission);
	}
	#endif

	// Update is called once per frame
	void Update () {
		UpdateFindName (_inputField.text, true);
		if (_loading != null) {
			_loading.SetActive (!FriendManager.Instance.HasAllFriends && DeviceUtility.IsConnectInternet());
		}
	}

	private void UpdateFindName(string text, bool changeScrollView){
		_findName = text;
		if (!_lastFindName.Equals (_findName)) {
			UpdateInputField (_findName, changeScrollView);
			LogUtility.Log ("find friend name = " + _findName);
		}
	}

	private void UpdateInputField(string name, bool changeScrollView){
		List<FriendData> dataList = FindFriend (name);
		if (_toggleType == FriendToggleType.Send) {
			_sendControl.DataUpdateList (dataList, changeScrollView);
		} else if (_toggleType == FriendToggleType.Collect) {
			_collectControl.DataUpdateList (dataList, changeScrollView);
		} else if (_toggleType == FriendToggleType.Invite) {
			_inviteControl.DataUpdateList (dataList, changeScrollView);
		}
		_lastFindName = name;
	}

	void OnClick(GameObject obj){
		AudioManager.Instance.PlaySound (AudioType.Click);
		if (obj == _exitButton) {
            Hide();	
		} else if (obj == _inputClearButton) {
			_inputField.text = "";
			UpdateFindName (_inputField.text, true);
		} else if (obj == _toggles [(int)FriendToggleType.Send].gameObject) {
			SetFriendToggle (FriendToggleType.Send);
		} else if (obj == _toggles [(int)FriendToggleType.Collect].gameObject) {
			SetFriendToggle (FriendToggleType.Collect);
		} else if (obj == _toggles [(int)FriendToggleType.Invite].gameObject) {
			SetFriendToggle (FriendToggleType.Invite);
		}
	}

	private void ShowToggle(FriendToggleType type){
		if (type == FriendToggleType.Send) {
			_sendControl.gameObject.SetActive (true);
			_collectControl.gameObject.SetActive (false);
			_inviteControl.gameObject.SetActive (false);
		} else if (type == FriendToggleType.Collect) {
			_sendControl.gameObject.SetActive (false);
			_collectControl.gameObject.SetActive (true);
			_inviteControl.gameObject.SetActive (false);
		} else if (type == FriendToggleType.Invite) {
			_sendControl.gameObject.SetActive (false);
			_collectControl.gameObject.SetActive (false);
			_inviteControl.gameObject.SetActive (true);
		}
	}

	public void SetFriendToggle(FriendToggleType type){
		bool change = _lastToggleType != type;

		if (change) {
			_inputField.text = "";
			// 更新上一个界面	
			UpdateFindName (_inputField.text, true);

			_toggleType = type;
			ShowToggle (_toggleType);
			_toggles [(int)_toggleType].isOn = true;

			// 更新新界面
			UpdateInputField (_inputField.text, true);

			_lastToggleType = _toggleType;
		}
	}

	private List<FriendData> FindFriend(string name){
		List<FriendData> list = new List<FriendData>();

		if (_toggleType == FriendToggleType.Send) {
			list.AddRange(FriendManager.Instance.SendList);
		} else if (_toggleType == FriendToggleType.Collect) {
			list.AddRange(FriendManager.Instance.CollectList);
		} else if (_toggleType == FriendToggleType.Invite) {
			list.AddRange(FriendManager.Instance.InviteList);
		}

		if (string.IsNullOrEmpty (name)) {
			return list;
		}

		// 找到符合名字的玩家
		if (list != null){
			list = ListUtility.FilterList(list, (FriendData data)=>{
				string dataName = data.Name.ToLower();
				string compareName = name.ToLower();
				return dataName.Contains(compareName);
			});
		}

		return list;
	}

    public void Show()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }

    }

    private void Hide()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void SelfClose(Action callBack)
    {
        gameObject.SetActive(false);
        callBack();
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(_canvas, _exitButton.gameObject))
        {
            gameObject.SetActive(false);
            _windowInfoReceipt = null;
            callBack(true);
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }
}
