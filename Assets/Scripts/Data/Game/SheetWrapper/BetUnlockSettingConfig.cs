using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetUnlockSettingConfig : SimpleSingleton<BetUnlockSettingConfig>
{
	public static readonly string Name = "BetUnlockSetting";
	// 解锁最大bet时需要的等级
	private static int _betMaxLevel = 0;

	private BetUnlockSettingSheet _sheet;
	// bet 解锁dict
	private Dictionary<int, long> _dict = new Dictionary<int, long>();

	public BetUnlockSettingConfig(){
		LoadData ();
	}

	private void LoadData(){
		_sheet = GameConfig.Instance.LoadExcelAsset<BetUnlockSettingSheet>(Name);
		InitData ();
	}

	private void InitData(){
		_dict.Clear();
		ListUtility.ForEach (_sheet.DataArray, (BetUnlockSettingData data) => {
			_dict.Add(data.Level, data.MaxBet);
		});
		int length = _sheet.DataArray.Length;
		_betMaxLevel = _sheet.DataArray [length - 1].Level;
	}

    public ulong GetMaxBet(string machineName)
    {
        ulong result;
		int level = (int)UserBasicData.Instance.UserLevel.Level;
        ulong[] betOptions = BetOptionConfig.Instance.GetMachineBetOptions(machineName);
        bool ignoreLockBet = MachineUnlockSettingConfig.Instance.IsVipMachine(machineName);

        if (ignoreLockBet)
            result = betOptions[betOptions.Length - 1];
        else
            result = level >= _betMaxLevel ? (ulong)_dict[_betMaxLevel] : (ulong)_dict[level];

        return result;
    }

	public bool IsMaxBet(int level){
		return level == _betMaxLevel;
	}

	public bool IsSameBet(int oldLevel, int newLevel){
		if (_dict.ContainsKey (newLevel) && _dict.ContainsKey (oldLevel))
			return _dict [oldLevel] == _dict [newLevel];
		else if (_dict.ContainsKey (oldLevel)) {
			return _dict [oldLevel] == _dict [_betMaxLevel];
		}

		return true;
	}

	public static void Reload()
	{
		Debug.Log("Reload BetUnlockSettingConfig");
		BetUnlockSettingConfig.Instance.LoadData();
	}
}
