using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class PayRotaryTableSystem : Singleton<PayRotaryTableSystem>
{
    public readonly string PRTPurchaseItemId = "23";
    public Animator PRtAnimator;
    private IRandomGenerator _roller = new LCG((uint)new System.Random().Next(), null);

    void Start()
    {
        CitrusEventManager.instance.AddListener<OnDailyRotaryAnimEnd>(AskPlayGame);
    }
    void OnDestroy()
    {
        CitrusEventManager.instance.RemoveListener<OnDailyRotaryAnimEnd>(AskPlayGame);
    }

    public void Init()
    {
    }

    public void AskPlayGame(OnDailyRotaryAnimEnd e)
    {
        if (CheckIfCanPlay())
        {
            EnterGame();
        }
        else
        {
            CitrusEventManager.instance.Raise(new OnPayRotaryTableOver(null));
        }
    }

    private bool CheckIfCanPlay()
    {
        bool result = false;

        result = !TimeUtility.IsSameDay(UserDeviceLocalData.Instance.FirstEnterGameTime, NetworkTimeHelper.Instance.GetNowTime());
        LogUtility.Log("WheelOfLuck : need show wheelOfLuck : " + result + "   firstEnterGameDate : " + UserDeviceLocalData.Instance.FirstEnterGameTime);
		if (!GroupConfig.Instance.IsProductExist(StoreType.WheelOfLuck))
			result = false;
        return result;
    }

    private void EnterGame()
    {
        UIManager.Instance.ShowPopup<PayRotaryTableUiControler>(UIManager.PayRotaryTableUiPath);

        SetAnalysisData();
    }

    public void PayForPlayingGame()
    {
        StoreManager.Instance.InitiatePurchase(PRTPurchaseItemId);
        AudioManager.Instance.PlaySound(AudioType.Click); 
    }

    private void SetAnalysisData()
    {
        StoreController.Instance.CurrStoreAnalysisData = new StoreAnalysisData();
        StoreController.Instance.CurrStoreAnalysisData.OpenPosition = OpenPos.Lobby.ToString();
        StoreController.Instance.CurrStoreAnalysisData.StoreEntrance = StoreType.WheelOfLuck.ToString();

        AnalysisManager.Instance.OpenShop();
    }

    public PayRotaryTableData GameResultData(int signInDays, IAPData data)
    {
        PayRotaryTableData result = null;
        List<int> indexList = ListUtility.CreateIntList(0, PayRotaryTableConfig.Instance.ListSheet.Count);
        List<float> ratioList = new List<float>();

        LogUtility.Log("PayRotaryTableSystem  signInDays: " + signInDays);

        ListUtility.ForEach(PayRotaryTableConfig.Instance.ListSheet, x =>
        {
            if (x.Prob.Length > signInDays)
            {
                ratioList.Add(x.Prob[signInDays]);
            }
            else
            {
                Debug.LogError("PayRotaryTableSystem : Error Prob Index, id : " + x.BonusID + "   signInDays:"  + signInDays);
                ratioList.Add(1);
            }
        });
        int resultIndex = RandomUtility.RollSingleIntByRatios(_roller, indexList, ratioList);
        result = PayRotaryTableConfig.Instance.ListSheet[resultIndex];

        //add credits to propertyTrace
        PropertyTrackManager.Instance.OnPayGameRewardUser(data.TransactionId, (ulong)result.Bonus);
        return result;
    }
}
