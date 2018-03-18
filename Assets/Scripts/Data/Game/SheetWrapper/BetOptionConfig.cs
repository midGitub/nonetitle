using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BetMachineInfo{
	public string _type;
	public int[] _options;
	public string[] _machienNames;

	public BetMachineInfo(string type, int[] options, string[] names){
		_type = type;
		_options = options;
		_machienNames = names;
	}
}

public class BetOptionConfig : SimpleSingleton<BetOptionConfig> {
	public static readonly string Name = "BetOption";
	private static readonly string _defaultTypeName = "type1";
	private BetOptionSheet _sheet;

	private Dictionary<string, BetMachineInfo> _dict = new Dictionary<string, BetMachineInfo>();


	public BetOptionConfig(){
		LoadData();
	}

	private void LoadData(){
		_sheet = GameConfig.Instance.LoadExcelAsset<BetOptionSheet>(Name);
		_dict.Clear();
		Init();
	}

	public static void Reload(){
		Debug.Log("Reload BetOptions Config");
		BetOptionConfig.Instance.LoadData();
	}

	private void Init(){
		ListUtility.ForEach(_sheet.DataArray, (BetOptionData data)=>{
			BetMachineInfo info = new BetMachineInfo(data.Key, data.Options, data.Machines);
			_dict[data.Key] = info;
		});
	}

	public ulong[] GetMachineBetOptions(string machine){
		foreach(var pair in _dict){
			if (pair.Value._machienNames.Contains(machine)){
				List<ulong> ret = ListUtility.MapList<int,ulong>(pair.Value._options, (int i)=>{
					 return (ulong)i; 
				});
				return ret.ToArray();
			}
		}
		// 默认的type options
		if (_dict.ContainsKey(_defaultTypeName)){
			List<ulong> ret = ListUtility.MapList<int,ulong>(_dict[_defaultTypeName]._options, (int i)=>{
					return (ulong)i; 
			});
			return ret.ToArray();
		}

		return new ulong[0];
	}
}
