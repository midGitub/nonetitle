﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoubleWinAdUiController : PopUpControler
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _watchVideoButton;
    [SerializeField] private Text _freeText;
    [SerializeField] private Text _forYouText;
    [SerializeField] private Text _winningText;
    [SerializeField] private Text _nextSpinText;
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _watchVideoText;
    [SerializeField] private Text _tiemr;
    [SerializeField] private Text _freeCredisText;

    public PuzzleMachine PuzzleMachineObj;
    public BaseRewardADController AdController;

    private readonly string _adTypeName = "doubleWin";

    void Start()
    {
        RegisterHandler();
    }

    void OnDestroy()
    {
        UnRegisterHandler();
    }

    void OnEnable()
    {
        SetWatchButtonState(PuzzleMachineObj);
    }
    void RegisterHandler()
    {
        PuzzleMachineObj.StartSpinEventHandler += SetWatchButtonState;
        PuzzleMachineObj.EndRoundEventHandler += SetWatchButtonState;
    }

    void UnRegisterHandler()
    {
        PuzzleMachineObj.StartSpinEventHandler -= SetWatchButtonState;
        PuzzleMachineObj.EndRoundEventHandler -= SetWatchButtonState;
    }

    protected override void InitText()
    {
        _freeText.text = LocalizationConfig.Instance.GetValue("bonusAd_free");
        _forYouText.text = LocalizationConfig.Instance.GetValue("bonusAd_forYou");
        _winningText.text = LocalizationConfig.Instance.GetValue("bonusAd_yourWinning");
        _nextSpinText.text = LocalizationConfig.Instance.GetValue("bonusAd_nextSpin");
        _timerText.text = LocalizationConfig.Instance.GetValue("bonusAd_endsIn");
        _watchVideoText.text = LocalizationConfig.Instance.GetValue("bonusAd_watchVideo");
        _freeCredisText.text = string.Format(LocalizationConfig.Instance.GetValue("bonusAd_doubleWin_freeCredits"), 
            AdBonusConfig.Instance.GetAdBonusDataByAdType(_adTypeName).BasicRewardCredits);
    }

    public IEnumerator SetTimer(int duration)
    {
        yield return TimeUtility.StartTimerMMSS(NetworkTimeHelper.Instance.GetNowTime(), _tiemr, duration, Close);
    }

    public override void Init()
    {
        base.Init();     
        InitSortingLayer();
        InitButtons();
    }

    void InitSortingLayer()
    {
        CanvasComponent.sortingLayerName = StringDefine.SorintLayerUi;
    }

    void InitButtons()
    {
        _exitButton.onClick.RemoveAllListeners();
        _watchVideoButton.onClick.RemoveAllListeners();
        _exitButton.onClick.AddListener(Close);
        _watchVideoButton.onClick.AddListener(WatchVideo);
    }

    void SetWatchButtonState(PuzzleMachine puzzleMachine)
    {
        if (puzzleMachine != null)
        {
            _watchVideoButton.interactable = puzzleMachine._state == MachineState.Idle && puzzleMachine._spinMode != SpinMode.Auto;
        }
    }

    void WatchVideo()
    {
        AudioManager.Instance.PlaySound(AudioType.Click);
        AdController.PlayAd(RewardAdType.DoubleWinning);
    }
}