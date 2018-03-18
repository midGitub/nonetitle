using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using CitrusFramework;

public class MachineCommentController : MonoBehaviour {

    public CommentStarUI[] commentStarUI = new CommentStarUI[5];
    public Button submitButton;
    public Text priceTxt;
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Canvas _canvas;

    private int _commentStarVal;
    private ulong _rewardVal;

    private WindowInfo _windowInfoReceipt = null;

    public void OnEnable()
    {
        _commentStarVal = 0;
        submitButton.interactable = false;
    }

    void Start () {
        Canvas canvas = gameObject.GetComponent<Canvas> ();
        canvas.worldCamera = Camera.main;
        _rewardVal = (ulong)Convert.ToInt32(MapSettingConfig.Instance.MapSettingMap[SettingKeyMap.mCRewardVal]);
        priceTxt.text = 
            StringUtility.FormatNumberStringWithComma(_rewardVal);

        commentStarUI[0].starButton.onClick.AddListener(OnClick1);
        commentStarUI[1].starButton.onClick.AddListener(OnClick2);
        commentStarUI[2].starButton.onClick.AddListener(OnClick3);
        commentStarUI[3].starButton.onClick.AddListener(OnClick4);
        commentStarUI[4].starButton.onClick.AddListener(OnClick5);
    }

    private void LightStarsByVal()
    {
        int i;
        for (i = 0; i < commentStarUI.Length; i++)
        {
            if (i < _commentStarVal)
            {
                commentStarUI[i].LightStar();
            }
            else
            {
                commentStarUI[i].LightOffStar();
            }
        }
    }

    public void OnClick1()
    {
        submitButton.interactable = true;
        if (_commentStarVal != 1)
        {
            _commentStarVal = 1;
            LightStarsByVal();
        }
    }

    public void OnClick2()
    {
        submitButton.interactable = true;
        if (_commentStarVal != 2)
        {
            _commentStarVal = 2;
            LightStarsByVal();
        }
    }

    public void OnClick3()
    {
        submitButton.interactable = true;
        if (_commentStarVal != 3)
        {
            _commentStarVal = 3;
            LightStarsByVal();
        }
    }

    public void OnClick4()
    {
        submitButton.interactable = true;
        if (_commentStarVal != 4)
        {
            _commentStarVal = 4;
            LightStarsByVal();
        }
    }

    public void OnClick5()
    {
        submitButton.interactable = true;
        if (_commentStarVal != 5)
        {
            _commentStarVal = 5;
            LightStarsByVal();
        }
    }

    public void ShowCollectioningCoinsEffect()
    {
        CitrusEventManager.instance.Raise(new CollectingCoinsEffectEvent());
    }

    public void Submit()
    {
        UserDeviceLocalData.Instance.LastMachineCommentTime = NetworkTimeHelper.Instance.GetNowTime();
        UserDeviceLocalData.Instance.MachineCommentTimesToday += 1;
        UserMachineData.Instance.SetFirstTimeFlag(MapScene.CurrentMachineName, false);
        ShowCollectioningCoinsEffect();
        UserBasicData.Instance.AddCredits(_rewardVal, FreeCreditsSource.RateMachineBonus, true);
        AnalysisManager.Instance.MachineComment(MapScene.CurrentMachineName, _commentStarVal);

        if (_commentStarVal == 5)
        {
            //call storeInStore ui open
            CitrusEventManager.instance.Raise(new MachineGotFiveStarEvent());
        }

        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    public void Show()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

    public void Hide()
    {
        UserDeviceLocalData.Instance.LastRefuseMachineCommentTime = NetworkTimeHelper.Instance.GetNowTime();
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    void OnDestroy()
    {
        if (_windowInfoReceipt != null)
        {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void SelfClose(Action callBack)
    {
        gameObject.SetActive(false);
        _commentStarVal = 0;
        LightStarsByVal();
        callBack(); 
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(StoreController.Instance.StoreCanvas, _exitButton.gameObject))
        {
            gameObject.SetActive(false);
            _commentStarVal = 0;
            LightStarsByVal();
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
        _commentStarVal = 0;
        LightStarsByVal();
        _windowInfoReceipt = null;
    }
}
