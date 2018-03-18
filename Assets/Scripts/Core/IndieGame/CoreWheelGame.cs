using System.Collections;
using System.Collections.Generic;

public class CoreWheelGame {
	CoreMachine _machine;
	BasicConfig _basicConfig;
	IRandomGenerator _roller;
	WheelConfig[] _wheelConfigs;
	WheelData[] _wheelDatas;
	float _totalWinRatio;

	public CoreWheelGame(CoreMachine machine){
		_machine = machine;
		_basicConfig = machine.MachineConfig.BasicConfig;
		_roller = _machine.Roller;
		
		_totalWinRatio = 0.0f;
	}

	private WheelConfig[] InitWheelConfigs(CoreMachine machine){
		PayoutData triggerPayoutData = machine.PlayModule.TriggerPayoutData;
		WheelConfig[] result;
		if (triggerPayoutData != null && triggerPayoutData.WheelNames.Length > 0){
			result = new WheelConfig[triggerPayoutData.WheelNames.Length];
			for(int i = 0; i < triggerPayoutData.WheelNames.Length; i++)
			{
				result[i] = machine.MachineConfig.GetCurWheelConfig(machine.SpinResult.LuckyMode, triggerPayoutData.WheelNames[i]);
			}
		}
		else{
			result = new WheelConfig[0];
		}
		return result;
	}

	private WheelData[] InitWheelDatas(WheelConfig[] configs){
		WheelData[] results = ListUtility.MapList(configs, (WheelConfig config)=>{
			return WheelHelper.RandomCreateWheelData(config, _roller);
		}).ToArray();

		return results;
	}

	public void Reset(){
		_totalWinRatio = 0.0f;
	}

	public float FetchWinRatio(){
		float totalRatio = WheelHelper.GetTotalRatio(_wheelDatas);
		return totalRatio;
	}
	public ulong GetWinAmount(ulong betAmount){
		ulong winAmount = (ulong)(betAmount * _totalWinRatio);
		return winAmount;
	}

	#if UNITY_EDITOR
	public MachineTestIndieGameResult SimulateUserPlay(ulong betAmount){
		Reset();

		_wheelConfigs = InitWheelConfigs(_machine);
		_wheelDatas = InitWheelDatas(_wheelConfigs);

		MachineTestIndieGameResult result = new MachineTestIndieGameResult();
		float ratio = FetchWinRatio();
		result._winAmount = GetWinAmount(betAmount);
		result._customData = ratio.ToString();
		return result;
	}
	#endif


}
