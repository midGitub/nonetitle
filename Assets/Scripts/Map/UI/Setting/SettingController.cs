using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using System;


public class SettingController : Singleton<SettingController>
{
	public Canvas SettingCanvas;
	public ChangeGameObjectButton MusicButton;
	public ChangeGameObjectButton SoundButton;
	public Text VersionText;
	public Text UDIDText;
    [SerializeField]
    private Button _exitButton;
	[SerializeField]
	private GameObject _goldenFinger;

    private WindowInfo _windowInfoReceipt;

	private void Awake()
	{
		CitrusEventManager.instance.AddListener<LikeUsInFacebookEvent>(OnUserLikeOurAppInFacebook);
	}

	private void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<LikeUsInFacebookEvent>(OnUserLikeOurAppInFacebook);
	}

	public void OpenSettingUI()
	{
	    SettingCanvas.worldCamera = Camera.main;
		SettingCanvas.gameObject.SetActive(true);
		VersionText.text = "Version: " + BuildUtility.GetBundleVersion() + "." + BuildUtility.GetVersionCode () +" / " + LiveUpdateManager.Instance.GetResourceVersion();
		UDIDText.text = "UDID: " + UserBasicData.Instance.UDID;
        ActiveGoldenFingerInDebugMode();
	}

	private void ActiveGoldenFingerInDebugMode()
	{
		#if DEBUG
		_goldenFinger.SetActive(true);
		#else
		_goldenFinger.SetActive(false);
		#endif
	}

	public void OnEnable()
	{
		MusicButton.UpdateButtonState(AudioManager.Instance.IsMusicOn);
		SoundButton.UpdateButtonState(AudioManager.Instance.IsSoundOn);
	}

	public void GoldenFingerButtonDown()
	{
		#if DEBUG
		GameObject goldenFinger = UIManager.Instance.OpenGoldenFingerUi();
		goldenFinger.SetActive(!goldenFinger.activeSelf);
		#endif
	}

	public void MusicButtonDown()
	{
		AudioManager.Instance.IsMusicOn = !AudioManager.Instance.IsMusicOn;
		MusicButton.UpdateButtonState(AudioManager.Instance.IsMusicOn);
	}

	public void SoundButtonDown()
	{
		AudioManager.Instance.IsSoundOn = !AudioManager.Instance.IsSoundOn;
		SoundButton.UpdateButtonState(AudioManager.Instance.IsSoundOn);
	}

	public void OnApplicationQuit()
	{
		//Debug.Log("Application ending after " + Time.time + " seconds");
	}

	public void ShowFeedBack()
	{
        // Xhj 打开feedBack需要先隐藏Setting
        Hide();
		UIManager.Instance.OpenFeedBackUI();
	}

    public void Show()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, SettingCanvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }

    }
        
    public void Hide()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    public void Open()
    {
        OpenSettingUI();
    }

    private void SelfClose(Action callBack)
    {
        SettingCanvas.gameObject.SetActive(false);
        callBack();
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(SettingCanvas, _exitButton.gameObject))
        {
            SettingCanvas.gameObject.SetActive(false);
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
        SettingCanvas.gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }

	void OnUserLikeOurAppInFacebook(LikeUsInFacebookEvent e)
	{
		//TODO UI Controler need konw it's self state, just like open, close. otherwise this function will always be called, that not what we want(called only in open state)
		Hide();
	}
}
