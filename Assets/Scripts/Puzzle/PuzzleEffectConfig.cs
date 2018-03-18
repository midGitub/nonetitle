using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PuzzleEffectConfig
{
	public static string MachineEffectPath = "Effect/Prefab/";

	//ReelLight
	public static string FX_Machine_Idle_ReelLight_Format = "FX_{0}_Idle_ReelLight";
	public static string FX_Machine_Spin_ReelLight_Format = "FX_{0}_Spin_ReelLight";
	public static string FX_Machine_Spin_ReelLightEnd_Format = "FX_{0}_Spin_ReelLightEnd";

	//ReelSideLight
	public static string FX_Machine_Idle_ReelSideLight_Format = "FX_{0}_Idle_ReelSideLight";
	public static string FX_Machine_Spin_ReelSideLight_Format = "FX_{0}_Spin_ReelSideLight";
	public static string FX_Machine_Spin_ReelSideLightEnd_Format = "FX_{0}_Spin_ReelSideLightEnd";
	public static string FX_Machine_Respin_ReelSideLight_Format = "FX_{0}_Respin_ReelSideLight";
	public static string FX_Machine_NormalWin_ReelSideLight_Format = "FX_{0}_LowWin_ReelSideLight";

	public static string FX_Machine_BigWin_ReelSurroundings_Format = "FX_{0}_HighWin_ReelSurroundings";

	public static string FX_Machine_LowWin_FrameLight_Format = "FX_{0}_LowWin_FrameLight";
	public static string FX_Machine_HighWin_FrameLight_Format = "FX_{0}_HighWin_FrameLight";
	public static string FX_Machine_Hype_FrameLight_Format = "FX_{0}_Hype_FrameLight";

	public static string FX_Machine_Symbol_Up_Format = "FX_{0}_SymbolUp_Reel";
	public static string FX_Machine_Symbol_Down_Format = "FX_{0}_SymbolDown_Reel";

	public static string Get_FX_Machine_Idle_ReelLight(string machineName)
	{
		return string.Format(FX_Machine_Idle_ReelLight_Format, machineName);
	}

	public static string Get_FX_Machine_Spin_ReelLight(string machineName)
	{
		return string.Format(FX_Machine_Spin_ReelLight_Format, machineName);
	}

	public static string Get_FX_Machine_Spin_ReelLightEnd(string machineName)
	{
		return string.Format(FX_Machine_Spin_ReelLightEnd_Format, machineName);
	}

	public static string Get_FX_Machine_Idle_ReelSideLight(string machineName)
	{
		return string.Format(FX_Machine_Idle_ReelSideLight_Format, machineName);
	}

	public static string Get_FX_Machine_Spin_ReelSideLight(string machineName)
	{
		return string.Format(FX_Machine_Spin_ReelSideLight_Format, machineName);
	}

	public static string Get_FX_Machine_Spin_ReelSideLightEnd(string machineName)
	{
		return string.Format(FX_Machine_Spin_ReelSideLightEnd_Format, machineName);
	}

	public static string Get_FX_Machine_Respin_ReelSideLight(string machineName)
	{
		return string.Format(FX_Machine_Respin_ReelSideLight_Format, machineName);
	}

	public static string Get_FX_Machine_NormalWin_ReelSideLight(string machineName)
	{
		return string.Format(FX_Machine_NormalWin_ReelSideLight_Format, machineName);
	}

	public static string Get_FX_Machine_BigWin_ReelSurroundings(string machineName)
	{
		return string.Format(FX_Machine_BigWin_ReelSurroundings_Format, machineName);
	}

	public static string Get_FX_Machine_LowWin_FrameLight(string machineName)
	{
		return string.Format(FX_Machine_LowWin_FrameLight_Format, machineName);
	}

	public static string Get_FX_Machine_HighWin_FrameLight(string machineName)
	{
		return string.Format(FX_Machine_HighWin_FrameLight_Format, machineName);
	}

	public static string Get_FX_Machine_Hype_FrameLight(string machineName)
	{
		return string.Format(FX_Machine_Hype_FrameLight_Format, machineName);
	}

	public static string Get_FX_Machine_Symbol_Up_Reel(string machineName)
	{
		return string.Format (FX_Machine_Symbol_Up_Format, machineName);
	}

	public static string Get_FX_Machine_Symbol_Down_Reel(string machineName)
	{
		return string.Format (FX_Machine_Symbol_Down_Format, machineName);
	}
}
