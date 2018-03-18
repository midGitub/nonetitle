//#define ADJUST_TEST
using CitrusFramework;
using UnityEngine.Purchasing;

public class SDKEventManager : SimpleSingleton<SDKEventManager> {

    public void OnPurchaseVerifySuccess(bool sandbox, IAPData iapData, string extraInfo = null)
    {
        string localItemId = iapData.LocalItemId;
        var iapItem = IAPCatalogConfig.Instance.FindIAPItemByID(localItemId);
        LogUtility.Log("SDKEventManager : isSandbox : " + sandbox + "   localItemId :  " + iapItem.ID + "   receipt : " + iapData.Receipt + "  tokens count : " + iapData.tokens.Count);
        foreach (var kv in iapData.tokens)
        {
            LogUtility.Log("SDKEventManager : tokens key : " +  kv.Key + "   tokens value : " + kv.Value);
        }
        //the sanbox flag is only vertify appstore account which server send to client, so we need to vertify gp account locally
        //filter sandbox purchase data

#if ADJUST_TEST
        AdjustManager.Instance.Purchase(iapData);
#endif

        if (sandbox)
        {
#if DEBUG
            AnalysisManager.Instance.SuccessVerify(extraInfo);
#endif
        }
        else
        {
            AnalysisManager.Instance.SuccessVerify(extraInfo);
            AdjustManager.Instance.Purchase(iapData);

#if Trojan_FB
            FacebookHelper.LogPurchase(iapItem.Title, iapItem.Price.ToString());
#endif
        }
    }

}
