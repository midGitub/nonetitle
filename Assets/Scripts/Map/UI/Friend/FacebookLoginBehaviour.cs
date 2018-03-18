using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using System;
#if Trojan_FB
using Facebook.Unity;
#endif

public class FacebookLoginBehaviour : MonoBehaviour{

	private static readonly string _hintName = "facebookLogin_hint";

	// 提示语
	public Text[] _Texts;
	// 登录按钮
	public GameObject _loginButton;
	// 游客按钮（等同于退出）
	public GameObject _guestButton;
	// 错误按钮
	public GameObject _errorUI;
	//login reward credits text
	public Text LoginRewardText;
	public GameObject[] LoginRewardUi;

	public WidgetJumpController _jumpController;
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Canvas _canvas;

    private WindowInfo _windowInfoReceipt = null;

	// Use this for initialization
	void Start () {
		EventTriggerListener.Get(_loginButton).onClick += Onclick;
		EventTriggerListener.Get(_guestButton).onClick += Onclick;

		InitTextHint ();
		InitLoginRewardUiState();
		InitLoginFacebookRewardText();

        CitrusEventManager.instance.AddListener<UserLoginOrRegisterEvent>(LoginCallback);
		CitrusEventManager.instance.AddListener<LikeUsInFacebookEvent>(OnUserLikeOurAppInFacebook);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDestroy(){
		if(CitrusEventManager.instance != null)
		{
            CitrusEventManager.instance.RemoveListener<UserLoginOrRegisterEvent> (LoginCallback);
			CitrusEventManager.instance.RemoveListener<LikeUsInFacebookEvent>(OnUserLikeOurAppInFacebook);
		}

		//todo by nichos:
		//Do we need this code? Or it should be deleted?
		if(_windowInfoReceipt != null && WindowManager.Instance != null)
		{
			WindowManager.Instance.TellClosed(_windowInfoReceipt);
			_windowInfoReceipt = null; 
		}
	}

	void Onclick(GameObject obj){
		AudioManager.Instance.PlaySound (AudioType.Click);
		if (obj == _loginButton) {
			LogUtility.Log ("facebook login button click");
			if (DeviceUtility.IsConnectInternet()){
				LoadingManager.Instance.ShowLoading (true);
			}
			LogInWithFaceBook ();
		} else if (obj == _guestButton) {
			LogUtility.Log ("guest button click");
            Hide();	
		}
	}

	private void InitTextHint(){
		for (int i = 1; i <= _Texts.Length; ++i) {
			string key = _hintName + i.ToString ();
			_Texts [i-1].text = LocalizationConfig.Instance.GetValue (key);
		}
	}

	private void InitLoginRewardUiState()
	{
		if (null != LoginRewardUi) 
		{
			bool firstLogin = UserBasicData.Instance.IsFirstLoadingFaceBook;
			for(int i = 0; i < LoginRewardUi.Length; i++)
			{
				LoginRewardUi [i].SetActive(firstLogin);
			}
		}
	}

	private void InitLoginFacebookRewardText()
	{
		LoginRewardText.text = "+" + StringUtility.FormatNumberString((ulong)CoreConfig.Instance.LuckyConfig.FaceBookLoginAddCoins, true, true);
	}

	private void LogInWithFaceBook()
	{
		// 没联网
		if(Application.internetReachability == NetworkReachability.NotReachable)
		{
			// 使用fb弹出ERROR
			_errorUI.SetActive(true);
			return;
		}

		Debug.Log(FacebookHelper.IsLoggedIn);
		#if Trojan_FB
		if(!FacebookHelper.IsLoggedIn)
		{
			UserDeviceLocalData.Instance.ShouldHandleUserDataLoss = false;
			FacebookHelper.LoginWithFB(HandleFacebookLoginCallback);
		}
		#endif
	}

	#if Trojan_FB
	public void HandleFacebookLoginCallback(ILoginResult result)
	{
	    bool loginSucceed = false;
		if(result != null && FacebookUtility.ResultValid(result))
		{
			if (result.Cancelled) {
				LogUtility.Log ("HandleFacebookLoginCallback cancelled", Color.red);	
			} else{
				LogUtility.Log ("HandleFacebookLoginCallback successed", Color.yellow);
			    loginSucceed = true;
			}
		}
		else
		{
			LogUtility.Log ("HandleFacebookLoginCallback failed", Color.red);
		}

        //when fb login succeed, do not close loading circle meanwhile, we still need some time to register or login fb from our server, close loading circle after that down
        LoadingManager.Instance.ShowLoading(loginSucceed);
		FacebookHelper.RemoveLoginFBCallback(HandleFacebookLoginCallback);	
	}
	#endif

    public void LoginCallback(UserLoginOrRegisterEvent msg)
    {
        LoadingManager.Instance.ShowLoading(false);
        // 按钮无法点击
        _loginButton.GetComponent<Button>().interactable = !FacebookHelper.IsLoggedIn;
		// 关闭面板
        Hide();
	}

	public void TryShow()
	{
		if (_windowInfoReceipt == null)
		{
			_windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
			WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
		}
	}

	void Open()
	{
		if (gameObject != null) {
			gameObject.SetActive(true);
		}
	}

    public void TryEnterLobbyShow()
    {
		if(_windowInfoReceipt == null && CanOpenEnterLobby())
        {
			_windowInfoReceipt = new WindowInfo(OpenEnterLobby, ManagerClose, _canvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

	bool CanOpenEnterLobby()
	{
		bool result = !FacebookHelper.IsLoggedIn && !AlreadyPoppedToday();
		return result;
	}

    void OpenEnterLobby()
    {
		if (gameObject != null) {
			gameObject.SetActive(true); 
		}
		UserDeviceLocalData.Instance.LastPopFaceBookLogin = NetworkTimeHelper.Instance.GetNowTime();
    }

	public void Hide()
	{
		SelfClose(() => {
			WindowManager.Instance.TellClosed(_windowInfoReceipt);
			_windowInfoReceipt = null;
		});
	}

    private bool AlreadyPoppedToday()
    {
        string lastTime = UserDeviceLocalData.Instance.LastPopFaceBookLogin.ToString("d");
        string nowTime = NetworkTimeHelper.Instance.GetNowTime().ToString("d");
        return lastTime == nowTime;
    }

    private void SelfClose(Action callBack)
    {
        if (gameObject.activeInHierarchy)
        {
            if (_jumpController != null)
            {
                _jumpController.Open(false, () =>
                    {
                        // 其实就是跳出
                        gameObject.SetActive(false); 
                        callBack();
                    });
            }
            else
            {
                // 其实就是跳出
                gameObject.SetActive(false); 
                callBack();
            } 
        }
        else
            callBack();
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(_canvas, _exitButton.gameObject))
        {
            if (_jumpController != null) {
                _jumpController.Open (false, () => {
                    // 其实就是跳出
                    gameObject.SetActive(false); 
                    _windowInfoReceipt = null;
                    callBack(true);
                });
            } else {
                // 其实就是跳出
                gameObject.SetActive(false); 
                _windowInfoReceipt = null;
                callBack(true);
            }
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

	void OnUserLikeOurAppInFacebook(LikeUsInFacebookEvent e)
	{
		//TODO UI Controler need konw it's self state, just like open, close. otherwise this function will always be called, that not what we want(called only in open state)
		Hide();
	}
}