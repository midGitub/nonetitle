using System.Collections;
using System.Collections.Generic;
using System;

public class JackpotBonusPoolManager  {
	public static readonly float LINEAR_DELTA_TIME = 15 * 60;
	//奖金奖池
	protected JackpotBonusPool[] _bonusPool = new JackpotBonusPool[(int)JackpotPoolType.Max];
	//奖池类型
	protected JackpotType _jackpotType;
	//上次中奖奖池索引
	protected int _lastTriggeBonusPoolIndex = -1;
	// 对应机台
	protected string _machineName = "M9";

	//奖池元数据
	protected JackpotData[] _jackpotPoolDataArray = new JackpotData[(int)JackpotPoolType.Max];

	public int LastTirggeBonusPoolIndex {
		get { return _lastTriggeBonusPoolIndex; }
	}

	public JackpotData[] JackpotPoolDataArray{
		get { return _jackpotPoolDataArray; }
		set { _jackpotPoolDataArray = value; }
	}

	public JackpotType JackpotType {
		get { return _jackpotType; }
		set { _jackpotType = value; }
	}

	public string MachineName {
		get { return _machineName; }
		set { _machineName = value; }
	}

	public JackpotBonusPoolManager(string name){
		_machineName = name;
		_jackpotType = JackpotType.FourJackpot;
	}

	public void Init(Action<string, ulong> func){
		// 0 is singlepool
		for (int i = 0; i < _bonusPool.Length; ++i) {
			_bonusPool[i] = JackpotPoolFactory.CreateBonusPool((JackpotPoolType)i);
			_bonusPool [i].SetMachineName (_machineName);
			_bonusPool [i].AddListener (func);
		}

		// 是否是重置数据还是线性增加
		#if CORE_DLL
		DateTime lastexit = DateTime.Now;
		DateTime now = DateTime.Now;
		#else
		DateTime lastexit = UserDeviceLocalData.Instance.LastExitTime;
		DateTime now = NetworkTimeHelper.Instance.GetNowTime();
		#endif

		TimeSpan span = now - lastexit;
		if (span.Seconds > LINEAR_DELTA_TIME || UserDeviceLocalData.Instance.IsNewGame) {
			CreateJackpotPoolDefault ();
			CoreDebugUtility.Log ("CreateJackpotPoolDefault");
		} else {
			CreateJackpotPoolLinear ((float)span.Seconds);
			CoreDebugUtility.Log ("CreateJackpotPoolLinear span.Seconds = "+span.Seconds);
		}
	}

	private void CreateJackpotPoolDefault(){
		if (_jackpotType == JackpotType.Single) {
			_bonusPool [(int)JackpotPoolType.Single].CreateDefaultBonus (-1);
		} else {
			int referenceBonus = -1;
			referenceBonus = _bonusPool [(int)JackpotPoolType.Colossal].CreateDefaultBonus (-1);
			for (int i = (int)JackpotPoolType.Mega; i < (int)JackpotPoolType.Max; ++i) {
				_bonusPool [i].CreateDefaultBonus (referenceBonus);
			}
		}
	}

	private void CreateJackpotPoolLinear(float deltaTime){
		CreateJackpotPoolDefault ();

		// calculate linear
		for(int i = 0; i < _bonusPool.Length; ++i){
			JackpotData data = UserBasicData.Instance.GetJackpotData ((JackpotPoolType)i, _machineName);
			_bonusPool[i].SetBonusLinear (data.CurrentBonus, data.NextBonus, deltaTime);
		}
	}

	private void SaveJackpotData(){
		UserBasicData.Instance.SetJackpotData (JackpotPoolType.Single, CreateJackpotData(JackpotPoolType.Single), false, _machineName);
		UserBasicData.Instance.SetJackpotData (JackpotPoolType.Colossal, CreateJackpotData(JackpotPoolType.Colossal), false, _machineName);
		UserBasicData.Instance.SetJackpotData (JackpotPoolType.Mega, CreateJackpotData(JackpotPoolType.Mega), false, _machineName);
		UserBasicData.Instance.SetJackpotData (JackpotPoolType.Huge, CreateJackpotData(JackpotPoolType.Huge), false, _machineName);
		UserBasicData.Instance.SetJackpotData (JackpotPoolType.Big, CreateJackpotData(JackpotPoolType.Big), false, _machineName);
	}

	private JackpotData CreateJackpotData(JackpotPoolType type){
		JackpotBonusPool pool = _bonusPool [(int)type];
		JackpotData data = new JackpotData ((int)pool.CurrentBonus, (int)pool.NextBonus);
		return data;
	}

	public void SetJackpotType(JackpotType type){
		_jackpotType = type;
	}

	public void RefreshJackpotPools(){
		if (_jackpotType == JackpotType.Single) {
			_bonusPool [0].RefreshBonus ();
		} else if (_jackpotType == JackpotType.FourJackpot) {
			for (int i = 1; i < _bonusPool.Length; ++i) {
				_bonusPool [i].RefreshBonus ();
			}
		}
	}

	public bool IsJackpotTriggeBonus(){
		for (int i = 0; i < _bonusPool.Length; ++i) {
			if (_bonusPool [i].IsTriggerBonus()) {
				_lastTriggeBonusPoolIndex = i;
				return true;
			}
		}
		return false;
	}

	public JackpotBonusPool GetJackpotBonusPool(JackpotType type, JackpotPoolType poolType = JackpotPoolType.None){
		if (type == JackpotType.Single) {
			return _bonusPool [(int)JackpotPoolType.Single];
		} else {
			return _bonusPool [(int)poolType];
		}
	}

	public void SaveExitTimeAndJackpotData(){
		#if !CORE_DLL
		UserDeviceLocalData.Instance.LastExitTime = NetworkTimeHelper.Instance.GetNowTime();
		#endif

		SaveJackpotData();
	}
}
