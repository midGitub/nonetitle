using System.Collections;
using System.Collections.Generic;
using System;

// Note:
// When adding a new state, you should add the following code:
// (1) PuzzleDefine.CanSmallGameStateHypeDict
// (2) PlayModuleFactory.CreatePlayModule
public enum SmallGameState
{
	None,
	FreeSpin,
	Rewind,
	FixWild,
	Wheel,
	Jackpot,
	TapBox,
	SwitchSymbol,
	Count,
}

[Flags]
public enum SmallGameMomentType
{
	None = 0x0,
	Front = 0x1,
	Behind = 0x2,
	Count = 0xFF,
}

// 触发类型 来触发SmallGameState里的小游戏
public enum TriggerType{
	None,
	Collect, // 收集触发
	Payout, // 指定payout触发
	UnorderCount, //Unorder区域symbol达到某个数量触发
	Max
}

// fixwild玩法类型
public enum FixWildType{
	None,
	Frozen,// 冰冻机台
	FishHunting,// 捕鱼机台
	Max,
}

// freespin玩法的类型
public enum FreeSpinType{
	None,
	FixCount, // 固定次数
	SpinUntilLose,// 转到失败
	ReachBonusWild,
	Max
}

public static class CoreSmallGameDefine
{
	//add in the future
}

