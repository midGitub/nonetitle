using System.Collections;
using System.Collections.Generic;

public partial class UserBasicData : UserDataBase
{
	static public string Name = "UserBasicData";
	//pay protection
	static private string _payProtectionEnableTag = "PayProtection";

	// jackpotdata数据分隔符, 下面2个要对应
	private char[] _jackpotSeperates = new char[]{'+'};
	private string _jackpotSeperateSymbol = "+";

	static private UserBasicData _instance = null;
	static public UserBasicData Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new UserBasicData();
				_instance.Init();
			}
			return _instance;
		}
	}

	static private string[] _jackpotTagArray = new string[(int)JackpotPoolType.Max] {
		"jackpot_single","jackpot_colossal","jackpot_mega","jackpot_huge","jackpot_big"
	};
	static private string _jackpotTag = "Jackpot";
	static private string _longLuckyTag = "LongLucky";
	static private string _shortLuckyTag = "ShortLucky";

	private JackpotData[] _jackpotDataArray = new JackpotData[(int)JackpotPoolType.Max];
	private Dictionary<string, JackpotData[]> _jackpotDict = new Dictionary<string, JackpotData[]> ();
	// 这个是实际存储的数据,需要反序列化为_jackptDict
	private Dictionary<string, string> _jackpotDictSerialize = new Dictionary<string, string> ();

	private int _longLucky;
	private int _shortLucky;
	// 收集玩法中不同下注对应的收集数量
	private Dictionary<ulong, int> _betCollectNumDictM6;
	private static string _betCollectNumDictM6Tag = "betCollectNumDictM6";

	public int LongLucky { get { return _longLucky; } }
	public int ShortLucky { get { return _shortLucky; } }
	public Dictionary<ulong, int> BetCollectNumDictM6
	{
		get { return _betCollectNumDictM6; }
	}

	//付费玩家的保护机制开关
	private bool _payProtectionEnable = false;

	public bool PayProtectionEnable { 
		get { 
			return _payProtectionEnable; 
		}
		set {
			_payProtectionEnable = value;
			Save ();
		}
	}

	#region Init

	protected UserBasicData()
	{
		CoreDebugUtility.Log("### UserBasicData constructed ###");
	}

	private void Init()
	{
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

		CoreDebugUtility.Log("### UserBasicData.Save() ###");
	}

	private void ReadCore(ES2Reader reader)
	{
		if(IsTagExist(_longLuckyTag))
			_longLucky = reader.Read<int>(_longLuckyTag);
		else
			_longLucky = CoreConfig.Instance.LuckyConfig.FistStartGameBonusLongLucky;

		bool isEnsure = EnsureCorrectLongLucky();
		if (isEnsure) {
			DisablePayProtection ();
		}

		if(IsTagExist(_shortLuckyTag))
			_shortLucky = reader.Read<int>(_shortLuckyTag);
		else
			_shortLucky = 0;

		_betCollectNumDictM6 = IsTagExist(_betCollectNumDictM6Tag)
			? reader.ReadDictionary<ulong, int>(_betCollectNumDictM6Tag)
			: new Dictionary<ulong, int>();

		_payProtectionEnable = ReadTag<bool> (reader, _payProtectionEnableTag, false);
	}

	private void WriteCore(ES2Writer writer)
	{
		writer.Write(_longLucky, _longLuckyTag);
		writer.Write(_shortLucky, _shortLuckyTag);
		writer.Write(_betCollectNumDictM6, _betCollectNumDictM6Tag);
		writer.Write<bool> (_payProtectionEnable, _payProtectionEnableTag);
	}

	#endregion

	public JackpotData GetJackpotData(JackpotPoolType type, string machineName)
	{
		if (_jackpotDict.ContainsKey (machineName)) {
			return _jackpotDict [machineName] [(int)type];
		}
		return _jackpotDataArray[(int)type];
	}

	public void SetJackpotData(JackpotPoolType type, JackpotData data, bool dirty = false, string machineName = "default")
	{
		LazyCreateJackpotDictArray (machineName);
		#if true
		_jackpotDict [machineName] [(int)type] = data;
		#else
		_jackpotDataArray[(int)type] = data;
		#endif
		if(dirty)
		{
			Save();
		}
	}

	public void SetLongLucky(int num, bool save)
	{
		_longLucky = num;
		//to patch the users who have the bug that longLucky < 0
		bool isEnsure = EnsureCorrectLongLucky();
		if (isEnsure) {
			DisablePayProtection ();
		}
		if(save)
			Save();
	}

	public void AddLongLucky(int num, bool save)
	{
		if(num < 0)
		{
			CoreDebugUtility.LogError("AddLongLucky parameter should > 0");
			CoreDebugUtility.Assert(false);
		}
		
		_longLucky += num;
		bool isEnsure = EnsureCorrectLongLucky();
		if (isEnsure) {
			DisablePayProtection ();
		}
		if(save)
			Save();
	}

	public void SubtractLongLucky(int num, bool save)
	{
		if(num < 0)
		{
			CoreDebugUtility.LogError("SubtractLongLucky parameter should > 0");
			CoreDebugUtility.Assert(false);
		}
		
		_longLucky -= num;
		bool isEnsure = EnsureCorrectLongLucky();
		if (isEnsure) {
			DisablePayProtection ();
		}
		if(save)
			Save();
	}

	private bool EnsureCorrectLongLucky()
	{
		if (_longLucky <= 0) {
			_longLucky = 0;
			return true;
		}
		return false;
	}

	public void CleanLongLucky()
	{
		_longLucky = 0;
		Save();
	}

	public void AddShortLucky(int num, bool save)
	{
		_shortLucky += num;
		if(save)
			Save();
	}
	public void SetShortLucky(int num, bool save)
	{
		_shortLucky = num;
		if(save)
			Save();
	}

	public void SubtractShortLucky(int num, bool save)
	{
		_shortLucky -= num;
		if(_shortLucky < 0)
			_shortLucky = 0;
		if(save)
			Save();
	}

	public void ResetShortLucky(bool save)
	{
		_shortLucky = 0;
		if(save)
			Save();
	}

	#region pay protection 

	public void EnablePayProtection(){
		_payProtectionEnable = true;
		CoreDebugUtility.Log ("Pay Protection is enable");
	}

	public void DisablePayProtection(){
		_payProtectionEnable = false;
		CoreDebugUtility.Log ("Pay Protection is disable");
	}

	#endregion

	#region jackpot

	// 旧数据拷贝
	public void CopyFromOldJackpotData(){
		JackpotData[] _dataArray = new JackpotData[_jackpotDataArray.Length];
		for (int i = 0; i < _dataArray.Length; ++i) {
			_dataArray [i] = _jackpotDataArray [i];
		}
		_jackpotDict ["M9"] = _dataArray;
	}

	private void LazyCreateJackpotDictArray(string machineName){
		if (!_jackpotDict.ContainsKey (machineName)) {
			JackpotData[] array = new JackpotData[_jackpotDataArray.Length];
			_jackpotDict.Add (machineName, array);
		}
	}

	private JackpotData[] DeserializeJackpot(string s){
		string[] strArray = s.Split (_jackpotSeperates);
		JackpotData[] array = ListUtility.MapList(strArray, (string str)=>{
			return JackpotData.Deserialize(str);
		}).ToArray();

		return array;
	}

	private string SerializeJackpot(JackpotData[] array){
		string[] resultArray = ListUtility.MapList(array, (JackpotData data)=>{
			return JackpotData.Serialize(data);
		}).ToArray();

		string result = string.Join(_jackpotSeperateSymbol, resultArray);
//		CoreDebugUtility.Log ("SerializeJackpot result = " + result);
		return result;
	}

	private Dictionary<string, string> ConvertToJackpotDictSerialize(Dictionary<string, JackpotData[]> dict){
		Dictionary<string, string> returnDict = new Dictionary<string, string> ();
		foreach (var pair in dict) {
			returnDict [pair.Key] = SerializeJackpot (pair.Value);
		}
		return returnDict;
	}

	private Dictionary<string, JackpotData[]> ConvertToJackpotDict(Dictionary<string, string> dict){
		Dictionary<string, JackpotData[]> returnDict = new Dictionary<string, JackpotData[]> ();
		foreach (var pair in dict) {
			returnDict [pair.Key] = DeserializeJackpot (pair.Value);
		}
		return returnDict;
	}

	#endregion
}
