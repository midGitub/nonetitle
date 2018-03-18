
using System.Collections;
using UnityEngine;

public class StoreRewardAdController : BaseRewardADController
{
    public override void TryShowAdButton()
    {
        StartCoroutine(OnRewardEffectOver());
    }

    public override void GetBonus()
    {
        base.GetBonus();
        AddCoins();
    }

    void AddCoins()
    {
        ADSData data = ADSConfig.Instance.Sheet.dataArray[(int)UserBasicData.Instance.PlayerPayState];
        UserBasicData.Instance.AddCredits((ulong)data.RewardCredit, FreeCreditsSource.NotFree, true);
    }
    IEnumerator OnRewardEffectOver()
    {
        StoreRewardADSButton button = BindRewardAdButton as StoreRewardADSButton;
        Debug.Assert(button != null , "can not convert BindRewardAdButton into StoreRewardADSButton");
        while (button.RewardEffetIsPlaying)
        {
            yield return null;
        }

        base.TryShowAdButton();
    }
}
