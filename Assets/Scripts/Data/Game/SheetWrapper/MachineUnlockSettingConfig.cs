using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapMachineType
{
    Normal = 0,
    Tiny = 1,
    Vip = 2,
}

public class MachineUnlockSettingConfig : SimpleSingleton<MachineUnlockSettingConfig>
{
	public static readonly string Name = "MachineUnlockSetting";

	// version1_3版本全部机台数
	private static readonly int _machineVersion1_3_NumMax = 10;

	//机台动画间隔延迟
	private static readonly float DEFAULT_START_DELAY = 1;
	private static readonly float DEFAULT_PLAYAGAIN_DELAY = 15;

	private MachineUnlockSettingSheet _sheet;
    private Dictionary<string, MachineUnlockSettingData> _machineDataDict = new Dictionary<string, MachineUnlockSettingData>();

	// 机台名 - 机台列表索引值
	private Dictionary<string, int> _machine2index = new Dictionary<string, int>();
	// 机台总数
	private int _dataLength;
	// 机台顺序数组
	public string[] MapMachineList;
    // 机台列表
    public Dictionary<MapMachineType, List<string>> MapMachineDic;
	// 机台信息
	private Dictionary<string, MachineData> _mapMachineDataDict = new Dictionary<string, MachineData> ();
	// version1_3所有机台
	public string[] AllMachineNameVersion1_3;

	private string[] _localAssetMachines;
	public string[] LocalAssetMachines { get { return _localAssetMachines; } }

	public struct MachineData
	{
		public string Name;
		public int UnlockLevel;
		public float StartDelay;
		public float PlayAgainDelay;
	    public MapMachineType MachineType;

		public MachineData(string name, int lv,  float startDelay, float playAgainDelay, MapMachineType type)
        {
			Name = name;
			UnlockLevel = lv;
			StartDelay = startDelay;
			PlayAgainDelay = playAgainDelay;
            MachineType = type;
            //Debug.Log("Machinedata created = "+name+" "+lv+" "+pos+" "+startDelay+" "+playAgainDelay);
        }
	}

	public MachineUnlockSettingConfig(){
		LoadData ();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<MachineUnlockSettingSheet>(Name);

		_machineDataDict.Clear ();
		_machine2index.Clear ();
		_mapMachineDataDict.Clear ();

		_dataLength = _sheet.DataArray.Length;

		ListUtility.ForEach (_sheet.DataArray, (data) => {
            _machineDataDict.Add(data.Key, data);
		});

		InitMapMachine ();
		InitLocalAssetMachines();
		InitAllMachineNameVersion1_3 ();
	}

	void InitMapMachine()
    {
        MapMachineList = new string[_dataLength];
        MapMachineDic = new Dictionary<MapMachineType, List<string>>();

	    for (int i = 0; i < _dataLength; i++)
	    {
	        string key = _sheet.DataArray[i].Key;
            MachineUnlockSettingData settingData = _sheet.DataArray[i];
            MachineData data = new MachineData(settingData.Key, settingData.Val, settingData.StarDelay, settingData.PlayAgainDelay, (MapMachineType)settingData.MapMachineType);
            _mapMachineDataDict.Add(key, data);
            _machine2index.Add(key, i);

	        MapMachineType machineType = (MapMachineType)settingData.MapMachineType;

            if (!MapMachineDic.ContainsKey(machineType))
            {
                MapMachineDic.Add(machineType, new List<string>());
            }

            MapMachineDic[machineType].Add(key);
            MapMachineList[i] = key;
        }

        if (MapSettingConfig.Instance.IsTinyMachineRoomEnable)
            MapMachineDic[MapMachineType.Tiny].Add("comingsoon");
        else
            MapMachineDic[MapMachineType.Normal].Add("comingsoon");
    }

	void InitLocalAssetMachines()
	{
		List<string> names = ListUtility.CreateList(MapMachineList, LiveUpdateConfig._localAssetMachineLength);
		_localAssetMachines = names.ToArray();
	}

	void InitAllMachineNameVersion1_3(){
		AllMachineNameVersion1_3 = new string[_machineVersion1_3_NumMax];
		for (int i = 0; i < AllMachineNameVersion1_3.Length; i++) {
			AllMachineNameVersion1_3 [i] = MapMachineList [i];
		}
	}

	public static void Reload()
	{
		Debug.Log("Reload MachineUnlockSettingConfig");
		MachineUnlockSettingConfig.Instance.LoadData();
	}

	public int GetUnlockLevel(string machineName){
		if (_machineDataDict.ContainsKey (machineName)) {
			return _machineDataDict[machineName].Val;
		}

		return int.MaxValue;
	}

    public bool IsVipMachine(string machineName)
    {
        bool result = false;
        if (_machineDataDict != null && _machineDataDict.ContainsKey(machineName))
        {
            result = (MapMachineType) _machineDataDict[machineName].MapMachineType == MapMachineType.Vip;
        }

        return result;
    }

    public int GetUnlockVipLevel(string machineName)
    {
        if (_machineDataDict.ContainsKey(machineName))
        {
            return _machineDataDict[machineName].UnlockVipLv;
        }

        return int.MaxValue;
    }

    public int GetMachineIndex(string machineName){
		if (_machine2index.ContainsKey (machineName)) {
			return _machine2index [machineName];
		}

		return -1;
	}

	public float GetMachineStartDelay(string name){
		if (_mapMachineDataDict.ContainsKey (name)) {
			return _mapMachineDataDict [name].StartDelay;
		}

		return DEFAULT_START_DELAY;
	}

	public float GetMachinePlayAgainDelay(string name){
		if (_mapMachineDataDict.ContainsKey (name)) {
			return _mapMachineDataDict [name].PlayAgainDelay;
		}

		return DEFAULT_PLAYAGAIN_DELAY;
	}

    public bool IsTinyMachine(string name)
    {
        return MapMachineDic.ContainsKey(MapMachineType.Tiny) && MapMachineDic[MapMachineType.Tiny].Contains(name);
    }
}
