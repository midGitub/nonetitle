
using System;
using System.Collections;
using CitrusFramework;
using UnityEngine;

public class StoreRewardADSButton : BaseRewardAdButton
{
    public bool RewardEffetIsPlaying;

    void Start()
    {
        SetButtonClickListener();
    }

    void OnEnable()
    {
        ShowAdButton(false);
        RewardAdController.TryShowAdButton();
    }

    void OnDisable()
    {
        RewardAdController.StopShowAdButtonCoroutine();
    }

    void OnDestroy()
    {
        RewardAdController.StopShowAdButtonCoroutine();
    }

    public override void OnButtonClick()
    {
        RewardAdType type = RewardAdType.None;
        
        switch (StoreController.Instance.CurrStoreAnalysisData.OpenPosition)
        {
            case "Lobby":
                type = RewardAdType.Lobby;
                break;
            case "GameUp":
                type = RewardAdType.GameUpBuy;
                break;
            case "GameBelow":
                type = RewardAdType.GameBelowBuy;
                break;
        }
        RewardAdController.PlayAd(type);
    }

    public override void OnAdClose()
    {
        base.OnAdClose();
        StoreController.Instance.bigStoreController.HideBigStore();
    }

    public override void GetBonus()
    {
        PlayRewardEffect();
    }

    void PlayRewardEffect()
    {
        GetCredits.Instance.PlayGetCreditEffect();
        RewardEffetIsPlaying = true;
        UnityTimer.Instance.StartTimer(GetCredits.Instance.EffectContinueTime, OnRewardEffectOver);
    }

    void OnRewardEffectOver()
    {
        RewardEffetIsPlaying = false;
    }
}
