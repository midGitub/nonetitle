using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompensationUiController : PopUpControler
{
    public Text CompensationText;
    public Text CoinNum;
    public Button ExitButton;

    private readonly string _dicePurcahse = "Dice";
    private readonly string _wheelOfLuckPurchase = "WheelOfLuck";
    private CompensationData _compensationData;
    private IAPData _iapData;

    public override void Init()
    {
        base.Init();
        ExitButton.onClick.AddListener(GiveCompensationCredits);
        RegisterCloseButton(ExitButton);
    }

    public void HandleRevalidPurchase(IAPData data)
    {
        IAPCatalogData iapData = IAPCatalogConfig.Instance.FindIAPItemByID(data.LocalItemId);
        CompensationData compensationData = CompensationConfig.Instance.GetCompensationData(iapData.Title);
        _compensationData = compensationData;
        _iapData = data;

        if (compensationData != null)
        {
            SetContent(compensationData);
            Open();
            GivePayGameCredits();
        }
    }

    public void SetContent(CompensationData data)
    {
        CoinNum.text = data.ExtraCredits.ToString();
        CompensationText.text = string.Format(data.Content, data.ExtraCredits);
    }

    void GivePayGameCredits()
    {
        ulong payGameRewardCredit = 0;

        if (_compensationData.Type == _dicePurcahse)
        {
            payGameRewardCredit = (ulong)(UserDeviceLocalData.Instance.DiceRatio * (int)UserDeviceLocalData.Instance.DiceInitCredits);
        }
        else if (_compensationData.Type == _wheelOfLuckPurchase)
        {
            payGameRewardCredit = (ulong)PayRotaryTableConfig.Instance.ListSheet[0].Bonus;
        }

        if (payGameRewardCredit > 0)
        {
            StoreManager.Instance.AddCreditsAndLuckyByItemId(_iapData.LocalItemId, (long)payGameRewardCredit);
            PropertyTrackManager.Instance.OnPayGameRewardUser(_iapData.TransactionId, payGameRewardCredit);
        }
    }

    void GiveCompensationCredits()
    {        
        if (_compensationData != null)
        {
            UserBasicData.Instance.AddCredits((ulong)_compensationData.ExtraCredits, FreeCreditsSource.NotFree, false);
            PropertyTrackManager.Instance.OnPayGameRewardUser(_iapData.TransactionId, (ulong)_compensationData.ExtraCredits);          
        }
    }
}
