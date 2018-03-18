using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompensationConfig : SimpleSingleton<CompensationConfig>
{

    public static readonly string Name = "Compensation";

    private CompensationSheet _compensationSheet;
    private readonly string _defaultKey = "Others";
    private CompensationData _defaultData;
    public CompensationConfig()
    {
        LoadData();
    }

    void LoadData()
    {
        _compensationSheet = GameConfig.Instance.LoadExcelAsset<CompensationSheet>(Name);
        _defaultData = GetCompensationData(_defaultKey);
    }

    public CompensationData GetCompensationData(string itemTitle)
    {
        CompensationData result = null;
        ListUtility.ForEach(_compensationSheet.dataArray, data =>
        {
            if (itemTitle.Contains(data.Type))
            {
                result = data;
            }
        });

        if (result == null)
        {
            result = _defaultData;
        }
        return result;
    }

    public static void Reload()
    {
        Debug.Log("Reload MapSettingConfig");
        CompensationConfig.Instance.LoadData();
    }
}
