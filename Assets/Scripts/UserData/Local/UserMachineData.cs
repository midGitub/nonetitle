using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System;

public class InitMachineSeedInfo
{
	public uint _seed;
	public bool _isFromServer;

	public InitMachineSeedInfo(uint seed, bool isFromServer)
	{
		_seed = seed;
		_isFromServer = isFromServer;
	}

	public static string Serialize(InitMachineSeedInfo info)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>() {
			{ "seed", info._seed },
			{ "isFromServer", info._isFromServer }
		};
		return Json.Serialize(dict);
	}

	public static InitMachineSeedInfo Deserialize(string s)
	{
		InitMachineSeedInfo result = null;
		Dictionary<string, object> dict = Json.Deserialize(s) as Dictionary<string, object>;
		if(dict.ContainsKey("seed") && dict.ContainsKey("isFromServer"))
		{
			uint seed = Convert.ToUInt32(dict["seed"]);
			bool isFromServer = Convert.ToBoolean(dict["isFromServer"]);
			result = new InitMachineSeedInfo(seed, isFromServer);
		}
		return result;
	}
}

public partial class UserMachineData
{
	static private string _initMachineSeedInfoDictTag = "InitMachineSeedInfoDict";
	static private string _currentMachineTag = "CurrentMachine";

	private Dictionary<string, InitMachineSeedInfo> _initMachineSeedInfoDict = new Dictionary<string, InitMachineSeedInfo>();
	private string _currentMachine;

	public string CurrentMachine { get { return _currentMachine; } set { _currentMachine = value; } }

	public Dictionary<string, InitMachineSeedInfo> InitMachineSeedInfoDict
	{
		get { return _initMachineSeedInfoDict; }
		set { _initMachineSeedInfoDict = value; }
	}

	public override void Reset()
	{
	}

	[Obsolete("Useless but left for possible future use")]
	private void SetInitMachineSeedToLocalRandom(string machineName)
	{
		Debug.Assert(!_machineSeedDict.ContainsKey(machineName) && !_initMachineSeedInfoDict.ContainsKey(machineName));

		System.Random r = new System.Random();
		uint seed = (uint)r.Next();

		InitMachineSeedInfo info = new InitMachineSeedInfo(seed, false);
		_initMachineSeedInfoDict[machineName] = info;

		_machineSeedDict[machineName] = seed;
	}

	[Obsolete("Useless but left for possible future use")]
	private void SetInitMachineSeedToFetchedSeed(string machineName, uint seed)
	{
		if(_initMachineSeedInfoDict.ContainsKey(machineName))
		{
			InitMachineSeedInfo info = _initMachineSeedInfoDict[machineName];
			Debug.Assert(!info._isFromServer);
			info._seed = seed;
			info._isFromServer = true;
		}
		else
		{
			InitMachineSeedInfo info = new InitMachineSeedInfo(seed, true);
			_initMachineSeedInfoDict[machineName] = info;
		}

		//don't forget to reset machine seed
		_machineSeedDict[machineName] = seed;
	}

	public override void Read(ES2Reader reader)
	{
		ReadCore(reader);

		if(IsTagExist(_initMachineSeedInfoDictTag))
			_initMachineSeedInfoDict = reader.ReadDictionary<string, InitMachineSeedInfo>(_initMachineSeedInfoDictTag);
		else
			_initMachineSeedInfoDict = new Dictionary<string, InitMachineSeedInfo>();

		if(IsTagExist(_currentMachineTag))
			_currentMachine = reader.Read<string>(_currentMachineTag);
		else
			_currentMachine = "";
	}

	public override void Write(ES2Writer writer)
	{
		WriteCore (writer);

		writer.Write<string, InitMachineSeedInfo>(_initMachineSeedInfoDict, _initMachineSeedInfoDictTag);
		writer.Write<string>(_currentMachine, _currentMachineTag);
	}

	// This function is a public interface before, and it's obsolete now.
	// But it might be used for future, so leave it here.
	private uint GetMachineSeedObsolete(string machineName)
	{
		if(!_machineSeedDict.ContainsKey(machineName))
			SetInitMachineSeedToLocalRandom(machineName);
		return _machineSeedDict[machineName];
	}

	public uint GetMachineSeed(string machineName)
	{
		if(!_machineSeedDict.ContainsKey(machineName))
		{
			uint seed = MachineSeedManager.Instance.GetRandomSeed(machineName);

			InitMachineSeedInfo info = new InitMachineSeedInfo(seed, false);
			_initMachineSeedInfoDict[machineName] = info;

			_machineSeedDict[machineName] = seed;
		}

		return _machineSeedDict[machineName];
	}

	#region Public methods

	public uint GetInitMachineSeed(string machineName)
	{
		uint result = 0;
		if(_initMachineSeedInfoDict.ContainsKey(machineName))
			result = _initMachineSeedInfoDict[machineName]._seed;
		return result;
	}

	public void SaveFetchedInitMachineSeedDict(Dictionary<string, uint> seedDict)
	{
		foreach(var pair in seedDict)
		{
			SetInitMachineSeedToFetchedSeed(pair.Key, pair.Value);
		}

		Save();
	}

    public bool GetIsFirstTimeFlag(string machineName)
    {
        if (_machineInfoDict.ContainsKey(machineName))
        {
            return _machineInfoDict[machineName].FirstTimeFlag;
        }
        else
        {
            return false;
        }
    }

    public void SetFirstTimeFlag(string machineName, bool flag)
    {
        if (_machineInfoDict.ContainsKey(machineName))
        {
            _machineInfoDict[machineName].FirstTimeFlag = flag;
            Save(); 
        }
    }

	#endregion
}
