using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapSettingConfig : SimpleSingleton<MapSettingConfig>
{
	public static readonly string Name = "MapSetting";

	private MapSettingSheet _mapSettingSheet;

	public string MachineAssetUrlDebug;
	public string MachineAssetUrlRelease;

	private int _minSpinCountOfPopTimeLimitStore;
	public int MinSpinCountOfPopTimeLimitStore { get { return _minSpinCountOfPopTimeLimitStore; } }

    private Dictionary<string, string> _mapSettingMap = new Dictionary<string, string>();
    public Dictionary<string, string> MapSettingMap { get { return _mapSettingMap; } }

	public bool IsSkipVerifyPurchase
	{
		get {
			bool result = false;
			string str = "";
			if(_mapSettingMap.TryGetValue("IsSkipVerifyPurchase", out str))
				result = (str == "1");
			return result;
		}
	}

    public bool IsVipRoomEnable
    {
        get {
            bool result = false;
            string str = "";
            if (_mapSettingMap.TryGetValue("EnableVipRoom", out str))
                result = (str == "1");
            return result;
        }
    }

    public bool IsTinyMachineRoomEnable
    {
        get
        {
            bool result = false;
            string str = "";
            if (_mapSettingMap.TryGetValue("EnableTinyRoom", out str))
                result = (str == "1");
            return result;
        }
    }

    public MapSettingConfig()
	{
		LoadData();
	}

	void LoadData()
	{
		_mapSettingSheet = GameConfig.Instance.LoadExcelAsset<MapSettingSheet>(Name);
#if false
		MachineAssetUrlDebug = ListUtility.FindFirstOrDefault(_mapSettingSheet.dataArray, (obj) => { return obj.Key == "MachineAssetUrlDebug"; }).Val;
		MachineAssetUrlRelease = ListUtility.FindFirstOrDefault(_mapSettingSheet.dataArray, (obj) => { return obj.Key == "MachineAssetUrlRelease"; }).Val;
#else
		MachineAssetUrlDebug = ServerConfig.MachineAssetUrlDebug;
		MachineAssetUrlRelease = ServerConfig.MachineAssetUrlRelease;
#endif

		_mapSettingMap.Clear();
        foreach (var item in _mapSettingSheet.dataArray)
        {
            _mapSettingMap.Add(item.Key, item.Val);
        }

		if(_mapSettingMap.ContainsKey("MinSpinCountOfPopTimeLimitStore"))
		{
			string s = _mapSettingMap["MinSpinCountOfPopTimeLimitStore"];
			int.TryParse(s, out _minSpinCountOfPopTimeLimitStore);
		}
		else
		{
			_minSpinCountOfPopTimeLimitStore = 50;
		}
	}

	public string Read(string key,string defautvalue)
	{
		if (_mapSettingMap.ContainsKey(key))
			return _mapSettingMap [key];
		else
			return defautvalue;
	}

	public T Read<T>(string key, T defautvalue)
	{
		T value = defautvalue;
		if (_mapSettingMap.ContainsKey(key))
		{
			string str = _mapSettingMap[key];
			if (!str.IsNullOrEmpty())
			{
				try
				{
					value = (T)Convert.ChangeType((object)str, typeof(T));
				}
				catch(Exception)
				{
					value = defautvalue;
				}
			}
		}
		return value;
	}

	public static void Reload()
	{
		Debug.Log("Reload MapSettingConfig");
		MapSettingConfig.Instance.LoadData();
	}
}
