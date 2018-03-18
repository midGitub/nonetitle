using UnityEngine;
using System.Collections;

public static class MachineTestUtility
{
	public static BaseJoyConfig GetBaseJoyConfig(MachineConfig machineConfig, MachineTestLuckyMode mode, SpinResultType resultType)
	{
		Debug.Assert(!machineConfig.BasicConfig.IsMultiLine);

		BaseJoyConfig result = null;
		if(mode == MachineTestLuckyMode.Normal)
		{
			if(resultType == SpinResultType.Win)
				result = machineConfig.PayoutConfig;
			else if(resultType == SpinResultType.NearHit)
				result = machineConfig.NearHitConfig;
			else
				Debug.Assert(false);
		}
		else if(mode == MachineTestLuckyMode.Lucky)
		{
			if(resultType == SpinResultType.Win)
				result = machineConfig.LuckyPayoutConfig;
			else if(resultType == SpinResultType.NearHit)
				result = machineConfig.LuckyNearHitConfig;
			else
				Debug.Assert(false);
		}
		else
		{
			Debug.Assert(false);
		}
		return result;
	}

	public static BaseJoyDistConfig GetBaseJoyDistConfig(MachineConfig machineConfig, MachineTestLuckyMode mode, SpinResultType resultType)
	{
		Debug.Assert(machineConfig.BasicConfig.IsMultiLine);

		BaseJoyDistConfig result = null;
		if(mode == MachineTestLuckyMode.Normal)
		{
			if(resultType == SpinResultType.Win)
				result = machineConfig.PayoutDistConfig;
			else if(resultType == SpinResultType.NearHit)
				result = machineConfig.NearHitDistConfig;
			else
				Debug.Assert(false);
		}
		else if(mode == MachineTestLuckyMode.Lucky)
		{
			if(resultType == SpinResultType.Win)
				result = machineConfig.LuckyPayoutDistConfig;
			else if(resultType == SpinResultType.NearHit)
				result = machineConfig.LuckyNearHitDistConfig;
			else
				Debug.Assert(false);
		}
		else
		{
			Debug.Assert(false);
		}
		return result;
	}

	public static int GetJoyRowCount(MachineConfig machineConfig, MachineTestLuckyMode mode, SpinResultType resultType)
	{
		int result = 0;
		if(machineConfig.BasicConfig.IsMultiLine)
		{
			if(machineConfig.BasicConfig.IsMultiLineExhaustive)
			{
				BaseJoyDistConfig config = GetBaseJoyDistConfig(machineConfig, mode, resultType);
				result = config.JoyDataArray.Length;
			}
		}
		else
		{
			BaseJoyConfig config = GetBaseJoyConfig(machineConfig, mode, resultType);
			result = config.JoyDataArray.Length;
		}
		return result;
	}

	public static float GetJoyOverallHit(MachineConfig machineConfig, MachineTestLuckyMode mode, SpinResultType resultType, int index)
	{
		float result = 0.0f;
		if(machineConfig.BasicConfig.IsMultiLine)
		{
			if(machineConfig.BasicConfig.IsMultiLineExhaustive)
			{
				BaseJoyDistConfig config = GetBaseJoyDistConfig(machineConfig, mode, resultType);
				result = config.OverallHitArray[index];
			}
		}
		else
		{
			BaseJoyConfig config = GetBaseJoyConfig(machineConfig, mode, resultType);
			result = config.OverallHitArray[index];
		}
		return result;
	}

	public static float GetJoyTotalProb(MachineConfig machineConfig, MachineTestLuckyMode mode, SpinResultType resultType)
	{
		float result = 0.0f;
		if(machineConfig.BasicConfig.IsMultiLine)
		{
			if(machineConfig.BasicConfig.IsMultiLineExhaustive)
			{
				BaseJoyDistConfig config = GetBaseJoyDistConfig(machineConfig, mode, resultType);
				result = config.TotalProb;
			}
		}
		else
		{
			BaseJoyConfig config = GetBaseJoyConfig(machineConfig, mode, resultType);
			result = config.TotalProb;
		}
		return result;
	}
}

