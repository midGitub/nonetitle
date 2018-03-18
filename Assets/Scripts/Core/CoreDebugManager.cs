using System.Collections;
using System.Collections.Generic;

public class CoreDebugManager : SimpleSingleton<CoreDebugManager>
{
	public bool IsHugeWinMode;
	public float WinProbInHugeWinMode = 0.85f;
	public float NearHitProbInHugeWinMode
	{
		get { return (1.0f - WinProbInHugeWinMode) * 0.5f; }
	}
	public float LossProbInHugeWinMode
	{
		get { return (1.0f - WinProbInHugeWinMode) * 0.5f; }
	}
}
