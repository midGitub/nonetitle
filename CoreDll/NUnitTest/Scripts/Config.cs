using System.Collections;
using System.Collections.Generic;
using System;
using NUnit.Framework;

public class UserConfig
{
	public string _userName;
	public int _initLucky;
	public int _initCredits;
	public int _spinCount;
	public int _betAmount;
	public bool _isRandomSeed;
	public uint _startSeed;
}

public class CaseConfig
{
	public string _machineName;
	public string _userName;
}

public class Config
{
	public List<string> _machineNames;
	public Dictionary<string, UserConfig> _userConfigDict;
	public List<CaseConfig> _caseConfigs;
}

public static class ConfigHelper
{
	public static Config GetConfig()
	{
		string jsonStr = FileUtility.ReadFile("../../Config/config.json");
		JSONObject jsonObject = new JSONObject(jsonStr);

		Config config = new Config();

		List<JSONObject> machineNameObjects = jsonObject.GetField("MachineNames").list;
		List<string> machineNames = ListUtility.MapList(machineNameObjects, (JSONObject o) => {
			return o.str;
		});
		config._machineNames = machineNames;

		Dictionary<string, UserConfig> userConfigDict = new Dictionary<string, UserConfig>();
		JSONObject userObjectDict = jsonObject.GetField("Users");
		foreach(string key in userObjectDict.keys)
		{
			JSONObject obj = userObjectDict.GetField(key);
			int initLucky = (int)obj.GetField("InitLucky").n;
			int initCredits = (int)obj.GetField("InitCredits").n;
			int spinCount = (int)obj.GetField("SpinCount").n;
			int betAmount = (int)obj.GetField("BetAmount").n;
			bool isRandomSeed = obj.GetField("IsRandomSeed").b;
			uint startSeed = (uint)obj.GetField("StartSeed").n;

			UserConfig userConfig = new UserConfig {
				_userName = key, 
				_initLucky = initLucky,
				_initCredits = initCredits,
				_spinCount = spinCount,
				_betAmount = betAmount,
				_isRandomSeed = isRandomSeed,
				_startSeed = startSeed
			};
			userConfigDict.Add(key, userConfig);
		}
		config._userConfigDict = userConfigDict;

		List<CaseConfig> caseConfigs = new List<CaseConfig>();
		List<JSONObject> caseObjects = jsonObject.GetField("Cases").list;
		foreach(JSONObject obj in caseObjects)
		{
			string machineName = obj.GetField("MachineName").str;
			string userName = obj.GetField("UserName").str;
			CaseConfig caseConfig = new CaseConfig {
				_machineName = machineName,
				_userName = userName
			};
			caseConfigs.Add(caseConfig);
		}
		config._caseConfigs = caseConfigs;

		return config;
	}
}

