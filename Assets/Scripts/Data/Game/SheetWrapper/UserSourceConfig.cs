using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSourceConfig : SimpleSingleton<UserSourceConfig> {

    public static readonly string Name = "UserSource";

    private UserSourceSheet _userSourceSheet;
    public UserSourceConfig()
    {
        LoadData();
    }

    void LoadData()
    {
        _userSourceSheet = GameConfig.Instance.LoadExcelAsset<UserSourceSheet>(Name);
    }

    public static void Reload()
    {
        Debug.Log("Reload UserSourceConfig");
        UserSourceConfig.Instance.LoadData();
    }

    public string GetSourceStr(int id)
    {
        string source = "unknow";
        UserSourceData result = ListUtility.FindFirstOrDefault(_userSourceSheet.dataArray, x => x.ID == id);
        if (result == null)
            LogUtility.Log("Can't find source id in config, id : " + id);
        else
            source = result.Key;

        return source;
    }

    public int GetAdStrategyId(int strategyId)
    {
        return ListUtility.FindFirstOrDefault(_userSourceSheet.dataArray, x => x.ID == strategyId).AdStrategy;
    }

    public int GetAdStrategyId(string userSource)
    {
        return ListUtility.FindFirstOrDefault(_userSourceSheet.dataArray, x => x.Key == userSource).AdStrategy;
    }
}
