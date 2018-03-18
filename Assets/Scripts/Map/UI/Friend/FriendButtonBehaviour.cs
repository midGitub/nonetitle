using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public enum FriendButtonType{
	Login,
	Friend,
}

public class FriendButtonBehaviour : MonoBehaviour {
	
	public GameObject _loginButton;
	public GameObject _friendButton;

	// UI父节点
	public GameObject _uiRoot;

	private FriendButtonType _buttonType = FriendButtonType.Login;

	// Use this for initialization
	void Start () {
		// 判断当前是否登陆FACEBOOK
		_buttonType = FacebookHelper.IsLoggedIn ? FriendButtonType.Friend : FriendButtonType.Login;
		ChangeButtonType (_buttonType);

		EventTriggerListener.Get (_loginButton).onClick += OnClick;
		EventTriggerListener.Get (_friendButton).onClick += OnClick;

		CitrusEventManager.instance.AddListener<UserLoginOrRegisterEvent> (LoginCallback);
		CitrusEventManager.instance.AddListener<UserLogoutEvent> (LogoutCallback);
        CitrusEventManager.instance.AddListener<FaceBookLoginPopEvent>(OnPop);
	}

	// Update is called once per frame
	void Update () {
		ChangeButtonType (FacebookHelper.IsLoggedIn ? FriendButtonType.Friend : FriendButtonType.Login);
	}

	void OnDestroy(){
		CitrusEventManager.instance.RemoveListener<UserLoginOrRegisterEvent> (LoginCallback);
		CitrusEventManager.instance.RemoveListener<UserLogoutEvent> (LogoutCallback);
        CitrusEventManager.instance.RemoveListener<FaceBookLoginPopEvent>(OnPop);
	}

    private void OnPop(FaceBookLoginPopEvent e)
    {
        UIManager.Instance.PopOpenFacebookLoginUI(_uiRoot);
    }

	private void OnClick(GameObject obj){
		AudioManager.Instance.PlaySound (AudioType.Click);
		if (obj == _loginButton) {
			// 打开login界面
			UIManager.Instance.CickOpenFacebookLoginUI(_uiRoot);
		} else if (obj == _friendButton) {
			// 打开好友界面
			UIManager.Instance.OpenFriendUI (_uiRoot);
		}
	}

	public void ChangeButtonType(FriendButtonType type){
		_buttonType = type;
		if (_buttonType == FriendButtonType.Login) {
			_loginButton.SetActive (true);
			_friendButton.SetActive (false);
		} else if (_buttonType == FriendButtonType.Friend) {
			_loginButton.SetActive (false);
			_friendButton.SetActive (true);
		}
	}

	private void LoginCallback(UserLoginOrRegisterEvent msg){
		if (UserLoginStateHelper.Instance.IsFacebookLoginState) {
			ChangeButtonType (FriendButtonType.Friend);
			Debug.Log ("facebookbutton login");
		}
	}

	private void LogoutCallback(UserLogoutEvent msg){
		ChangeButtonType (FriendButtonType.Login);
		Debug.Log ("facebookbutton logout");
	}
}
