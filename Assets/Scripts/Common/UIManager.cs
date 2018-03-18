using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class UIManager : Singleton<UIManager>
{
    private static readonly string _faceboolLoginPath = "Map/Friend/FacebookLoginUI";
    private static readonly string _friendPath = "Map/Friend/FriendUI";
    private static readonly string _friendCoinEffectPath = "Map/Friend/FaceBook_coinsIM";
    private static readonly string _mailUIPath = "Map/Mail/MailUI";
    private static readonly string _feedBackUIPath = "Map/SettingPrefab/feedback";
	private static readonly string _scoreAppUiPath = "Map/ScoreApp/ScoreAppUi";
    private static readonly string _machineCommentUIPath = "Map/MachineComment/MachineCommentUI";
    private static readonly string _quitGameUIPath = "Map/QuitGameUI";
	private static readonly string _jackpotNotifyPath = "Map/Jackpot/JackpotNotify";
	private static readonly string _goldenFingerUiPath = "Map/UIBar/DebugConsole";
	private static readonly string _bankruptCompensatePath = "Map/Bankrupt/BankruptCredits";

    public static readonly string DiceUiPath = "Game/Ui/DiceUi/DiceUi";
    public static readonly string PayRotaryTableUiPath = "Map/PayRotaryTable/PayRotaryTable";
    public static readonly string DoubleWinRewardAdUiPath = "Game/Ui/MachineRewardAd/DoubleWinRewardAdUi";
    public static readonly string FreeMaxBetRewardAdUiPath = "Game/Ui/MachineRewardAd/FreeMaxBetRewardAdUi";
    public static readonly string ChristmasActivityUiPath = "Map/Activities/Christmas/ChristmasActivityUi";
    public static readonly string ChristmasMaxWinBillboardPath = "Game/UI/BillBoard/ChristmasMaxWin";
    public static readonly string DoubleHourBonusPopupPath = "Map/Activities/Christmas/DoubleHourBonusPopup";
    public static readonly string DoubleHourBonusBillboardPath = "Game/UI/BillBoard/DoubleHourBonus";
    public static readonly string _noGiftPath = "Map/Gift/NoGiftUI";
    public static readonly string CompensationUiPath = "Map/Compensation/CompensationPopup";
    public static readonly string VipMachineUnlockNotifyUiPath = "Map/VipMachine/VipMachineUnlockNotifyUi";
    public static readonly string VipMachineBillboardPath = "Game/UI/BillBoard/VipMachineBillboard";

    public void Init()
    {
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 创建轮盘
	public void OpenWheelUI(GameObject obj, PuzzleMachine puzzleMachine, WheelConfig[] wheelConfigs, Callback<bool> endFunc)
    {
		obj.SetActive(true);
        MachineWheel wheel = obj.GetComponent<MachineWheel>();
		if (wheel != null)
        {
			wheel.InitWheel(puzzleMachine, wheelConfigs);
			wheel.FinishEvent = endFunc;
        }
    }

    // 创建FACEBOOK登录界面
    public GameObject CickOpenFacebookLoginUI(GameObject parent)
    {
        GameObject obj = UGUIUtility.FindOrInstantiateUI<FacebookLoginBehaviour>(parent, _faceboolLoginPath);
        obj.SetActive(false);
        FacebookLoginBehaviour behaviour = obj.GetComponent<FacebookLoginBehaviour>();
        behaviour.TryShow();
        return obj;
    }

    public GameObject PopOpenFacebookLoginUI(GameObject parent)
    {
//        GameObject parent = GameObject.Find("HUD");
        GameObject obj = UGUIUtility.FindOrInstantiateUI<FacebookLoginBehaviour>(parent, _faceboolLoginPath); 
        obj.SetActive(false);
        FacebookLoginBehaviour behaviour = obj.GetComponent<FacebookLoginBehaviour>();
        behaviour.TryEnterLobbyShow();
        return obj;
    }

    // 创建好友界面
    public GameObject OpenFriendUI(GameObject parent)
    {
        GameObject obj = UGUIUtility.FindOrInstantiateUI<FriendUIBehaviour>(parent, _friendPath);
        obj.SetActive(false);
        FriendUIBehaviour behaviour = obj.GetComponent<FriendUIBehaviour>();
        behaviour.Show();
        return obj;
    }

    // 创建好友界面收完礼物特效
    public GameObject OpenFriendCoinEffect(GameObject parent)
    {
        GameObject obj = UGUIUtility.FindOrInstantiateUI<FriendCoinController>(parent, _friendCoinEffectPath);
        if (obj != null)
            obj.SetActive(true);
        return obj;
    }

    private GameObject MailUI;

    public GameObject OpenMailUI()
    {
        MailUIController controller = Transform.FindObjectOfType<MailUIController>();
        if (controller == null){
            MailUI = UGUIUtility.InstantiateUI(MailUI, _mailUIPath);
            // By nichos:
            // This kind of code is very ugly and bad! It will cause OnEnable() be called twice for this controller
            // But we can't delete it since it might cause other serious issues when opening multiple menus at the same time
            MailUI.SetActive(false);
            controller = MailUI.GetComponent<MailUIController>();
            controller.Show();
        }else {
            MailUI = controller.gameObject;
            controller.Show();
        }

        return MailUI;
    }

    private GameObject feedBackUI;

    public GameObject OpenFeedBackUI()
    {
        feedBackUI = UGUIUtility.InstantiateUI(feedBackUI, _feedBackUIPath);
        feedBackUI.SetActive(false);
        FeedBackController controller = feedBackUI.GetComponent<FeedBackController>();
        controller.Show();
         
        return feedBackUI;
    }

	private GameObject _scoreAppUi;
	public GameObject OpenScoreAppUi()
	{
		_scoreAppUi = UGUIUtility.InstantiateUI(_scoreAppUi, _scoreAppUiPath);
		_scoreAppUi.SetActive(false);
		Debug.Assert(_scoreAppUi != null, "Can not find scoreAppUI Obj");
		ScoreAppUiControler controller = _scoreAppUi.GetComponent<ScoreAppUiControler>();
		Debug.Assert(controller != null, "scoreApp controller missed!");
		controller.Show();


		return _scoreAppUi;
	}

	private GameObject _diceUi;
	public GameObject OpenDiceUi()
	{
	    _diceUi = LoadPopupAtPath(DiceUiPath);
        _diceUi.SetActive(false);

		Debug.Assert(_diceUi != null, "Can not find _diceUi Obj");
		DiceUiControler controller = _diceUi.GetComponent<DiceUiControler>();
		Debug.Assert(controller != null, "diceUi controller missed!");
		controller.Show();

		return _diceUi;
	}

    private GameObject _machineCommentUI;

    public void OpenMachineCommentUI()
    {
        _machineCommentUI = UGUIUtility.InstantiateUI(_machineCommentUI, _machineCommentUIPath);
        _machineCommentUI.SetActive(false);
        MachineCommentController controller = _machineCommentUI.GetComponent<MachineCommentController>();
        controller.Show();
    }

    private GameObject _quitGameUI;

    public void OpenQuitGameUI()
    {
        _quitGameUI = UGUIUtility.InstantiateUI(_quitGameUI, _quitGameUIPath);
        _quitGameUI.SetActive(false);
        QuitGameUIController controller = _quitGameUI.GetComponent<QuitGameUIController>();
        controller.Show(); 
    }

	public GameObject OpenJackpotNotifyUI(GameObject parent){
		GameObject obj = UGUIUtility.FindOrInstantiateUI<JackpotNotifyBehaviour>(parent, _jackpotNotifyPath);
		if (obj != null) {
			obj.SetActive(true);

			JackpotNotifyBehaviour behaviour = obj.GetComponent<JackpotNotifyBehaviour> ();
			if (behaviour != null) {
				behaviour.ShowNotify ();
			}
		}
		
		return obj;
	}

	private GameObject _goldenFingerUi;
	public GameObject OpenGoldenFingerUi()
	{
		_goldenFingerUi = UGUIUtility.InstantiateUI(_goldenFingerUi, _goldenFingerUiPath);
		Debug.Assert(_goldenFingerUi != null, "Can not find GoldFinger Obj");
		return _goldenFingerUi;
	}

	public GameObject OpenBankruptCreditsUI(int credits){
		BankruptCompensateBehaviour behaviour = Transform.FindObjectOfType<BankruptCompensateBehaviour> ();
		if (behaviour == null) {
			GameObject obj = AssetManager.Instance.LoadAsset<GameObject> (_bankruptCompensatePath);
			if (obj != null) {
				GameObject bankruptObj = UGUIUtility.CreateObj (obj, null);
				if (bankruptObj != null) {
					behaviour = bankruptObj.GetComponent<BankruptCompensateBehaviour> ();
					behaviour.ShowUI (true);
				}
				return bankruptObj;
			}
		} else {
			behaviour.ShowUI (true);
			return behaviour.gameObject;
		}
		return null;
	}

    public GameObject OpenNoGiftUI(NoGiftType type){
        GameObject obj = null;
		NoGiftBehaviour behaviour = Transform.FindObjectOfType<NoGiftBehaviour> ();
        
        if (behaviour == null){
            obj = UGUIUtility.CreateObj(_noGiftPath);
        }else{
            obj = behaviour.gameObject;
        }

        if (obj != null){
            behaviour = obj.GetComponent<NoGiftBehaviour>();
            if (behaviour != null){
                behaviour.ShowUI(type);
            }
        }

        return obj;
    }

	#region pop up

    public Dictionary<string, GameObject> LoadedPopups = new Dictionary<string, GameObject>();

    public void CleanPopupsOnSceneLoad()
    {
        LoadedPopups.Clear();
    }

    public GameObject LoadPopupAtPath(string prefabPath)
    {
        bool loadedPopupIsNull = LoadedPopups.ContainsKey(prefabPath) && LoadedPopups[prefabPath] == null;

        if (loadedPopupIsNull)
        {
            Debug.Assert(LoadedPopups[prefabPath] != null, "UIManager : Error, Can not find popup obj when Popups contains it's key , path : " + prefabPath);
        }

        if (!LoadedPopups.ContainsKey(prefabPath) || loadedPopupIsNull)
        {
            LoadedPopups[prefabPath] = UGUIUtility.InstantiateUI(null, prefabPath);
        }

        return LoadedPopups[prefabPath];
    }

    public void ShowPopup<T>(string prefabPath) where T : PopUpControler
    {
        if (!LoadedPopups.ContainsKey(prefabPath))
        {
            LoadPopupAtPath(prefabPath);
        }

        //the controler script need attach to the root of Ui Gameobject
        T controller = LoadedPopups[prefabPath].GetComponent<T>();
        Debug.Assert(controller != null, "popupControler is missed!");
        if (controller != null)
        {
            controller.Open();
        }
    }

    public void ClosePopup<T>(string prefabPath) where T : PopUpControler
    {
        T controller = LoadedPopups[prefabPath].GetComponent<T>();
        Debug.Assert(controller != null, "popupControler is missed!");
        if (controller != null)
        {
            controller.Close();
        }
    }

	#endregion
}
