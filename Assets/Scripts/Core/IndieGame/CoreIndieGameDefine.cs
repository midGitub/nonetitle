using System.Collections.Generic;

#if UNITY_EDITOR

public class MachineTestIndieGameResult
{
	public ulong _winAmount = 0;
	public string _customData;
}

public static class CoreIndieGameDefine
{
	// It's better to define the count in BasicConfig sheet,
	// But since it's used only once, define it here is ok
	public static int MaxTapChipCount = 15;
}

#endif


