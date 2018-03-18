using System.Collections;
using System.Collections.Generic;

public class RewardResultConfig
{
	public static readonly string Name = "RewardResult";

	private RewardResultSheet _sheet;
	private MachineConfig _machineConfig; //ref

	public RewardResultSheet Sheet { get { return _sheet; } }

	public RewardResultConfig(RewardResultSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;
		_machineConfig = machineConfig;
	}

	public int[] GetStopIndexes(RewardResultData data)
	{
		return new int[]{ data.Reel1, data.Reel2, data.Reel3 };
	}
}
