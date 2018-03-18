using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class MachineRewardAdManager : MonoBehaviour
{
    [SerializeField] private List<MachineBonusAdItem> _bonusAdObjs;
    [SerializeField] private PuzzleMachine _puzzleMachine;
    [SerializeField] private GameObject[] _freeMaxWinEffect;
    [SerializeField] private GameObject _doubleWinEffect;
    [SerializeField] private Animator _doubleWinAnimCtrl;
    [SerializeField] private DoubleWinRewardAdButton _doubleWinRewardAdButton;
    [SerializeField] private FreeMaxBetRewardAdButton _freeMaxBetRewardAdButton;
    [SerializeField] private DoubleWinRewardAdController _doubleWinAdCtrl;
    [SerializeField] private FreeMaxBetRewardAdController _freeMaxBetAdCtrl;

    private int _curBonusAdTypeId;
    private bool _isShowingUnfinishedAd;

    public bool IsShowingUnfinishedAd
    {
        get { return _isShowingUnfinishedAd;}
        set { _isShowingUnfinishedAd = value; }
    }

    public delegate void SetSpinTypeOnAdOver(SpecialMode type);

    public event SetSpinTypeOnAdOver OnMachineRewardAdOver = delegate { };

    void Start()
    {
        RegisterHandler();
    }

    void OnDestroy()
    {
        UnRegisterHandler();
    }

    void RegisterHandler()
    {
        _puzzleMachine.StartSpinEventHandler += OnSpinStart;
        OnMachineRewardAdOver += _puzzleMachine.EnterSpecialMode;
        _puzzleMachine.EnterSpecialModeHandler += EnterSpecialMode;
        _puzzleMachine.EndSpecialModeHandler += EndSpecialMode;
        _puzzleMachine.DuringSpecialModeHandler += DuringSpecialMode;
    }

    void UnRegisterHandler()
    {
        _puzzleMachine.StartSpinEventHandler -= OnSpinStart;
        OnMachineRewardAdOver -= _puzzleMachine.EnterSpecialMode;
        _puzzleMachine.EnterSpecialModeHandler -= EnterSpecialMode;
        _puzzleMachine.EndSpecialModeHandler -= EndSpecialMode;
        _puzzleMachine.DuringSpecialModeHandler -= DuringSpecialMode;
    }

    public void Init()
    {
        GameObject doubleWinUiGo = UIManager.Instance.LoadPopupAtPath(UIManager.DoubleWinRewardAdUiPath);
        GameObject freeMaxBetUiGo = UIManager.Instance.LoadPopupAtPath(UIManager.FreeMaxBetRewardAdUiPath);

        doubleWinUiGo.SetActive(false);
        freeMaxBetUiGo.SetActive(false);

        DoubleWinAdUiController doubleWinUiCtrl = doubleWinUiGo.GetComponent<DoubleWinAdUiController>();
        FreeMaxBetAdUiController freeMaxBetUiCtrl = freeMaxBetUiGo.GetComponent<FreeMaxBetAdUiController>();

        _doubleWinRewardAdButton.PopupController = doubleWinUiCtrl;
        doubleWinUiCtrl.AdController = _doubleWinAdCtrl;
        doubleWinUiCtrl.PuzzleMachineObj = _puzzleMachine;
        _freeMaxBetRewardAdButton.PopupController = freeMaxBetUiCtrl;
        freeMaxBetUiCtrl.AdController = _freeMaxBetAdCtrl;
        freeMaxBetUiCtrl.PuzzleMachineObj = _puzzleMachine;

        InitUiState();
    }

    void InitUiState()
    {
        ListUtility.ForEach(_bonusAdObjs, x => x.gameObject.SetActive(false));

        LogBonusAdInfos();
        CheckUnfinishedAdState();
    }

    public void OnAdOver(SpecialMode mode)
    {
        _curBonusAdTypeId = 0;

        if (_puzzleMachine._specialMode == SpecialMode.Normal)
        {
            OnMachineRewardAdOver(mode);
        }
    }

    public void LogBonusAdInfos()
    {
        LogUtility.Log(string.Format("AdBonusModule : TotalPayAmout : {0}, " +
                                     " TotalSpinCount : {1},  " +
                                     "RegisterDay : {2}",
                                     UserBasicData.Instance.TotalPayAmount,
                                     UserMachineData.Instance.TotalSpinCount,
                                     UserBasicData.Instance.RegistrationTime),
                                     Color.yellow);
    }

    void OnSpinStart(PuzzleMachine machine)
    {
       CheckAdShowState();
    }

    void CheckUnfinishedAdState()
    {
        if (!TimeUtility.IsDatePast(UserDeviceLocalData.Instance.LastMachineAdEndTime))
        {
            _isShowingUnfinishedAd = true;
            ShowBonusAdObj(UserDeviceLocalData.Instance.LastMachineAdId);
        }
        else
        {
           CheckAdShowState();
        }
    }

    void CheckAdShowState()
    {
        AdBonusData bonusData = AdBonusConfig.Instance.GetBestFitAdBonusData(_puzzleMachine.MachineName);
        if (bonusData != null)
        {
            LogUtility.Log("AdBonusModule : show rewardAdBonusButton , button type : " + bonusData.BonusType);
            _isShowingUnfinishedAd = false;
            ShowBonusAdObj(bonusData.AdTypeId);
        }
    }

    public void ShowBonusAdObj(int adTypeId)
    {
        Debug.Assert(_bonusAdObjs.Count > adTypeId, "MachineRewardAdError : can not find obj with id : " + adTypeId);
        _curBonusAdTypeId = adTypeId >= _curBonusAdTypeId ? adTypeId : _curBonusAdTypeId;
        GameObject needShowBonusGo = null;

        ListUtility.ForEach(_bonusAdObjs, x =>
        {
            x.gameObject.SetActive(false);

            if (x.BonusAdTypeId == _curBonusAdTypeId)
            {
                needShowBonusGo = x.gameObject;
            }          
        });

        if (needShowBonusGo != null)
        {
            needShowBonusGo.SetActive(true);
        }

    }

    void EnterSpecialMode(PuzzleMachine machine)
    {
        switch (machine._specialMode)
        {
            case SpecialMode.DoubleWin:
                ShowDoubleWinSymbol(true);
                break;
            case SpecialMode.FreeMaxBet:
                PlayFreeMaxBetWinEffect(true);
                break;
        }
    }

    void EndSpecialMode(PuzzleMachine machine)
    {
        switch (machine._specialMode)
        {
            case SpecialMode.DoubleWin:
                ShowDoubleWinSymbol(false);
                break;
            case SpecialMode.FreeMaxBet:
                PlayFreeMaxBetWinEffect(false);
                break;
        }
    }

    void DuringSpecialMode(PuzzleMachine machine)
    {
        switch (machine._specialMode)
        {
            case SpecialMode.DoubleWin:
                PlayDoubleWinEffect();
                break;
        }
    }

    void PlayFreeMaxBetWinEffect(bool play)
    {
        ListUtility.ForEach(_freeMaxWinEffect, x => { x.SetActive(play);});
    }

    void ShowDoubleWinSymbol(bool show)
    {
        _doubleWinAnimCtrl.SetTrigger("idle");
        _doubleWinEffect.SetActive(show);
    }

    void PlayDoubleWinEffect()
    {
        _doubleWinAnimCtrl.SetTrigger("play");
    }
}
