using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdStrategyConfig : SimpleSingleton<AdStrategyConfig> {

    public static readonly string Name = "AdStrategy";

    private AdStrategySheet _adStrategySheet;
    public AdStrategyConfig()
    {
        LoadData();
    }

    void LoadData()
    {
        _adStrategySheet = GameConfig.Instance.LoadExcelAsset<AdStrategySheet>(Name);
    }

    public static void Reload()
    {
        Debug.Log("Reload AdStrategyConfig");
        AdStrategyConfig.Instance.LoadData();
    }

    private AdStrategyData GetStrategyDataById(int id)
    {
        return ListUtility.FindFirstOrDefault(_adStrategySheet.dataArray, x =>  x.ID == id);
    }

    public bool IsInterstitialActive(int id)
    {
        AdStrategyData data = GetStrategyDataById(id);
        bool result = data == null || data.InterstitialMachine == 1;

        return result;
    }

    public bool IsHomeInInterstitialActive(int id)
    {
        AdStrategyData data = GetStrategyDataById(id);
        bool result = data == null || data.InterstitialHomeIn == 1;

        return result;
    }

    public bool IsDoubleWinRewardAdActive(int id)
    {
        AdStrategyData data = GetStrategyDataById(id);
        bool result = data == null || data.RewardDoubleWin == 1;

        return result;
    }

    public bool IsRewardLobbyAdActive(int id)
    {
        AdStrategyData data = GetStrategyDataById(id);
        bool result = data == null || data.RewardLobby == 1;

        return result;
    }
}
