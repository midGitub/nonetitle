using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;
using UnityEngine.UI;

public class ScoreAppUiControler : MonoBehaviour {

	private WindowInfo _windowInfoReceipt;

	public GameObject CollectEffect;
	public Button JumpToStore;
	public CloseGameObject CloseGameObject;
	public Text FreeCreditsCount;
	public Text JumpToAppStoreText;
	public Text JumpToGooglePlayText;

    [SerializeField]
    private Button _exitButton;
    [SerializeField]
	private Canvas _cavas;

	private bool _pauseByMe;

	public void OnEnable()
	{
		Init();
	}

	private void Init()
	{
	}
		
	void Start () {
		_cavas.worldCamera = Camera.main;
		InitUiTexts();
	}

	void Update ()
	{
		
	}

	private void InitUiTexts()
	{
		FreeCreditsCount.text = StringUtility.FormatNumberString((ulong)CoreConfig.Instance.MiscConfig.ScoredInStoreReward, true, true);

		#if UNITY_EDITOR
		#elif UNITY_ANDROID
		JumpToAppStoreText.gameObject.SetActive(false);
		JumpToGooglePlayText.gameObject.SetActive(true);
		#elif UNITY_IOS
		JumpToAppStoreText.gameObject.SetActive(true);
		JumpToGooglePlayText.gameObject.SetActive(false);
		#endif
	}

	public void Show()
	{
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, _cavas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt); 
        }
	}

	public void Open()
	{
		gameObject.SetActive(true);
	}

	public void SelfClose(Action callback)
	{
		if(CloseGameObject != null)
		{
			CloseGameObject.Close(_cavas.gameObject, callback);
			ScoreAppSystem.Instance.SetClosePageTime();
		}
	}

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(_cavas, _exitButton.gameObject))
        {
            if(CloseGameObject != null)
            {
                CloseGameObject.Close(_cavas.gameObject, () => {
                    _windowInfoReceipt = null;
                    callBack(true);
                });
                ScoreAppSystem.Instance.SetClosePageTime();
            }
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        _cavas.gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }

	public void Hide()
	{
		SelfClose(() => {
			WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
		});
	}

    void OnDestroy()
    {
		if (_windowInfoReceipt != null && WindowManager.Instance != null)
        {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        }
    }

	public void JumpToStoreScorePage()
	{
		string scorePageUrl = "";
#if UNITY_IOS
        scorePageUrl = PackageConfigManager.Instance.CurPackageConfig.AppScoreUrl;
#elif UNITY_ANDROID
		scorePageUrl = ServerConfig.GoogleStoreScorePageUrl;
#endif

        Application.OpenURL(scorePageUrl);
		_pauseByMe = true;
	} 

	void OnApplicationPause(bool isPause)
	{
		if(!isPause && _pauseByMe)
		{
			ScoreAppSystem.Instance.UserScoredApp();
			Hide();
			_pauseByMe = false;
		}
	}
}
