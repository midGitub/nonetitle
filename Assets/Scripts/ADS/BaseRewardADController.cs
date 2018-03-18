using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public abstract class BaseRewardADController : MonoBehaviour
{
    [SerializeField]
    protected BaseRewardAdButton BindRewardAdButton;
    private Coroutine _tryShowAdButton;
    private float _loadingUiContinueTime = 3f;
    public int AdDurationTime;

    public void Update()
    {
		if (UserBasicData.Instance.IsPayUser)
            BindRewardAdButton.ShowAdButton(false);
    }

    public virtual void TryShowAdButton()
    {
        _tryShowAdButton = StartCoroutine(TryUpdateShowAdButton());
    }

    public void StopShowAdButtonCoroutine()
    {
        if (_tryShowAdButton != null)
        {
            StopCoroutine(_tryShowAdButton);
        }
    }

    IEnumerator TryUpdateShowAdButton()
    {
        while (!CanShowAdUiButton())
        {
            yield return new WaitForSeconds(1);
        }

        OnAdShow();
    }

    protected virtual bool CanShowAdUiButton()
    {
		bool canPlay = false;
		#if UNITY_EDITOR
		canPlay = true;
		#else
		canPlay = ADSManager.Instance.HaveVideoAD() && UserBasicData.Instance.IsFreeUser;
		#endif
        return canPlay;
    }

    public virtual void PlayAd(RewardAdType rewardAdType)
	{
        ADSManager.Instance.SetAnalysisData(rewardAdType);
#if UNITY_EDITOR
        LogUtility.Log("do not play reward video in editot mode");
        OnAdClose();
        GetBonus();
#else
	    if (ADSManager.Instance.HaveVideoAD())
	    {
            LoadingManager.Instance.ShowLoading(true);
            //sometimes there isn't a adClose callback, so we need to close loadingUi few seconds later to avoid soft lock
            UnityTimer.Instance.StartTimer(_loadingUiContinueTime, () => LoadingManager.Instance.ShowLoading(false));
            ADSManager.Instance.ADFinishedGetBonus = GetBonus;
            ADSManager.Instance.RewardBasedVideoClosed = OnAdClose;
            ADSManager.Instance.ShowVideoADS();
            LogUtility.Log("AdsManager : PlayAd Function Called", Color.yellow);
        }
#endif
    }

    public virtual void OnAdShow()
    {
        BindRewardAdButton.OnAdShow();
    }

    public virtual void OnAdClose()
    {
        LoadingManager.Instance.ShowLoading(false);
        BindRewardAdButton.OnAdClose();
        LogUtility.Log("AdsManager : OnAdClose Called", Color.yellow);
    }

    public virtual void GetBonus()
    {
        BindRewardAdButton.GetBonus();
    }
}
