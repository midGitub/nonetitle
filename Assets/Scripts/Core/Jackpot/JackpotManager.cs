using System.Collections;
using System.Collections.Generic;
using System;

public class JackpotManager : SimpleSingleton<JackpotManager> {
	private List<string> _jackpotNames = new List<string> ();
	private Dictionary<string, JackpotBonusPoolManager> _dict = new Dictionary<string, JackpotBonusPoolManager>();

	public void Init(Action<string, ulong> func){
		_jackpotNames.AddRange(ListUtility.CreateList (CoreDefine.singleJackpotMachines, CoreDefine.singleJackpotMachines.Length));
		_jackpotNames.AddRange(ListUtility.CreateList (CoreDefine.fourJackpotMachines, CoreDefine.fourJackpotMachines.Length));

		ListUtility.ForEach (_jackpotNames, (string name)=>{
			JackpotBonusPoolManager manager = JackpotBonusPoolManagerFactory.CreateManager(name);
			if (manager != null){
				manager.Init(func);
				_dict.Add(name, manager);
			}
		});
	}

	public JackpotBonusPoolManager GetManager(string name){
		if (_dict.ContainsKey (name)) {
			return _dict [name];
		}

		return null;
	}

	public JackpotType GetJackpotType(string jackpot){
		if (_dict.ContainsKey (jackpot)) {
			if (_dict [jackpot] != null) {
				return _dict [jackpot].JackpotType;
			}
		}

		return JackpotType.FourJackpot;
	}

	public JackpotBonusPool GetJackpotBonusPool(JackpotType type, JackpotPoolType poolType, string jackpotName){
		if (_dict.ContainsKey (jackpotName)) {
			if (_dict [jackpotName] != null) {
				return _dict [jackpotName].GetJackpotBonusPool (type, poolType);
			}
		}

		return null;
	}

	public void RefreshJackpotPools(){
		foreach (var pool in _dict) {
			if (pool.Value != null) {
				pool.Value.RefreshJackpotPools ();
			}
		}
	}

	public void SaveExitTimeAndJackpotData(){
		foreach (var pool in _dict) {
			if (pool.Value != null) {
				pool.Value.SaveExitTimeAndJackpotData ();
			}
		}
	}

}
