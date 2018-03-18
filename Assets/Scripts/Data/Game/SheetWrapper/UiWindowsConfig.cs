using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiWindowsConfig : SimpleSingleton<UiWindowsConfig> {

    public static readonly string Name = "UiWindows";

    private UiWindowsSheet _uiWindowsSheet;
    public UiWindowsConfig()
    {
        LoadData();
    }

    void LoadData()
    {
        _uiWindowsSheet = GameConfig.Instance.LoadExcelAsset<UiWindowsSheet>(Name);
    }

    public List<UiWindow> GetWindowsByShowTime(ShowWindowTime time, bool fliterByGroup = false)
    {
        List<UiWindow> result = new List<UiWindow>();
        List<UiWindow> resultForGroup = new List<UiWindow>();

        ListUtility.ForEach(_uiWindowsSheet.dataArray, data =>
        {
            if (data.ShowTime == (int)time)
                result.Add((UiWindow)data.ID);
        });

        if (fliterByGroup)
        {
            List<int> usersWindows = GroupConfig.Instance.GetAvailabelWindows();
            ListUtility.ForEach(usersWindows, id =>
            {
                ListUtility.ForEach(result, window =>
                {
                    if ((int)window == id)
                        resultForGroup.Add(window);
                });
            });
        }

        return fliterByGroup ? resultForGroup : result;
    }

    public static void Reload()
    {
        Debug.Log("Reload UiWindowsConfig");
        UiWindowsConfig.Instance.LoadData();
    }
}
