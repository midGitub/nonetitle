using System.Collections;
using System.Collections.Generic;
using System;
using CitrusFramework;

public class JackpotWinManager : SimpleSingleton<JackpotWinManager>
{
	private CoreMachine _machine;
	private MachineConfig _machineConfig;
	private CoreConfig _coreConfig;
	private JackpotSettingConfig _jackpotConfig;
	private float _respinPointSum = 0.0f;
	private float _deltaPoint = 0.0f;

	public void SetCoreMachine(CoreMachine machine){
		_machine = machine;
		_machineConfig = machine.MachineConfig;
		_coreConfig = CoreConfig.Instance;
		_jackpotConfig = _coreConfig.JackpotSettingConfig;
		_respinPointSum = 0.0f;
		_deltaPoint = 0.0f;
	}

	public void CheckAddJackpotPoint(bool isRespin, WinType winType, CoreSpinResult result){
		if (isRespin) {
			AddRespinPointSum (result.BetAmount);
		} else {
			if (winType != WinType.None) {
				AddUserJackpotPoint (result.BetAmount);
			}
			ResetRespinPointSum ();
		}
	}

	public void AddUserJackpotPoint(ulong betAmount)
	{
		float addPoint = (float)Math.Sqrt((double)betAmount) * _coreConfig.JackpotSettingConfig.PointIncreaseFactor;
		addPoint += _respinPointSum;
		_deltaPoint = addPoint;
		UserDeviceLocalData.Instance.JackpotPoint += addPoint;
	}

	public void ResetRespinPointSum(){
		_respinPointSum = 0.0f;
	}

	public void AddRespinPointSum(ulong betAmount){
		float addPoint = (float)Math.Sqrt((double)betAmount) * _coreConfig.JackpotSettingConfig.PointIncreaseFactor;
		_respinPointSum += addPoint;
	}

	public float GetJackpotWinRatio(JackpotPoolType type){
		if (UserDeviceLocalData.Instance.JackpotPoint <= 0.0f) {
			return 0.0f;
		} else {
			return CalculateJackpotWinRatio (type);
		}
	}

	private float CalculateJackpotWinRatio(JackpotPoolType type){
		float threshold = 0.0f;
		if (type == JackpotPoolType.Single) {
			threshold = _jackpotConfig.SingleJackpotPointThreshold;
		} else if (type == JackpotPoolType.Colossal) {
			threshold = _jackpotConfig.FourJackpotColossalPointThreshold;
		} else if (type == JackpotPoolType.Mega) {
			threshold = _jackpotConfig.FourJackpotMegaPointThreshold;
		} else if (type == JackpotPoolType.Huge) {
			threshold = _jackpotConfig.FourJackpotHugePointThreshold;
		} else if (type == JackpotPoolType.Big) {
			threshold = _jackpotConfig.FourJackpotBigPointThreshold;
		}

		return JackpotHelper.GetJackpotWinRatio (
			_jackpotConfig.WinCalculateFactor,
			_deltaPoint,
			UserDeviceLocalData.Instance.JackpotPoint,
			threshold
		);
	}

	public bool IsJackpotMachineSinglePool(string name){
		return ListUtility.IsAnyElementSatisfied (CoreDefine.singleJackpotMachines, (string s) => {
			return name.Equals(s);
		});
	}

	public bool IsJackpotMachineFourPool(string name){
		return ListUtility.IsAnyElementSatisfied (CoreDefine.fourJackpotMachines, (string s) => {
			return name.Equals(s);
		});
	}

	public bool ShouldCheckWinRatio(string name){
		// four pool
		if (IsJackpotMachineFourPool(name)) {
			for (int i = (int)JackpotPoolType.Colossal; i < (int)JackpotPoolType.Max; ++i) {
				JackpotBonusPool pool = JackpotManager.Instance.GetJackpotBonusPool (JackpotType.FourJackpot, (JackpotPoolType)i, name);
				if (pool != null) {
					ulong currentBonus = pool.CurrentBonus;
					ulong nextBonus = pool.NextBonus;
					if (currentBonus > nextBonus * _jackpotConfig.StartWinFactor) {
						return true;
					}
				}
			}
		} 
		else if (IsJackpotMachineSinglePool(name)) { // single pool
			JackpotBonusPool singlePool = JackpotManager.Instance.GetJackpotBonusPool (JackpotType.Single, JackpotPoolType.None, name);
			if (singlePool != null) {
				ulong currentBonus = singlePool.CurrentBonus;
				ulong nextBonus = singlePool.NextBonus;
				if (currentBonus > nextBonus * _jackpotConfig.StartWinFactor) {
					return true;
				}
			}
		}

		return false;
	}

	public string CheckJackpotWinType(float ratio, string name){
		if (IsJackpotMachineFourPool (name)) {
			float colossalRatio = CalculateJackpotWinRatio (JackpotPoolType.Colossal);
			float megaRatio = CalculateJackpotWinRatio (JackpotPoolType.Mega);
			float hugeRatio = CalculateJackpotWinRatio (JackpotPoolType.Huge);
			float bigRatio = CalculateJackpotWinRatio (JackpotPoolType.Big);

			if (ratio < colossalRatio) {
				return "Colossal";
			} else if (ratio < megaRatio) {
				return "Mega";
			} else if (ratio < hugeRatio) {
				return "Huge";
			} else if (ratio < bigRatio) {
				return "Big";
			}
		} else if (IsJackpotMachineSinglePool (name)) {
			float singleRatio = CalculateJackpotWinRatio (JackpotPoolType.Single);
			if (ratio < singleRatio) {
				return "Single";
			}
		}
		return "";
	}


	public bool CheckJackpotWin(IRandomGenerator roller, ref string jackpotType, string name, CoreSpinInput spinInput){
		bool result = false;
		if (_machineConfig.BasicConfig.IsJackpot && ShouldCheckWinRatio(name)){
			float ratio = roller.NextFloat();
			jackpotType = CheckJackpotWinType (ratio, name);
			// 需要判断最低jackpot bet
			bool isSatisfiedLeastBet = spinInput.BetAmount >= _machineConfig.BasicConfig.JackpotMinBet;
			if (!string.IsNullOrEmpty (jackpotType) && isSatisfiedLeastBet) {
				result = true;
			}
		}
		return result;
	}

	public int GetJackpotPayoutData(string jackpotType, PayoutData[] datas, CoreSpinInput spinInput){
		int result = -1;
		if (_machineConfig.BasicConfig.IsJackpot) {
			// 需要判断最低jackpot bet
			bool isSatisfiedLeastBet = spinInput.BetAmount >= _machineConfig.BasicConfig.JackpotMinBet;
			if (!string.IsNullOrEmpty (jackpotType) && isSatisfiedLeastBet) {
				for (int i = 0; i < datas.Length; ++i) {
					PayoutData data = datas[i];
					if (jackpotType.Equals (data.JackpotType)) {
						result = i;
						break;
					}
				}
			}
		}

		return result;
	}

	public int GetJackpotMultilineData(IRandomGenerator generator){
		int result = -1;
		if (_machineConfig.BasicConfig.IsJackpot) {
			int[] indexes = _machineConfig.BasicConfig.JackpotPayoutIndexes;
			result = RandomUtility.RollSingleElement(generator, indexes) - 1;
		}

		return result;
	}

	public void RefreshJackpotPoint(JackpotPoolType type){
		float score = 0.0f;
		if (type == JackpotPoolType.Colossal)
			score = _jackpotConfig.FourJackpotColossalPointThreshold;
		else if (type == JackpotPoolType.Mega)
			score = _jackpotConfig.FourJackpotMegaPointThreshold;
		else if (type == JackpotPoolType.Huge)
			score = _jackpotConfig.FourJackpotHugePointThreshold;
		else if (type == JackpotPoolType.Big)
			score = _jackpotConfig.FourJackpotBigPointThreshold;
		else if (type == JackpotPoolType.Single)
			score = _jackpotConfig.SingleJackpotPointThreshold;

		UserDeviceLocalData.Instance.JackpotPoint -= score;
	}

	public void RefreshWinAmount(CoreSpinResult result, JackpotPoolType type, JackpotType jackpotType, string machineName){
		JackpotBonusPool pool = JackpotManager.Instance.GetJackpotBonusPool (jackpotType, type, machineName);
		result.RefreshJackpotWinAmount ((ulong)pool.CurrentBonus);
		pool.ResetBonus ();
	}

	public void RefreshLucky(CoreSpinResult result){
		float add = (float)result.WinAmount * _jackpotConfig.WinLuckyFactor;
		_machine.LuckyManager.LongLuckyManager.AddLongLucky ((int)add);
	}

	public void RefreshJackpotWin(GameData gamedata, CoreSpinResult result){
		if (result.IsJackpotWin) {
			JackpotType jackpotType = JackpotManager.Instance.GetJackpotType (result.CoreMachine.Name);
			JackpotPoolType poolType;

			if (jackpotType == JackpotType.Single) {
				poolType =  JackpotPoolType.Single;
				RefreshJackpotPoint (poolType);
				RefreshWinAmount (result, poolType, JackpotType.Single, _machine.Name);
			} else if (jackpotType == JackpotType.FourJackpot) {
				poolType = JackpotDefine.GetJackpotPoolType (result.PayoutData.JackpotType);
				RefreshJackpotPoint (poolType);
				RefreshWinAmount (result, poolType, JackpotType.FourJackpot, _machine.Name);
			}

			RefreshLucky (result);
			gamedata.AddWinAmount(result.WinAmount);
		}
	}
}

