using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using System;

public class MachineInfo
{
	private static readonly string _betCollectNumDictTag = "betCollectNumDict";
	private static readonly string _spinCountTag = "spinCount";
	private static readonly string _smallGameSpinCountTag = "smallGameSpinCount";
    private static readonly string _firstTimeFlagTag = "firstTimeFlag";

	private Dictionary<ulong, int> _betCollectNumDict = new Dictionary<ulong, int> ();
	private int _spinCount;
	private int _smallGameSpinCount;
    private bool _firstTimeFlag = true;

	public Dictionary<ulong, int> BetCollectNumDict{
		get { return _betCollectNumDict; }
		set { _betCollectNumDict = value; }
	}
	public int SpinCount { get { return _spinCount; } set { _spinCount = value; } }
	public int SmallGameSpinCount { get { return _smallGameSpinCount; } set { _smallGameSpinCount = value; } }
    public bool FirstTimeFlag { get { return _firstTimeFlag; } set { _firstTimeFlag = value; } }

	public MachineInfo(){
	}

	public void IncreaseSpinCount(SmallGameState state)
	{
		if(state == SmallGameState.None)
			++_spinCount;
		else
			++_smallGameSpinCount;
	}

	public void SetBetCollectDict(Dictionary<ulong, int> dict){
		foreach (KeyValuePair<ulong, int> betPair in dict) {
			_betCollectNumDict.Add (betPair.Key, betPair.Value);
		}
	}

	public static string Serialize(MachineInfo info)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();

		//1 betCollectNumDict
		Dictionary<string, string> betCollectNumStrDict = new Dictionary<string, string> ();

		foreach(var pair in info.BetCollectNumDict)
			betCollectNumStrDict.Add(pair.Key.ToString(), pair.Value.ToString());

		dict[_betCollectNumDictTag] = Json.Serialize(betCollectNumStrDict);

		//2 spinCount
		dict[_spinCountTag] = info.SpinCount;
		dict[_smallGameSpinCountTag] = info.SmallGameSpinCount;

        dict[_firstTimeFlagTag] = info.FirstTimeFlag;

		string result = Json.Serialize(dict);
		return result;
	}

	public static MachineInfo Deserialize(string s)
	{
		MachineInfo result = null;
		Dictionary<string, object> dict = Json.Deserialize(s) as Dictionary<string, object>;
		result = new MachineInfo();

		//1 betCollectNumDict
		if(dict.ContainsKey(_betCollectNumDictTag))
		{
			string betCollectNumDictStr = dict[_betCollectNumDictTag] as string;
			Dictionary<string, object> betCollectNumStrDict = Json.Deserialize(betCollectNumDictStr) as Dictionary<string, object>;
			foreach(var p in betCollectNumStrDict)
			{
				ulong k = Convert.ToUInt64(p.Key);
				int v = Convert.ToInt32(p.Value);
				result.BetCollectNumDict[k] = v;
			}
		}

		//2 spinCount
		if(dict.ContainsKey(_spinCountTag))
		{
			result.SpinCount = Convert.ToInt32( dict[_spinCountTag]);
		}

		//3 smallGameSpinCount
		if(dict.ContainsKey(_smallGameSpinCountTag))
		{
			result.SmallGameSpinCount = Convert.ToInt32(dict[_smallGameSpinCountTag]);
		}

        //4 firstTimeFlag
        if (dict.ContainsKey(_firstTimeFlagTag))
        {
            result.FirstTimeFlag = Convert.ToBoolean(dict[_firstTimeFlagTag]);
        }

		return result;
	}
}

public partial class UserMachineData : UserDataBase
{
	static public string Name = "UserMachineData";

	static private UserMachineData _instance = null;
	static public UserMachineData Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new UserMachineData();
				_instance.Init();
			}
			return _instance;
		}
	}

	static private string _machineSeedDictTag = "MachineSeedDict";
	static private string _totalSpinCountTag = "TotalSpinCount";
	static private string _machineInfoTag = "MachineInfo";
	static private string _machineUnlockTag = "MachineUnlock";

	private Dictionary<string, uint> _machineSeedDict = new Dictionary<string, uint>();
	private int _totalSpinCount;
	private Dictionary<string, MachineInfo> _machineInfoDict = new Dictionary<string, MachineInfo> ();
	private Dictionary<string, bool> _machineUnlockDict = new Dictionary<string, bool>();

	public Dictionary<string, uint> MachineSeedDict
	{
		get { return _machineSeedDict; }
		set { _machineSeedDict = value; }
	}
	public int TotalSpinCount
	{
		get { return _totalSpinCount; }
		set { _totalSpinCount = value; }
	}
	public Dictionary<string, MachineInfo> MachineInfoDict{
		get { return _machineInfoDict; }
		set { _machineInfoDict = value; }
	}

	public Dictionary<string, bool> MachineUnlockDict{
		get { return _machineUnlockDict; }
		set { _machineUnlockDict = value; }
	}

	#region Init

	protected UserMachineData()
	{
	}

	private void Init()
	{
		InitMachineUnlock ();
	}

	protected override string GetFileName()
	{
		#if CORE_DLL
		return Name;
		#else
		return UserDataFileController.GetUserDataFileName(Name, this);
		#endif
	}

	#endregion

	#region Read Write

	public override void Save()
	{
		base.Save();

		CoreDebugUtility.Log("### UserMachineData.Save() ###");
	}

	private void ReadCore(ES2Reader reader)
	{
		if(IsTagExist(_machineSeedDictTag))
			_machineSeedDict = reader.ReadDictionary<string, uint>(_machineSeedDictTag);
		else
			_machineSeedDict = new Dictionary<string, uint>();

		if(IsTagExist(_totalSpinCountTag))
			_totalSpinCount = reader.Read<int>(_totalSpinCountTag);
		else
			_totalSpinCount = 0;

		_machineInfoDict = new Dictionary<string, MachineInfo> ();
		if (IsTagExist (_machineInfoTag)) {
			Dictionary<string, string> infoStrDict = reader.ReadDictionary<string, string> (_machineInfoTag);
			foreach(var pair in infoStrDict)
			{
				MachineInfo info = MachineInfo.Deserialize(pair.Value);
				_machineInfoDict[pair.Key] = info;
			}
		}
		else {
			CopyOldBetCollectNumDict ();
		}

		if(IsTagExist(_machineUnlockTag))
			_machineUnlockDict = reader.ReadDictionary<string, bool>(_machineUnlockTag);
		else
			_machineUnlockDict = new Dictionary<string, bool>();
	}

	private void WriteCore(ES2Writer writer)
	{
		writer.Write<string, uint>(_machineSeedDict, _machineSeedDictTag);
		writer.Write<int>(_totalSpinCount, _totalSpinCountTag);
		Dictionary<string, string> infoStrDict = new Dictionary<string, string>();
		foreach(var pair in _machineInfoDict)
		{
			string jsonStr = MachineInfo.Serialize(pair.Value);
			infoStrDict.Add(pair.Key, jsonStr);
		}
		writer.Write<string, string> (infoStrDict, _machineInfoTag);
		writer.Write<string, bool> (_machineUnlockDict, _machineUnlockTag);
	}

	#endregion

	public void SaveMachineSeed(string machineName, uint seed)
	{
		_machineSeedDict[machineName] = seed;
		//don't save to file here, since this function is called lots of times in one spin round
		//Save();
	}

	public void IncreaseSpinCount(string machineName, bool save, SmallGameState state)
	{
		++_totalSpinCount;
				
		LazyInitMachineInfo(machineName);

		_machineInfoDict[machineName].IncreaseSpinCount(state);

		if(save)
			Save();
	}

	// 旧数据拷贝
	public void CopyOldBetCollectNumDict(){
		Dictionary<ulong, int> betDict = UserBasicData.Instance.BetCollectNumDictM6;
		if(betDict != null && betDict.Count > 0)
		{
			MachineInfo info = new MachineInfo ();
			info.SetBetCollectDict (betDict);
			_machineInfoDict.Add ("M6", info);
			UserBasicData.Instance.BetCollectNumDictM6.Clear();
		}
	}

	public int GetBetCollectNum(string machine, ulong betAmount){
		MachineInfo info = null;
		if(_machineInfoDict.ContainsKey(machine))
			info = _machineInfoDict [machine];
		if (info != null) {
			if (info.BetCollectNumDict.ContainsKey (betAmount)) {
				return info.BetCollectNumDict [betAmount];
			} else {
				CoreDebugUtility.Log ("GetBetCollectNum key is invalid " + betAmount);
				return 0;
			}
		} else {
			CoreDebugUtility.Log ("_machineInfoDict key is invalid "+machine);
			return 0;
		}
	}

	public void SetBetCollectNum(string machine, ulong betAmount, int count){
		LazyInitMachineInfo(machine);

		MachineInfo info = _machineInfoDict [machine];
		info.BetCollectNumDict [betAmount] = count;
	}

	#if UNITY_EDITOR
	//this function is only used for machine test
	public void ClearBetCollectNumDict(string machine)
	{
		if(_machineInfoDict.ContainsKey(machine))
		{
			MachineInfo info = _machineInfoDict[machine];
			info.BetCollectNumDict.Clear();

			Save();
		}
	}
	#endif

	public int GetSpinCountOfMachine(string machineName, SmallGameState smallGameState)
	{
		int result = 0;
		if(_machineInfoDict.ContainsKey(machineName))
		{
			if(smallGameState == SmallGameState.None)
				result = _machineInfoDict[machineName].SpinCount;
			else
				result = _machineInfoDict[machineName].SmallGameSpinCount;
		}
		return result;
	}

	private void LazyInitMachineInfo(string machineName)
	{
		if(!_machineInfoDict.ContainsKey(machineName))
			_machineInfoDict[machineName] = new MachineInfo();
	}

	public bool IsMachineUnlock(string machine){
		bool result = false;
		if (_machineUnlockDict.ContainsKey (machine)) {
			result = _machineUnlockDict [machine];
		}
		return result;
	}

	public void SetMachineUnlock(string machine, bool unlock, bool dirty){
		_machineUnlockDict [machine] = unlock;

		if (dirty) {
			Save ();
		}
	}

	public void InitMachineUnlock(){
		for (int i = 0; i < CoreDefine.AllMachineNames.Length; ++i) {
			string name = CoreDefine.AllMachineNames [i];
			bool isUnlock = MachineUnlockHelper.CheckMachineUnlock(name);
			bool dirty = false;
			if (i == CoreDefine.AllMachineNames.Length - 1) {
				dirty = true;
			}
			SetMachineUnlock(name, isUnlock, dirty);
		}
	}
}
